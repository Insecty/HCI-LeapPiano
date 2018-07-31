using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicController : MonoBehaviour {

	public GameObject[] note2key;
	public float[] scollDist;
	public int curNote;
	void Start(){
		
	}
	void Update () {
		
	}

	public void Next(){
		ReturnColor ();
		if (curNote == 13)
        {
            curNote = 13;
            // restart
            GameObject.FindGameObjectWithTag("restart").GetComponent<practice>().showRestart();
        }
        else
        {
            curNote = curNote + 1;
        }
		CurColor ();
	}

	public void CurColor(){
		note2key [curNote].GetComponent<MeshRenderer> ().material.color = Color.blue;
	}

	public void ReturnColor(){
		note2key [curNote].GetComponent<MeshRenderer> ().material.color = Color.white;
	}

	public void StartColor(){
		if (GameController.instance.CurrentState == GameController.MODE.BASIC) {
			curNote = 0;
			CurColor ();
		}
	}

}
