using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

public class quitWindow : Window {

	public quitWindow() { }

    protected override void OnInit()
    {
        this.contentPane = UIPackage.CreateObject("Package1", "quitwindow").asCom;
        contentPane.GetChild("quitbt").onClick.Add(() => { Application.Quit(); });
    }
}
