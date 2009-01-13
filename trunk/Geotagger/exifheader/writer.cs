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
    // Losslessly update the exif data in a JPG file.
    class JpegWriter
    {
        public GpsLocation gpsLocation
        {
            set { mGpsData = value; }
        }

        // Use the data collected in the ExifDirectory to extract the thumbnail, if available.
        public byte[] GetThumbnail(ExifDirectory dir, byte[] exif)
        {
            byte[] thumbnail = null;
            uint thumbnailSize = 0;
            uint thumbnailOffset = 0;

            // Recursively search through the ExifDirectory for the thumbnail offset and length
            // If they are found then return a byte array containing the thumbnail.
            foreach (ExifEntry e in dir)
            {
                switch (e.tag)
                {
                    case Tag.THUMBNAIL_OFFSET:
                        thumbnailOffset = mMemOps.GetUInt32(e.data, 0);
                        break;

                    case Tag.THUMBNAIL_LENGTH:
                        thumbnailSize = mMemOps.GetUInt32(e.data, 0);
                        break;

                    default:
                        // If this is a subdir, then search that for the thumbnail.
                        // In theory, if the thumbnail size and offset are in different
                        // directories, then we won't find it.  Let's hope that doesn't happen.
                        if (e.subdir != null)
                        {
                            thumbnail = GetThumbnail(e.subdir, exif);
                            if (thumbnail != null)
                            {
                                break;
                            }
                        }
                        break;
                }
            }

            // Store the thumbnail.
            if (thumbnailSize > 0 && thumbnailOffset > 0)
            {
                thumbnail = new byte[thumbnailSize];
                CopyBytes(thumbnail, thumbnailSize, exif, (int)thumbnailOffset);
                Console.WriteLine("Thumbnail found, size={0}, offset={1}", thumbnailSize, thumbnailOffset);
            }
            else
            {
                Console.WriteLine("No thumbnail found, size={0}, offset={1}", thumbnailSize, thumbnailOffset);
            }

            return thumbnail;
        }

        public void UpdateThumbnail(ExifDirectory dir, byte[] thumbnail, uint thumbnailOffset)
        {
            // Recursively search through the ExifDirectory for the thumbnail offset and length
            // If they are found then return a byte array containing the thumbnail.
            foreach (ExifEntry e in dir)
            {
                switch (e.tag)
                {
                    case Tag.THUMBNAIL_OFFSET:
                        mMemOps.SetUInt32(e.data, 0, thumbnailOffset);
                        break;

                    case Tag.THUMBNAIL_LENGTH:
                        // We don't need to write the length since it should be the same.
                        // In fact, this would be a good place to confirm that
                        if (mMemOps.GetUInt32(e.data, 0) != (uint)thumbnail.Length)
                        {
                            throw new Exception("The thumbnail length is invalid");
                        }
                        //mMemOps.SetUInt32(e.data, 0, (uint)thumbnail.Length);
                        break;

                    default:
                        if (e.subdir != null)
                        {
                            UpdateThumbnail(e.subdir, thumbnail, thumbnailOffset);
                        }
                        break;
                }
            }
        }

        // Postprocess the data, update the thumbnail location, and write it to the stream.
        public void OutputExif(Stream outfile, byte[] head, byte[] data, byte[] thumbnail)
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

        public void WriteDataToFile(string filename)
        {
            if (mGpsData.HasValue)
            {
                Console.WriteLine("Writing GPS data to {0}", filename);
            }

            // Open the file for reading
            FileStream infile = new FileStream(filename, FileMode.Open, FileAccess.Read);

            // Create the output stream - memory backed, not file.
            MemoryStream outfile = new MemoryStream();

            // Scan through the file looking for existing data
            // If data does not exist then add it to the file and update the headers
            // If it does exist, then replace it and update the headers.
            bool result = ProcessJpegSections(outfile, infile);

            if (result)
            {
#if DEBUG
                File.WriteAllBytes("test.jpg", outfile.GetBuffer());
                DebugCompareStreams(outfile, infile);
#endif
            }

            outfile.Close();
        }

        // Process infile, writing the output to outfile.
        // Returns false if there were any errors.
        // On error, the contents of outfile may be invalid.
        // infile will not be modified, except for the current position
        // of the stream.
        private bool ProcessJpegSections(Stream outfile, Stream infile)
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
                int got = infile.Read(sectionData, 2, length-2);
                if (got != length-2)
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
                            if (!ProcessExif(outfile, sectionData))
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

        private bool ProcessExif(Stream outfile, byte[] exif)
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

            // Initialize dynamic exif data here and hope that it is filled out in ProcessExifDir.
            mExifData.Reset();

            // Process the RAW exif data into an ExifDirectory for further parsing.
            // First directory starts 16 bytes in.  All offset are relative to 8 bytes in.
            ExifDirectory dir = ProcessExifDir(outfile, exif, 8 + firstOffset, 8, exif.Length - 8, 0);

            if (dir != null)
            {
                // If we succeeded in parsing the Exif directory, then let's write out all the data.

                // We need to...
                // 1. Create a new GPS dir entry with the new data.
                // 2. Overwrite the offset of the entry to point to our new data.
                // 3. Write the new GPS entry at the end of the exif data (alignment?)
                // 4. Update the exif size.
                // 5. Write the exif data out to the file and dump the rest of the file.
                //
                // TODO: Fixup the exif data instead of simply skipping the old data.
                // There is more room for error in that situation, but it would be more
                // "correct".  We could collect each entry into a data structure and,
                // after fixing up the GPS data, write them all out to the file.

                // Copy the exif header to the output stream up to firstOffset
                byte[] head = new byte[8+firstOffset];
                CopyBytes(head, (uint)(8 + firstOffset), exif, 0);

                // Process the directory and extract the thumbnail.
                byte[] thumbnail = GetThumbnail(dir, exif);

                // Convert the ExifDirectory back into RAW exif data.
                byte[] data = dir.BuildData(mMemOps, (uint)firstOffset);
                int dataLength = data.Length;

                // Assuming that the data will be the same size if we update the thumbnail data,
                // update the thumbnail in the ExifDirectory and rebuild the data.
                uint thumbnailOffset = (uint)(head.Length + data.Length);
                UpdateThumbnail(dir, thumbnail, thumbnailOffset - (uint)firstOffset);

                // Regen the data again.
                data = dir.BuildData(mMemOps, (uint)firstOffset);
                if (data.Length != dataLength)
                {
                    throw new Exception("The data length changed");
                }

                OutputExif(outfile, head, data, thumbnail);
            }

            return (dir != null);
        }

        private static int DirEntryOffset(int start, int entry)
        {
            return start + 2 + (12 * entry);
        }

        private ExifDirectory ProcessExifSubDirTag(Stream outfile, byte[] exif, ExifFormat format, int offsetBase, int offset, int length, int nestingLevel)
        {
            if (format != ExifFormat.EXIF_ULONG)
            {
                Console.WriteLine("ERROR: offset is invalid");
                return null;
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

        private ExifDirectory ProcessExifDir(Stream outfile, byte[] exif, int start, int offsetBase, int length, int nestingLevel)
        {
            ExifDirectory directory = new ExifDirectory();

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
                CopyBytes(entry.data, entry.size, exif, dataoffset);

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

        private bool ProcessXmp(Stream outfile, byte[] xmp)
        {
            outfile.Write(xmp, 0, xmp.Length);
            return true;
        }

        // Read one byte from the instream and write it through to the outstream
        // Returns the byte
        private byte ReadThroughByte(Stream outstream, Stream instream)
        {
            int result = instream.ReadByte();
            outstream.WriteByte((byte)result);
            return (byte)result;
        }

        // Read one byte from the instream
        // Returns the byte
        private byte ReadByte(Stream instream)
        {
            return (byte)instream.ReadByte();
        }

        private void ReadThroughRemaining(Stream outfile, Stream infile)
        {
            byte[] buffer = new byte[1024];

            int amountRead = 0;
            while ( (amountRead = infile.Read(buffer, 0, 1024)) > 0)
            {
                outfile.Write(buffer, 0, amountRead);
            }
        }

        private void CopyBytes(byte[] destination, uint size, byte[] bytes, int offset)
        {
            Array.Copy(bytes, offset, destination, 0, size);
        }

#if DEBUG
        void DebugCompareStreams(Stream s1, Stream s2)
        {
            s1.Seek(0, SeekOrigin.Begin);
            s2.Seek(0, SeekOrigin.Begin);

            if (s1.Length != s2.Length)
            {
                // Different lengths.
                Console.WriteLine("ERROR: DebugCompareStreams - Streams are different lengths, ({0} and {1})", s1.Length, s2.Length);
                //return;
            }

            int b1 = 0;
            int b2 = 0;
            do
            {
                b1 = s1.ReadByte();
                b2 = s2.ReadByte();
            }
            while ((b1 == b2) && (b1 != -1));

            if (b1 != b2)
            {
                Console.WriteLine("ERROR: DebugCompareStreams - Streams do not match!");
            }
        }
#endif

        // Value is nullable and initializes to null.
        // We only is it if is not null.
        private GpsLocation? mGpsData;

        // Data that is collected when processing the exif directory.
        // This is information we need to help write out a new exif directory.
        struct ExifParseData
        {
            // The offset of the location where the GPS directory data is stored.
            public int      mGpsOffset;

            public void Reset()
            {
                mGpsOffset = 0;
            }
        };
        
        private ExifParseData mExifData;        
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

    public enum Tag
    {
        INTEROP_INDEX        = 0x0001,
        INTEROP_VERSION      = 0x0002,
        IMAGE_WIDTH          = 0x0100,
        IMAGE_LENGTH         = 0x0101,
        BITS_PER_SAMPLE      = 0x0102,
        COMPRESSION          = 0x0103,
        PHOTOMETRIC_INTERP   = 0x0106,
        FILL_ORDER           = 0x010A,
        DOCUMENT_NAME        = 0x010D,
        IMAGE_DESCRIPTION    = 0x010E,
        MAKE                 = 0x010F,
        MODEL                = 0x0110,
        SRIP_OFFSET          = 0x0111,
        ORIENTATION          = 0x0112,
        SAMPLES_PER_PIXEL    = 0x0115,
        ROWS_PER_STRIP       = 0x0116,
        STRIP_BYTE_COUNTS    = 0x0117,
        X_RESOLUTION         = 0x011A,
        Y_RESOLUTION         = 0x011B,
        PLANAR_CONFIGURATION = 0x011C,
        RESOLUTION_UNIT      = 0x0128,
        TRANSFER_FUNCTION    = 0x012D,
        SOFTWARE             = 0x0131,
        DATETIME             = 0x0132,
        ARTIST               = 0x013B,
        WHITE_POINT          = 0x013E,
        PRIMARY_CHROMATICITIES = 0x013F,
        TRANSFER_RANGE       = 0x0156,
        JPEG_PROC            = 0x0200,
        THUMBNAIL_OFFSET     = 0x0201,
        THUMBNAIL_LENGTH     = 0x0202,
        Y_CB_CR_COEFFICIENTS = 0x0211,
        Y_CB_CR_SUB_SAMPLING = 0x0212,
        Y_CB_CR_POSITIONING  = 0x0213,
        REFERENCE_BLACK_WHITE= 0x0214,
        RELATED_IMAGE_WIDTH  = 0x1001,
        RELATED_IMAGE_LENGTH = 0x1002,
        CFA_REPEAT_PATTERN_DIM = 0x828D,
        CFA_PATTERN1         = 0x828E,
        BATTERY_LEVEL        = 0x828F,
        COPYRIGHT            = 0x8298,
        EXPOSURETIME         = 0x829A,
        FNUMBER              = 0x829D,
        IPTC_NAA             = 0x83BB,
        EXIF_OFFSET          = 0x8769,
        INTER_COLOR_PROFILE  = 0x8773,
        EXPOSURE_PROGRAM     = 0x8822,
        SPECTRAL_SENSITIVITY = 0x8824,
        GPSINFO              = 0x8825,
        ISO_EQUIVALENT       = 0x8827,
        OECF                 = 0x8828,
        EXIF_VERSION         = 0x9000,
        DATETIME_ORIGINAL    = 0x9003,
        DATETIME_DIGITIZED   = 0x9004,
        COMPONENTS_CONFIG    = 0x9101,
        CPRS_BITS_PER_PIXEL  = 0x9102,
        SHUTTERSPEED         = 0x9201,
        APERTURE             = 0x9202,
        BRIGHTNESS_VALUE     = 0x9203,
        EXPOSURE_BIAS        = 0x9204,
        MAXAPERTURE          = 0x9205,
        SUBJECT_DISTANCE     = 0x9206,
        METERING_MODE        = 0x9207,
        LIGHT_SOURCE         = 0x9208,
        FLASH                = 0x9209,
        FOCALLENGTH          = 0x920A,
        MAKER_NOTE           = 0x927C,
        USERCOMMENT          = 0x9286,
        SUBSEC_TIME          = 0x9290,
        SUBSEC_TIME_ORIG     = 0x9291,
        SUBSEC_TIME_DIG      = 0x9292,
        WINXP_TITLE          = 0x9c9b, // Windows XP - not part of exif standard.
        WINXP_COMMENT        = 0x9c9c, // Windows XP - not part of exif standard.
        WINXP_AUTHOR         = 0x9c9d, // Windows XP - not part of exif standard.
        WINXP_KEYWORDS       = 0x9c9e, // Windows XP - not part of exif standard.
        WINXP_SUBJECT        = 0x9c9f, // Windows XP - not part of exif standard.

        FLASH_PIX_VERSION    = 0xA000,
        COLOR_SPACE          = 0xA001,
        EXIF_IMAGEWIDTH      = 0xA002,
        EXIF_IMAGELENGTH     = 0xA003,
        RELATED_AUDIO_FILE   = 0xA004,
        INTEROP_OFFSET       = 0xA005,
        FLASH_ENERGY         = 0xA20B,
        SPATIAL_FREQ_RESP    = 0xA20C,
        FOCAL_PLANE_XRES     = 0xA20E,
        FOCAL_PLANE_YRES     = 0xA20F,
        FOCAL_PLANE_UNITS    = 0xA210,
        SUBJECT_LOCATION     = 0xA214,
        EXPOSURE_INDEX       = 0xA215,
        SENSING_METHOD       = 0xA217,
        FILE_SOURCE          = 0xA300,
        SCENE_TYPE           = 0xA301,
        CFA_PATTERN          = 0xA302,
        CUSTOM_RENDERED      = 0xA401,
        EXPOSURE_MODE        = 0xA402,
        WHITEBALANCE         = 0xA403,
        DIGITALZOOMRATIO     = 0xA404,
        FOCALLENGTH_35MM     = 0xA405,
        SCENE_CAPTURE_TYPE   = 0xA406,
        GAIN_CONTROL         = 0xA407,
        CONTRAST             = 0xA408,
        SATURATION           = 0xA409,
        SHARPNESS            = 0xA40A,
        DISTANCE_RANGE       = 0xA40C,

        // Special non-exif tags used for internal purposes
        // Use more than 16 bits to avoid collisions with real tags.
        EXIF_OFFSET_APPENDED = 0x10000,
    }
}
