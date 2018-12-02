using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;

public class WebStream : MonoBehaviour
{
    public RawImage frame;
    CustomWebRequest customWebRequest;
    CustomWebUpload customWebUpload;
    UnityWebRequest webRequest;
    UnityWebRequest webUpload;
    [Tooltip("Set this to change the url of the streaming IP camera")]
    public string url = "http://192.168.0.203/stream";
    [Tooltip("Set this to change the servo connection")]
    public string servoURL = "http://192.168.0.206/pantilt";
    [Tooltip("Set this to change the received data buffer increases lag")]
    public int BufferSize = 100000;
    [Tooltip("Connect button to connect to device")]
    public Button ConnectButton;
    [Tooltip("Connecting button status")]
    public Button ConnectingButton;
    [Tooltip("Disconnect button to disconnect from device")]
    public Button DisconnectButton;
    [Tooltip("IP address input field")]
    public InputField InputIPAddress;
    [Tooltip("Pass rate field decreases lag increases distortion")]
    public InputField InputPassRate;
    [Tooltip("Buffer rate input by user during runtime")]
    public InputField InputBufferRate;
    [Tooltip("Received data pass rate input by user during runtime")]
    public InputField InputRxDataRate;
    [Tooltip("Text to display pass percentage")]
    public Text PercentageDisplay;
    [Tooltip("Information panel to display results")]
    public Image InfoPanel;
    public Vector3 rightLeft;
    public Vector3 upDown;
    private string moveData;
    byte[] byteArray;


    byte[] bytes = new byte[100000];

    public void onConnect()
     {
        bytes = new byte[BufferSize];
        if (InputIPAddress.textComponent.text == "")
        {
            return;
        }
        url = InputIPAddress.textComponent.text;
        Debug.Log(url);
        ConnectButton.gameObject.SetActive(false);
        InputBufferRate.gameObject.SetActive(false);
        InputRxDataRate.gameObject.SetActive(true);
        customWebRequest = new CustomWebRequest(bytes);
        GetVideo();
        StartCoroutine(initConnecting());
    }

   IEnumerator initConnecting()
    {
        
        ConnectingButton.gameObject.SetActive(true);
        yield return new WaitForSeconds(6);
        if(customWebRequest.Connected == true)
        {
            ConnectingButton.gameObject.SetActive(false);
            DisconnectButton.gameObject.SetActive(true);
        }
        else
        {
            ConnectingButton.gameObject.SetActive(false);
            onDisconnect();
            PercentageDisplay.text = "Please check connection!!!";
        }

    }

    public void onDisconnect()
    {
        InputIPAddress.textComponent.text = "";
        DisconnectButton.gameObject.SetActive(false);
        ConnectButton.gameObject.SetActive(true);
        InputBufferRate.gameObject.SetActive(true);
        InputRxDataRate.gameObject.SetActive(false);
        PercentageDisplay.text = "change buffer rate only when disconnected";
        webRequest.Dispose();
    }


    public void onPassRateEdit(string passRate)
    {
        if (InputPassRate.text == "")
        {
            return;
        }
        float updateRate = float.Parse("." + InputPassRate.text);
        customWebRequest.ImagePassPercent = updateRate;
    }
    public void onBufferRate(string BufferRate)
    {
        if (InputBufferRate.text == "")
        {
            return;
        }
        BufferSize = Int32.Parse(InputBufferRate.text);
        customWebRequest.bufferSize = BufferSize; ;
     }

    public void onRxDataRate(string passRate)
    {
        if (InputRxDataRate.text == "")
        {
            return;
        }
        float RxDataRate = float.Parse("." + InputRxDataRate.text);
        customWebRequest.RxDataPassRate = RxDataRate; ;
    }



    public void Start()
    {
        InputIPAddress.text = url;

    }
        
    public void move(string postData)
    {
        moveData = postData;
        string data = "?move=" + postData;
        UnityWebRequest www = new UnityWebRequest(servoURL + data);
        www.Send();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
           // Debug.Log("Upload complete!");
        }
    }

    public void GetVideo()
    {
        webRequest = new UnityWebRequest(url);
        webRequest.downloadHandler = customWebRequest;
        webRequest.Send();
        
    }

    private void Update()
    {
        if(!ConnectButton.IsActive())
        {
                PercentageDisplay.text = "Pass Rate: " + customWebRequest.ImagePassPercent * 100 + "%" +
                                     "\nRx Pass Rate: " + customWebRequest.RxDataPassRate * 100 + "%"+
                                     "\nConnected: " + customWebRequest.Connected +
                                     "\nContent-Length: " + customWebRequest.contentLength +
                                     "\nRx Data: " + customWebRequest.RxDataLength + 
                                     "\nJPEG: " + customWebRequest.JPEGsize +
                                     "\nBuffer: " + customWebRequest.bufferSize +
                                     "\nmoving: " + moveData;
        }



        if (Input.GetKey(KeyCode.A))
            move("left");

        if (Input.GetKey(KeyCode.D))
            move("right");

        if (Input.GetKey(KeyCode.W))
            move("up");

        if (Input.GetKey(KeyCode.S))
            move("down");

        if (Input.GetKey(KeyCode.LeftShift))
            move("center");
/*
#elif UNITY_IOS

        rightLeft = new Vector3(0, Input.acceleration.x, 0);
        upDown = new Vector3(Input.acceleration.y, 0, 0);

        if (Input.acceleration.x > .2f)
            move("right");
        
        if (Input.acceleration.x < -.2f)
            move("left");
        
        if (Input.acceleration.y < -.95f)
            move("up");
        
        if (Input.acceleration.y > -.75f)
            move("down");
#endif
      */  
    }


    private void OnApplicationQuit()
    {
        if(webRequest!=null){

            webRequest.Dispose();
        }
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus == false)
        {
            InputPassRate.gameObject.SetActive(false);
            InfoPanel.gameObject.SetActive(false);
            InputBufferRate.gameObject.SetActive(false);
        }
        else
        {
            InputPassRate.gameObject.SetActive(true);
            InfoPanel.gameObject.SetActive(true);
            InputBufferRate.gameObject.SetActive(true);
        }
    }


   
}
