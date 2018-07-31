using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;
using UnityEngine.SceneManagement;

public class about : MonoBehaviour {

    private GComponent mainUI;

	// Use this for initialization
	void Start () {
        mainUI = GetComponent<UIPanel>().ui;
        mainUI.GetChild("returnbt").onClick.Add(() => { SceneManager.LoadScene("init"); });
    }
}
