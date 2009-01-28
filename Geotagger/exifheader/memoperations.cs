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

namespace ExifHeader
{
    public enum ByteOrder
    {
        UNKNOWN,
        INTEL,
        MOTOROLA,
    };

    public class MemOperations
    {
        public MemOperations()
        {
            mExifByteOrder = ByteOrder.UNKNOWN;
        }

        public MemOperations(ByteOrder byteorder)
        {
            mExifByteOrder = byteorder;
        }

        public void SetByteOrder(ByteOrder byteorder)
        {
            mExifByteOrder = byteorder;
        }

        public ByteOrder GetByteOrder()
        {
            return mExifByteOrder;
        }

        public uint GetUInt32(byte[] bytes, int offset)
        {
            uint result = 0;
            if (mExifByteOrder == ByteOrder.MOTOROLA)
            {
                result = (uint)(bytes[offset] << 24 | bytes[offset + 1] << 16 | bytes[offset + 2] << 8 | bytes[offset + 4]);
            }
            else if (mExifByteOrder == ByteOrder.INTEL)
            {
                result = (uint)(bytes[offset + 3] << 24 | bytes[offset + 2] << 16 | bytes[offset + 1] << 8 | bytes[offset]);
            }
            else
            {
                // Fatal error
                throw new Exception("Have not yet set the byte order");
            }
            return result;
        }

        public void SetUInt32(byte[] bytes, uint offset, uint value)
        {
            if (mExifByteOrder == ByteOrder.MOTOROLA)
            {
                bytes[offset    ] = (byte)((value >> 24) & 0xFF);
                bytes[offset + 1] = (byte)((value >> 16) & 0xFF);
                bytes[offset + 2] = (byte)((value >> 8) & 0xFF);
                bytes[offset + 3] = (byte)((value) & 0xFF);
            }
            else if (mExifByteOrder == ByteOrder.INTEL)
            {
                bytes[offset + 3] = (byte)((value >> 24) & 0xFF);
                bytes[offset + 2] = (byte)((value >> 16) & 0xFF);
                bytes[offset + 1] = (byte)((value >> 8) & 0xFF);
                bytes[offset    ] = (byte)((value) & 0xFF);
            }
            else
            {
                // Fatal error
                throw new Exception("Have not yet set the byte order");
            }
        }

        public int GetInt32(byte[] bytes, int offset)
        {
            return (int)GetUInt32(bytes, offset);
        }

        public ushort GetUInt16(byte[] bytes, int offset)
        {
            ushort result = 0;
            if (mExifByteOrder == ByteOrder.MOTOROLA)
            {
                result = (ushort)(bytes[offset] << 8 | bytes[offset + 1]);
            }
            else if (mExifByteOrder == ByteOrder.INTEL)
            {
                result = (ushort)(bytes[offset + 1] << 8 | bytes[offset]);
            }
            else
            {
                // Fatal error
                throw new Exception("Have not yet set the byte order");
            }
            return result;
        }

        public void SetUInt16(byte[] bytes, uint offset, uint value)
        {
            if (mExifByteOrder == ByteOrder.MOTOROLA)
            {
                bytes[offset + 0] = (byte)((value >> 8) & 0xFF);
                bytes[offset + 1] = (byte)((value) & 0xFF);
            }
            else if (mExifByteOrder == ByteOrder.INTEL)
            {
                bytes[offset + 1] = (byte)((value >> 8) & 0xFF);
                bytes[offset + 0] = (byte)((value) & 0xFF);
            }
            else
            {
                // Fatal error
                throw new Exception("Have not yet set the byte order");
            }
        }

        public void CopyBytes(byte[] destination, uint size, byte[] bytes, int offset)
        {
            Array.Copy(bytes, offset, destination, 0, size);
        }

        private ByteOrder mExifByteOrder;
    }
}
