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
        public uint size;
        public Tag tag;
        public ExifFormat format;
        public uint components;
        public byte[] data;
        public ExifDirectory subdir;
    }

    public class ExifDirectory : List<ExifEntry>
    {
        public uint Output(Stream outfile, MemOperations memOps, uint offsetBase)
        {
            byte[] data = BuildData(memOps, offsetBase);
            outfile.Write(data, 0, data.Length);
            return (uint)data.Length;
        }

        public uint AlignDirectoryOffset(uint dataOffset)
        {
            // Picasa seems to align each directory to 4 bytes.
            // I'm not sure if this is needed, but it shouldn't hurt.
            return (dataOffset + 3) & ~(uint)3;
        }

        public byte[] BuildData(MemOperations memOps, uint offsetBase)
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
            offset = WriteUInt16(data, offset, memOps, dirCount);

            // Any extra data will follow this directory.
            // It will be 12 * count bytes past where we wrote the count
            // plus 4 to account for the padding for any possible appended directory.
            uint dataOffset = offset + (12 * dirCount) + 4;

            uint[] offsetList = new uint[dirCount];

            // Output the element
            for (int i = 0; i < dirCount; ++i)
            {
                ExifEntry e = this[i];
                offset = WriteUInt16(data, offset, memOps, (uint)e.tag);
                offset = WriteUInt16(data, offset, memOps, (uint)e.format);
                offset = WriteUInt32(data, offset, memOps, e.components);


                switch (e.tag)
                {
                    case Tag.EXIF_OFFSET:
                    case Tag.INTEROP_OFFSET:
                    case Tag.GPSINFO:
                    case Tag.MAKER_NOTE:

                        dataOffset = AlignDirectoryOffset(dataOffset);

                        e.data = e.subdir.BuildData(memOps, offsetBase + dataOffset);
                        offset = WriteUInt32(data, offset, memOps, offsetBase + dataOffset);
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
                            offset = WriteUInt32(data, offset, memOps, offsetBase + dataOffset);

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
                appendedData = appendedDir.BuildData(memOps, offsetBase + dataOffset);
                offset = WriteUInt32(data, offset, memOps, offsetBase + dataOffset);
                appendedOffset = dataOffset;
                dataOffset += (uint)appendedData.Length;
            }
            else
            {
                offset = WriteUInt32(data, offset, memOps, 0);
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

        private static uint WriteUInt32(byte[] bytes, uint offset, MemOperations memOps, uint value)
        {
            memOps.SetUInt32(bytes, offset, value);
            return offset + 4;
        }

        private static uint WriteUInt16(byte[] data, uint offset, MemOperations memOps, uint value)
        {
            memOps.SetUInt16(data, offset, value);
            return offset + 2;
        }
    }
}
