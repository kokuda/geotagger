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
	
	this.MarkerClick = function(id)
	{
		window.external.MarkerClick(id);
	}
	
	this.MarkerDrop = function(id, lat, lng)
	{
		window.external.MarkerDrop(id, lat, lng);
	}
	
	this.Alert = function(msg, title)
	{
		window.external.Alert(msg, title);
	}
}

//////////////////////////////////////////////////////////////////////////////
// ClearTrack: Removes all track points from the map.
function GTMInterface_ClearTrack(lat, lng, htmltext)
{
	if (GTMInterface.mMapInterface != null)
	{
		GTMInterface.mMapInterface.ClearTrack(lat,lng,htmltext);
	}
}

//////////////////////////////////////////////////////////////////////////////
// StartTrack: Indicates the begining of a track point list.
function GTMInterface_StartTrack()
{
	if (GTMInterface.mMapInterface != null)
	{
		GTMInterface.mMapInterface.StartTrack();
	}
}

//////////////////////////////////////////////////////////////////////////////
// AddTrackPoint: Adds a track point to the map.
function GTMInterface_AddTrackPoint(lat, lng)
{
	if (GTMInterface.mMapInterface != null)
	{
		GTMInterface.mMapInterface.AddTrackPoint(lat,lng);
	}
}

//////////////////////////////////////////////////////////////////////////////
// EndTrack: Indicates the end of a track point list.
function GTMInterface_EndTrack()
{
	if (GTMInterface.mMapInterface != null)
	{
		GTMInterface.mMapInterface.EndTrack();
	}
}

//////////////////////////////////////////////////////////////////////////////
// CreateMarker: Create a marker on the map.
function GTMInterface_CreateMarker(id, lat, lng)
{
	if (GTMInterface.mMapInterface != null)
	{
		return GTMInterface.mMapInterface.CreateMarker(id, lat, lng);
	}

	return null;
}

//////////////////////////////////////////////////////////////////////////////
// MoveMarker: Move an existing marker on the map.
function GTMInterface_MoveMarker(marker, lat, lng)
{
	if ((GTMInterface.mMapInterface != null) && (marker != null))
	{
		return GTMInterface.mMapInterface.MoveMarker(marker, lat, lng);
	}
	
	return null;
}

function GTMInterface_Search(address)
{
	if (GTMInterface.mMapInterface != null)
	{
		GTMInterface.mMapInterface.Search(address);
	}
}
