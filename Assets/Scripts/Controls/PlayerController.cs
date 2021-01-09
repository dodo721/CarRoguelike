using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CameraFollower))]
public class PlayerController : MonoBehaviour
{
    [Serializable]
    public struct ControllerAction {
        public UnityEvent hold;
        public UnityEvent down;
        public UnityEvent up;
    }
    private Dictionary<string, ControllerAction> actionRegister;
    
    // Serializable version
    [Serializable]
    private struct ControllerActionSerialized {
        public ControllerAction actions;
        public string input;
    }

    [SerializeField]
    private List<ControllerActionSerialized> actionRegisterSerialized = new List<ControllerActionSerialized>();

    public struct ControllerActionContext {
        public GameObject hovered;
        public PlayerControllable actor;
        public CharacterController controller;
        public float inputValue;
    };

    public ControllerActionContext context;
    public bool overrideDefaultControls = false;

    public string[] Inputs {
        get {
            string[] inputs = new string[actionRegisterSerialized.Count];
            for (int i = 0; i < actionRegisterSerialized.Count; i++) {
                inputs[i] = actionRegisterSerialized[i].input;
            }
            return inputs;
        }
    }

    public PlayerControllable controlling;
    public Transform cameraTransform;
    private CameraFollower cameraFollower;
    public float speed;
    private CharacterController controller;
    public static PlayerController i;
    private Rigidbody draggingObject;
    private GameObject hovered;
    private GameObject selected;
    public Vector3 direction;

    // RAYCAST LAYERMASK CONSTANTS

    ///<summary>Masks the layers "Ignore Raycast" and "Invisible Walls"</summary>
    public static readonly int LAYER_MASK_DEFAULT_IGNORES = ~((1 << 2) | (1 << 10));

    ///<summary>Masks the layer "Invisible Walls"</summary>
    public static readonly int LAYER_MASK_IGNORE_WALLS_ONLY = ~(1 << 10);

    ///<summary>Masks the layer "Ignore Raycast"</summary>
    public static readonly int LAYER_MASK_IGNORE_OBJECTS_ONLY = ~(1 << 2);

    ///<summary>Masks the layers "Player", "Ignore Raycast" and "Invisible Walls"</summary>
    public static readonly int LAYER_MASK_IGNORE_PLAYER = ~((1 << 2) | (1 << 10) | (1 << 3));

    ///<summary>Masks the layer "Player"</summary>
    public static readonly int LAYER_MASK_IGNORE_PLAYER_ONLY = ~(1 << 3);

    ///<summary>Masks the layers "Player" and "Invisible Walls"</summary>
    public static readonly int LAYER_MASK_IGNORE_PLAYER_AND_WALLS_ONLY = ~((1 << 3) | (1 << 10));

    ///<summary>Masks the layers "Player" and "Ingnore Raycasts"</summary>
    public static readonly int LAYER_MASK_IGNORE_PLAYER_AND_OBJECTS_ONLY = ~((1 << 3) | (1 << 2));

    ///<summary>Masks no layers</summary>
    public static readonly int LAYER_MASK_ALL = ~0;

    // Setup singleton
    void Awake () {
        if (i == null) i = this;
        else Debug.LogError("There should only be 1 PlayerController in the scene!");
        // Convert action register to dictionary for performance
        actionRegister = new Dictionary<string, ControllerAction>();
        foreach (ControllerActionSerialized cas in actionRegisterSerialized) {
            actionRegister.Add(cas.input, cas.actions);
        }
    }

    void Start () {
        cameraFollower = GetComponent<CameraFollower>();
    }

    public Vector3 getDirectionToMouse() {
        Vector2 mousePos = new Vector2((Input.mousePosition.x / Screen.width) - 0.5f, (Input.mousePosition.y / Screen.height) - 0.5f);
        Vector3 translationWorldSpace = new Vector3(mousePos.x, 0, mousePos.y);
        Vector3 translationCameraSpace = PlayerController.i.cameraTransform.TransformDirection(translationWorldSpace);
        return translationCameraSpace.normalized;
    }

    // Update is called once per frame
    void Update()
    {

        List<PlayerControllable> allTheBees = FindObjectsOfType<PlayerControllable>().ToList();
        
        if (controlling == null) {
            if (allTheBees.Count > 0) {
                int index = allTheBees.IndexOf(controlling);
                index = (allTheBees.Count + index - 1) % allTheBees.Count;
                SetControllable(allTheBees[index]);
            } else {
                // TODO DEATH
                return;
            }
        }

        if (controlling != null && controller == null) {
            controller = controlling.GetComponent<CharacterController>();
        }

        // Build the context for an action
        context.hovered = GetHoveredGameObject();
        context.actor = controlling;
        context.controller = controller;

        // Perform controller actions
        foreach (string input in actionRegister.Keys) {
            // Add individual input values to context
            float inputVal = Input.GetAxisRaw(input);
            context.inputValue = inputVal;
            // Is button being held? Prepare the action
            if (Mathf.Abs(inputVal) > 0) {
                actionRegister[input].hold.Invoke();
            }
            // Was the button pushed down? Trigger immediate behaviour
            if (Input.GetButtonDown(input)) {
                actionRegister[input].down.Invoke();
            }
            // Is the button released? Finish the action
            if (Input.GetButtonUp(input)) {
                actionRegister[input].up.Invoke();
            }
        }

        if (!overrideDefaultControls) {

            if (controlling != null) {

                direction = new Vector3();

                if (Input.GetKey(KeyCode.W)) {
                    direction += Vector3.forward;
                }
                if (Input.GetKey(KeyCode.S)) {
                    direction += Vector3.back;
                }
                if (Input.GetKey(KeyCode.A)) {
                    direction += Vector3.left;
                }
                if (Input.GetKey(KeyCode.D)) {
                    direction += Vector3.right;
                }

                direction.Normalize();

                Vector3 translationWorldSpace = direction * speed * Time.deltaTime;
                Vector3 translationCameraSpace = cameraTransform.TransformDirection(translationWorldSpace);
                controller.Move(translationCameraSpace);
                controller.Move(Vector3.down * (controller.transform.position.y - controlling.LockHeight));

            }

        }
    }

#if UNITY_EDITOR
    /// <summary>
    ///  Add a new input axis slot to the action register
    /// </summary>
    /// <param name="input">The input axis to add</param>
    /// <returns>True if the action was succesfully added, false if the input was already added</returns>
    public bool AddInputAxis (string input) {
        foreach (ControllerActionSerialized cas in actionRegisterSerialized) {
            if (cas.input == input) return false;
        }
        ControllerActionSerialized newCas = new ControllerActionSerialized();
        newCas.input = input;
        actionRegisterSerialized.Add(newCas);
        return true;
    }

    /// <summary>
    ///  Remove an input axis slot from the register
    /// </summary>
    /// <param name="input">The input axis to remove</param>
    public void RemoveInputAxis (string input) {
        ControllerActionSerialized toRemove = new ControllerActionSerialized();
        bool found = false;
        foreach (ControllerActionSerialized cas in actionRegisterSerialized) {
            if (cas.input == input) {
                toRemove = cas;
                found = true;
                break;
            }
        }
        if (found) {
            actionRegisterSerialized.Remove(toRemove);
        }
    }

    /// <summary>
    ///  Changes an input key and keeps associated actions
    /// </summary>
    /// <param name="from">The input axis to change</param>
    /// <param name="to">New name for the input axis</param>
    public bool ChangeInputAxis (string from, string to) {
        int index = -1;
        foreach (ControllerActionSerialized cas in actionRegisterSerialized) {
            if (cas.input == from) {
                index = actionRegisterSerialized.IndexOf(cas);
                break;
            }
        }
        if (index != -1) {
            ControllerActionSerialized cas = actionRegisterSerialized[index];
            cas.input = to;
            actionRegisterSerialized[index] = cas;
            return true;
        }
        return false;
    }
    
    /// <summary>
    ///  Get the related actions to an input axis
    /// </summary>
    /// <param name="input">The input axis to search with</param>
    /// <returns>The related actions</returns>
    public ControllerAction? GetActionsFromInput (string input) {
        foreach (ControllerActionSerialized cas in actionRegisterSerialized) {
            if (cas.input == input) {
                return cas.actions;
            }
        }
        return null;
    }
#endif

    /// <summary>
    ///  Get the GameObject currently under the mouse.
    /// </summary>
    /// <remarks>
    ///  Uses default layer mask: avoid Player and Ignore Raycast layers
    /// </remarks>
    public GameObject GetHoveredGameObject () {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // Avoid player and ignore raycast layers by default
        int layerMask = LAYER_MASK_IGNORE_PLAYER;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            return hit.collider.gameObject;
        }
        else return null;
    }

    /// <summary>
    ///  Get the GameObject currently under the mouse.
    /// </summary>
    /// <param name="layerMask">
    ///  The layer mask to use on the raycast
    /// </param>
    public GameObject GetHoveredGameObject (int layerMask) {
        // Check to see if the mouse is over the interactable
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            return hit.collider.gameObject;
        }
        else return null;
    }

    public PlayerControllable GetControllable () {
        return controlling;
    }
    public void SetControllable (PlayerControllable controllable) {
        controlling = controllable;
        controller = controllable.GetComponent<CharacterController>();
        if (cameraFollower == null) {
            cameraFollower = GetComponent<CameraFollower>();
        }
        cameraFollower.target = controllable.cameraTarget;
        cameraTransform = controllable.cameraTarget;
    }

#if UNITY_EDITOR
    void OnDrawGizmos () {
        GUIStyle bigTextStyle = new GUIStyle();
        bigTextStyle.fontSize = 15;
        bigTextStyle.fontStyle = FontStyle.Bold;
        bigTextStyle.normal.textColor = Color.black;
        bigTextStyle.alignment = TextAnchor.MiddleCenter;
        UnityEditor.Handles.Label(transform.position + Vector3.up, "PLAYER CAMERA", bigTextStyle);
    }
#endif

}
