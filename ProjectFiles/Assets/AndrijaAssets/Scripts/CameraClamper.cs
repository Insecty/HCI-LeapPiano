using UnityEngine;
using System.Collections;

public class CameraClamper : MonoBehaviour {

    public float MaximumXCameraPosition = 0;
    public float MinimumXCameraPosition = 0;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 camPos = Camera.main.transform.position;
        
        if (camPos.x > MaximumXCameraPosition)
        {
            camPos.x = MaximumXCameraPosition;
        }
        else if (camPos.x < MinimumXCameraPosition)
        {
            camPos.x = MinimumXCameraPosition;
        }

        Camera.main.transform.position = camPos;
	}
}
