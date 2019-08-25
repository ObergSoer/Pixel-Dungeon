using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class kameraBewegung : MonoBehaviour
{

    //public float cameraDistOffset = 0;
    private Camera hauptKamera;
    private GameObject spieler;
    private GameObject kamera;

    // Use this for initialization
    void Start()
    {
        hauptKamera = GetComponent<Camera>();
        spieler = GameObject.Find("Player");
        kamera = GameObject.Find("Main Camera"); 
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 kameraInfo = kamera.transform.transform.position;
        Vector3 spielerInfo = spieler.transform.transform.position;
        hauptKamera.transform.position = new Vector3(spielerInfo.x, spielerInfo.y, kameraInfo.z);
    }
}
