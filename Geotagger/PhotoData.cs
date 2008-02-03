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

//////////////////////////////////////////////////////////////////////////////
// PhotoData contains the relevant data about a photograph.
// The class itself represents useful information about a photograph.
//////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

namespace Geotagger
{
    class PhotoData
    {
        //////////////////////////////////////////////////////////////////////
        // public methods
        //////////////////////////////////////////////////////////////////////

        public PhotoData(string fileName)
        {
            // Initialize from the given photo.
            mFileName = fileName;

            // Load the image to extract the available properties.
            Image srcImage = Image.FromFile(mFileName);

            // Load a scaled down thumbnail of the image.
            mThumbNail = LoadImage(srcImage, SMALL_IMAGE_WIDTH, SMALL_IMAGE_HEIGHT);

            mWidth = srcImage.Width;
            mHeight = srcImage.Height;
            mDateTime = GetDateTimeOriginal(srcImage);

            // There is no marker until it is set.
            mMapMarkerObject = null;
            mNearestPoint = null;

        }

        public void SetLocation(float lat, float lng, float ele)
        {
            mLatitude = lat;
            mLongitude = lng;
            mElevation = ele;
        }

        public Image thumbnail
        {
            get
            {
                return mThumbNail;
            }
        }

        public int width
        {
            get
            {
                return mWidth;
            }
        }

        public int height
        {
            get
            {
                return mHeight;
            }
        }

        public DateTime dateTime
        {
            get
            {
                return mDateTime;
            }
        }

        public float latitude
        {
            get
            {
                return mLatitude;
            }
        }

        public float longitude
        {
            get
            {
                return mLongitude;
            }
        }

        public float elevation
        {
            get
            {
                return mElevation;
            }
        }

        public Object markerObject
        {
            get
            {
                return mMapMarkerObject;
            }

            set
            {
                mMapMarkerObject = value;
            }
        }

        public GPSTrackNearestPoint nearestPoint
        {
            get
            {
                return mNearestPoint;
            }

            set
            {
                mNearestPoint = value;
            }
        }

        //////////////////////////////////////////////////////////////////////
        // Private methods
        //////////////////////////////////////////////////////////////////////
        
        // Load a small memory copy of the image
        // Scales the image to within the boundaries specified, padding
        // any non-square boundary.
        private static Image LoadImage(Image srcImage, int width, int height)
        {
            int srcWidth = srcImage.Width;
            int srcHeight = srcImage.Height;
            float widthScale = (float)width / (float)srcWidth;
            float heightScale = (float)height / (float)srcHeight;
            float scale = 1.0f;
            int dstX = 0;
            int dstY = 0;

            // Set the final scale value to the smaller of the two.
            // Also calculate the offset to center the final image.
            if (widthScale < heightScale)
            {
                scale = widthScale;
                dstY = (int)((height - (srcHeight * scale)) / 2.0f);
            }
            else
            {
                scale = heightScale;
                dstX = (int)((width - (srcWidth * scale)) / 2.0f);
            }

            // Now we have the destination size.
            int destWidth = (int)(srcWidth * scale);
            int destHeight = (int)(srcHeight * scale);

            Bitmap thumbnail = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            Graphics outGraphics = Graphics.FromImage(thumbnail);
            outGraphics.Clear(Color.Transparent);
            outGraphics.DrawImage(srcImage,
                new Rectangle(dstX, dstY, destWidth, destHeight),
                new Rectangle(0, 0, srcWidth, srcHeight),
                GraphicsUnit.Pixel);

            outGraphics.Dispose();

            return thumbnail;
        }

        private static DateTime GetDateTimeOriginal(Image img)
        {
            PropertyItem prop = img.GetPropertyItem(EXIF_DateTimeOriginal);
            string date = System.Text.Encoding.ASCII.GetString(prop.Value);

            System.Globalization.DateTimeFormatInfo format = new System.Globalization.DateTimeFormatInfo();
            format.DateSeparator = ":";
            format.TimeSeparator = ":";
            format.FullDateTimePattern = "yyyy/MM/dd HH:mm:ss\\\0";
            DateTime dateTime = DateTime.ParseExact(date, "F", format);

            return (dateTime);
        }



        //////////////////////////////////////////////////////////////////////
        // Private members
        //////////////////////////////////////////////////////////////////////

        // Constants
        private const int SMALL_IMAGE_WIDTH  = 256;
        private const int SMALL_IMAGE_HEIGHT = 256;

        // EXIF IDs
        private const int EXIF_DateTimeOriginal = 0x9003;

        // Instance
        private Image                   mThumbNail;
        private Object                  mMapMarkerObject;
        private GPSTrackNearestPoint    mNearestPoint;

        // Image properties (Should these be in their own structure or class)?
        private string      mFileName;
        private int         mWidth;
        private int         mHeight;
        private DateTime    mDateTime;
        private float       mLatitude;
        private float       mLongitude;
        private float       mElevation;
    }
}
