using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class IPCamera : MonoBehaviour {
    //[HideInInspector]
    public Byte[] JpegData;
    //[HideInInspector]
    //public string resolution = "480x360";
    public string url;
    public InputField InputIPAddress;

    private Texture2D texture;
    private Stream stream;
    private WebResponse resp;
    private HttpWebRequest req;
    public RawImage frame;
    public Text Status;
    public Button ConnectButton;
    public Button DisconnectButton;


    public void onConnect()
    {
       if(InputIPAddress.textComponent.text == "")
        {
            return;
        }
        url = InputIPAddress.textComponent.text;
        Debug.Log(url);
        DisconnectButton.gameObject.SetActive(true);
        ConnectButton.gameObject.SetActive(false);
        GetVideo();

    }
    public void onDisconnect()
    {
        InputIPAddress.textComponent.text = "";
        DisconnectButton.gameObject.SetActive(false);
        ConnectButton.gameObject.SetActive(true);
        req.Abort();
    }

    public void Start()
    {
        InputIPAddress.textComponent.text = "http://192.168.0.8/stream";
    }
    
    public void GetVideo()
    {
        texture = new Texture2D(2, 2);
        // create HTTP request
        //resolution = "320x240";
        req = (HttpWebRequest)WebRequest.Create(url);
        //req.Credentials = new NetworkCredential("login", "password");
        // get response
        resp = req.GetResponse();
        // get response stream
        stream = resp.GetResponseStream();
        StartCoroutine(GetFrame());
    }

   
    public IEnumerator GetFrame()
    {
        JpegData = new Byte[56000];

        while (true)
        {
            int bytesToRead = FindLength(stream);
            if (bytesToRead == -1)
            {
                //                print("End of stream");
                yield break;
            }

            int leftToRead = bytesToRead;

            while (leftToRead > 0)
            {
                //Debug.Log(leftToRead);
                leftToRead -= stream.Read(JpegData, bytesToRead - leftToRead, leftToRead);
                yield return null;
            }

          //  MemoryStream ms = new MemoryStream(JpegData, 0, bytesToRead, false, true);

            if (texture.LoadImage(JpegData))
            {
                //Debug.Log("Image Loaded!");
            }
            frame.canvasRenderer.SetTexture(texture);
            stream.ReadByte(); // CR after bytes
            stream.ReadByte(); // LF after bytes
        }
    }

    int FindLength(Stream stream)
    {
        int b;
        string line = "";
        int result = -1;
        bool atEOL = false;

        while ((b = stream.ReadByte()) != -1)
        {
            if (b == 10) continue; // ignore LF char
            if (b == 13)
            { // CR
                if (atEOL)
                {  // two blank lines means end of header
                    stream.ReadByte(); // eat last LF
                    return result;
                }
                if (line.StartsWith("Content-Length:"))
                {
                    result = Convert.ToInt32(line.Substring("Content-Length:".Length).Trim());
                }
                else
                {
                    line = "";
                }
                atEOL = true;
            }
            else
            {
                atEOL = false;
                line += (char)b;
                Debug.Log("atEOL = " + line);
            }
        }
        return -1;
    }

    private void OnApplicationQuit()
    {
        req.Abort();
    }

}
