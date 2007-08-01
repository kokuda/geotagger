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

namespace Geotagger
{
    public partial class Form1 : Form
    {
        private HttpServer mHttpServer;
        private GPSTrack mGPSTrack;

        public Form1()
        {
            InitializeComponent();

            // Initialize Members
            mGPSTrack = new GPSTrack();
            mHttpServer = new HttpServer(8080);

            // Register the callback interface to the Javascript.
            webBrowser1.ObjectForScripting = new ScriptInterface(webBrowser1);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Start the HTTP server for the browser to use.
            mHttpServer.Startup();

            // Load the page from the HttpServer.
            webBrowser1.Navigate(new Uri("http://127.0.0.1:8080/html/start.html"));
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            mHttpServer.Shutdown();
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
                        Image image = Image.FromFile(file);
                        imageListLarge.Images.Add(file, image);
                        imageListSmall.Images.Add(file, image);
                        listView1.Items.Add(file, Path.GetFileName(file), file);
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
    }
}