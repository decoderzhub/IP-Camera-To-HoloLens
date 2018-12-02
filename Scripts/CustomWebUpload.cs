using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class CustomWebUpload : DownloadHandlerScript {

    /////////////////////////////////////////////////////////////////////////////////////////////////////
    ///      Standard scripted download handler - allocates memory on each ReceiveData callback.      ///
    /////////////////////////////////////////////////////////////////////////////////////////////////////
    
    public int contentLength;
    WebStream webStream;

    public CustomWebUpload() : base()
    {
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///    Pre-allocated scripted download handler reuses the supplied byte array to deliver data and eliminates memory allocation.   ///
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public CustomWebUpload(byte[] buffer) : base(buffer)
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


        if (byteFromCamera == null || byteFromCamera.Length < 1)
        {
            Debug.Log("CustomWebRequest :: ReceiveData - received a null/empty buffer");
            return false;
        }
        return true;
    }
    public int ReceivedCameraData()
    {
        return contentLength;
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

   
    ////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///          Converts String back to a Byte Array.                                                   /// 
    ////////////////////////////////////////////////////////////////////////////////////////////////////////
    public static byte[] StringToByteArray(string hex)
    {
        int count = 0;
        while (hex.Length % 2 != 0)
        {
            hex = hex.Insert(hex.Length - 7, "0");
            count++;
            if (count == 5) { return null; }
        }
        int NumberChars = hex.Length;
        byte[] bytes = new byte[NumberChars / 2];
        for (int i = 0; i < NumberChars; i += 2)
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        return bytes;
    }
    ////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///          Converts String back to a Byte Array.                                                   /// 
    ////////////////////////////////////////////////////////////////////////////////////////////////////////
    public static string FromHexString(string hexString)
    {
        var bytes = new byte[hexString.Length / 2];
        for (var i = 0; i < bytes.Length; i++)
        {
            bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
        }

        return Encoding.UTF8.GetString(bytes); // returns content length
    }


}
