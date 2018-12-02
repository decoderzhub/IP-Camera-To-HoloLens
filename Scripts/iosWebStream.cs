using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class iosWebStream : MonoBehaviour {
    public RawImage frame;
    CameraDownloadHandler cameraDownloadHandler;
    UnityWebRequest webRequest;
    [Tooltip("Set this to change the url of the streaming IP camera")]
    public string url = "http://192.168.0.203/stream";
    [Tooltip("Connecting button status")]
    public Button ConnectButton;
    [Tooltip("Connecting button status")]
    public Button DisconnectButton;
    [Tooltip("IP address input field")]
    byte[] bytes = new byte[100000];
    int locoDataLength;
    int locoContentLength;
    byte[] cameraBytesRx = new byte[100000];
    public float ImagePassPercent = .70f;
    public float RxDataPassRate = .20f;
    Texture2D camTexture;
    public void onConnect()
    {
        ConnectButton.gameObject.SetActive(false);
        DisconnectButton.gameObject.SetActive(true);
        cameraDownloadHandler = new CameraDownloadHandler(bytes);
        GetVideo();
       
    }

    public void onDisconnect()
    {
        DisconnectButton.gameObject.SetActive(false);
        ConnectButton.gameObject.SetActive(true);
        webRequest.Dispose();
    }

      public void GetVideo()
    {
        webRequest = new UnityWebRequest(url);
        webRequest.downloadHandler = cameraDownloadHandler;
        webRequest.Send();
        
    }
    // Use this for initialization
    void Start () {
         camTexture = new Texture2D(2, 2);

    }

    // Update is called once per frame
    void Update () {
        if(DisconnectButton.IsActive() == true)
        {
            Debug.Log("DataLength: " + cameraDownloadHandler.DataLength());
            Debug.Log("BytesRx: " + cameraDownloadHandler.CameraBytesReceived().Length);
            locoDataLength = cameraDownloadHandler.DataLength();
            cameraBytesRx = cameraDownloadHandler.CameraBytesReceived();
            locoContentLength = FindLength(cameraBytesRx);
            Debug.Log("ContentLength: " + locoContentLength);
            if (locoContentLength == 0)
            {
                Debug.Log("Not enough Bytes to read!");
               
                return;
            }


                string Stream = BitConverter.ToString(cameraBytesRx).Replace("-", "");

            //string Stream = Encoding.Default.GetString(cameraBytesRx);
                
                Stream = ToHexString(Stream);
                byte[] RecievedImageByte = new byte[Stream.Length / 2];//receieving array same size as bytes from camera
                Debug.Log("contains FFD8: "+ Stream.Contains("FFD8"));
                if (Stream.Contains("FFD8"))
                {
                Stream = Stream.Substring(Stream.IndexOf("FFD8", StringComparison.CurrentCultureIgnoreCase)).Trim();
                    if (Stream.Contains("FFD9"))
                    {
                    Stream = Stream.Substring(Stream.IndexOf("FFD8", StringComparison.CurrentCultureIgnoreCase), Stream.IndexOf("FFD9", StringComparison.CurrentCultureIgnoreCase) + 4);
                        Debug.Log(Stream);
                    }
                    else
                    {
                        return;
                    }

                    //convert string to byte committing changes
                    if (StringToByteArray(Stream) != null)
                    {
                        RecievedImageByte = StringToByteArray(Stream);
                    }
                if (Stream.IndexOf("FFD8", StringComparison.CurrentCultureIgnoreCase) == 0)
                    {

                        if (imageReadable(locoContentLength, Stream))
                        {
                            //create the image array with the length of the Stream
                            byte[] completeImageByte = new byte[Stream.Length];
                            DisplayImage(RecievedImageByte, completeImageByte, Stream);
                            RecievedImageByte = new byte[Stream.Length / 2];
                            completeImageByte = new byte[100000];
                            Stream = "";
                        }
                        else
                        {
                            Debug.Log("Image not readable!");
                        }


                    }

                }

            


        }

        
	}
    /////////////////////////////////////////////////////////////////
    ///          Returns if the image is readable                 ///
    /////////////////////////////////////////////////////////////////
    public bool imageReadable(int contentLength, string inputStream)
    {
        float passRate;
        float dataPassRate;
        passRate = contentLength * ImagePassPercent;
        dataPassRate = contentLength * RxDataPassRate;
        if (inputStream.Length / 2 > contentLength || inputStream.Length / 2 < passRate || locoDataLength < dataPassRate)
        {

            return false;
        }
        else
        {
            return true;
        }

    }
    //////////////////////////////////////////////
    ///          Display Image                 ///
    //////////////////////////////////////////////
    public bool DisplayImage(byte[] RecievedImageByte, byte[] completeImageByte, string inputStream)
    {
        Buffer.BlockCopy(RecievedImageByte, inputStream.IndexOf("FFD8", StringComparison.CurrentCultureIgnoreCase), completeImageByte, 0, inputStream.Length / 2);
        camTexture.LoadImage(completeImageByte);
        frame.color = Color.white;
        frame.texture = camTexture;

        return true;

    }
    public int FindLength(byte[] bytesReceived)
    {
        int position = 1;
        int content = 0;
        string inputStream = "";

        inputStream = BitConverter.ToString(bytesReceived).Replace("-","");

        position = inputStream.IndexOf("683A20", StringComparison.CurrentCultureIgnoreCase) + 6;
            int contentToRead = inputStream.IndexOf("FFD8", StringComparison.CurrentCultureIgnoreCase) - position;

            if (inputStream.Contains("683A20"))
            {
                if (contentToRead > 0)
                {
                    string contentLength = inputStream.Substring(position, contentToRead);
                    if (contentLength.Length > 32) { return 0; }
                    content = Int32.Parse(FromHexString(contentLength));
                }
            }
            return content;
    }

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

    public static string FromHexString(string hexString)
    {
        var bytes = new byte[hexString.Length / 2];
        for (var i = 0; i < bytes.Length; i++)
        {
            bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
        }

        return Encoding.UTF8.GetString(bytes); // returns content length
    }

    public static string ToHexString(string str)
    {
        var sb = new StringBuilder();
        var bytes = Encoding.Default.GetBytes(str);
        foreach (var t in bytes)
        {
            sb.Append(t.ToString("X2"));
        }

        return sb.ToString(); // returns: "48656C6C6F20776F726C64" for "Hello world"
    }
}
