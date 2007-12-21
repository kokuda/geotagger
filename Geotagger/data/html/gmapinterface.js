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

//////////////////////////////////////////////////////////////////////////////
// Create Singleton GMapInterface as Google Map interface to geotagger.
var GMapInterface = new function()
{
	//////////////////////////////////////////////////////////////////////////
	// Public members
	this.mMap = null;

	//////////////////////////////////////////////////////////////////////////
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
					GTMInterface.SingleClick(point.lat(), point.lng());
				}
			}
		);
	};

	//////////////////////////////////////////////////////////////////////////
	// ClearTrack: Removes all track points from the map.
	this.ClearTrack = function ()
	{
		mMap.clearOverlays();
	};

	//////////////////////////////////////////////////////////////////////////
	// StartTrack: Indicates the begining of a track point list.
	this.StartTrack = function ()
	{
		var pointarray = [];

		// Temporarily add a new function for adding the track points.
		this.AddTrackPoint = function (lat, lng)
		{
			var latlng = new GLatLng(lat, lng);
			pointarray.push(latlng)
		}

		this.EndTrack = function ()
		{
			// Remove the temporary functions
			this.AddTrackPoint = null;
			this.EndTrack = null;

			var polyline = new GPolyline(pointarray, "#ff0000", 5, 0.8);
			mMap.addOverlay(polyline);
		};
	};

	//////////////////////////////////////////////////////////////////////////
	// CreateMarker: Create a marker on the map (probably for a photo
	this.CreateMarker = function (id, lat, lng)
	{
		var marker = new GMarker(new GLatLng(lat, lng), {draggable: true, dragCrossMove: true, bouncy:true });
		GEvent.addListener(marker, "click",
			function()
			{
				GTMInterface.MarkerClick(id);
				//marker.openInfoWindowHtml("test");
			}
		);
		
		GEvent.addListener(marker, "dragend",
			function()
			{
				var latlng = marker.getLatLng();
				GTMInterface.MarkerDrop(id, latlng.lat(), latlng.lng());
			}
		);

		mMap.addOverlay(marker);
		
		return marker;
	}

	//////////////////////////////////////////////////////////////////////////
	// MoveMarker: Moves a marker to a new location.
	this.MoveMarker = function (marker, lat, lng)
	{
		marker.setLatLng(new GLatLng(lat, lng));
	}
}