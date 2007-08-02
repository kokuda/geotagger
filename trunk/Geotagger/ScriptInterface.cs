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
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Geotagger
{
    [ComVisibleAttribute(true)]
    public class ScriptInterface
    {
        private System.Windows.Forms.WebBrowser mWebBrowser;

        public ScriptInterface(System.Windows.Forms.WebBrowser webBrowser)
        {
            mWebBrowser = webBrowser;
        }

        public void CallJavaScript(string jsFunc, Object[] args)
        {
            mWebBrowser.Document.InvokeScript(jsFunc, args);
        }

        // Called when user clicks on the specified lat,lng on the map.
        public void SingleClick(float lat, float lng)
        {
            CallJavaScript("GTMInterface_CreateMarker", new String[] { lat.ToString(), lng.ToString(), "<html>Hello World</html>" });
        }
    }
}