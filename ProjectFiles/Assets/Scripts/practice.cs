using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;
using UnityEngine.SceneManagement;

public class practice : MonoBehaviour {

    private GComponent mainUI;
    private musicWindow musicwin;
    private modeWindow modewin;
    private GMovieClip amovie;
    private restartWindow restartwin;

    void Start () {
        mainUI = GetComponent<UIPanel>().ui;
        mainUI.GetChild("bactbt").onClick.Add(() => { SceneManager.LoadScene("select"); });
        amovie = mainUI.GetChild("ready").asMovieClip;
        //amovie.SetPlaySettings(0,5,1,-1);
        amovie.visible = false;

        // music window
        musicwin = new musicWindow(amovie);
        musicwin.SetXY(Screen.width / 2 - 500, Screen.height / 10);
        mainUI.GetChild("musicbt").onClick.Add(() => { musicwin.Show(); });

        // mode window
        modewin = new modeWindow(amovie);
        modewin.SetXY(Screen.width / 2 - 500, Screen.height / 10);

        restartwin = new restartWindow();
        restartwin.SetXY(Screen.width / 2 - 500, Screen.height / 10);

        musicwin.Show();
        mainUI.GetChild("modebt").onClick.Add(() => { modewin.Show(); });
	}

    public void showRestart()
    {
        restartwin.Show();
    }

    public void showMode()
    {
        modewin.Show();
;    }
}
