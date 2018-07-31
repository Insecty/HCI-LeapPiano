using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

	public static GameController instance;

	public enum MODE {BASIC=0, SLOW=1, NORMAL=2, FAST=3};

	public GameObject TwinkleStarNotes;

	public GameObject ScollingSheet;

    public GameObject musicName;

    public GameObject Marker;

    public MODE CurrentState;

	public Transform bctransform;

	GameObject notes;

	GameObject scollingsheet;

    GameObject musicname;

    GameObject marker;

	// Use this for initialization
	void Start () {
		instance = this;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ChangeStateToBasic(){
		DestoryCurrent ();
		CurrentState = MODE.BASIC;
		GameObject parent = GameObject.FindGameObjectWithTag ("Parent");
		parent.GetComponent<BasicController> ().curNote = 0;
		parent.GetComponent<BasicController> ().StartColor ();
		scollingsheet = Instantiate (ScollingSheet, new Vector3((2.0f*28.40f/8.8197f)*(6.8021f/2.0f)-147.0f+1240.0f, 140.0f, 0), Quaternion.identity);
		scollingsheet.transform.SetParent (bctransform, false);
        musicname = Instantiate(musicName, new Vector3(10.7f,92.8f,-975.9f), Quaternion.identity);
        musicname.transform.SetParent(bctransform, false);
        marker = Instantiate(Marker, new Vector3(-392, -173, 0), Quaternion.identity);
        marker.transform.SetParent(bctransform, false);
        Debug.Log ("initial " + scollingsheet.transform.position);
	}

	public void ChangeStateToSlow(){
		CurrentState = MODE.SLOW;
		DestoryCurrent ();
		notes = Instantiate (TwinkleStarNotes, Vector3.zero, Quaternion.identity);
		scollingsheet = Instantiate (ScollingSheet, new Vector3((2.0f*28.40f/8.8197f)*(6.8021f/2.0f)-147.0f+1240.0f, 140.0f, 0), Quaternion.identity);
		scollingsheet.transform.SetParent (bctransform, false);
        musicname = Instantiate(musicName, new Vector3(10.7f, 92.8f, -975.9f), Quaternion.identity);
        musicname.transform.SetParent(bctransform, false);
        marker = Instantiate(Marker, new Vector3(-530, -173, 0), Quaternion.identity);
        marker.transform.SetParent(bctransform, false);
    }

	public void ChangeStateToNormal(){
		CurrentState = MODE.NORMAL;
		DestoryCurrent ();
		notes = Instantiate (TwinkleStarNotes, Vector3.zero, Quaternion.identity);
		scollingsheet = Instantiate (ScollingSheet, new Vector3((3.0f*28.40f/8.8197f)*(6.8021f/2.0f)-147.0f+1240.0f, 140.0f, 0), Quaternion.identity);
		scollingsheet.transform.SetParent (bctransform, false);
        musicname = Instantiate(musicName, new Vector3(10.7f, 92.8f, -975.9f), Quaternion.identity);
        musicname.transform.SetParent(bctransform, false);
        marker = Instantiate(Marker, new Vector3(-530, -173, 0), Quaternion.identity);
        marker.transform.SetParent(bctransform, false);
    }

	public void ChangeStateToFast(){
		CurrentState = MODE.FAST;
		DestoryCurrent ();
		notes = Instantiate (TwinkleStarNotes, Vector3.zero, Quaternion.identity);
		scollingsheet = Instantiate (ScollingSheet, new Vector3((5.0f*28.40f/8.8197f)*(6.8021f/2.0f)-147.0f+1240.0f, 140.0f, 0), Quaternion.identity);
		scollingsheet.transform.SetParent (bctransform, false);
        musicname = Instantiate(musicName, new Vector3(10.7f, 92.8f, -975.9f), Quaternion.identity);
        musicname.transform.SetParent(bctransform, false);
        marker = Instantiate(Marker, new Vector3(-530, -173, 0), Quaternion.identity);
        marker.transform.SetParent(bctransform, false);
    }

	void DestoryCurrent(){
		Destroy(notes);
		Destroy(scollingsheet);
        Destroy(musicname);
        Destroy(marker);
	}

	public void ScrollDist(){
		GameObject parent = GameObject.FindGameObjectWithTag ("Parent");
		int curNote = parent.GetComponent<BasicController> ().curNote;
        Vector3 pos = scollingsheet.transform.position;// localposition;
		pos.x -= parent.GetComponent<BasicController> ().scollDist [curNote];
		scollingsheet.transform.position = pos;
		Debug.Log ("pos " + pos);
		Debug.Log ("trans " + scollingsheet.transform.position);
		Debug.Log ("ScrollDist " + parent.GetComponent<BasicController> ().scollDist [curNote]);
	}
}
