using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[RequireComponent(typeof(Image))]
public class TabButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] TabGroup tabGroup;

    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip bttnClick;
    [Range(0, 1)]
    [SerializeField] float audVol;

    public Image background;

    public UnityEvent onTabSelected;
    public UnityEvent onTabDeselected;

    private void Start()
    {
        background = GetComponent<Image>();
        tabGroup.AddButton(this);
    }

    public void Select()
    {
        onTabSelected?.Invoke();
    }

    public void Deselect()
    {
        onTabDeselected?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tabGroup.OnTabEnter(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tabGroup.OnTabExit(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        tabGroup.OnTabSelected(this);
        aud.PlayOneShot(bttnClick, audVol);
    }
}
