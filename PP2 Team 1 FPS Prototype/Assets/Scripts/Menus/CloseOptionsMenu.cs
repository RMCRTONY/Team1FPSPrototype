using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CloseOptionsMenu : MonoBehaviour
{
    public Button cancelButton;

    private void Awake()
    {
        cancelButton.onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        gameManager.instance.closeOptionsMenu();
    }
}
