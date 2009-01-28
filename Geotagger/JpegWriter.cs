//////////////////////////////////////////////////////////////////////////////
//
//    This file is part of Geotagger: A tool for geotagging photographs
//    Copyright (C) 2009  Kaz Okuda (http://notions.okuda.ca)
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
using System.Collections.Generic;
using System.IO;

namespace Geotagger
{
    class JpegWriter
    {
        private Settings mSettings;

        // Constructor
        public JpegWriter(Settings settings)
        {
            mSettings = settings;
        }

        // Make a backup of the given file into a subdirectory called "backup".
        public void MakeBackup(string filename)
        {
            FileInfo info = new FileInfo(filename);

            // Make sure that the backup directory exists.
            string backuppath = info.DirectoryName + "\\backup";
            Directory.CreateDirectory(backuppath);

            // Copy the file to the backup directory.
            string newname = backuppath + "\\" + info.Name;

            // We should probably NOT overwrite the file if it exists there as that
            // could be a previous backup which, most likely, is better than the current file.
            // However, the user might expect it to overwrite the old backup.
            // Perhaps we should show an error message if this occurs and prompt the user
            // for the appropriate action?
            if (File.Exists(newname))
            {
                System.Windows.Forms.DialogResult result = System.Windows.Forms.MessageBox.Show(
                    "The backup file " + newname + " already exists, would you like to replace it?",
                    "Backup Warning",
                    System.Windows.Forms.MessageBoxButtons.YesNo);

                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    info.CopyTo(newname, true);
                }
                else
                {
                    // Do not overwrite the file
                }
            }
            else
            {
                info.CopyTo(newname);
            }
        }

        // Write the given GPS data to the file, overwriting the file in the process.
        // A backup should be made just in case something goes wrong.
        public void WriteDataToFile(string filename, ExifHeader.GpsLocation gpsData)
        {
            ExifHeader.JpegProcessor writer = new ExifHeader.JpegProcessor();

            // Open the file for reading
            FileStream infile = new FileStream(filename, FileMode.Open, FileAccess.Read);

            // Create the output stream - memory backed, not file.
            MemoryStream outfile = new MemoryStream();

            Action<ExifHeader.ExifDirectory> action = delegate(ExifHeader.ExifDirectory dir)
            {
                // 1. Remove old GPS data from the ExifDirectory.
                // 2. Insert new GPS data.

                dir.RemoveTag(ExifHeader.Tag.GPSINFO);
                dir.CreateGps(gpsData);

            };

            // Scan through the file looking for existing data
            // If data does not exist then add it to the file and update the headers
            // If it does exist, then replace it and update the headers.
            bool result = writer.ProcessJpegFile(outfile, infile, action);

            if (result)
            {
                // We will eventually overwrite the original, but not until we are
                // satisfied that this code is solid.  At the very least we should
                // make a backup.

                // For now we will write to a subdirectory of the source called "geotagged".
                // Make sure that the directory exists.
                FileInfo info = new FileInfo(filename);
                string outpath = info.DirectoryName + "\\geotagged";
                Directory.CreateDirectory(outpath);
                string newname = outpath + "\\" + info.Name;

                // Write the file to the new directory.
                WriteStream(outfile, newname);

                if (mSettings.DebugMode)
                {
                    LaunchCompare(filename, newname);

                    // Run the proces again, but with our own file to ensure that
                    // we can read it and we write it out again the same.
                    outfile.Seek(0, SeekOrigin.Begin);
                    MemoryStream test2 = new MemoryStream();
                    writer.ProcessJpegFile(test2, outfile, action);
                    if (DebugCompareStreams(test2, outfile))
                    {
                        Console.WriteLine("test1 matches test2");
                    }
                    else
                    {
                        // Write out the file that doesn't match so we can investigate.
                        WriteStream(test2, "test2.jpg");
                        LaunchCompare(newname, "test2.jpg");
                    }
                }
            }

            outfile.Close();
        }

        private void WriteStream(Stream stream, string filename)
        {
            byte[] outdata = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(outdata, 0, (int)stream.Length);
            File.WriteAllBytes(filename, outdata);
        }

        public void UnitTest(string filename)
        {
            ExifHeader.JpegProcessor writer = new ExifHeader.JpegProcessor();

            // Open the file for reading
            FileStream infile = new FileStream(filename, FileMode.Open, FileAccess.Read);

            // Create the output stream - memory backed, not file.
            MemoryStream outfile = new MemoryStream();

            // Scan through the file looking for existing data
            // If data does not exist then add it to the file and update the headers
            // If it does exist, then replace it and update the headers.
            bool result = writer.ProcessJpegFile(outfile, infile, null);

            if (result)
            {
                if (DebugCompareStreams(outfile, infile))
                {
                    Console.WriteLine("Output matches {0}", filename);
                }
                else
                {
                    // The files are different so we should compare them
                    WriteStream(outfile, "test.jpg");
                    LaunchCompare(filename, "test.jpg");
                }

                // Run the test again, but with our own file to ensure that
                // we can read it and we write it out again the same.
                outfile.Seek(0, SeekOrigin.Begin);
                MemoryStream test2 = new MemoryStream();
                writer.ProcessJpegFile(test2, outfile, null);
                if (DebugCompareStreams(test2, outfile))
                {
                    Console.WriteLine("test1 matches test2");
                }
                else
                {
                    // The files are different so we should compare them
                    WriteStream(outfile, "test2.jpg");
                    LaunchCompare("test1.jpg", "test2.jpg");
                }
            }

            outfile.Close();
        }

        private bool DebugCompareStreams(Stream s1, Stream s2)
        {
            bool result = false;

            s1.Seek(0, SeekOrigin.Begin);
            s2.Seek(0, SeekOrigin.Begin);

            if (s1.Length != s2.Length)
            {
                // Different lengths.
                Console.WriteLine("ERROR: DebugCompareStreams - Streams are different lengths, ({0} and {1})", s1.Length, s2.Length);
                //return;
            }

            int b1 = 0;
            int b2 = 0;
            do
            {
                b1 = s1.ReadByte();
                b2 = s2.ReadByte();
            }
            while ((b1 == b2) && (b1 != -1) && (b2 != -1));

            if (b1 != b2)
            {
                Console.WriteLine("ERROR: DebugCompareStreams - Streams do not match!");
            }
            else
            {
                Console.WriteLine("DebugComareStreams - Streams are identical");
                result = true;
            }

            return result;
        }

        private void LaunchCompare(string file1, string file2)
        {
            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo();
            info.FileName = "comparejpeg.bat";
            info.Arguments = "\"" + file1 + "\"" + " " + "\"" + file2 + "\"";
            info.UseShellExecute = true;
            System.Diagnostics.Process.Start(info);
        }
    }
}