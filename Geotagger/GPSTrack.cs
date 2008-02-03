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
        public int      mSegmentId;
    }

    public class GPSTrackNearestPoint
    {
        // Stores details about the nearest point in a GPS track
        // to some other point in space.
        // This must, at a minimum, include the two points in the track around
        // the test point and the estimated location between them.
        // It should also store details about the points, like whether they cross
        // a tracklog boundary, as this can help determine which of the three
        // points is most likely the correct one.
        public GPSTrackPoint mCalculated;
        public GPSTrackPoint mBefore;
        public GPSTrackPoint mAfter;
        public DateTime      mTime;
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
            int segmentId = 0;

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
                            parsingPoint.mSegmentId = segmentId;
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
                        else if (reader.Name == "trkseg")
                        {
                            // Incement the segment number
                            segmentId++;
                        }

                        break;
                }
            }
        }

        // Find the nearest track point to the given time.
        public GPSTrackNearestPoint FindNearest(DateTime time)
        {
            GPSTrackNearestPoint result = new GPSTrackNearestPoint();

            // This assumes that the track log is sorted by time.  What if the point is not found?
            // TODO:
            // 1. Use a binary search since the list is sorted.

            // Find the next point after the given time.
            int nextPoint = this.FindIndex(
                delegate(GPSTrackPoint point)
                {
                    // Return the first point that is greater than the given time.
                    // Assumes that the list is sorted.
                    return (point.mTime > time);
                }
            );

            // We will assume that nextPoint and nextPoint-1 are the two surrounding points.
            // We must also handle the edge cases.
            if (nextPoint < 0)
            {
                // Match not found so we are past the end of the point list.
                result.mCalculated = this[this.Count - 1];
                result.mBefore = result.mCalculated;
                result.mAfter = null;
                result.mTime = time;
            }
            else if (nextPoint == 0)
            {
                // Match found at first node.
                result.mCalculated = this[0];
                result.mBefore = null;
                result.mAfter = result.mCalculated;
                result.mTime = time;
            }
            else
            {
                // The point is in the middle somewhere
                result.mBefore = this[nextPoint - 1];
                result.mAfter = this[nextPoint];
                result.mCalculated = new GPSTrackPoint();
                {
                    // Initialize the calculated point with an interpolant between mBefore and mAfter.
                    // Calculate and normalize the point t between them.
                    System.TimeSpan timeDiff = result.mAfter.mTime - result.mBefore.mTime;
                    System.TimeSpan timeStep = time - result.mBefore.mTime;
                    float interval = (float)timeStep.Ticks / (float)timeDiff.Ticks;
                    result.mCalculated.mEle = result.mBefore.mEle + interval * (result.mAfter.mEle - result.mBefore.mEle);
                    result.mCalculated.mLat = result.mBefore.mLat + interval * (result.mAfter.mLat - result.mBefore.mLat);
                    result.mCalculated.mLon = result.mBefore.mLon + interval * (result.mAfter.mLon - result.mBefore.mLon);
                    result.mCalculated.mTime = time;

                    // Use the closest segment.
                    result.mCalculated.mSegmentId = interval < 0.5 ? result.mBefore.mSegmentId : result.mAfter.mSegmentId;

                }
                result.mTime = time;
            }

            return result;
        }
    }
}
