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
// The static data and methods contain the list of photo data used by Geotagger.
//////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Geotagger
{
    class PhotoData
    {
        //////////////////////////////////////////////////////////////////////
        // public static methods
        //////////////////////////////////////////////////////////////////////

        public static PhotoData AddPhoto(string fileName)
        {
            sPhotoContainer.Add(fileName, new PhotoData(fileName));
            return (PhotoData)sPhotoContainer[fileName];
        }

        public static PhotoData GetPhoto(string fileName)
        {
            return (PhotoData)sPhotoContainer[fileName];
        }

        public static int GetCount()
        {
            return sPhotoContainer.Count;
        }

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
            // More properties...
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

        //////////////////////////////////////////////////////////////////////
        // Private members
        //////////////////////////////////////////////////////////////////////

        // Constants
        private const int SMALL_IMAGE_WIDTH  = 128;
        private const int SMALL_IMAGE_HEIGHT = 128;

        // Static
        private static Hashtable sPhotoContainer = new Hashtable();

        // Instance
        private Image   mThumbNail;
        private string  mFileName;
        private int     mWidth;
        private int     mHeight;
    }
}
