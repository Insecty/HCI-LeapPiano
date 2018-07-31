using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteGenerator : MonoBehaviour {
	
	public GameObject note;

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Alpha1)) {
			Instantiate (note, new Vector3 (150.0f, 0, 0), Quaternion.identity);
			print ("here");
		}
		if (Input.GetKeyDown (KeyCode.Alpha2))
			Instantiate (note, new Vector3(155.0f, 0, 0), Quaternion.identity);
		if (Input.GetKeyDown (KeyCode.Alpha3))
			Instantiate (note, new Vector3(160.0f, 0, 0f), Quaternion.identity);
		if (Input.GetKeyDown (KeyCode.Alpha4))
			Instantiate (note, new Vector3(165.0f, 0, 0f), Quaternion.identity);		
		if (Input.GetKeyDown (KeyCode.Alpha5))
			Instantiate (note, new Vector3(170.0f, 0, 0f), Quaternion.identity);
		if (Input.GetKeyDown (KeyCode.Alpha6))
			Instantiate (note, new Vector3(175.0f, 0, 0f), Quaternion.identity);	
		if (Input.GetKeyDown (KeyCode.Alpha7))
			Instantiate (note, new Vector3(180.0f, 0, 0f), Quaternion.identity);	
	}
}
