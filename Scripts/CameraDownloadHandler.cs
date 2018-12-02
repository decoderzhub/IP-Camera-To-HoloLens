using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CameraDownloadHandler : DownloadHandlerScript {

    /////////////////////////////////////////////////////////////////////////////////////////////////////
    ///      Standard scripted download handler - allocates memory on each ReceiveData callback.      ///
    /////////////////////////////////////////////////////////////////////////////////////////////////////
    public int contentLength = 0;
    public byte[] cameraBytesReceived = new byte[100000];
    public int _dataLength = 0;
  
    public CameraDownloadHandler() : base()
    {
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///    Pre-allocated scripted download handler reuses the supplied byte array to deliver data and eliminates memory allocation.   ///
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public CameraDownloadHandler(byte[] buffer) : base(buffer)
    {

    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////
    ///     Required by DownloadHandler base class. Called when you address the 'bytes' property.     ///
    /////////////////////////////////////////////////////////////////////////////////////////////////////
    protected override byte[] GetData() { return null; }
    /////////////////////////////////////////////////////////////////////////////////////////////////////
    ///           Called once per frame when data has been received from the network.                 ///
    /////////////////////////////////////////////////////////////////////////////////////////////////////
    protected override bool ReceiveData(byte[] byteFromCamera, int dataLength)
    {
        _dataLength = dataLength;
        cameraBytesReceived = byteFromCamera;
       
        if (byteFromCamera == null || byteFromCamera.Length < 1)
        {
            Debug.Log("CustomWebRequest :: ReceiveData - received a null/empty buffer");
            return false;
        }

        
        return true;
    }

    public byte[] CameraBytesReceived()
    {
        return cameraBytesReceived;
    }

    public int DataLength()
    {
        return _dataLength;
    }
    
   
    
    ////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///          Called when all data has been received from the server and delivered via ReceiveData.   /// 
    ////////////////////////////////////////////////////////////////////////////////////////////////////////
    protected override void CompleteContent()
    {
        Debug.Log("CustomWebRequest :: CompleteContent - DOWNLOAD COMPLETE!");

    }
    ////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///          Called when a Content-Length header is received from the server.                        /// 
    ////////////////////////////////////////////////////////////////////////////////////////////////////////
    protected override void ReceiveContentLength(int contentLength)
    {
        Debug.Log(string.Format("CustomWebRequest :: ReceiveContentLength - length {0}", contentLength));
    }

   
   
}
