using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//stream for image based webcams

//Open Global IP's to try

//Parking lot

//http://107.144.24.100:88/record/current.jpg?rand=509157

//Beach

//http://107.144.24.100/record/current.jpg?rand=291145


public class ImageStream : MonoBehaviour {
    public string uri = "http://172.20.10.2/camstream";
    public RawImage frame;
    private Texture2D texture;

	// Use this for initialization
	void Start () {
        texture = new Texture2D(2, 2);
        StartCoroutine(GetImage());
    }
    
    IEnumerator GetImage()
    {
        yield return new WaitForEndOfFrame();
        using(WWW www = new WWW(uri))
        {
            yield return  www;
            www.LoadImageIntoTexture(texture);
            frame.texture = texture;
            Start();
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}
