using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

public class recordWindow : Window {
    public recordWindow() { }

    protected override void OnInit()
    {
        this.contentPane = UIPackage.CreateObject("mode", "replaywindow").asCom;
    }

}
