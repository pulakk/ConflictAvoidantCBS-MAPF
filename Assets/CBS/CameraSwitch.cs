using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitch : MonoBehaviour {

	public Camera[] cams; 
	int ithcam;

	// Use this for initialization
	void Start () {
		ithcam = 0;
		switchCam ();
		
	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetKeyDown (KeyCode.E)) {
			switchCam ();
		}
	}

	void switchCam(){
		for (int i=0;i<cams.Length;i++) {
			if (i == ithcam)
				cams [i].enabled = true;
			else
				cams [i].enabled = false;
		}
		ithcam = (ithcam + 1) % cams.Length;
	}
}
