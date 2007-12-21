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
using System.Xml;
using System.Runtime.InteropServices;

namespace Geotagger
{
    public class GPSTrackPoint
    {
        public float    mLat;
        public float    mLon;
        public float    mEle;
        public DateTime mTime;
    }

    public class GPSTrackPointList : List<GPSTrackPoint> { }

    public class GPSTrack : GPSTrackPointList
    {
        //private GPSTrackPointList mGPSTrackPointList;

        public GPSTrack()
        {
            //mGPSTrackPointList = new GPSTrackPointList();
        }

        public void LoadGPX(string fileName)
        {
            // Read the GPX file and add to the list of track log points.
            XmlTextReader reader = new XmlTextReader(fileName);
            string parsingName = "";
            GPSTrackPoint parsingPoint = null;

            // Empty the list first.
            this.Clear();

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        // Store the element name
                        parsingName = reader.Name;

                        if (parsingName == "trkpt")
                        {
                            // Starting a track point
                            parsingPoint = new GPSTrackPoint();
                            parsingPoint.mLat = float.Parse(reader.GetAttribute("lat"));
                            parsingPoint.mLon = float.Parse(reader.GetAttribute("lon"));
                        }
                        break;

                    case XmlNodeType.Text:
                        // Only parse elements of a track point.
                        if (parsingPoint != null)
                        {
                            if (parsingName == "ele")
                            {
                                // elevation
                                parsingPoint.mEle = float.Parse(reader.Value);
                            }
                            if (parsingName == "time")
                            {
                                // GMT Date and Time.
                                parsingPoint.mTime = DateTime.Parse(reader.Value);
                            }
                        }
                        break;

                    case XmlNodeType.EndElement:
                        if ((reader.Name == "trkpt") && (parsingPoint != null))
                        {
                            // Ending a track point.
                            //mGPSTrackPointList.Add(parsingPoint);
                            Add(parsingPoint);
                            parsingPoint = null;
                        }
                        break;
                }
            }
        }

        // Find the nearest track point to the given time.
        public GPSTrackPoint FindNearest(DateTime time)
        {
            // Initial version: Finds the first point in the track log that occured after the given time.
            // This assumes that the track log is sorted by time.  What if the point is not found?

            // TODO:
            // 1. Use a binary search since the list is sorted.
            // 2. Find the point before and after the specified time and interpolate between them.
            // 3. Return all three points (before, after, and "calculated") to be stored and shown to the user.

            GPSTrackPoint foundPoint = this.Find(
                delegate(GPSTrackPoint point)
                {
                    // Return the first point that is greater than the given time.
                    // Assumes that the list is sorted.
                    return (point.mTime > time);
                }
            );

            return foundPoint;
        }
    }
}
