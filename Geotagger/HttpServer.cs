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
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

class HttpProcessor
{

    private Socket s;
    private BufferedStream bs;
    private StreamReader sr;
    private StreamWriter sw;
    private String method;
    private String url;
    private String protocol;
    private Hashtable hashTable;

    public HttpProcessor(Socket s)
    {
        this.s = s;
        hashTable = new Hashtable();
    }

    public void process()
    {
        NetworkStream ns = new NetworkStream(s, FileAccess.ReadWrite);
        bs = new BufferedStream(ns);
        sr = new StreamReader(ns);
        sw = new StreamWriter(bs);
        parseRequest();
        readHeaders();
        writeURL();
        s.Shutdown(SocketShutdown.Both);
        ns.Close();
    }

    public void parseRequest()
    {
        String request = sr.ReadLine();
        string[] tokens = request.Split(new char[] { ' ' });
        method = tokens[0];
        url = tokens[1];
        protocol = tokens[2];
    }

    public void readHeaders()
    {
        String line;
        while ((line = sr.ReadLine()) != null && line != "")
        {
            string[] tokens = line.Split(new char[] { ':' });
            String name = tokens[0];
            String value = "";
            for (int i = 1; i < tokens.Length; i++)
            {
                value += tokens[i];
                if (i < tokens.Length - 1) tokens[i] += ":";
            }
            hashTable[name] = value;
        }
    }

    public void writeURL()
    {
        try
        {
            FileStream fs = new FileStream(url.Substring(1), FileMode.Open, FileAccess.Read);
            writeSuccess();
            BufferedStream bs2 = new BufferedStream(fs);
            byte[] bytes = new byte[4096];
            int read;
            while ((read = bs2.Read(bytes, 0, bytes.Length)) != 0)
            {
                bs.Write(bytes, 0, read);
            }
            bs2.Close();
        }
        catch (FileNotFoundException)
        {
            writeFailure();
            sw.WriteLine("File not found: " + url);
        }
        catch (DirectoryNotFoundException)
        {
            writeFailure();
            sw.WriteLine("Directory not found: " + url);
        }
        sw.Flush();
    }

    public void writeSuccess()
    {
        //sw.WriteLine("HTTP/1.0 200 OK");
        //sw.WriteLine("Connection: close");
        //sw.WriteLine();
    }

    public void writeFailure()
    {
        sw.WriteLine("HTTP/1.0 404 File not found");
        sw.WriteLine("Connection: close");
        sw.WriteLine();
    }
}

public class HttpServer
{

    // ============================================================
    // Data

    protected int mPort;
    protected bool mStop;
    protected Socket mListener;

    // ============================================================
    // Constructor

    public HttpServer()
        : this(80)
    {
    }

    public HttpServer(int port)
    {
        this.mPort = port;
    }

    // ============================================================
    // Start
    public void Startup()
    {
        mStop = false;
        Thread thread = new Thread(new ThreadStart(this.listen));
        thread.Start();
    }

    // ============================================================
    // Shutdown
    public void Shutdown()
    {
        mStop = true;
        mListener.Close();
    }

    // ============================================================
    // Listener

    private void listen()
    {
        mListener = new Socket(0, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), mPort);
        mListener.Bind(endpoint);
        mListener.Blocking = true;
        mListener.Listen(-1);
        while (!mStop)
        {
            try
            {
                Socket s = mListener.Accept();
                HttpProcessor processor = new HttpProcessor(s);
                Thread thread = new Thread(new ThreadStart(processor.process));
                thread.Start();
            }
            catch(System.Net.Sockets.SocketException e)
            {
                // Error code is WSACancelBlockingCall which is a result of calling
                // mListener.Close from Shutdown.  This is intended.
                if (e.ErrorCode != 10004)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }
}