using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivatorController : MonoBehaviour {
	
	bool active = false;
	
	public float RenderBackRate;
	
	float RenderBackTime;

	bool MaterialChanged = false;
	
	GameObject note;
	
	public GameObject Key;

	MeshRenderer Keymr;
	
	void Start(){
		Keymr = Key.GetComponent<MeshRenderer> ();
	}

	void Update(){
		if (active) {
			Keymr.material.color = Color.blue;
			RenderBackTime = Time.time + RenderBackRate;
//			Key.GetComponent<PianoKeyScript> ().PlayTone ();
			MaterialChanged = true;
			active = false;
		}
		
		if (MaterialChanged && Time.time > RenderBackTime) {
			Keymr.material.color = Color.white;
			MaterialChanged = false;
		}
		
	}
	
	void OnTriggerEnter(Collider other){
		active = true;
	}
	
	void OnTriggerExit(Collider other){
		Destroy (other.gameObject);
	}
}