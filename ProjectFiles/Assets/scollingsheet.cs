using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scollingsheet : MonoBehaviour {

	public float speed;

	// Use this for initialization
	void Start () {
		Rigidbody2D rb;
		switch(GameController.instance.CurrentState)
		{
		case GameController.MODE.SLOW:
			speed = 2.0f*28.40f/8.8197f;
			rb = GetComponent<Rigidbody2D> ();
			rb.velocity = new Vector3 (-speed, 0, 0);
			break;
		case GameController.MODE.NORMAL:
			speed = 3.0f*28.40f/8.8197f;
			rb = GetComponent<Rigidbody2D> ();
			rb.velocity = new Vector3 (-speed, 0, 0);
			break;
		case GameController.MODE.FAST:
			speed = 5.0f*28.40f/8.8197f;
			rb = GetComponent<Rigidbody2D> ();
			rb.velocity = new Vector3 (-speed, 0, 0);
			break;
		}
	}

	void Update () {
		
	}

	/*public void ScrollDist(){
		GameObject parent = GameObject.FindGameObjectWithTag ("Parent");
		int curNote = parent.GetComponent<BasicController> ().curNote;
		Vector3 pos = transform.position;
		pos.x -= parent.GetComponent<BasicController> ().scollDist [curNote];
		transform.position = pos;
		Debug.Log ("pos " + pos);
		Debug.Log ("trans " + transform.position);
		Debug.Log ("ScrollDist " + parent.GetComponent<BasicController> ().scollDist [curNote]);
	}*/
}
