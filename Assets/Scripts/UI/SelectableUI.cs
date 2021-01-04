using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
///  Represents a UI object that can be selected by UIMaster.
/// </summary>
/// <remarks>
///  OnPointerEnter/Exit/Click are used by this class, use the virtual methods PointerEnter, PointerExit and PointerClick provided instead.
/// </remarks>
public abstract class SelectableUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

    public bool MouseOver {
        get; private set;
    } = false;
    public bool Hovered {
        get; private set;
    }
    public bool Selected {
        get; private set;
    } = false;

    public void OnPointerEnter (PointerEventData eventData) {
        PointerEnter(eventData);
        MouseOver = true;
        UIMaster.i.HoverComponent(this);
    }

    public virtual void PointerEnter (PointerEventData eventData) {}

    public void Hover () {
        Hovered = true;
        OnHover();
    }

    protected virtual void OnHover () {}

    public void OnPointerExit (PointerEventData eventData) {
        PointerExit(eventData);
        MouseOver = false;
        UIMaster.i.UnhoverComponent(this);
    }

    public virtual void PointerExit (PointerEventData eventData) {}

    public void Unhover () {
        Hovered = false;
        OnUnhover();
    }
    protected virtual void OnUnhover () {}

    public void OnPointerClick (PointerEventData eventData) {
        PointerClick(eventData);
        UIMaster.i.SelectUI(this);
    }

    public void Select () {
        Selected = true;
        OnSelect();
    }

    public void Unselect () {
        Selected = false;
        OnUnselect();
    }

    public virtual void PointerClick (PointerEventData eventData) {}
    protected virtual void OnSelect () {}
    protected virtual void OnUnselect () {}

}