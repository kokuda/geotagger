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
// Object for interfacing the GeoTagger application with no Maps (offline).
//////////////////////////////////////////////////////////////////////////////

//////////////////////////////////////////////////////////////////////////////
// Create Singleton NoMapInterface offline interface to geotagger.
var NoMapInterface = new function()
{
	//////////////////////////////////////////////////////////////////////////
	// Public members
	this.mMarkerList = [];
	this.mPointList = [];
	this.mMap = null;


	//////////////////////////////////////////////////////////////////////////
	// Always available
	this.IsAvailable = function()
	{
		return true;
	}

	//////////////////////////////////////////////////////////////////////////
	// Initialize NoMapInterface (Does nothing, but exists to mirror the GMap
	// interface.
	this.Initialize = function(map)
	{
		// Create a basic offline view with no map.
		this.mMap = map;
		this.mMap.innerHTML = 'Offline<br /><br /><table><tr><td style="vertical-align:top"><div id="left"></div></td><td style="vertical-align:top"><div id="right"></div></td></tr></table>';
	};

	//////////////////////////////////////////////////////////////////////////
	// ClearTrack: Removes all track points from the map.
	this.ClearTrack = function ()
	{
		this.mPointList = [];
	};

	//////////////////////////////////////////////////////////////////////////
	// StartTrack: Indicates the begining of a track point list.
	this.StartTrack = function ()
	{
		// Temporarily add a new function for adding the track points.
		this.AddTrackPoint = function (lat, lng)
		{
			var latlng = new Object;
			latlng.lat = lat;
			latlng.lng = lng;
			this.mPointList.push(latlng);
		}

		this.EndTrack = function ()
		{
			// Remove the temporary functions
			this.AddTrackPoint = null;
			this.EndTrack = null;

			var html = "";
			for(var i=0; i < this.mPointList.length; ++i)
			{
				html += "point " + i + " @ " + this.mPointList[i].lat + " / " + this.mPointList[i].lng + "<br />";
			}
			document.getElementById("left").innerHTML = html;

		};
	};

	//////////////////////////////////////////////////////////////////////////
	// CreateMarker: Create a marker on the map (probably for a photo
	this.CreateMarker = function (id, lat, lng)
	{
		var marker = new Object;
		marker.id = id;
		marker.lat = lat;
		marker.lng = lng;

		this.mMarkerList.push(marker);

		document.getElementById("right").innerHTML += "Marker " + id + " @ " + lat + "/" + lng + "<br />";

		return marker;
	}

	//////////////////////////////////////////////////////////////////////////
	// MoveMarker: Moves a marker to a new location.
	this.MoveMarker = function (marker, lat, lng)
	{
		marker.lat = lat;
		marker.lng = lng;
		document.getElementById("right").innerHTML += "Marker " + marker.id + " @ " + lat + "/" + lng + "<br />";
	}

	//////////////////////////////////////////////////////////////////////////
	// Search: Find the given address and navigate the map to it.
	this.Search = function (address)
	{
		document.getElementById("right").innerHTML += "Search '" + address + "' <br />";
	}
}
