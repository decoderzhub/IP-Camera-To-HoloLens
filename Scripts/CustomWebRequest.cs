using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

    public class CustomWebRequest : DownloadHandlerScript
    {
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        ///      Standard scripted download handler - allocates memory on each ReceiveData callback.      ///
        /////////////////////////////////////////////////////////////////////////////////////////////////////
        GameObject Image;
        public float ImagePassPercent = .50f;
        public float RxDataPassRate = .50f;
        public int contentLength;
        public int JPEGsize;
        public int bufferSize;
        public int RxDataLength;
        public byte[] RxData;
        public bool Connected = false;
        WebStream webStream;
        Texture2D camTexture = new Texture2D(2, 2);
        public float timeElapsed;
        public int prevLength;
        public CustomWebRequest() : base()
        {
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///    Pre-allocated scripted download handler reuses the supplied byte array to deliver data and eliminates memory allocation.   ///
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public CustomWebRequest(byte[] buffer) : base(buffer)
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
            RxData = byteFromCamera;
            RxDataLength = dataLength; //return dataLength to webstream to display on screen.
            bufferSize = byteFromCamera.Length; //return bufferSize to be displayed on screen.

            if (byteFromCamera == null || byteFromCamera.Length < 1)
            {
                Debug.Log("CustomWebRequest :: ReceiveData - received a null/empty buffer");
                return false;
            }

            contentLength = FindLength(byteFromCamera); // find the length of the JPEG
            Debug.Log(contentLength);
            if (contentLength == 0)
            {
                //Debug.Log("Not enough Bytes to read!");
                byteFromCamera = new byte[bufferSize];
                return true;
            }


            //string inputStream = Encoding.Default.GetString(byteFromCamera);
            //inputStream = ToHexString(inputStream);
            string inputStream = BitConverter.ToString(byteFromCamera).Replace("-", "");

            byte[] RecievedImageByte = new byte[inputStream.Length / 2];                   //receieving array same size as bytes from camera
            inputStream = inputStream.Substring(inputStream.IndexOf("FFD8")).Trim();
            if (inputStream.Contains("FFD9"))
            {
                inputStream = inputStream.Substring(inputStream.IndexOf("FFD8"), inputStream.IndexOf("FFD9") + 4);
            }
            else
            {
                byteFromCamera = new byte[bufferSize];
                return true;
            }
        //convert string to byte committing changes
        if (StringToByteArray(inputStream) != null)
        {
            RecievedImageByte = StringToByteArray(inputStream);
        }
        else { return true; }

            if (inputStream.IndexOf("FFD8") == 0)
            {

                if (imageReadable(contentLength, inputStream))
                {
                    //create the image array with the length of the inputStream
                    byte[] completeImageByte = new byte[inputStream.Length];
                    DisplayImage(RecievedImageByte, completeImageByte, inputStream);
                    RecievedImageByte = new byte[inputStream.Length / 2];
                    completeImageByte = new byte[bufferSize];
                    inputStream = "";
                }
                else
                {
                    //Debug.Log("Image not readable!");
                }


            }
            byteFromCamera = new byte[bufferSize];
            return true;
        }
               
        public int ReceivedCameraData()
        {
            return contentLength;
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
            if (inputStream.Length / 2 > contentLength || inputStream.Length / 2 < passRate  || RxDataLength < dataPassRate)
            {

                return false;
            }

        return true;

        }
        //////////////////////////////////////////////
        ///          Display Image                 ///
        //////////////////////////////////////////////
        public bool DisplayImage(byte[] RecievedImageByte, byte[] completeImageByte, string inputStream)
        {
            Buffer.BlockCopy(RecievedImageByte, inputStream.IndexOf("FFD8"), completeImageByte, 0, inputStream.Length / 2);
            Image = GameObject.Find("RawImage");   //We have to use GameObject.Find unless we instantiate the a prefab I'll add it later
            WebStream webStream = Image.GetComponent<WebStream>();
            RawImage screenDisplay = webStream.frame;
            JPEGsize = inputStream.Length / 2;
            camTexture.LoadImage(completeImageByte);
            Connected = true;
            //Debug.Log("inputStream length: " + count);   //length of bytes of the JPEG being loaded
            screenDisplay.color = Color.white;
            screenDisplay.texture = camTexture;

            return true;

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
        ///         Finds the length of the JPEG image received                                              /// 
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        public int FindLength(byte[] bytesReceived)
        {
            int position = 1;
            int content = 0;
            string inputStream = "";

        inputStream = BitConverter.ToString(bytesReceived).Replace("-", "");
        //Debug.Log(inputStream);
        //inputStream = Encoding.Default.GetString(bytesReceived);
        //inputStream = ToHexString(inputStream);

        position = inputStream.IndexOf("683A20") + 6;
            int contentToRead = inputStream.IndexOf("FFD8") - position;

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

