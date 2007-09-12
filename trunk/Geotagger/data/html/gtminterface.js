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
// Object for interfacing the GeoTagger application.
// Implemented using replacable object so that it could easily be replaced
// with an alternate map implementation (Google, Yahoo, etc.)
//////////////////////////////////////////////////////////////////////////////

//////////////////////////////////////////////////////////////////////////////
// Must give each external function a name like GTMInterface_CreateMarker
// because it would appear that the C# code can only call root functions
//////////////////////////////////////////////////////////////////////////////

// Construct Singleton with one method "Initialize".
GTMInterface = new function()
{
	//////////////////////////////////////////////////////////////////////////
	// Public members
	this.mMapInterface = null;

	//////////////////////////////////////////////////////////////////////////
	// Initialize
	this.Initialize = function(mapinterface)
	{
		this.mMapInterface = mapinterface
	}
	
	//////////////////////////////////////////////////////////////////////////
	// This object also wraps the calls into the game for consistency.
	this.SingleClick = function(lat, lng)
	{
		window.external.SingleClick(lat, lng);
	}
}

//////////////////////////////////////////////////////////////////////////////
// CreateMarker: Creates a marker at the given location which will popup the given
// html when it is clicked.
function GTMInterface_CreateMarker(lat, lng, htmltext)
{
	GTMInterface.mMapInterface.CreateMarker(lat,lng,htmltext);
}

//////////////////////////////////////////////////////////////////////////////
// ClearTrack: Removes all track points from the map.
function GTMInterface_ClearTrack(lat, lng, htmltext)
{
	GTMInterface.mMapInterface.ClearTrack(lat,lng,htmltext);
}

//////////////////////////////////////////////////////////////////////////////
// StartTrack: Indicates the begining of a track point list.
function GTMInterface_StartTrack()
{
	GTMInterface.mMapInterface.StartTrack();
}

//////////////////////////////////////////////////////////////////////////////
// AddTrackPoint: Adds a track point to the map.
function GTMInterface_AddTrackPoint(lat, lng)
{
	GTMInterface.mMapInterface.AddTrackPoint(lat,lng);
}

//////////////////////////////////////////////////////////////////////////////
// EndTrack: Indicates the end of a track point list.
function GTMInterface_EndTrack()
{
	GTMInterface.mMapInterface.EndTrack();
}