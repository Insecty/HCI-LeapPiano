using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

public class musicWindow : Window {

	public musicWindow(GMovieClip movie) { amovie = movie; }
    private int currentSong = 0;  // 当前选中的歌
    private GList list;
    private GMovieClip amovie;

    protected override void OnInit()
    {
        this.contentPane = UIPackage.CreateObject("practice", "musicwindow").asCom;
        contentPane.GetChild("confirmbt").onClick.Add(() => { playMusic(); });
        list = contentPane.GetChild("n2").asList;
        list.SetVirtualAndLoop();
        list.itemRenderer = RenderListItem;
        list.numItems = 3;
        list.scrollPane.onScroll.Add(DoSpecialEffect);
        DoSpecialEffect();
    }

    private void playMusic()
    {
        // 根据currentSong切换乐谱  1是小星星 其他的还没有 所以就都显示小星星也可以
        GameController.instance.ChangeStateToBasic();

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
                    if (bt.icon == UIPackage.GetItemURL("practice", "music" + (j + 1)))
                    {
                        currentSong = j;
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
        button.icon = UIPackage.GetItemURL("practice", "music" + (index + 1));
    }


}
