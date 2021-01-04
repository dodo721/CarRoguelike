using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIMaster : MonoBehaviour
{

    public Canvas screenCanvas;
    public Canvas worldCanvas;

    public Transform iconSpace;
    public RectTransform focusLock;
    public WorldBoundIcon charIconPrefab;
    public Dialogue dialoguePrefab;

    public static UIMaster i;

    private List<WorldBoundIcon> worldBoundIcons;
    private List<int> worldClickableLayers;
    public HashSet<SelectableUI> hoveredUI;
    public SelectableUI selectedUI;
    
    void Awake () {
        if (i == null)
            i = this;
        else {
            Debug.LogWarning("There should only be one UIMaster instance in the scene!\nTried to add " + this + ", but found " + i + " instead");
            return;
        }
        hoveredUI = new HashSet<SelectableUI>();
        worldBoundIcons = new List<WorldBoundIcon>();
        worldClickableLayers = new List<int>();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    /*
    void InitDialogue (CharacterStates character) {
        Dialogue dialogue = Instantiate(dialoguePrefab.gameObject, transform.position, dialoguePrefab.transform.rotation, worldCanvas.transform).GetComponent<Dialogue>();
        dialogue.canvas = worldCanvas.GetComponent<RectTransform>();
        dialogue.follow = character.transform;
        dialogue.gameObject.SetActive(false);
    }*/

    // Update is called once per frame
    void Update()
    {
        bool foundWorldClickable = RaycastForWorldObjectClickables();
        if (Input.GetMouseButtonDown(0) && hoveredUI.Count == 0 && !foundWorldClickable) {
            SelectUI(null);
        }
    }

    bool RaycastForWorldObjectClickables () {
        HashSet<WorldClickable> clickables = new HashSet<WorldClickable>();

        if (!EventSystem.current.IsPointerOverGameObject()) {
            Ray screenRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            foreach (int layer in worldClickableLayers) {
                RaycastHit hit;
                int layerMask = 1 << layer;
                if (Physics.Raycast(screenRay, out hit, Mathf.Infinity, layerMask)) {
                    WorldClickable clickable = hit.collider.GetComponent<WorldClickable>();
                    if (clickable != null) {
                        if (Input.GetMouseButtonDown(0) && selectedUI != clickable.partner)
                            SelectUI(clickable.partner);
                        else {
                            WorldHoverComponent(clickable.partner);
                        }
                        clickables.Add(clickable);
                    }
                }
            }
        }

        foreach (WorldBoundIcon icon in worldBoundIcons) {
            if (!clickables.Contains(icon.partner) && icon.WorldHovered && !icon.MouseOver) {
                WorldUnhoverComponent(icon);
            }
        }
        
        return clickables.Count > 0;
    }

    public void SelectUI (SelectableUI ui) {
        if (selectedUI == ui)
            return;
        if (selectedUI != null) {
            selectedUI.Unselect();
        }
        selectedUI = ui;
        if (selectedUI != null) {
            selectedUI.transform.SetAsLastSibling();
            selectedUI.Select();
        }
    }

    public void HoverComponent (SelectableUI component) {
        if (!hoveredUI.Contains(component)) {
            hoveredUI.Add(component);
            component.Hover();
        }
    }

    public void WorldHoverComponent (WorldBoundIcon component) {
        if (!hoveredUI.Contains(component)) {
            hoveredUI.Add(component);
            component.OnWorldHover();
        }
    }

    public void UnhoverComponent (SelectableUI component) {
        hoveredUI.Remove(component);
        component.Unhover();
    }

    public void WorldUnhoverComponent (WorldBoundIcon component) {
        hoveredUI.Remove(component);
        component.OnWorldUnhover();
    }

    public void BringComponentToFront (SelectableUI component) {
        if (selectedUI == component)
            return;
        else if (selectedUI == null)
            component.transform.SetAsLastSibling();
        else {
            int siblingCount = component.transform.parent.childCount;
            // Position as the second last UI element, behind the selected component
            component.transform.SetSiblingIndex(siblingCount - 2);
        }
    }

    public void RegisterWorldBoundIcon (WorldBoundIcon icon) {
        int layer = icon.partner.gameObject.layer;
        if (!worldClickableLayers.Contains(layer)) {
            worldClickableLayers.Add(layer);
            worldClickableLayers.Sort();
        }
        worldBoundIcons.Add(icon);
    }

    public override string ToString () {
        return "UIMaster (" + name + ")";
    }
}
