# Introduction #

Geotagger is a tool to help geotag photos. This will be done manually by locating a spot on a map or automatically by correlating with data from a GPS.  Geotagger will be written in C# and uses Google Maps in an embedded web browser for visualizing the GPS data and photo locations.

# Details #

## What Geotagger Is ##

Geotagger is designed to be a simple tool that will make adding geographical information into photographs as easy as possible.  I will be a single tool with no dependencies except a working internet connection (for the map data).  It will use GPS data to automate the process of geotagging as much as possible, but also have a simple drag-and-drop style manual interface if no GPS is available.

It may include an offline mode that would not include any maps (but could still be used to automated geotagging).

It is intended to be like the [Location Stamper from wwmx.org](http://research.microsoft.com/research/downloads/Details/eadb6a33-b1b8-4c4d-b713-64fae728f74f/Details.aspx?CategoryID=) but under active development.  I am also very concerned about data loss and the intention is that geotagger will not modify any part of the image except the GPS data in the EXIF header.

If possible, Geotagger will support as many image formats as possible, most notably camera formats such as JPEG and RAW.

## What Geotagger Is Not ##

Geotagger is not a universal GPS tool that can do everything.  It will not generate KML files, it will not generate web pages.  It will not cook dinner.  It will write GPS data in EXIF headers of image files - that's it.

## More Information ##

More information about my background with geotagging can be found here [http://notions.okuda.ca/geotagging/](http://notions.okuda.ca/geotagging/).  This will help define what is missing from the existing tools (mostly reliability), and why I want this tool.

Geotagger is being written for my own personal use but I thought that others may benefit from that, which is why I have made it public.  It is only intended, however, to be everything that I want out of the tool.