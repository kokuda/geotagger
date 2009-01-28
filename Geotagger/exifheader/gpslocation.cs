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

namespace ExifHeader
{
    public struct GpsCoord
    {
        public GpsCoord(Rational deg, Rational min, Rational sec)
        {
            mDegree = deg;
            mMinute = min;
            mSecond = sec;
        }

        public GpsCoord(decimal degrees)
        {
            // The digits to the left of the decimal point are the degrees.
            // The digits to the right, multiplied by 60, are the minuts.
            // Any remaining fractional part, multiplied by 60, is the seconds.

            decimal floorOfDegrees = System.Math.Floor(degrees);
            decimal minutes = (degrees - floorOfDegrees) * 60;
            decimal floorOfMinutes = System.Math.Floor(minutes);
            decimal seconds = (minutes - floorOfMinutes) * 60;

            mDegree = new Rational(floorOfDegrees, 0);
            mMinute = new Rational(floorOfMinutes, 0);
            mSecond = new Rational(seconds, 4);
        }

        public Rational degree
        {
            get { return mDegree; }
            set { mDegree = value; }
        }

        public Rational minute
        {
            get { return mMinute; }
            set { mMinute = value; }
        }

        public Rational second
        {
            get { return mSecond; }
            set { mSecond = value; }
        }

        private Rational mDegree;
        private Rational mMinute;
        private Rational mSecond;
    }

    public struct GpsLocation
    {
        public enum LatRef
        {
            NORTH,
            SOUTH
        }

        public enum LonRef
        {
            EAST,
            WEST
        }

        public GpsLocation(LatRef latref, GpsCoord lat, LonRef lonref, GpsCoord lon, Rational alt)
        {
            mLatRef = latref;
            mLat = lat;
            mLonRef = lonref;
            mLon = lon;
            mAlt = alt;
        }

        public GpsLocation(decimal lat, decimal lon, decimal alt)
        {
            // Convert from decimal to deg/min/sec.
            if (lat >= 0)
            {
                mLatRef = LatRef.NORTH;
            }
            else
            {
                mLatRef = LatRef.SOUTH;
                lat = -lat;
            }

            if (lon >= 0)
            {
                mLonRef = LonRef.EAST;
            }
            else
            {
                mLonRef = LonRef.WEST;
                lon = -lon;
            }

            mLat = new GpsCoord(lat);
            mLon = new GpsCoord(lon);
            mAlt = new Rational(alt, 3);
        }

        public void SetLat(LatRef latref, GpsCoord lat)
        {
            mLatRef = latref;
            mLat = lat;
        }

        public void SetLon(LonRef lonref, GpsCoord lon)
        {
            mLonRef = lonref;
            mLon = lon;
        }

        public void SetAltitude(Rational alt)
        {
            mAlt = alt;
        }

        public LatRef latRef
        {
            get { return mLatRef; }
        }

        public LonRef lonRef
        {
            get { return mLonRef; }
        }

        public GpsCoord lat
        {
            get { return mLat; }
        }

        public GpsCoord lon
        {
            get { return mLon; }
        }

        public Rational alt
        {
            get { return mAlt; }
        }

        private LatRef mLatRef;
        private LonRef mLonRef;
        private GpsCoord mLat;
        private GpsCoord mLon;
        private Rational mAlt;
    }
}
