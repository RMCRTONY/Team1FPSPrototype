using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabGroup : MonoBehaviour
{
    public List<TabButton> tabButtons; // all the tabs we got
    public Color tabIdle;
    public Color tabHover;
    public Color tabActive;
    public TabButton activeTab;
    public List<GameObject> tabContents;
   
    public void AddButton(TabButton button) // add the buttons to the group
    {
        if (tabButtons == null)
        {
            tabButtons = new List<TabButton>();
        }

        tabButtons.Add(button);
    }

    // methods below exist to provide feedback in the menus
    public void OnTabEnter(TabButton button)
    {
        ResetTabs();
        if (activeTab == null || button != activeTab)
        {
            button.background.color = tabHover;
        }
    }

    public void OnTabExit(TabButton button)
    {
        ResetTabs();
    }

    public void OnTabSelected(TabButton button) // reveal tab contents
    {
        if (activeTab != null)
        {
            activeTab.Deselect();
        }

        activeTab = button;

        activeTab.Select();

        ResetTabs();
        button.background.color = tabActive;
        int index = button.transform.GetSiblingIndex();
        for(int i = 0; i < tabContents.Count; i++)
        {
            if (index == i)
            {
                tabContents[i].SetActive(true);
            }
            else
            {
                tabContents[i].SetActive(false);
            }
        }
    }

    public void ResetTabs()
    {
        foreach(TabButton b in tabButtons)
        {
            if (activeTab != null && b == activeTab) { continue; } // won't reset the tab if it is active
            b.background.color = tabIdle;
        }
    }
}
