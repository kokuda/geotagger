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
    public class ExifFormat
    {
        public const int EXIF_BYTE = 1;
        public const int EXIF_STRING = 2;
        public const int EXIF_FMT_USHORT = 3;
        public const int EXIF_ULONG = 4;
        public const int EXIF_URATIONAL = 5;
        public const int EXIF_SBYTE = 6;
        public const int EXIF_UNDEFINED = 7;
        public const int EXIF_SSHORT = 8;
        public const int EXIF_SLONG = 9;
        public const int EXIF_SRATIONAL = 10;
        public const int EXIF_SINGLE = 11;
        public const int EXIF_DOUBLE = 12;

        public const int EXIF_MIN = EXIF_BYTE;
        public const int EXIF_MAX = EXIF_DOUBLE;

        public ExifFormat(int format)
        {
            mFormat = format;
        }

        public uint size
        {
            get { return ExifFormatSize[mFormat]; }
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(int))
            {
                return obj.Equals(mFormat);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return mFormat.GetHashCode();
        }

        static public bool operator ==(ExifFormat that, int format)
        {
            return format == that.mFormat;
        }

        static public bool operator !=(ExifFormat that, int format)
        {
            return format != that.mFormat;
        }

        public static explicit operator int(ExifFormat f)
        {
            return f.mFormat;
        }

        public static explicit operator uint(ExifFormat f)
        {
            return (uint)f.mFormat;
        }

        public static explicit operator ExifFormat(int f)
        {
            return new ExifFormat(f);
        }

        static private uint[] ExifFormatSize = { 0, 1, 1, 2, 4, 8, 1, 1, 2, 4, 8, 4, 8 };
        private int mFormat;
    
    }
}
