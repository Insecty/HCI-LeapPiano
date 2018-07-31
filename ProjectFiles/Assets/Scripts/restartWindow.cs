using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

public class restartWindow : Window {

    public restartWindow() { }

    protected override void OnInit()
    {
        this.contentPane = UIPackage.CreateObject("practice", "restartwindow").asCom;
        contentPane.GetChild("restartbt").onClick.Add(() => { restart(); });
    }

    void restart()
    {// restart after practice one song
        GameObject.FindGameObjectWithTag("restart").GetComponent<practice>().showMode();
        this.Hide();
    }
}
