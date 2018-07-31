using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;
using UnityEngine.SceneManagement;
using RenderHeads.Media.AVProMovieCapture;

public class free : MonoBehaviour {

    private GComponent mainUI;
    private effectWindow effectwin;

	void Start () {
        mainUI = GetComponent<UIPanel>().ui;
        effectwin = new effectWindow();
        effectwin.SetXY(Screen.width / 2 - 500, Screen.height / 4);
        mainUI.GetChild("backbt").onClick.Add(() => { SceneManager.LoadScene("select"); });
        mainUI.GetChild("effectbt").onClick.Add(() => { effectwin.Show(); });
        mainUI.GetChild("recordbt").onClick.Add(() => { GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CaptureFromCamera>().enabled = true; });
        
	}
}
