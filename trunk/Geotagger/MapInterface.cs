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

namespace Geotagger
{
    class MapInterface
    {
        // Constructor
        public MapInterface(System.Windows.Forms.WebBrowser webBrowser)
        {
            mWebBrowser = webBrowser;
        }

        // Clear the GPS track from the map.
        public void ClearTrack()
        {
            CallJavaScript("GTMInterface_ClearTrack");
        }

        // Show the GPS track on the map.
        public void ShowTrack(GPSTrack track)
        {
            CallJavaScript("GTMInterface_StartTrack");
            foreach (GPSTrackPoint p in track)
            {
                CallJavaScript("GTMInterface_AddTrackPoint", new String[] { p.mLat.ToString(), p.mLon.ToString() });
            }
            CallJavaScript("GTMInterface_EndTrack");
        }

        public Object CreateMarker(int id, GPSTrackPoint location)
        {
            return CallJavaScript("GTMInterface_CreateMarker", new String[] { id.ToString(), location.mLat.ToString(), location.mLon.ToString() });
        }

        public void MoveMarker(Object marker, GPSTrackPoint location)
        {
            // One way to implement this is to tell the marker object to move itself.
            // I'm not exactly sure how to implement this, though it would be interesting to try someday.
            //Type t = marker.GetType();
            //t.InvokeMember("MoveMarker", ...

            // For now we implement it by passing this marker object back into the Action Script to manipulate.
            CallJavaScript("GTMInterface_MoveMarker", new Object[] { marker, location.mLat.ToString(), location.mLon.ToString() });
        }

        ///////////////////////////////////////////////////////////////////////
        // Private
        ///////////////////////////////////////////////////////////////////////

        private System.Windows.Forms.WebBrowser mWebBrowser;

        private Object CallJavaScript(string jsFunc)
        {
            return mWebBrowser.Document.InvokeScript(jsFunc);
        }

        private Object CallJavaScript(string jsFunc, Object[] args)
        {
            return mWebBrowser.Document.InvokeScript(jsFunc, args);
        }
    }
}
