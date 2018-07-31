using UnityEngine;
using System.Collections;

public class AudioObjectCameraMatcher : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void LateUpdate () {
	    // Find all objects with name starting with "AudioObject"
        // and set their position to main camera position
        foreach (AudioObject audioObject in (AudioObject[])GameObject.FindObjectsOfType(typeof(AudioObject)))
        {
            audioObject.gameObject.transform.position = Camera.main.transform.position;
        }
	}
}
