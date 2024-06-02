using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DisplayUniqueKeybind : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI disp;

    // Start is called before the first frame update
    void OnEnable()
    {
        disp.text = UserInput.instance.GetInteractBinding();
    }
}
