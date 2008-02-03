//////////////////////////////////////////////////////////////////////////////
//
//    This file is part of Geotagger: A tool for geotagging photographs
//    Copyright (C) 2008  Kaz Okuda (http://notions.okuda.ca)
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
// PhotoCollection is a collection (hashtable) of PhotoData objects.
//////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace Geotagger
{
    class PhotoCollection
    {
        //////////////////////////////////////////////////////////////////////
        // public methods
        //////////////////////////////////////////////////////////////////////

        public PhotoCollection()
        {
            mDictionary = new Dictionary<string, PhotoData>();
        }

        public PhotoData AddPhoto(string fileName)
        {
            mDictionary.Add(fileName, new PhotoData(fileName));
            return mDictionary[fileName];
        }

        public PhotoData GetPhoto(string fileName)
        {
            PhotoData result = null;
            try
            {
                result = mDictionary[fileName];
            }
            catch (KeyNotFoundException)
            {
                result = null;
            }

            return result;
        }

        public int GetCount()
        {
            return mDictionary.Count;
        }

        //////////////////////////////////////////////////////////////////////
        // Private methods
        //////////////////////////////////////////////////////////////////////
        

        //////////////////////////////////////////////////////////////////////
        // Private members
        //////////////////////////////////////////////////////////////////////

        private Dictionary<string, PhotoData> mDictionary;
    }
}
