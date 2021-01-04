using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
///  Represents a UI object paired to a world-space object.
/// </summary>
/// <remarks>
///  OnPointerEnter/Exit are used by this class, use the virtual methods PointerEnter and PointerExit provided instead.
/// </remarks>
[RequireComponent(typeof(RectTransform))]
public abstract class WorldBoundIcon : SelectableUI {

    [HideInInspector]
    public WorldClickable partner;
    private RectTransform rectTransform;
    public bool follow = false;
    public Vector3 followOffset;
    public bool followSmoothly;
    public float followSmoothness;
    public enum FocusMotion {
        STATIC, SMOOTH, SMOOTH_TIMED
    }
    public FocusMotion focusMotion;
    public float focusSmoothTime;
    private float focusStartTime;
    public bool focusable = false;

    [HideInInspector]
    public bool focused = false;
    public float scaleOnHover = 1;
    public float scaleOnSelect = 1;
    private Vector3 scaleNormal;
    private Vector3 scaleHover;
    private Vector3 scaleSelect;
    public Canvas renderTo;

    public bool WorldHovered {
        get; private set;
    }

    public bool IsHovered {
        get {
            return WorldHovered || Hovered;
        }
    }

    public virtual void Start () {
        Init();
    }

    public void Init () {
        rectTransform = GetComponent<RectTransform>();
        scaleNormal = rectTransform.localScale;
        scaleHover = scaleNormal * scaleOnHover;
        scaleSelect = scaleNormal * scaleOnSelect;
        transform.localRotation = Quaternion.identity;
        transform.localPosition = Vector3.zero;
    }

    public void BindToWorldObject (GameObject worldObject) {
        partner = worldObject.GetComponent<WorldClickable>();
        if (partner == null)
            partner = worldObject.AddComponent<WorldClickable>();
        partner.partner = this;
        renderTo = partner.renderTo;
        UIMaster.i.RegisterWorldBoundIcon(this);
        OnBind();
    }

    protected virtual void OnBind () {}

    void MoveToFocusLock () {
        Vector3 focusLockPos = UIMaster.i.focusLock.position;
        if (focusMotion == FocusMotion.SMOOTH) {
            rectTransform.position = Vector3.Lerp(rectTransform.position, focusLockPos, followSmoothness * Time.deltaTime);
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, scaleSelect, followSmoothness * Time.deltaTime);
        } else if (focusMotion == FocusMotion.STATIC) {
            rectTransform.position = UIMaster.i.focusLock.position;
            rectTransform.localScale = scaleSelect;
        } else if (focusMotion == FocusMotion.SMOOTH_TIMED) {
            float fac = (Time.time - focusStartTime) / focusSmoothTime;
            rectTransform.position = Vector3.Lerp(rectTransform.position, focusLockPos, fac);
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, scaleSelect, fac);
        }
    }

    void MoveFromFocusLock (Vector3 targetPos) {
        Vector3 focusLockPos = UIMaster.i.focusLock.position;
        if (focusMotion == FocusMotion.SMOOTH_TIMED) {
            float fac = (Time.time - focusStartTime) / focusSmoothTime;
            rectTransform.position = Vector3.Lerp(rectTransform.position, targetPos, fac);
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, scaleNormal, fac);
        }
    }

    public virtual void LateUpdate () {
        if (Selected && focusable) {
            MoveToFocusLock();
            focused = true;
        } else {
            focused = false;
        }
        if (!focused && partner != null) {
            Vector3 target = rectTransform.position;
            if (followSmoothly)
                target = Vector3.Lerp(rectTransform.position, GetFollowPosition(), followSmoothness * Time.deltaTime);
            else
                target = GetFollowPosition();
            
            if (focusMotion == FocusMotion.SMOOTH_TIMED && (Time.time - focusStartTime <= focusSmoothTime))
                MoveFromFocusLock(target);
            else
                rectTransform.position = target;
        }
    }

    public Vector3 GetFollowPosition () {
        Camera cam = renderTo.worldCamera == null ? Camera.main : renderTo.worldCamera;
        Vector3 target = rectTransform.position;
        if (renderTo.renderMode == RenderMode.ScreenSpaceCamera) {
            Vector3 sp = cam.WorldToScreenPoint(partner.transform.position + followOffset);
            Vector3 worldPoint;
            RectTransformUtility.ScreenPointToWorldPointInRectangle((RectTransform)rectTransform.parent, sp, cam, out worldPoint);
            target = worldPoint;
        } else {
            target = cam.WorldToScreenPoint(partner.transform.position + followOffset);
            target.z = 0;
        }
        return target;
    }

    public void OnWorldHover () {
        WorldHovered = true;
        if (!Selected && !Hovered) {
            rectTransform.localScale = scaleHover;
            UIMaster.i.BringComponentToFront(this);
            OnAnyHover();
        }
    }

    public void OnWorldUnhover () {
        WorldHovered = false;
        if (!Selected) {
            rectTransform.localScale = scaleNormal;
            UIMaster.i.BringComponentToFront(this);
            OnAnyUnhover();
        }
    }

    protected override void OnHover () {
        if (!Selected && !WorldHovered) {
            rectTransform.localScale = scaleHover;
            UIMaster.i.BringComponentToFront(this);
            OnAnyHover();
        }
    }

    protected override void OnUnhover () {
        if (!Selected) {
            rectTransform.localScale = scaleNormal;
            UIMaster.i.BringComponentToFront(this);
            OnAnyUnhover();
        }
    }

    public virtual void OnAnyHover () {}

    public virtual void OnAnyUnhover () {}

    protected override void OnSelect () {
        if (focusable && focusMotion == FocusMotion.SMOOTH_TIMED) {
            focusStartTime = Time.time;
        }
    }

    protected override void OnUnselect () {
        if (focusable && focusMotion == FocusMotion.SMOOTH_TIMED) {
            focusStartTime = Time.time;
        } else
            rectTransform.localScale = scaleNormal;
    }

}