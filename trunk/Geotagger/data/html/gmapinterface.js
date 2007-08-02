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
// Object for interfacing the GeoTagger application with Google Maps.
//////////////////////////////////////////////////////////////////////////////

// Create Singleton GMapInterface as Google Map interface to geotagger.
var GMapInterface = new function()
{
	this.mMap = null;

	// Initialize GMapInterface with the Google Map
	this.Initialize = function(map)
	{
		mMap = map;

		// Initialize callback handlers for the Google Map
		
		// Handle the map click event and return it to GeoTagger.
		GEvent.addListener(map, "click",
			function(marker, point)
			{
				// If marker is null then this is a new point.
				if (marker == null)
				{
					window.external.SingleClick(point.lat(), point.lng());
				}
			}
		);
	};

	// CreateMarker: Creates a marker at the given location which will popup the given
	// html when it is clicked.
	this.CreateMarker = function (lat, lng, htmltext)
	{
		var marker = new GMarker(new GLatLng(lat,lng));
		GEvent.addListener(marker, "click",
			function()
			{
				marker.openInfoWindowHtml(htmltext);
			}
		);

		mMap.addOverlay(marker);
	};
}
