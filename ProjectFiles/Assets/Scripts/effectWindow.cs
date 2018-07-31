using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;
using Object = UnityEngine.Object;

public class effectWindow : Window {

	public effectWindow() { }
    private int currentEffect = 0;
    private GList list;
    private GameObject[] objs;

    protected override void OnInit()
    {
        this.contentPane = UIPackage.CreateObject("mode", "selecteffect").asCom;
        contentPane.GetChild("confirmbt").onClick.Add(() => { changeEffect(); });
        list = contentPane.GetChild("n2").asList;
        list.SetVirtualAndLoop();
        list.itemRenderer = RenderListItem;
        list.numItems = 3;
        list.scrollPane.onScroll.Add(DoSpecialEffect);
        DoSpecialEffect();
    }

    private void changeEffect()
    {
        objs = GameObject.FindGameObjectsWithTag("pianokey");
        switch (currentEffect)
        {
            case 0:
                foreach(GameObject obj in objs)
                {
                    obj.GetComponent<SpawnPrefabOnKeyPlay>().changeEffect(0);
                }
                break;
            case 1:
                foreach (GameObject obj in objs)
                {
                    obj.GetComponent<SpawnPrefabOnKeyPlay>().changeEffect(1);
                }
                break;
            case 2:
                foreach (GameObject obj in objs)
                {
                    obj.GetComponent<SpawnPrefabOnKeyPlay>().changeEffect(2);
                }
                break;
        }

        this.Hide();
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
                    if (bt.icon == UIPackage.GetItemURL("mode", "effect" + (j + 1)))
                    {
                        currentEffect = j;
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
        button.icon = UIPackage.GetItemURL("mode", "effect" + (index + 1));
    }


}
