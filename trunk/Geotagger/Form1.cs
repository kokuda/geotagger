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
        private PhotoCollection mPhotoCollection;
        private Settings        mSettings;

        public Form1()
        {
            InitializeComponent();

            // Initialize Members
            mGPSTrack = new GPSTrack();
            mHttpServer = new HttpServer(8080);
            mScriptInterface = new ScriptInterface(this);
            mMapInterface = new MapInterface(webBrowser1);
            mPhotoCollection = new PhotoCollection();
            mSettings = new Settings();

            // Register the callback interface to the Javascript.
            webBrowser1.ObjectForScripting = mScriptInterface;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Start the HTTP server for the browser to use.
            mHttpServer.Startup();
            int port = mHttpServer.GetPort();

            // Load the page from the HttpServer.
            LoadGoogleMaps();
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

        public void Map_MarkerDrop(int photoIndex, decimal lat, decimal lng)
        {
            Debug.WriteLine("MarkerDrop(" + photoIndex + "," + lat + "," + lng + ")");
            PhotoData photo = mPhotoCollection.GetPhoto(listView1.Items[photoIndex].ImageKey);
            photo.SetLocation(lat, lng, photo.elevation);
            listView1.SelectedItems.Clear();
            listView1.Items[photoIndex].Selected = true;
        }

        private void LoadGoogleMaps()
        {
            int port = mHttpServer.GetPort();
            webBrowser1.Navigate(new Uri("http://127.0.0.1:" + port + "/html/start.html"));
            // If we had a notification of when the map was loaded we could reload the track

            // Update the check mark
            bingToolStripMenuItem.Checked = false;
            googleToolStripMenuItem.Checked = true;
        }

        private void LoadBingMaps()
        {
            int port = mHttpServer.GetPort();
            webBrowser1.Navigate(new Uri("http://127.0.0.1:" + port + "/html/bing.html"));
            // If we had a notification of when the map was loaded we could reload the track

            // Update the check mark
            bingToolStripMenuItem.Checked = true;
            googleToolStripMenuItem.Checked = false;
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
                        if (mPhotoCollection.GetPhoto(file) == null)
                        {
                            PhotoData data = mPhotoCollection.AddPhoto(file);
                            imageListSmall.Images.Add(file, data.thumbnail);
                            ListViewItem item = listView1.Items.Add(file, Path.GetFileName(file), file);
                            item.Selected = true;
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
            PhotoData photo = mPhotoCollection.GetPhoto(e.Item.ImageKey);
            pictureBox1.Image = photo.thumbnail;
            labelTimeOutput.Text = photo.dateTime.ToString("G");
            labelLocationOutput.Text = String.Format("{0:F3}, {0:F3}", photo.latitude, photo.longitude);
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
                PhotoData photo = mPhotoCollection.GetPhoto(listView1.Items[index].ImageKey);

                // Find the nearest point
                DateTime realTime = photo.dateTime.AddHours(this.trackBarTimeOffset.Value);
                GPSTrackNearestPoint location = mGPSTrack.FindNearest(realTime);
                Debug.WriteLine("Locating " + photo.dateTime.ToString() + " at " + location.mCalculated.mLat + "," + location.mCalculated.mLon);

                // Store the calculated location.
                photo.nearestPoint = location;

                // If there is no markerObject on the map for this photo then add one.
                if (photo.markerObject == null)
                {
                    photo.SetLocation(location.mCalculated.mLat, location.mCalculated.mLon, location.mCalculated.mEle);
                    photo.markerObject = mMapInterface.CreateMarker(index, location.mCalculated);
                }
                else
                {
                    // There is already a marker on the map for this photo so just move it.
                    photo.SetLocation(location.mCalculated.mLat, location.mCalculated.mLon, location.mCalculated.mEle);
                    mMapInterface.MoveMarker(photo.markerObject, location.mCalculated);
                }
            }
        }

        private void useCalculatedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // For each selected photo (or none if none are selected) set the photos
            // back to the previously calculated position.
            ListView.SelectedIndexCollection selectedIndices = listView1.SelectedIndices;
            foreach (int index in selectedIndices)
            {
                // Get the photo
                PhotoData photo = mPhotoCollection.GetPhoto(listView1.Items[index].ImageKey);
                if (photo.nearestPoint != null)
                {
                    // There is already a marker on the map for this photo so just move it.
                    GPSTrackPoint location = photo.nearestPoint.mCalculated;
                    photo.SetLocation(location.mLat, location.mLon, location.mEle);
                    mMapInterface.MoveMarker(photo.markerObject, location);
                }
            }
        }

        private void usePreviousToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // For each selected photo (or none if none are selected) move the photo
            // to the nearest track point before the calculated point.
            ListView.SelectedIndexCollection selectedIndices = listView1.SelectedIndices;
            foreach (int index in selectedIndices)
            {
                // Get the photo
                PhotoData photo = mPhotoCollection.GetPhoto(listView1.Items[index].ImageKey);
                if (photo.nearestPoint != null)
                {
                    // There is already a marker on the map for this photo so just move it.
                    GPSTrackPoint location = photo.nearestPoint.mBefore;
                    if (location != null)
                    {
                        photo.SetLocation(location.mLat, location.mLon, location.mEle);
                        mMapInterface.MoveMarker(photo.markerObject, location);
                    }
                }
            }
        }

        private void useNextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // For each selected photo (or none if none are selected) move the photos
            // to the nearest track point after the calculated point.
            ListView.SelectedIndexCollection selectedIndices = listView1.SelectedIndices;
            foreach (int index in selectedIndices)
            {
                // Get the photo
                PhotoData photo = mPhotoCollection.GetPhoto(listView1.Items[index].ImageKey);
                if (photo.nearestPoint != null)
                {
                    // There is already a marker on the map for this photo so just move it.
                    GPSTrackPoint location = photo.nearestPoint.mAfter;
                    photo.SetLocation(location.mLat, location.mLon, location.mEle);
                    mMapInterface.MoveMarker(photo.markerObject, location);
                }
            }
        }

        private void trackBarTimeOffset_ValueChanged(object sender, EventArgs e)
        {
            int offset = this.trackBarTimeOffset.Value;
            this.textBoxTimeOffset.Text = String.Format("{0} hour{1}", offset, offset==1 ? "" : "s");
        }

        private void toolStripButtonSave_Click(object sender, EventArgs e)
        {
            // Save the previously located photos.
            // Maybe we can group them into two lists, those with data and those without?
            // Maybe just a flag in the list indicating which ones have data that needs to be saved.
            // Maybe we save whichever are highlighted.

            // Use one Writer repeatedly, resetting the location with each photo.
            JpegWriter writer = new JpegWriter(mSettings);

            foreach (ListViewItem i in listView1.Items)
            {
                // Get the photo
                string filename = i.ImageKey;
                PhotoData photo = mPhotoCollection.GetPhoto(filename);
                if (photo.nearestPoint != null)
                {
                    // Instead of making a backup, we will not overwrite the original... yet.
                    // Once we are more confident that needing the backup will be the exception
                    // and not the rule we can change this back.
                    //writer.MakeBackup(filename);
                    writer.WriteDataToFile(filename, new ExifHeader.GpsLocation(photo.latitude, photo.longitude, photo.elevation));
                }
            }
        }

        private void testExifWritingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Run a series of tests of a list of images to help
            // test the JPEG Exif processing code

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
                JpegWriter writer = new JpegWriter(mSettings);

                // Read the files
                foreach (String file in dialog1.FileNames)
                {
                    writer.UnitTest(file);
                }
            }
        }

        private void debugModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mSettings.DebugMode = !mSettings.DebugMode;
            ((ToolStripMenuItem)sender).Checked = mSettings.DebugMode;
        }

        private void contentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Show the help.
            Help.ShowHelp(this, helpProvider1.HelpNamespace);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 about = new AboutBox1();
            about.ShowDialog();
        }

        private void textBoxSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                mMapInterface.Search(textBoxSearch.Text);
            }
        }

        private void googleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadGoogleMaps();
        }

        private void bingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadBingMaps();
        }
    }
}