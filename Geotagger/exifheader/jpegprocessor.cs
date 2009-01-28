//////////////////////////////////////////////////////////////////////////////
//
//    This file is part of Geotagger: A tool for geotagging photographs
//    Copyright (C) 2007  Kaz Okuda (http://notions.okuda.ca)
//
//    Geotagger is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ExifHeader
{
    class JpegProcessor
    {
        // Process infile, writing the output to outfile.
        // Runs the exifAction on the ExifDirectory before it is output.
        // Returns false if there were any errors.
        // On error, the contents of outfile may be invalid.
        // infile will not be modified, except for the current position
        // of the stream.
        public bool ProcessJpegFile(Stream outfile, Stream infile, Action<ExifDirectory> exifAction)
        {
            if (ReadThroughByte(outfile, infile) != 0xff || ReadThroughByte(outfile, infile) != M_SOI)
            {
                // Not a JPEG file,
                return false;
            }

            bool result = false;
            bool done = false;
            while (!done && (infile.Position < infile.Length))
            {
                byte marker = 0xFF;

                // Skip any padding
                for (int a = 0; a < 7; a++)
                {
                    marker = ReadThroughByte(outfile, infile);
                    if (marker != 0xff) break;
                }

                if (marker == 0xff)
                {
                    Console.WriteLine("ERROR: Error processing file, could not find next marker");
                    break;
                }

                // Read the length of the section.
                // Note that this is not written into outfile yet,
                // we might change the size later.
                byte lh = ReadByte(infile);
                byte ll = ReadByte(infile);
                int length = lh << 8 | ll;

                if (length < 2)
                {
                    Console.WriteLine("ERROR: Invalid marker length");
                    break;
                }

                // Read the entire section into memory
                byte[] sectionData = new byte[length];

                // The length includes the length bytes, which was not yet written to outfile.
                sectionData[0] = lh;
                sectionData[1] = ll;

                // Read the rest of the section after the length
                int got = infile.Read(sectionData, 2, length - 2);
                if (got != length - 2)
                {
                    Console.WriteLine("ERROR: Premature end of file?");
                    break;
                }

                switch (marker)
                {
                    case M_SOS:
                    case M_EOI:
                        // End of header, just write it out and skip past the end of the file.
                        outfile.Write(sectionData, 0, sectionData.Length);
                        ReadThroughRemaining(outfile, infile);
                        result = true;
                        done = true;
                        break;

                    case M_EXIF:
                        // Process the Exif data

                        // If the section starts with "Exif" then this is a standard Exif section.
                        // If the section starts with "http:" then it is an XMP section.

                        if (Encoding.ASCII.GetString(sectionData, 2, 4).CompareTo("Exif") == 0)
                        {
                            if (!ProcessExif(outfile, sectionData, exifAction))
                            {
                                done = true;
                                result = false;
                            }
                        }
                        else if (Encoding.ASCII.GetString(sectionData, 2, 5).CompareTo("http:") == 0)
                        {
                            if (!ProcessXmp(outfile, sectionData))
                            {
                                done = true;
                                result = false;
                            }
                        }
                        else
                        {
                            // Don't know what this is, so just pass through
                            outfile.Write(sectionData, 0, sectionData.Length);
                        }
                        break;

                    default:
                        // We don't care about this section, so just write it out as-is
                        outfile.Write(sectionData, 0, sectionData.Length);
                        break;
                }
            }

            return result;
        }

        ///////////////////////////////////////////////////////////////////////

        // Postprocess the data, update the thumbnail location, and write it to the stream.
        private void OutputExif(Stream outfile, byte[] head, byte[] data, byte[] thumbnail)
        {
            uint thumbnailLength = thumbnail == null ? 0 : (uint)thumbnail.Length;
            uint exiflength = (uint)(head.Length + data.Length + thumbnailLength);

            // Update the exif size in the head.
            uint lh = exiflength >> 8 & 0xFF;
            uint ll = exiflength & 0xFF;
            head[0] = (byte)lh;
            head[1] = (byte)ll;

            outfile.Write(head, 0, head.Length);
            outfile.Write(data, 0, data.Length);
            if (thumbnailLength > 0)
            {
                outfile.Write(thumbnail, 0, thumbnail.Length);
            }
        }

        ///////////////////////////////////////////////////////////////////////

        private bool ProcessExif(Stream outfile, byte[] exif, Action<ExifDirectory> exifAction)
        {
            mMemOps = new MemOperations();
            Console.WriteLine("Exif header is {0} bytes long", exif.Length);

            string header = Encoding.ASCII.GetString(exif, 2, 6);
            if (header.CompareTo("Exif\0\0") != 0)
            {
                Console.WriteLine("ERROR: Incorrect exif header \"{0}\"", header);
                return false;
            }

            // Calculate the byte order of the header data.
            string byteorder = Encoding.ASCII.GetString(exif, 8, 2);
            if (byteorder.CompareTo("II") == 0)
            {
                mMemOps.SetByteOrder(ByteOrder.INTEL);
                Console.WriteLine("Intel byte order");
            }
            else if (byteorder.CompareTo("MM") == 0)
            {
                mMemOps.SetByteOrder(ByteOrder.MOTOROLA);
                Console.WriteLine("Motorola byte order");
            }
            else
            {
                Console.WriteLine("ERROR: Unknown exif byte order \"{0}\"", byteorder);
                return false;
            }

            // Check the next value for correctness.
            uint exifstart = mMemOps.GetUInt16(exif, 10);
            if (exifstart != 0x2a)
            {
                Console.WriteLine("ERROR: Invalid Exif start {0}", exifstart);
                return false;
            }

            int firstOffset = mMemOps.GetInt32(exif, 12);
            if (firstOffset < 8 || firstOffset > 16)
            {
                // Usually set to 8, but other values valid too.
                Console.WriteLine("Suspicious offset of first IFD value {0}", firstOffset);
            }

            // Process the RAW exif data into an ExifDirectory for further parsing.
            // First directory starts 16 bytes in.  All offset are relative to 8 bytes in.
            ExifDirectory dir = ProcessExifDir(outfile, exif, 8 + firstOffset, 8, exif.Length - 8, 0);

            if (dir != null)
            {
                // If we succeeded in parsing the Exif directory,
                // then let's write out all the data.

                // Do the user action on the directory
                if (exifAction != null)
                {
                    exifAction(dir);
                }

                // Copy the exif header to the output stream up to firstOffset
                byte[] head = new byte[8+firstOffset];
                mMemOps.CopyBytes(head, (uint)(8 + firstOffset), exif, 0);

                // Process the directory and extract the thumbnail.
                byte[] thumbnail = dir.GetThumbnail(exif, (uint)firstOffset);

                // Convert the ExifDirectory back into RAW exif data.
                byte[] data = dir.BuildData((uint)firstOffset);
                int dataLength = data.Length;

                // Assuming that the data will be the same size if we update the thumbnail data,
                // update the thumbnail in the ExifDirectory and rebuild the data.
                uint thumbnailOffset = (uint)(head.Length + data.Length);
                dir.UpdateThumbnail(thumbnail, thumbnailOffset - (uint)firstOffset);

                // Regen the data again.
                data = dir.BuildData((uint)firstOffset);
                if (data.Length != dataLength)
                {
                    throw new Exception("The data length changed");
                }

                OutputExif(outfile, head, data, thumbnail);
            }

            return (dir != null);
        }

        ///////////////////////////////////////////////////////////////////////

        private static int DirEntryOffset(int start, int entry)
        {
            return start + 2 + (12 * entry);
        }

        ///////////////////////////////////////////////////////////////////////

        private ExifDirectory ProcessExifSubDirTag(Stream outfile, byte[] exif, ExifFormat format, int offsetBase, int offset, int length, int nestingLevel)
        {
            if (format != ExifFormat.EXIF_ULONG)
            {
                Console.WriteLine("WARNING: subdir format is unusual {0}", format);
//                return null;
            }
            // Should this be a UInt?  Should we just use uint for everything?
            int subdirStart = offsetBase + mMemOps.GetInt32(exif, offset + 8);
            if ((subdirStart < offset) || (subdirStart > offset + length))
            {
                Console.WriteLine("ERROR: offset is invalid");
                return null;
            }
            return ProcessExifDir(outfile, exif, subdirStart, offsetBase, length, nestingLevel + 1);
        }

        ///////////////////////////////////////////////////////////////////////

        private ExifDirectory ProcessExifDir(Stream outfile, byte[] exif, int start, int offsetBase, int length, int nestingLevel)
        {
            ExifDirectory directory = new ExifDirectory(mMemOps);

            if (nestingLevel > 4)
            {
                Console.WriteLine("ERROR: Maximum directory nesting exceeded (corrupt exif header)");
                return null;
            }

            ushort numDirEntries = mMemOps.GetUInt16(exif, start);
            Console.WriteLine("Found {0} Exif dir entries", numDirEntries);

            int end = DirEntryOffset(start, numDirEntries);
            if (end > length)
            {
                Console.WriteLine("ERROR: Error parsing Exif directory.  Have {0} bytes, need {1} bytes", length, end);
                return null;
            }

            for (int i = 0; i < numDirEntries; ++i)
            {
                int offset = DirEntryOffset(start, i);
                int tag = mMemOps.GetUInt16(exif, offset);
                int format = mMemOps.GetUInt16(exif, offset + 2);
                int components = (int)mMemOps.GetUInt32(exif, offset + 4);

                // Sanity check
                if ((format > (int)ExifFormat.EXIF_MAX) || (format < (int)ExifFormat.EXIF_MIN))
                {
                    Console.WriteLine("ERROR: Illegal format value {0}", format);
                    continue;
                }

                if (components > 0x10000)
                {
                    Console.WriteLine("ERROR: Too many components {0}", components);
                    continue;
                }

                ExifEntry entry = new ExifEntry();
                entry.tag = (Tag)tag;
                entry.format = new ExifFormat(format);
                entry.components = (uint)components;
                entry.size = entry.format.size  * entry.components;

                // If the data size is > 4 then the entry is an offset to the data.
                // If the data size is < 4 then the entry is the data.
                int dataoffset = 0;
                if (entry.size > 4)
                {
                    dataoffset = mMemOps.GetInt32(exif, offset + 8) + offsetBase;
                }
                else
                {
                    dataoffset = offset + 8;
                }

                entry.data = new byte[entry.size];
                mMemOps.CopyBytes(entry.data, entry.size, exif, dataoffset);

                switch (entry.tag)
                {
                    case Tag.GPSINFO:
                        {
                            Console.WriteLine("Found GPSINFO Tag");
                            ExifDirectory dir = ProcessExifSubDirTag(outfile, exif, new ExifFormat(format), offsetBase, offset, length, nestingLevel);
                            if (dir == null)
                            {
                                return null;
                            }
                            else
                            {
                                entry.subdir = dir;
                            }
                            break;
                        }

                    case Tag.INTEROP_OFFSET:
                        {
                            Console.WriteLine("Found INTEROP_OFFSET Tag");
                            ExifDirectory dir = ProcessExifSubDirTag(outfile, exif, new ExifFormat(format), offsetBase, offset, length, nestingLevel);
                            if (dir == null)
                            {
                                return null;
                            }
                            else
                            {
                                entry.subdir = dir;
                            }
                            break;
                        }

                    case Tag.EXIF_OFFSET:
                        {
                            Console.WriteLine("Found EXIF_OFFSET Tag");
                            ExifDirectory dir = ProcessExifSubDirTag(outfile, exif, new ExifFormat(format), offsetBase, offset, length, nestingLevel);
                            if (dir == null)
                            {
                                return null;
                            }
                            else
                            {
                                entry.subdir = dir;
                            }
                        }
                        break;

                    case Tag.MAKER_NOTE:
                        {
                            Console.WriteLine("Found MAKER_NOTE Tag");
                            ExifDirectory dir = ProcessExifSubDirTag(outfile, exif, new ExifFormat(format), offsetBase, offset, length, nestingLevel);
                            if (dir == null)
                            {
                                return null;
                            }
                            else
                            {
                                entry.subdir = dir;
                            }
                        }
                        break;

                    default:
                        // We don't care about any entries other than the GPS info
                        break;
                }

                directory.Add(entry);
            }

            if (end < length + 4)
            {
                // Apparently there can be another directory entry appended after this.
                // I think that the offset is unsigned, but I don't think it should ever be large
                // enough to care.
                int nextoffset = (int)mMemOps.GetUInt32(exif, DirEntryOffset(start, numDirEntries));
                if (nextoffset != 0)
                {
                    if (nextoffset > length)
                    {
                        // Invalid offset
                        return null;
                    }
                    else
                    {
                        Console.WriteLine("Found extra dir entry at the end of the last one");
                        ExifDirectory dir = ProcessExifDir(outfile, exif, offsetBase + nextoffset, offsetBase, length, nestingLevel + 1);
                        if (dir == null)
                        {
                            return null;
                        }
                        else
                        {
                            ExifEntry entry = new ExifEntry();
                            entry.tag = Tag.EXIF_OFFSET_APPENDED;
                            entry.format = new ExifFormat(ExifFormat.EXIF_ULONG);
                            entry.components = (uint)1;
                            entry.size = entry.format.size * entry.components;
                            entry.subdir = dir;
                            entry.size = 4;
                            entry.data = new byte[entry.size];
                            directory.Add(entry);
                        }
                    }
                }
            }

            return directory;
        }

        ///////////////////////////////////////////////////////////////////////

        private bool ProcessXmp(Stream outfile, byte[] xmp)
        {
            outfile.Write(xmp, 0, xmp.Length);
            return true;
        }

        ///////////////////////////////////////////////////////////////////////

        // Read one byte from the instream and write it through to the outstream
        // Returns the byte
        private byte ReadThroughByte(Stream outstream, Stream instream)
        {
            int result = instream.ReadByte();
            outstream.WriteByte((byte)result);
            return (byte)result;
        }

        ///////////////////////////////////////////////////////////////////////

        // Read one byte from the instream
        // Returns the byte
        private byte ReadByte(Stream instream)
        {
            return (byte)instream.ReadByte();
        }

        ///////////////////////////////////////////////////////////////////////

        private void ReadThroughRemaining(Stream outfile, Stream infile)
        {
            byte[] buffer = new byte[1024];

            int amountRead = 0;
            while ( (amountRead = infile.Read(buffer, 0, 1024)) > 0)
            {
                outfile.Write(buffer, 0, amountRead);
            }
        }

        ///////////////////////////////////////////////////////////////////////

        private MemOperations mMemOps;

        // Constants
        private const byte M_SOI = 0xD8;          // Start Of Image (beginning of datastream)
        private const byte M_EOI = 0xD9;          // End Of Image (end of datastream)
        private const byte M_SOS = 0xDA;          // Start Of Scan (begins compressed data)
        private const byte M_JFIF= 0xE0;          // Jfif marker
        private const byte M_EXIF= 0xE1;          // Exif marker.  Also used for XMP data!
        private const byte M_COM = 0xFE;          // COMment 
        private const byte M_DQT = 0xDB;
        private const byte M_DHT = 0xC4;
        private const byte M_DRI = 0xDD;
        private const byte M_IPTC= 0xED;          // IPTC marker

        private const int  M_XMP = 0x10E1;        // Not a real tag (same value in file as Exif!)
    }
}
