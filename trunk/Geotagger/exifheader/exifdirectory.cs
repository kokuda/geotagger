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
    public struct ExifEntry
    {
        public Tag tag;
        public ExifFormat format;
        public uint components;
        public uint size;
        public byte[] data;
        public ExifDirectory subdir;

        // Construct ExifEntry with initial values.
        // data will be allocated, but must be initialized.
        public ExifEntry(Tag t, ExifFormat f, uint c, uint s)
        {
            tag = t;
            format = f;
            components = c;
            size = s;
            data = new byte[size];
            subdir = null;
        }
    }

    public class ExifDirectory : List<ExifEntry>
    {
        // Construct with a MemOperations object for the byte order.
        // We could change the output byte order by changing this object.
        public ExifDirectory(MemOperations memOps)
        {
            mMemOps = memOps;
        }

        ///////////////////////////////////////////////////////////////////////

        // Use the data collected in the ExifDirectory to extract the thumbnail, if available.
        public byte[] GetThumbnail(byte[] exif, uint firstOffset)
        {
            byte[] thumbnail = null;
            uint thumbnailSize = 0;
            uint thumbnailOffset = 0;

            // Recursively search through the ExifDirectory for the thumbnail offset and length
            // If they are found then return a byte array containing the thumbnail.
            foreach (ExifEntry e in this)
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
                            thumbnail = e.subdir.GetThumbnail(exif, firstOffset);
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
                mMemOps.CopyBytes(thumbnail, thumbnailSize, exif, (int)(thumbnailOffset + firstOffset));
                Console.WriteLine("Thumbnail found, size={0}, offset={1}", thumbnailSize, thumbnailOffset);
            }
            else
            {
                //Console.WriteLine("No thumbnail found, size={0}, offset={1}", thumbnailSize, thumbnailOffset);
            }

            return thumbnail;
        }

        ///////////////////////////////////////////////////////////////////////

        public void UpdateThumbnail(byte[] thumbnail, uint thumbnailOffset)
        {
            // Recursively search through the ExifDirectory for the thumbnail offset and length
            // If they are found then return a byte array containing the thumbnail.
            foreach (ExifEntry e in this)
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
                            e.subdir.UpdateThumbnail(thumbnail, thumbnailOffset);
                        }
                        break;
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////

        public void RemoveTag(Tag tag)
        {
            RemoveAll(delegate(ExifHeader.ExifEntry e)
            {
                if (e.tag == ExifHeader.Tag.GPSINFO)
                {
                    return true;
                }
                else
                {
                    if (e.subdir != null)
                    {
                        e.subdir.RemoveTag(tag);
                    }
                    return false;
                }
            });
        }

        public void CreateGps(GpsLocation gpsData)
        {
            ExifDirectory dir = new ExifDirectory(mMemOps);
            ExifEntry e;

            // Insert Version Info                
            e = new ExifEntry(0, new ExifFormat(1), 4, 4);
            e.data[0] = 2;
            e.data[1] = 0;
            e.data[2] = 0;
            e.data[3] = 0;
            dir.Add(e);

            // Insert the lat ref
            e = new ExifEntry((Tag)1, new ExifFormat(2), 2, 2);
            if (gpsData.latRef == GpsLocation.LatRef.NORTH)
            {
                e.data[0] = (byte)'N';
            }
            else
            {
                e.data[0] = (byte)'S';
            }
            e.data[1] = 0;
            dir.Add(e);

            // Insert the lat
            e = new ExifEntry((Tag)2, new ExifFormat(5), 3, 24);
            mMemOps.SetUInt32(e.data, 0, (uint)gpsData.lat.degree.numerator);
            mMemOps.SetUInt32(e.data, 4, (uint)gpsData.lat.degree.denominator);
            mMemOps.SetUInt32(e.data, 8, (uint)gpsData.lat.minute.numerator);
            mMemOps.SetUInt32(e.data, 12, (uint)gpsData.lat.minute.denominator);
            mMemOps.SetUInt32(e.data, 16, (uint)gpsData.lat.second.numerator);
            mMemOps.SetUInt32(e.data, 20, (uint)gpsData.lat.second.denominator);
            dir.Add(e);

            // Insert the lon ref
            e = new ExifEntry((Tag)3, new ExifFormat(2), 2, 2);
            if (gpsData.lonRef == GpsLocation.LonRef.EAST)
            {
                e.data[0] = (byte)'E';
            }
            else
            {
                e.data[0] = (byte)'W';
            }
            e.data[1] = 0;
            dir.Add(e);

            // Insert the lon
            e = new ExifEntry((Tag)4, new ExifFormat(5), 3, 24);
            mMemOps.SetUInt32(e.data, 0, (uint)gpsData.lon.degree.numerator);
            mMemOps.SetUInt32(e.data, 4, (uint)gpsData.lon.degree.denominator);
            mMemOps.SetUInt32(e.data, 8, (uint)gpsData.lon.minute.numerator);
            mMemOps.SetUInt32(e.data, 12, (uint)gpsData.lon.minute.denominator);
            mMemOps.SetUInt32(e.data, 16, (uint)gpsData.lon.second.numerator);
            mMemOps.SetUInt32(e.data, 20, (uint)gpsData.lon.second.denominator);
            dir.Add(e);

            // Insert alt sign (It seems that 1 means negative?)
            e = new ExifEntry((Tag)5, new ExifFormat(1), 1, 1);
            Rational alt = gpsData.alt;
            if (((float)alt) < 0)
            {
                e.data[0] = 1;
                alt = -alt;
            }
            else
            {
                e.data[0] = 0;
            }
            dir.Add(e);

            // Insert the alt
            e = new ExifEntry((Tag)6, new ExifFormat(5), 1, 8);
            mMemOps.SetUInt32(e.data, 0, (uint)alt.numerator);
            mMemOps.SetUInt32(e.data, 4, (uint)alt.denominator);
            dir.Add(e);

            ExifEntry entry = new ExifEntry(Tag.GPSINFO, new ExifFormat(4), 1, 4);
            entry.subdir = dir;
            this.Insert(this.Count - 1, entry);
        }

        ///////////////////////////////////////////////////////////////////////

        public byte[] BuildData(uint offsetBase)
        {
            uint offset = 0;
            uint dirCount = (uint)this.Count;

            // First let's figure out if there is an appended directory after this.
            // If the last entry is of type EXIF_OFFSET_APPENDED then it is an appended
            // directory and is handled differently.
            if (this[this.Count - 1].tag == Tag.EXIF_OFFSET_APPENDED)
            {
                // Ignore the last entry until later.
                dirCount--;
            }
            
            byte[] data = new byte[2 + dirCount * 12 + 4];

            // Write out the number of entries.
            offset = WriteUInt16(data, offset, dirCount);

            // Any extra data will follow this directory.
            // It will be 12 * count bytes past where we wrote the count
            // plus 4 to account for the padding for any possible appended directory.
            uint dataOffset = offset + (12 * dirCount) + 4;

            uint[] offsetList = new uint[dirCount];

            // Output the element
            for (int i = 0; i < dirCount; ++i)
            {
                ExifEntry e = this[i];
                offset = WriteUInt16(data, offset, (uint)e.tag);
                offset = WriteUInt16(data, offset, (uint)e.format);
                offset = WriteUInt32(data, offset, e.components);


                switch (e.tag)
                {
                    case Tag.EXIF_OFFSET:
                    case Tag.INTEROP_OFFSET:
                    case Tag.GPSINFO:
                    case Tag.MAKER_NOTE:

                        dataOffset = AlignDirectoryOffset(dataOffset);

                        e.data = e.subdir.BuildData(offsetBase + dataOffset);
                        offset = WriteUInt32(data, offset, offsetBase + dataOffset);
                        offsetList[i] = dataOffset;
                        dataOffset += (uint)e.data.Length;
                        this[i] = e;
                        break;

                    default:
                        // If the data size is <= 4 then we write it out next,
                        // otherwise we write out an offset to the data.
                        if (e.size > 4)
                        {
                            // We will put the data at the end of the elements
                            // so just write out the offset here.
                            offset = WriteUInt32(data, offset, offsetBase + dataOffset);

                            // record this for later so we can write out the data.
                            offsetList[i] = dataOffset;

                            // Update the data offset to point past that data.
                            dataOffset += e.size;
                        }
                        else
                        {
                            // Store the offset as 0 so we don't try to write out the data.
                            offsetList[i] = 0;

                            //outfile.Write(e.data, 0, (int)e.size);

                            // Don't assume that e.data.Length is the same as e.size.
                            // For EXIF_OFFSET elements, the data is an offset, so the size is 4,
                            // but we use e.data to store the actual data being referenced.
                            //e.data.CopyTo(data, offset);
                            Array.Copy(e.data, 0, data, offset, e.size);

                            offset += 4;
                        }
                        break;
                }
            }

            // Append any extra directories to the end
            uint appendedOffset = 0;
            byte[] appendedData = null;
            if (this[this.Count - 1].tag == Tag.EXIF_OFFSET_APPENDED)
            {
                ExifDirectory appendedDir = this[this.Count - 1].subdir;

                dataOffset = AlignDirectoryOffset(dataOffset);

                // Build a directory entry, but we will append it to the directory
                // instead of including it in the directory
                appendedData = appendedDir.BuildData(offsetBase + dataOffset);
                offset = WriteUInt32(data, offset, offsetBase + dataOffset);
                appendedOffset = dataOffset;
                dataOffset += (uint)appendedData.Length;
            }
            else
            {
                offset = WriteUInt32(data, offset, 0);
            }

            // Ensure that there is enough space for all the output
            uint dataSize = dataOffset;
            if (dataSize > data.Length)
            {
                byte[] newdata = new byte[dataSize];
                data.CopyTo(newdata, 0);
                data = newdata;
            }

            for (int i = 0; i < dirCount; ++i)
            {
                // Write out the data blocks.
                if (offsetList[i] != 0)
                {
                    this[i].data.CopyTo(data, offsetList[i]);
                }
            }

            // Append the appended exif directory.
            if (appendedData != null)
            {
                appendedData.CopyTo(data, appendedOffset);
            }

            return data;
        }

        ///////////////////////////////////////////////////////////////////////

        private uint WriteUInt32(byte[] bytes, uint offset, uint value)
        {
            mMemOps.SetUInt32(bytes, offset, value);
            return offset + 4;
        }

        ///////////////////////////////////////////////////////////////////////

        private uint WriteUInt16(byte[] data, uint offset, uint value)
        {
            mMemOps.SetUInt16(data, offset, value);
            return offset + 2;
        }

        ///////////////////////////////////////////////////////////////////////

        private uint AlignDirectoryOffset(uint dataOffset)
        {
            // Picasa seems to align each directory to 4 bytes.
            // I'm not sure if this is needed, but it shouldn't hurt.
            return (dataOffset + 3) & ~(uint)3;
        }

        ///////////////////////////////////////////////////////////////////////

        private MemOperations mMemOps;
    }
}
