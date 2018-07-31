using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteController : MonoBehaviour {
	
	Rigidbody rb;
	
	public float speed;

	// Use this for initialization
	void Awake () {
		switch(GameController.instance.CurrentState)
		{
		case GameController.MODE.SLOW:
			speed = 2.0f;
			break;
		case GameController.MODE.NORMAL:
			speed = 3.0f;
			break;
		case GameController.MODE.FAST:
			speed = 5.0f;
			break;
		}

		rb = GetComponent<Rigidbody> ();
		rb.velocity = new Vector3 (0, 0, -speed);
	}

}
