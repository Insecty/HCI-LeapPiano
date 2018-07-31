using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;
using UnityEngine.SceneManagement;

public class init : MonoBehaviour {

    private GComponent mainUI;
    private quitWindow quitwindow;

	void Start () {
        mainUI = GetComponent<UIPanel>().ui;
        quitwindow = new quitWindow();
        quitwindow.SetXY(Screen.width / 2 - 305, Screen.height / 3);

        mainUI.GetChild("startbt").onClick.Add(() => { SceneManager.LoadScene("select"); });
        mainUI.GetChild("quitbt").onClick.Add(() => { quitwindow.Show(); });
        mainUI.GetChild("aboutbt").onClick.Add(() => { SceneManager.LoadScene("about"); });
	}
}
