using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PianoKeyPracticeScript : MonoBehaviour {

	public MusicTone MusicTone;
	public int Octave = 3;

	public float ResetPlayTime = 0.5f;
	private float currentResetPlayTime = 0;

	public float MaxZRotation = 6.0f;

	private Vector3 initialPosition;

	public float ZRotationResetVelocity = -6;

	public float MouseClickDownRotation = 600;
	public float MouseClickSmoothTime = 0.5f;
	private Vector3 smoothVelocity = Vector3.zero;
	public bool colorChange = false;
	public bool isHalf;
	private float smooth = 0f;

	void Start () {
		currentResetPlayTime = ResetPlayTime;
		initialPosition = transform.position;
	}
		
	void Update () 
	{
		if (GameController.instance.CurrentState == GameController.MODE.BASIC) {
			if (colorChange) {
				if (isHalf) {
					if (this.GetComponent<MeshRenderer> ().material.color != Color.black) {
						smooth += Time.deltaTime;
						this.GetComponent<MeshRenderer> ().material.color = Color.Lerp (Color.red, Color.black, smooth);

					} else {
						colorChange = false;
						smooth = 0;

					}	
				} else {
					if (this.GetComponent<MeshRenderer> ().material.color != Color.white) {
						smooth += Time.deltaTime;
						this.GetComponent<MeshRenderer> ().material.color = Color.Lerp (Color.red, Color.white, smooth);

					} else {
						colorChange = false;
						smooth = 0;
					}	
				}
			}
		}

		if (transform.rotation.eulerAngles.z > 0.1f)
		{
			transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, 0, Time.deltaTime * ZRotationResetVelocity));
			transform.GetComponent<Rigidbody>().angularDrag = 0;
			transform.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
		}

		if (transform.rotation.eulerAngles.z > MaxZRotation)
		{
			transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, MaxZRotation);
			transform.GetComponent<Rigidbody>().angularDrag = 0;
			transform.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
		}

		if (transform.rotation.eulerAngles.z < 0)
		{
			transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
			transform.GetComponent<Rigidbody>().angularDrag = 0;
			transform.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
		}

		if (currentResetPlayTime < ResetPlayTime)
			currentResetPlayTime += Time.deltaTime;

		transform.position = initialPosition;
		transform.GetComponent<Rigidbody>().velocity = Vector3.zero;              
	}

	public void OnMouseDown()
	{
		if (currentResetPlayTime >= ResetPlayTime){
			currentResetPlayTime = 0.0f;

			PlayTone ();

			if (GameController.instance.CurrentState == GameController.MODE.BASIC) {
				GameObject parent = GameObject.FindGameObjectWithTag ("Parent");
				int curNote = parent.GetComponent<BasicController> ().curNote;
				GameObject curKey = parent.GetComponent<BasicController> ().note2key [curNote];
				if (curKey == this.gameObject) {
					parent.GetComponent<BasicController> ().Next ();
					Debug.Log ("Next Key");
					GameController.instance.ScrollDist();

				} else {
					this.GetComponent<MeshRenderer> ().material.color = Color.red;
					colorChange = true;
				}
			}
				
			transform.rotation = Quaternion.Euler(Vector3.SmoothDamp(transform.rotation.eulerAngles,
				transform.rotation.eulerAngles + new Vector3(0, 0, MouseClickDownRotation), ref smoothVelocity, MouseClickSmoothTime));
		}
	}

	void ResetColor(){
		if (isHalf) {
			this.GetComponent<MeshRenderer> ().material.color = Color.black;
		} else {
			this.GetComponent<MeshRenderer> ().material.color = Color.white;
		}
	}

	public void PlayTone(){
		TonePlayer.Instance.PlayTone(MusicTone, Octave);
	}

	void OnCollisionEnter(Collision collision)
	{
		Debug.Log("Collision of me, " + gameObject.name + " with " + collision.gameObject.name);

		Debug.Log("cbmaxy: " + GetComponent<Collider>().bounds.max.y + " c.trans.pos.y " + collision.transform.position.y);

		if(GetComponent<Collider>().bounds.max.x > collision.contacts[0].point.x && GetComponent<Collider>().bounds.min.x < collision.contacts[0].point.x
			&& GetComponent<Collider>().bounds.min.y<collision.contacts[0].point.y)
		{
			if (currentResetPlayTime >= ResetPlayTime)
			{
				currentResetPlayTime = 0.0f;

				TonePlayer.Instance.PlayTone(MusicTone, Octave);
			}
		}
	}
}
