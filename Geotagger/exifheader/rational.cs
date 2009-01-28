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
    public struct Rational
    {
        public Rational(int num, int den)
        {
            mNumerator = num;
            mDenominator = den;
        }

        public Rational(int num)
        {
            mNumerator = num;
            mDenominator = 1;
        }

        // Construct a Rational from a decimal.
        // num is the value
        // precision is the number of decimal places
        public Rational(decimal num, int precision)
        {
            // Keep three decimal places of precision.
            mDenominator = (int)System.Math.Pow(10, precision);
            mNumerator = (int)System.Math.Round(num * mDenominator);
        }

        public int numerator
        {
            get { return mNumerator; }
            set { mNumerator = value; }
        }

        public int denominator
        {
            get { return mDenominator; }
            set { mDenominator = value; }
        }

        public override string ToString()
        {
            return System.String.Format("{0}/{1}", mNumerator, mDenominator);
        }

        public static Rational operator -(Rational r)
        {
            return new Rational(-r.numerator, r.denominator);
        }

        // Explicit conversion to float
        public static explicit operator float(Rational r)
        {
            return ((float)r.numerator) / ((float)r.denominator);
        }

        private int mNumerator;
        private int mDenominator;
    }
}
