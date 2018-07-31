using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

public class modeWindow : Window {

    public modeWindow(GMovieClip movie) { amovie = movie; }
    private int currentMode = 0; // 当前用户模式 0是等待模式 1是正常速度 2快速 3慢速 123都是乐谱滑动
    private GList list;
    private GMovieClip amovie;

    protected override void OnInit()
    {
        this.contentPane = UIPackage.CreateObject("practice", "modelview").asCom;
        contentPane.GetChild("confirmbt").onClick.Add(() => { changeMode(); });
        list = contentPane.GetChild("n5").asList;
        list.SetVirtualAndLoop();
        list.itemRenderer = RenderListItem;
        list.numItems = 4;
        list.scrollPane.onScroll.Add(DoSpecialEffect);
        DoSpecialEffect();
    }

    private void changeMode()
    {
        // 根据currentMode切换模式
        switch (currentMode)
        {
            case 0:
                GameController.instance.ChangeStateToBasic();
                break;
            case 1:
                GameController.instance.ChangeStateToSlow();
                break;
            case 2:
                GameController.instance.ChangeStateToNormal();
                break;
            case 3:
                GameController.instance.ChangeStateToFast();
                break;
        }
        this.Hide();
        // show gif

        amovie.visible = true;
        amovie.SetPlaySettings(0, 6, 1, -1);
    }

    private void DoSpecialEffect()
    {
        float listCenter = list.scrollPane.posX + list.viewWidth / 2;

        for (int i = 0; i < list.numChildren; i++)
        {
            GObject item = list.GetChildAt(i);
            float itemCenter = item.x + item.width / 2;
            float itemWidth = item.width;
            float distance = Mathf.Abs(listCenter - itemCenter);
            if (distance < itemWidth / 2)
            {
                item.SetScale(1, 1);
                GButton bt = item.asButton;
                for (int j = 0; j < 3; j++)
                {
                    if (bt.icon == UIPackage.GetItemURL("practice", "mode" + (j + 1)))
                    {
                        currentMode = j;
                    }
                }
            }
            else
            {
                item.SetScale(0.8f, 0.8f);
            }
        }
    }

    private void RenderListItem(int index, GObject obj)
    {
        GButton button = obj.asButton;
        button.SetPivot(0.5f, 0.5f);
        button.icon = UIPackage.GetItemURL("practice", "mode" + (index + 1));
    }
}
