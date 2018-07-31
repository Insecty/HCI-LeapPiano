using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;
using UnityEngine.SceneManagement;

public class select : MonoBehaviour {

    private GComponent mainUI;
    private GList list;
    private int currentMode = 0;

	void Start () {
        mainUI = GetComponent<UIPanel>().ui;
        mainUI.GetChild("backbt").onClick.Add(() => { SceneManager.LoadScene("init"); });
        // 可以根据currentMode在loadscene之后改变场景或者load不同的scene
        mainUI.GetChild("freebt").onClick.Add(() => { SceneManager.LoadScene(3); });
        mainUI.GetChild("teachbt").onClick.Add(() => { SceneManager.LoadScene("practice"); });

        list = mainUI.GetChild("n23").asList;
        list.SetVirtualAndLoop();
        list.itemRenderer = RenderListItem;
        list.numItems = 3;
        list.scrollPane.onScroll.Add(DoSpecialEffect);
        DoSpecialEffect();
    }

	private void DoSpecialEffect()
    {
        float listCenter = list.scrollPane.posX + list.viewWidth/2;

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
                for(int j = 0; j < 3; j++)
                {
                    if (bt.icon == UIPackage.GetItemURL("Package1", "bt" + (j + 1)))
                    {
                        currentMode = j;
                        if(currentMode == 0)
                        {
                            mainUI.GetChild("freebt").onClick.Add(() => { SceneManager.LoadScene(3); });
                        }
                        else if(currentMode == 1)
                        {
                            mainUI.GetChild("freebt").onClick.Add(() => { SceneManager.LoadScene(4); });
                        }
                        else if(currentMode == 2)
                        {
                            mainUI.GetChild("freebt").onClick.Add(() => { SceneManager.LoadScene(5); });
                        }
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
        button.icon = UIPackage.GetItemURL("Package1", "bt" + (index + 1));
    }
}
