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
// Object for interfacing the GeoTagger application with Bing Maps.
//////////////////////////////////////////////////////////////////////////////

//////////////////////////////////////////////////////////////////////////////
// Create Singleton BMapInterface as Bing Map interface to geotagger.
var BMapInterface = new function()
{
	//////////////////////////////////////////////////////////////////////////
	// Public members
	this.mMap = null;
	this.mGeocoder = null;

	//////////////////////////////////////////////////////////////////////////
	// Check the availability of Bing Maps
	this.IsAvailable = function()
	{
		return true;
	}

	//////////////////////////////////////////////////////////////////////////
	// Initialize BMapInterface with the Bing Map
	this.Initialize = function(map)
	{
		try
		{
			mMap = new VEMap(map);
			mMap.LoadMap(new VELatLong(49.2, -123), 10);

			var draggingElement = null;
			var dragging = false;
			var clickX = null;
			var clickY = null;
			var mouseHandler = function(e)
			{
				if (e.eventName == "onmousedown" && e.elementID != null)
				{
					var shape = mMap.GetShapeByID(e.elementID);
					if (shape.GetType() == VEShapeType.Pushpin)
					{
						draggingElement = shape;
						dragging = false;	// Sticky
						clickX = e.mapX;
						clickY = e.mapY;
						return true;
					}
				}
				else if (e.eventName == "onmouseup" && draggingElement != null)
				{
					if (dragging)
					{
						var points = draggingElement.GetPoints();
						GTMInterface.MarkerDrop(draggingElement.myID, points[0].Latitude, points[0].Longitude);
					}
					else
					{
						GTMInterface.MarkerClick(draggingElement.myID);
					}
					draggingElement = null;
				}
				else if (e.eventName == "onmousemove" && draggingElement != null)
				{
					if (!dragging)
					{
						// If we have moved sufficiently then we can begin dragging.
						if (Math.abs(clickX - e.mapX) > 10 || Math.abs(clickY - e.mapY) > 10)
						{
							// Unstick.
							dragging = true;
						}
					}
					else
					{
						var point = mMap.PixelToLatLong(new VEPixel(e.mapX, e.mapY));
						draggingElement.SetPoints(point);
						return true;
					}
				}
			}
			
			// Handle the map click event.
			mMap.AttachEvent("onmousedown", mouseHandler);
			mMap.AttachEvent("onmouseup", mouseHandler);
			mMap.AttachEvent("onmousemove", mouseHandler);
		}
		catch (e)
		{
			alert(e.message);
		}
	};

	//////////////////////////////////////////////////////////////////////////
	// ClearTrack: Removes all track points from the map.
	this.ClearTrack = function ()
	{
		mMap.DeleteAllShapes();
	};

	//////////////////////////////////////////////////////////////////////////
	// StartTrack: Indicates the begining of a track point list.
	this.StartTrack = function ()
	{
		var pointarray = [];

		// Temporarily add a new function for adding the track points.
		this.AddTrackPoint = function (lat, lng)
		{
			var latlng = new VELatLong(lat, lng);
			pointarray.push(latlng)
		}

		this.EndTrack = function ()
		{
			// Remove the temporary functions
			this.AddTrackPoint = null;
			this.EndTrack = null;

			var polyline = new VEShape(VEShapeType.Polyline, pointarray);
			polyline.HideIcon();
			mMap.AddShape(polyline);
		};
	};

	//////////////////////////////////////////////////////////////////////////
	// CreateMarker: Create a marker on the map (probably for a photo
	this.CreateMarker = function (id, lat, lng)
	{
		var marker = new VEShape(VEShapeType.Pushpin, new VELatLong(lat, lng));
		marker.myID = id;
		
		//GEvent.addListener(marker, "dragend",
		//	function()
		//	{
		//		var latlng = marker.getLatLng();
		//		GTMInterface.MarkerDrop(id, latlng.lat(), latlng.lng());
		//	}
		//);

		mMap.AddShape(marker);
		
		return marker;
	}

	//////////////////////////////////////////////////////////////////////////
	// MoveMarker: Moves a marker to a new location.
	this.MoveMarker = function (marker, lat, lng)
	{
		marker.SetPoints([new VELatLong(lat, lng)]);
	}

	//////////////////////////////////////////////////////////////////////////
	// Search: Find the given address and navigate the map to it.
	this.Search = function (address)
	{
		try
		{
			mMap.Find(null, address);
		}
		catch(e)
		{
			GTMInterface.Alert(e);
		}
	}
}
