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
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace Geotagger
{
    public partial class Form1 : Form
    {
        private HttpServer      mHttpServer;
        private GPSTrack        mGPSTrack;
        private ScriptInterface mScriptInterface;
        private MapInterface    mMapInterface;

        public Form1()
        {
            InitializeComponent();

            // Initialize Members
            mGPSTrack = new GPSTrack();
            mHttpServer = new HttpServer(8080);
            mScriptInterface = new ScriptInterface(this);
            mMapInterface = new MapInterface(webBrowser1);

            // Register the callback interface to the Javascript.
            webBrowser1.ObjectForScripting = mScriptInterface;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Start the HTTP server for the browser to use.
            mHttpServer.Startup();
            int port = mHttpServer.GetPort();

            // Load the page from the HttpServer.
            webBrowser1.Navigate(new Uri("http://127.0.0.1:"+port+"/html/start.html"));
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            mHttpServer.Shutdown();
        }

        // Pseudo events from the Javascript in the webBrowser.
        public void Map_SingleClick(float lat, float lng)
        {
            // Apply this location to each of the selected images.
            ListView.SelectedIndexCollection selectedIndices = listView1.SelectedIndices;
            foreach (int index in selectedIndices)
            {
                // Apply this lat/lng to this image.
            }
        }

        public void Map_MarkerClick(int photoIndex)
        {
            // The marker on the map was clicked, highlight the photo
            Debug.WriteLine("MarkerClick(" + photoIndex + ")");
            listView1.SelectedItems.Clear();
            listView1.Items[photoIndex].Selected = true;
        }

        public void Map_MarkerDrop(int photoIndex, float lat, float lng)
        {
            Debug.WriteLine("MarkerDrop(" + photoIndex + "," + lat + "," + lng + ")");
            PhotoData photoData = PhotoData.GetPhoto(listView1.Items[photoIndex].ImageKey);
            photoData.SetLocation(lat, lng, photoData.elevation);
            listView1.SelectedItems.Clear();
            listView1.Items[photoIndex].Selected = true;
        }

        // End it all!
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        // Load the GPX file.
        private void loadGPXToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Open a file dialog to select the GPX file.
            OpenFileDialog dialog1 = new OpenFileDialog();

            dialog1.Title = "Browse to find the gpx file";
            dialog1.Filter = "GPX File|*.gpx";
            dialog1.RestoreDirectory = true;

            if (dialog1.ShowDialog() == DialogResult.OK)
            {
                // We have a file selected, now load it
                mGPSTrack.LoadGPX(dialog1.FileName);

                // Clear the old track
                mMapInterface.ClearTrack();

                // Now show the current track.
                mMapInterface.ShowTrack(mGPSTrack);
            }
        }

        private void loadImagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Open a file dialog to select the images.
            OpenFileDialog dialog1 = new OpenFileDialog();

            dialog1.Multiselect = true;
            dialog1.Title = "Browse to find the images";
            dialog1.Filter =
                "Images (*.JPG)|*.JPG|" +
                "All files (*.*)|*.*";
            dialog1.RestoreDirectory = true;

            if (dialog1.ShowDialog() == DialogResult.OK)
            {
                // Read the files
                foreach (String file in dialog1.FileNames)
                {
                    // Add the image to the ImageList and ListView
                    try
                    {
                        // First check that the photo has not already been loaded.
                        if (PhotoData.GetPhoto(file) == null)
                        {
                            PhotoData data = PhotoData.AddPhoto(file);
                            imageListSmall.Images.Add(file, data.thumbnail);
                            listView1.Items.Add(file, Path.GetFileName(file), file);
                        }
                    }
                    catch (System.Security.SecurityException ex)
                    {
                        // The user lacks appropriate permissions to read files, discover paths, etc.
                        MessageBox.Show("Security error. Please contact your administrator for details.\n\n" +
                            "Error message: " + ex.Message + "\n\n" +
                            "Details (send to Support):\n\n" + ex.StackTrace
                        );
                    }
                    catch (Exception ex)
                    {
                        // Could not load the image - probably related to Windows file system permissions.
                        MessageBox.Show("Cannot display the image: " + file.Substring(file.LastIndexOf('\\'))
                            + ". You may not have permission to read the file, or " +
                            "it may be corrupt.\n\nReported error: " + ex.Message);
                    }
                }
            }
        }

        private void largeIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.View = View.LargeIcon;
        }

        private void detailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.View = View.Details;
        }

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            PhotoData photoData = PhotoData.GetPhoto(e.Item.ImageKey);
            pictureBox1.Image = photoData.thumbnail;
            labelTimeOutput.Text = photoData.dateTime.ToString("G");
            labelLocationOutput.Text = photoData.latitude + "," + photoData.longitude;
        }

        private void toolStripLoadImages_Click(object sender, EventArgs e)
        {
            loadImagesToolStripMenuItem_Click(sender, e);
        }

        private void toolStripLoadGPX_Click(object sender, EventArgs e)
        {
            loadGPXToolStripMenuItem_Click(sender, e);
        }

        private void toolStripLocate_Click(object sender, EventArgs e)
        {
            // For each selected photo (or all if none are selected) find the closest track points
            // on the map nearest that time.
            ListView.SelectedIndexCollection selectedIndices = listView1.SelectedIndices;
            foreach (int index in selectedIndices)
            {
                // Get the photo
                PhotoData photoData = PhotoData.GetPhoto(listView1.Items[index].ImageKey);

                // Find the nearest point
                GPSTrackPoint location = mGPSTrack.FindNearest(photoData.dateTime);
                Debug.WriteLine("Locating " + photoData.dateTime.ToString() + " at " + location.mLat + "," + location.mLon);

                // If there is no markerObject on the map for this photo then add one.
                if (photoData.markerObject == null)
                {
                    photoData.SetLocation(location.mLat, location.mLon, location.mEle);

                    // TODO: The marker object should probably exist somewhere else.
                    // It is not really part of the photo data, though there is one
                    // associated with each photo.  It should be part of the Form or application
                    // and map between photoData objects and map markers.  Perhaps this should
                    // be stored in the listView1 object?
                    photoData.markerObject = mMapInterface.CreateMarker(index, location);
                }
                else
                {
                    // There is already a marker on the map for this photo so just move it.
                    mMapInterface.MoveMarker(photoData.markerObject, location);
                }
            }
        }
    }
}