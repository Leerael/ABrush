using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputHandler : MonoBehaviour
{
    #region - Enums.
    public enum ObjectTypes
    {
        NewObj,
        SceneObj,
        CameraObj
    }

    public enum DragType
    {
        PositionX,
        PositionY,
        PositionZ,
        RotationX,
        RotationY,
        RotationZ,
        ScaleX,
        ScaleY,
        ScaleZ,
        ScaleAll,

        NONE
    }

    public enum TransformMode
    {
        Position,
        Rotate,
        Scale,

        NONE
    }

    private enum ManipulationMode
    {
        Camera,
        Scene,
        All
    }
    #endregion

    #region - Variables.
    public EventSystem eventSystem;

    private int quickTouchSensitivity = 50;
    private bool touchEventEaten = false;

    private bool ControllerChecked = false;
    private bool ControllerSelected = false;

    public Transform ItemHolder;
    public Transform SceneSpace;
    public Transform Camera;


    private int ClickCount = 0;
    private int ClickUpCount = 0;
    private float TimePassed = 0f;
    public float DoubleClickTime = 0.2f;
    public float DoubleClickHoldTime = 0.5f;

    private Vector2 StartMousedownPosition;
    private bool StartMousedownPosTracked;
    private Vector2 LastMouseDownPosition;
    private Vector2 CurrentMouseDownPosition;
    private bool lastMousePositionSet;
    private Vector2 LastMouseUpPosition;
    private float LastMouseUpTime;

    private Color normalColor = new Color(0.588f, 0.588f, 0.588f, 1);
    private Color selectedColor = new Color(1, 1, 1, 1);

    bool ItemPicked = false;

    public bool objectsBeingManipulated = false;
    public DragType dragType = DragType.NONE;
    public TransformMode transMode = TransformMode.NONE;
    private bool selectingAllObjects = false;

    public List<GameObject> selectedObjects = new List<GameObject>();
    public List<ObjectTypes> selectedObjectTypes = new List<ObjectTypes>();
    public List<Renderer> selectedObjectRenderers = new List<Renderer>();
    
    private ManipulationMode manipMode = ManipulationMode.All;

    public ColorPicker colourPicker;
    public Toggle colourPickerToggle;
    private Color selectedColour;

    private Material selectedMaterial;

    public Material[] matTemplates;
    public Toggle texturePickerToggle;
    
    public EventText eventText;
    #endregion


    void Update()
    {
        CheckInput();
    }
    private void CheckInput()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            // Find the object hit by the 'pointer'.
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                if (eventSystem.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                {
                    return;
                }
            }
            #if (UNITY_EDITOR)
            if (eventSystem.IsPointerOverGameObject())
            {
                return;
            }
            #endif
            touchEventEaten = false;
            ClickCount++;
            ClickUpCount = 0;
            TimePassed = 0;
            StartMousedownPosTracked = true;
            StartMousedownPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            CurrentMouseDownPosition = new Vector2(0, 0);
        }

        if (Input.GetButtonUp("Fire1"))
        {
            objectsBeingManipulated = false;
            StartMousedownPosTracked = false;
            if (touchEventEaten == false)
            {
                if (ClickCount > 1)
                {
                    CopySelectedObjects();
                    ClickCount = 0;
                    print("Double Click");
                }
            }
            if (ControllerSelected == true)
            {
                switch(transMode)
                {
                    case TransformMode.Position:
                        eventText.SetText(Color.white, "Positioned : " + selectedObjects.Count + " Objects");
                        break;
                    case TransformMode.Rotate:
                        eventText.SetText(Color.white, "Rotated : " + selectedObjects.Count + " Objects");
                        break;
                    case TransformMode.Scale:
                        eventText.SetText(Color.white, "Scaled : " + selectedObjects.Count + " Objects");
                        break;

                    case TransformMode.NONE:
                        break;

                    default:
                        break;
                }
                ControllerSelected = false;
                ControllerChecked = false;
                ClickCount = 0;
            }
            ClickUpCount++;
            LastMouseUpTime = Time.time;
            lastMousePositionSet = false;
            TimePassed = 0;
            LastMouseUpPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        }

        if (ClickCount > 0)
        {
            // Handle Single Clicks.
            if (ClickCount < 2 && ClickUpCount < 1) {
                if (ControllerChecked == false)
                {
                    ControllerChecked = true;
                    ControllerSelected = checkIfTransform();
                    StartMousedownPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                    print("Controller Selected = " + ControllerSelected);
                    if (ControllerChecked)
                    {
                        touchEventEaten = true;
                    }
                }

                if (TimePassed < DoubleClickTime)
                {
                    if (ControllerSelected == false) // If there isn't a controller selected we inspect for any drag inputs.
                    {
                        Vector2 mousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                        // Single Click 'Dragged' RIGHT.
                        if (mousePos.x > StartMousedownPosition.x + quickTouchSensitivity)
                        {
                            touchEventEaten = true;
                            ControllerChecked = false;
                            TimePassed = 0;
                            ClickCount = 0;
                            // Swith right through the transform options.
                            SwitchTransformOptionsRight();
                        }
                        // Single Click 'Dragged' LEFT.
                        else if (mousePos.x < StartMousedownPosition.x - quickTouchSensitivity)
                        {
                            touchEventEaten = true;
                            ControllerChecked = false;
                            TimePassed = 0;
                            ClickCount = 0;
                            // Swith left through the transform options.
                            SwitchTransformOptionsLeft();
                        }
                        // Single Click 'Dragged' UP.
                        else if (mousePos.y > StartMousedownPosition.y + quickTouchSensitivity)
                        {
                            touchEventEaten = true;
                            ControllerChecked = false;
                            TimePassed = 0;
                            ClickCount = 0;
                            // Open Colour Picker.
                            if (colourPickerToggle.isOn)
                            {
                                colourPickerToggle.isOn = false;
                            }
                            // Otherwise close the Colour Picker.
                            else
                            {
                                colourPickerToggle.isOn = true;
                            }
                        }
                        // Single Click 'Dragged' DOWN.
                        else if (mousePos.y < StartMousedownPosition.y - quickTouchSensitivity)
                        {
                            touchEventEaten = true;
                            ControllerChecked = false;
                            TimePassed = 0;
                            ClickCount = 0;
                            // Open Texture Picker.
                            if (texturePickerToggle.isOn)
                            {
                                texturePickerToggle.isOn = false;
                            }
                            // Otherwise close the Texture Picker.
                            else
                            {
                                texturePickerToggle.isOn = true;
                            }
                        }
                    }
                    else
                    {
                        HandleDrag();
                        print("Dragging before Timer");
                    }
                }
                else //If the max duration for a double click has passed then it's a gauranteed single click
                {
                    if (ControllerSelected == false)
                    {
                        HandleClick();
                        ControllerChecked = false;
                        touchEventEaten = true;
                        TimePassed = 0;
                        ClickCount = 0;
                        touchEventEaten = true;
                        print("Single Click");
                    }
                    else
                    {
                        HandleDrag();
                        print("Dragging after Timer");
                    }
                }
            }
            // Handle Double Clicks.
            else if (ClickCount > 1)
            {
                if (StartMousedownPosTracked == false)
                {
                    TimePassed = 0;
                    StartMousedownPosTracked = true;
                    touchEventEaten = false;
                    ControllerChecked = false;
                    StartMousedownPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                }

                Vector2 mousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                if (TimePassed >= DoubleClickHoldTime)
                {
                    touchEventEaten = true;
                    StartMousedownPosTracked = false;
                    TimePassed = 0;
                    ClickCount = 0;
                    // Delete selected objects.
                    DeleteSelectedObjects();
                    print("Double Click HELD");
                }
                else if (mousePos.x >= StartMousedownPosition.x + quickTouchSensitivity)
                {
                    touchEventEaten = true;
                    StartMousedownPosTracked = false;
                    TimePassed = 0;
                    ClickCount = 0;
                    // Undo.
                    print("Double Click Dragged RIGHT");
                }
                else if (mousePos.x <= StartMousedownPosition.x - quickTouchSensitivity)
                {
                    touchEventEaten = true;
                    StartMousedownPosTracked = false;
                    TimePassed = 0;
                    ClickCount = 0;
                    // Redo.
                    print("Double Click Dragged LEFT");
                }
                else if (mousePos.y >= StartMousedownPosition.y + quickTouchSensitivity)
                {
                    touchEventEaten = true;
                    StartMousedownPosTracked = false;
                    TimePassed = 0;
                    ClickCount = 0;
                    // Bind to the camera.
                    MoveSelectedObjectsToCamera();
                    print("Double Click Dragged UP");
                }
                else if (mousePos.y <= StartMousedownPosition.y - quickTouchSensitivity)
                {
                    touchEventEaten = true;
                    StartMousedownPosTracked = false;
                    TimePassed = 0;
                    ClickCount = 0;
                    // Bind to the scene.
                    MoveSelectedObjectsToScene();
                    print("Double Click Dragged DOWN");
                }
            }
        }
        TimePassed += Time.deltaTime; //Keep track of time passed after a click
                                      //Debug.Log(m_TimePassed);
    }


    public bool checkIfTransform()
    {
        // Get the current 'pointer' position and store it.
        Vector3 position = Input.mousePosition;
        CurrentMouseDownPosition = new Vector2(0, 0);
        // Set up the ray to cast from the screen to world space.
        Ray inputRay = UnityEngine.Camera.main.ScreenPointToRay(position);
        RaycastHit hit;
        // Find the object hit by the 'pointer'.
        if (Physics.Raycast(inputRay, out hit))
        {
            // Check if the tag symbolises that this object is important.
            switch (hit.collider.tag)
            {
                case "PositionX":
                    SetDragType(DragType.PositionX);
                    return true;
                case "PositionY":
                    SetDragType(DragType.PositionY);
                    return true;
                case "PositionZ":
                    SetDragType(DragType.PositionZ);
                    return true;
                case "RotationX":
                    SetDragType(DragType.RotationX);
                    return true;
                case "RotationY":
                    SetDragType(DragType.RotationY);
                    return true;
                case "RotationZ":
                    SetDragType(DragType.RotationZ);
                    return true;
                case "ScaleX":
                    SetDragType(DragType.ScaleX);
                    return true;
                case "ScaleY":
                    SetDragType(DragType.ScaleY);
                    return true;
                case "ScaleZ":
                    SetDragType(DragType.ScaleZ);
                    return true;
                case "ScaleAll":
                    SetDragType(DragType.ScaleAll);
                    return true;

                default:
                    return false;
            }
        }
        return false;
    }


    #region - Manipulate Methods.
    #region -- Position Methods.
    private void PositionX()
    {
        float diff = ((CurrentMouseDownPosition.x - LastMouseDownPosition.x) + (CurrentMouseDownPosition.y - LastMouseDownPosition.y)) / 200;
        foreach (GameObject obj in selectedObjects)
        {
            obj.transform.localPosition = new Vector3(obj.transform.localPosition.x + diff, obj.transform.localPosition.y, obj.transform.localPosition.z);
        }
    }
    private void PositionY()
    {
        float diff = ((CurrentMouseDownPosition.x - LastMouseDownPosition.x) + (CurrentMouseDownPosition.y - LastMouseDownPosition.y)) / 200;
        foreach (GameObject obj in selectedObjects)
        {
            obj.transform.localPosition = new Vector3(obj.transform.localPosition.x, obj.transform.localPosition.y + diff, obj.transform.localPosition.z);
        }
    }
    private void PositionZ()
    {
        float diff = ((CurrentMouseDownPosition.x - LastMouseDownPosition.x) + (CurrentMouseDownPosition.y - LastMouseDownPosition.y)) / 200;
        foreach (GameObject obj in selectedObjects)
        {
            obj.transform.localPosition = new Vector3(obj.transform.localPosition.x, obj.transform.localPosition.y, obj.transform.localPosition.z + diff);
        }
    }
    #endregion

    #region -- Rotate Methods.
    private void RotateX()
    {
        float diff = ((CurrentMouseDownPosition.x - LastMouseDownPosition.x) + (CurrentMouseDownPosition.y - LastMouseDownPosition.y)) / 5;
        foreach (GameObject obj in selectedObjects)
        {
            obj.transform.Rotate(diff, 0, 0);
            ManipulateObject manip = obj.GetComponent<ManipulateObject>();
            manip.Control_Position.transform.rotation = Quaternion.Euler(new Vector3(0, -90, 0));
        }
    }
    private void RotateY()
    {
        float diff = ((CurrentMouseDownPosition.x - LastMouseDownPosition.x) + (CurrentMouseDownPosition.y - LastMouseDownPosition.y)) / 5;
        foreach (GameObject obj in selectedObjects)
        {
            obj.transform.Rotate(0, diff, 0);
            ManipulateObject manip = obj.GetComponent<ManipulateObject>();
            manip.Control_Position.transform.rotation = Quaternion.Euler(new Vector3(0, -90, 0));
        }
    }
    private void RotateZ()
    {
        float diff = ((CurrentMouseDownPosition.x - LastMouseDownPosition.x) + (CurrentMouseDownPosition.y - LastMouseDownPosition.y)) / 5;
        foreach (GameObject obj in selectedObjects)
        {
            obj.transform.Rotate(0, 0, diff);
            ManipulateObject manip = obj.GetComponent<ManipulateObject>();
            manip.Control_Position.transform.rotation = Quaternion.Euler(new Vector3(0, -90, 0));
        }
    }
    #endregion

    #region -- Scale Methods.
    private void ScaleX()
    {
        float diff = ((CurrentMouseDownPosition.x - LastMouseDownPosition.x) + (CurrentMouseDownPosition.y - LastMouseDownPosition.y)) / 200;
        foreach (GameObject obj in selectedObjects)
        {
            obj.transform.localScale = new Vector3(obj.transform.localScale.x + diff, obj.transform.localScale.y, obj.transform.localScale.z);
        }
    }
    private void ScaleY()
    {
        float diff = ((CurrentMouseDownPosition.x - LastMouseDownPosition.x) + (CurrentMouseDownPosition.y - LastMouseDownPosition.y)) / 200;
        foreach (GameObject obj in selectedObjects)
        {
            obj.transform.localScale = new Vector3(obj.transform.localScale.x, obj.transform.localScale.y + diff, obj.transform.localScale.z);
        }
    }
    private void ScaleZ()
    {
        float diff = ((CurrentMouseDownPosition.x - LastMouseDownPosition.x) + (CurrentMouseDownPosition.y - LastMouseDownPosition.y)) / 200;
        foreach (GameObject obj in selectedObjects)
        {
            obj.transform.localScale = new Vector3(obj.transform.localScale.x, obj.transform.localScale.y, obj.transform.localScale.z + diff);
        }
    }
    private void ScaleAll()
    {
        float diff = ((CurrentMouseDownPosition.x - LastMouseDownPosition.x) + (CurrentMouseDownPosition.y - LastMouseDownPosition.y)) / 200;
        foreach (GameObject obj in selectedObjects)
        {
            obj.transform.localScale = new Vector3(obj.transform.localScale.x + diff, obj.transform.localScale.y + diff, obj.transform.localScale.z + diff);
        }
    }
    #endregion
    #endregion


    #region - Click methods.
    public void SwitchTransformOptionsLeft()
    {
        switch (transMode)
        {
            case TransformMode.Position:
                SetTransformMode((int)TransformMode.Rotate);
                break;
            case TransformMode.Rotate:
                SetTransformMode((int)TransformMode.Scale);
                break;
            case TransformMode.Scale:
                SetTransformMode((int)TransformMode.NONE);
                break;
            case TransformMode.NONE:
                SetTransformMode((int)TransformMode.Position);
                break;

            default:
                SetTransformMode((int)TransformMode.NONE);
                break;
        }
    }
    public void SwitchTransformOptionsRight()
    {
        switch (transMode)
        {
            case TransformMode.Position:
                SetTransformMode((int)TransformMode.NONE);
                break;
            case TransformMode.Rotate:
                SetTransformMode((int)TransformMode.Position);
                break;
            case TransformMode.Scale:
                SetTransformMode((int)TransformMode.Rotate);
                break;
            case TransformMode.NONE:
                SetTransformMode((int)TransformMode.Scale);
                break;

            default:
                SetTransformMode((int)TransformMode.NONE);
                break;
        }
    }


    public void SetDragType(DragType dragType)
    {
        this.dragType = dragType;
        objectsBeingManipulated = true;
    }

    private void HandleDrag()
    {
        CurrentMouseDownPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

        if (lastMousePositionSet == false)
        {
            LastMouseDownPosition = CurrentMouseDownPosition;
        }

        if (LastMouseDownPosition != null)
        {
            switch (dragType)
            {
                case DragType.PositionX:
                    PositionX();
                    break;
                case DragType.PositionY:
                    PositionY();
                    break;
                case DragType.PositionZ:
                    PositionZ();
                    break;
                case DragType.RotationX:
                    RotateX();
                    break;
                case DragType.RotationY:
                    RotateY();
                    break;
                case DragType.RotationZ:
                    RotateZ();
                    break;
                case DragType.ScaleX:
                    ScaleX();
                    break;
                case DragType.ScaleY:
                    ScaleY();
                    break;
                case DragType.ScaleZ:
                    ScaleZ();
                    break;
                case DragType.ScaleAll:
                    ScaleAll();
                    break;

                case DragType.NONE:
                    break;
            }
        }
        LastMouseDownPosition = CurrentMouseDownPosition;
        lastMousePositionSet = true;
    }
    /// <summary>
    /// Use this to handle any clicks.
    /// </summary>
    private void HandleClick()
    {
        // Get the current 'pointer' position and store it.
        Vector3 position = Input.mousePosition;
        CurrentMouseDownPosition = new Vector2(0, 0);
        // Set up the ray to cast from the screen to world space.
        Ray inputRay = UnityEngine.Camera.main.ScreenPointToRay(position);
        RaycastHit hit;
        // Find the object hit by the 'pointer'.
        if (Physics.Raycast(inputRay, out hit))
        {
            GameObject gameObj = hit.transform.gameObject;
            // Check if the tag symbolises that this object is important.
            switch (gameObj.tag)
            {
                case "ShapeTemplate":
                    HandleShapeTemplate(gameObj);
                    break;
                case "TextureTemplate":
                    HandleTextureTemplate(gameObj);
                    break;
                case "SceneObj":
                    SelectSceneObj(gameObj.transform.parent.gameObject);
                    break;
                case "CameraObj":
                    SelectCameraObj(gameObj.transform.parent.gameObject);
                    break;

                default:
                    break;
            }
        }
    }
    #endregion


    #region - Controller Template methods.
    /// <summary>
    /// Use this to handle inputs from the shapes template.
    /// </summary>
    /// <param name="gameObj"></param>
    private void HandleShapeTemplate(GameObject gameObj)
    {
        Renderer rend = null;
        if (ItemPicked) //destroy object in hand first
        {
            GameObject obj = ItemHolder.GetChild(0).gameObject;
            if (obj)
                rend = obj.transform.GetChild(0).GetComponent<Renderer>();
            Destroy(obj);
        }
        GameObject newObj = (GameObject)Instantiate(gameObj.transform.parent.gameObject);
        if (rend)
        {
            Renderer newObjRend = newObj.GetComponent<Renderer>();
            newObjRend = rend;
        }
        MoveObjectToHolder(newObj);
        ItemPicked = true;
    }
    /// <summary>
    /// Use this to handle inputs from the colours template.
    /// </summary>
    /// <param name="gameObj"></param>
    public void UpdateNewObjectColour(Color color)
    {
        if (ItemPicked)
        {
            GameObject obj = ItemHolder.GetChild(0).gameObject;
            if (obj)
            {
                obj.transform.GetChild(0).GetComponent<MeshRenderer>().material.color = color;
            }
        }
    }
    /// <summary>
    /// Use this to handle inputs from the textures template.
    /// </summary>
    /// <param name="gameObj"></param>
    private void HandleTextureTemplate(GameObject gameObj)
    {
        if (ItemPicked)
        {
            GameObject obj = ItemHolder.GetChild(0).gameObject;
            if (obj)
            {
                Transform child = obj.transform.GetChild(0);
                Renderer rend = child.GetComponent<Renderer>();
                Color matColour = rend.material.color;
                rend.material = gameObj.GetComponent<Renderer>().material;
                rend.material.color = matColour;
            }
        }
    }
    #endregion


    #region - UI methods.
    #region -- UI Select/Deselect all methods.
    /// <summary>
    /// This is the UI helper method for selecting all objects.
    /// </summary>
    public void SelectAll()
    {
        SelectAllObjects();
    }
    /// <summary>
    /// This is the UI helper method for deselecting all objects.
    /// </summary>
    public void DeselectAll()
    {
        DeselectAllObjects();
    }
    #endregion

    #region -- UI 'Selection Mode' methods.
    /// <summary>
    /// Use this method to set the Selection type to scene only.
    /// </summary>
    public void SetSelectOnlyScene()
    {
        manipMode = ManipulationMode.Scene;
        eventText.SetText(Color.yellow, "Selection Mode: SCENE ONLY");
    }
    /// <summary>
    /// Use this method to set the Selection type to camera only.
    /// </summary>
    public void SetSelectOnlyCamera()
    {
        manipMode = ManipulationMode.Camera;
        eventText.SetText(Color.yellow, "Selection Mode: CAMERA ONLY");
    }
    /// <summary>
    /// Use this method to set the Selection type to scene and camera.
    /// </summary>
    public void SetSelectAll()
    {
        manipMode = ManipulationMode.All;
        eventText.SetText(Color.yellow, "Selection Mode: ALL");
    }
    #endregion

    #region -- UI 'View Mode' methods.
    /// <summary>
    /// Use this method to set the View type to scene only.
    /// </summary>
    public void SetViewOnlyScene()
    {
        // De-activate the camera objects first.
        foreach (ManipulateObject manip in Camera.GetComponentsInChildren<ManipulateObject>(true))
        {
            manip.gameObject.SetActive(false);
        }
        // Now activate the scene objects.
        foreach (ManipulateObject manip in SceneSpace.GetComponentsInChildren<ManipulateObject>(true))
        {
            manip.gameObject.SetActive(true);
        }
        eventText.SetText(Color.yellow, "View Mode: SCENE ONLY");
    }
    /// <summary>
    /// Use this method to set the View type to camera only.
    /// </summary>
    public void SetViewOnlyCamera()
    {
        // De-activate the scene objects first.
        foreach (ManipulateObject manip in SceneSpace.GetComponentsInChildren<ManipulateObject>(true))
        {
            manip.gameObject.SetActive(false);
        }
        // Now activate the camera objects.
        foreach (ManipulateObject manip in Camera.GetComponentsInChildren<ManipulateObject>(true))
        {
            manip.gameObject.SetActive(true);
        }
        eventText.SetText(Color.yellow, "View Mode: CAMERA ONLY");
    }
    /// <summary>
    /// Use this method to set the View type to scene and camera.
    /// </summary>
    public void SetViewAll()
    {
        // Activate the scene objects.
        foreach (ManipulateObject manip in SceneSpace.GetComponentsInChildren<ManipulateObject>(true))
        {
            manip.gameObject.SetActive(true);
        }
        // Activate the camera objects.
        foreach (ManipulateObject manip in Camera.GetComponentsInChildren<ManipulateObject>(true))
        {
            manip.gameObject.SetActive(true);
        }
        eventText.SetText(Color.yellow, "View Mode: ALL");
    }
    #endregion

    #region -- UI 'Transform Mode' methods.
    /// <summary>
    /// Use this method to copy all selected objects.
    /// </summary>
    public void CopySelectedObjects()
    {
        int count = selectedObjects.Count;
        for (int i = 0; i < selectedObjects.Count; i++)
        {
            GameObject obj = selectedObjects[i];
            GameObject newObj = GameObject.Instantiate(obj, obj.transform.parent);
            // Setup the new object.
            newObj.transform.localScale = obj.transform.localScale;
            newObj.transform.localPosition = obj.transform.localPosition;
            newObj.transform.localRotation = obj.transform.localRotation;
            ManipulateObject manip = newObj.GetComponent<ManipulateObject>();
            manip.Control_Rotation.SetActive(false);
            manip.Control_Scale.SetActive(false);
            manip.Control_Position.SetActive(false);
            Renderer rend = newObj.transform.GetChild(0).GetComponent<Renderer>();
            rend.material.SetFloat("_Outline", 0.005f);
        }
        eventText.SetText(Color.green, "Copied : " + count + " Objects");
    }
    /// <summary>
    /// Use this method to delete all selected objects.
    /// </summary>
    public void DeleteSelectedObjects()
    {
        int count = selectedObjects.Count;
        foreach (GameObject obj in selectedObjects)
        {
            Destroy(obj);
        }
        selectedObjects = new List<GameObject>();
        selectedObjectTypes = new List<ObjectTypes>();
        selectedObjectRenderers = new List<Renderer>();
        eventText.SetText(Color.red, "Deleted : " + count + " Objects");
    }

    /// <summary>
    /// Use this to set the current transform mode.
    /// </summary>
    /// <param name="index"></param>
    public void SetTransformMode(int index)
    {
        transMode = (TransformMode)index;
        switch (transMode)
        {
            case TransformMode.Position:
                for (int i = 0; i < selectedObjects.Count; i++)
                {
                    GameObject obj = selectedObjects[i];
                    ManipulateObject manip = obj.GetComponent<ManipulateObject>();
                    manip.Control_Rotation.SetActive(false);
                    manip.Control_Scale.SetActive(false);
                    manip.Control_Position.SetActive(true);
                    if (selectedObjectTypes[i] == ObjectTypes.CameraObj)
                    {
                        Vector3 newRot = Camera.rotation.eulerAngles;
                        manip.Control_Position.transform.rotation = Quaternion.Euler(0, 0, -90);
                    }
                    else if (selectedObjectTypes[i] == ObjectTypes.SceneObj)
                    {
                        Vector3 newRot = SceneSpace.rotation.eulerAngles;
                        manip.Control_Position.transform.rotation = Quaternion.Euler(newRot.x, newRot.y - 90, newRot.z);
                    }
                }
                eventText.SetText(Color.white, "Transform Mode : POSITION");
                break;
            case TransformMode.Rotate:
                for (int i = 0; i < selectedObjects.Count; i++)
                {
                    GameObject obj = selectedObjects[i];
                    ManipulateObject manip = obj.GetComponent<ManipulateObject>();
                    manip.Control_Position.SetActive(false);
                    manip.Control_Scale.SetActive(false);
                    manip.Control_Rotation.SetActive(true);
                }
                eventText.SetText(Color.white, "Transform Mode : ROTATE");
                break;
            case TransformMode.Scale:
                for (int i = 0; i < selectedObjects.Count; i++)
                {
                    GameObject obj = selectedObjects[i];
                    ManipulateObject manip = obj.GetComponent<ManipulateObject>();
                    manip.Control_Position.SetActive(false);
                    manip.Control_Rotation.SetActive(false);
                    manip.Control_Scale.SetActive(true);
                }
                eventText.SetText(Color.white, "Transform Mode : SCALE");
                break;

            case TransformMode.NONE:
                for (int i = 0; i < selectedObjects.Count; i++)
                {
                    GameObject obj = selectedObjects[i];
                    ManipulateObject manip = obj.GetComponent<ManipulateObject>();
                    manip.Control_Position.SetActive(false);
                    manip.Control_Rotation.SetActive(false);
                    manip.Control_Scale.SetActive(false);
                }
                eventText.SetText(Color.white, "Transform Mode : NONE");
                break;
        }
    }
    #endregion

    #region -- UI 'Colour/Texture' methods.
    /// <summary>
    /// Use this to setup the current colour of the colour picker to the current colour of the 'new object'.
    /// </summary>
    public void SetupColour()
    {
        if (ItemHolder && ItemHolder.childCount > 0)
        {
            GameObject obj = ItemHolder.GetChild(0).gameObject;
            if (obj)
            {
                colourPicker.CurrentColor = obj.transform.GetChild(0).GetComponent<MeshRenderer>().material.color;
            }
        }
    }
    /// <summary>
    /// Use this to change the current selected global paint colour.
    /// </summary>
    /// <param name="colour"></param>
    public void SetSelectedColour(Color colour)
    {
        selectedColour = colour;
    }

    /// <summary>
    /// Use this to handle inputs from the UI colour picker.
    /// </summary>
    public void SetSelectedObjectsColours()
    {
        int count = selectedObjectRenderers.Count;
        foreach (Renderer rend in selectedObjectRenderers)
        {
            rend.material.color = selectedColour;
        }
        eventText.SetText(Color.white, "Changed Colours Of : " + count + " Objects");
    }

    /// <summary>
    /// Use this to handle inputs from the UI Texture picker.
    /// </summary>
    public void SetSelectedObjectsMaterials(int index)
    {
        int count = selectedObjectRenderers.Count;
        Material mat = matTemplates[index];
        foreach (Renderer rend in selectedObjectRenderers)
        {
            Color matColour = rend.material.color;
            rend.material = mat;
            rend.material.color = matColour;
            rend.material.SetFloat("_Outline", 1.34f);
        }
        eventText.SetText(Color.white, "Changed Textures Of : " + count + " Objects");
    }
    #endregion
    #endregion


    #region - Controller Methods.
    /// <summary>
    /// Use this to handle the creation of a new object.
    /// </summary>
    /// <param name="obj"></param>
    private void CreateNewObject(GameObject obj)
    {
        if (obj)
        {
            GameObject newObj = Instantiate(obj);
            eventText.SetText(Color.green, "New Object Created");
            MoveObjectToCamera(newObj);
            SelectCameraObj(newObj);
        }
    }
    /// <summary>
    /// Helper method for creating a new object.
    /// </summary>
    public void CreateNewObject()
    {
        if (ItemPicked) {
            CreateNewObject(ItemHolder.GetChild(0).gameObject);
        }
    }
    /// <summary>
    /// Use this to delete the currently selected item.
    /// </summary>
    public void DeleteNewObject()
    {
        if (ItemPicked)
        {
            GameObject obj = ItemHolder.GetChild(0).gameObject;
            if (obj)
            {
                Destroy(obj);
                eventText.SetText(Color.red, "New Object Cleared");
            }
            ItemPicked = false;
        }
    }
    #endregion


    #region - Select methods.
    /// <summary>
    /// Use this to handle the selection of a scene object.
    /// </summary>
    /// <param name="obj"></param>
    private void SelectSceneObj(GameObject obj)
    {
        if (manipMode == ManipulationMode.Scene || manipMode == ManipulationMode.All)
        {
            // Determine whether we can select or deselect the object.
            bool canSelect = true;
            for (int i = 0; i < selectedObjects.Count; i++)
            {
                if (selectedObjects[i] == obj)
                {
                    // Deselect the object.
                    DeselectObject(obj, i);
                    canSelect = false;
                    eventText.SetText(new Color(255 / 255, 164 / 255, 28 / 255, 255 / 255), "Deselected A Scene Object");
                    break;
                }
            }
            if (canSelect)
            {
                // Select the object.
                SelectObject(obj, ObjectTypes.SceneObj);
                eventText.SetText(Color.yellow, "Selected A Scene Object");
            }
        }
    }
    /// <summary>
    /// Use this to handle the selection of a camera object.
    /// </summary>
    /// <param name="obj"></param>
    private void SelectCameraObj(GameObject obj)
    {
        if (manipMode == ManipulationMode.Camera || manipMode == ManipulationMode.All)
        {
            // Determine whether we can select or deselect the object.
            bool canSelect = true;
            for (int i = 0; i < selectedObjects.Count; i++)
            {
                if (selectedObjects[i] == obj)
                {
                    // Deselect the object.
                    DeselectObject(obj, i);
                    canSelect = false;
                    eventText.SetText(new Color(255 / 255, 164 / 255, 28 / 255, 255 / 255), "Deselected A Camera Object");
                    break;
                }
            }
            if (canSelect)
            {
                // Select the object.
                SelectObject(obj, ObjectTypes.CameraObj);
                eventText.SetText(Color.yellow, "Selected A Camera Object");
            }
        }
    }

    #region -- Select Helper methods.
    /// <summary>
    /// Helper method to add the components for the object to the selections list.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="objType"></param>
    private void SelectObject(GameObject obj, ObjectTypes objType)
    {
        for (int i = 0; i < selectedObjects.Count; i++)
        {
            if (selectedObjects[i] == obj)
            {
                DeselectObject(obj, i);
                return;
            }
        }

        Renderer rend = obj.transform.GetChild(0).GetComponent<Renderer>();
        rend.material.SetFloat("_Outline", 1.35f);
        ManipulateObject manip = obj.GetComponent<ManipulateObject>();
        switch (transMode)
        {
            case TransformMode.Position:
                manip.Control_Rotation.SetActive(false);
                manip.Control_Scale.SetActive(false);
                manip.Control_Position.SetActive(true);
                if (objType == ObjectTypes.CameraObj)
                {
                    Vector3 newRot = Camera.rotation.eulerAngles;
                    manip.Control_Position.transform.rotation = Quaternion.Euler(0, 0, -90);
                }
                else if (objType == ObjectTypes.SceneObj)
                {
                    Vector3 newRot = SceneSpace.rotation.eulerAngles;
                    manip.Control_Position.transform.rotation = Quaternion.Euler(newRot.x, newRot.y - 90, newRot.z);
                }
                break;
            case TransformMode.Rotate:
                manip.Control_Position.SetActive(false);
                manip.Control_Scale.SetActive(false);
                manip.Control_Rotation.SetActive(true);
                break;
            case TransformMode.Scale:
                manip.Control_Position.SetActive(false);
                manip.Control_Rotation.SetActive(false);
                manip.Control_Scale.SetActive(true);
                break;
            case TransformMode.NONE:
                manip.Control_Position.SetActive(false);
                manip.Control_Rotation.SetActive(false);
                manip.Control_Scale.SetActive(false);
                break;

            default:
                manip.Control_Position.SetActive(false);
                manip.Control_Rotation.SetActive(false);
                manip.Control_Scale.SetActive(false);
                break;
        }
        selectedObjects.Add(obj);
        selectedObjectTypes.Add(objType);
        selectedObjectRenderers.Add(obj.transform.GetChild(0).GetComponent<Renderer>());
    }
    /// <summary>
    /// Helper method to remove the components for the object to the selections list.
    /// </summary>
    /// <param name="obj"></param>
    private void DeselectObject(GameObject obj, int index)
    {
        Renderer rend = obj.transform.GetChild(0).GetComponent<Renderer>();
        rend.material.SetFloat("_Outline", 0.005f);
        obj.GetComponent<ManipulateObject>().Control_Position.SetActive(false);
        obj.GetComponent<ManipulateObject>().Control_Rotation.SetActive(false);
        obj.GetComponent<ManipulateObject>().Control_Scale.SetActive(false);
        selectedObjects.RemoveAt(index);
        selectedObjectTypes.RemoveAt(index);
        selectedObjectRenderers.RemoveAt(index);
    }

    /// <summary>
    /// Use this method to select all objects (based on scene/camera/all).
    /// </summary>
    private void SelectAllObjects()
    {
        DeselectAllObjects();
        if (manipMode == ManipulationMode.Scene)
        {
            if (SceneSpace.childCount > 1)
            {
                int count = 0;
                foreach (Transform obj in SceneSpace.GetComponentsInChildren<Transform>())
                {
                    if (obj.tag == "SceneObj")
                    {
                        // Select the object.
                        SelectObject(obj.parent.gameObject, ObjectTypes.SceneObj);
                        count++;
                    }
                }
                eventText.SetText(new Color(255 / 255, 164 / 255, 28 / 255, 255 / 255), "Selected All Scene Objects : " + count + " Objects");
            }
        }
        else if (manipMode == ManipulationMode.Camera)
        {
            if (Camera.childCount > 1)
            {
                int count = 0;
                foreach (Transform obj in Camera.GetComponentsInChildren<Transform>())
                {
                    if (obj.tag == "CameraObj")
                    {
                        // Select the object.
                        SelectObject(obj.parent.gameObject, ObjectTypes.CameraObj);
                        count++;
                    }
                }
                eventText.SetText(Color.yellow, "Selected All Camera Objects : " + count + " Objects");
            }
        }
        else if (manipMode == ManipulationMode.All)
        {
            int sceneObjCount = 0;
            if (SceneSpace.childCount > 1)
            {
                foreach (Transform obj in SceneSpace.GetComponentsInChildren<Transform>())
                {
                    if (obj.tag == "SceneObj")
                    {
                        // Select the object.
                        SelectObject(obj.parent.gameObject, ObjectTypes.SceneObj);
                        sceneObjCount++;
                    }
                }
            }
            int camObjCount = 0;
            if (Camera.childCount > 1)
            {
                foreach (Transform obj in Camera.GetComponentsInChildren<Transform>())
                {
                    if (obj.tag == "CameraObj")
                    {
                        // Select the object.
                        SelectObject(obj.parent.gameObject, ObjectTypes.CameraObj);
                        camObjCount++;
                    }
                }
            }
            eventText.SetText(Color.yellow, "Selected All Objects : \n        " + camObjCount + " Cam Objects | " + sceneObjCount + " Scene Objects");
        }
    }
    /// <summary>
    /// Use this method to deselect all objects.
    /// </summary>
    private void DeselectAllObjects()
    {
        int count = selectedObjects.Count;
        for (int i = 0; i < selectedObjects.Count; i++)
        {
            selectedObjectRenderers[i].material.SetFloat("_Outline", 0.005f);
        }
        eventText.SetText(new Color(255 / 255, 164 / 255, 28 / 255, 255 / 255), "Deselected All Objects : " + count + " Objects");
        selectedObjects = new List<GameObject>();
        selectedObjectTypes = new List<ObjectTypes>();
        selectedObjectRenderers = new List<Renderer>();
    }
    #endregion
    #endregion


    #region - Object Manipulation Methods.
    #region -- Object Parenting Methods.
    /// <summary>
    /// Use this to set the 'scene' as the object's parent.
    /// </summary>
    /// <param name="obj"></param>
    private void MoveObjectToScene(GameObject obj)
    {
        obj.transform.SetParent(SceneSpace);
        obj.transform.rotation = new Quaternion();
        obj.transform.localPosition = new Vector3();
        obj.transform.GetChild(0).tag = "SceneObj";
        for (int i = 0; i < selectedObjects.Count; i++)
        {
            if (selectedObjects[i] == obj)
            {
                selectedObjectTypes[i] = ObjectTypes.SceneObj;
                break;
            }
        }
    }
    /// <summary>
    /// Helper method to child all selected objects to the scene space.
    /// </summary>
    public void MoveSelectedObjectsToScene()
    {
        for (int i = 0; i < selectedObjects.Count; i++)
        {
            GameObject obj = selectedObjects[i];
            obj.transform.SetParent(SceneSpace);
            obj.transform.GetChild(0).tag = "SceneObj";
            selectedObjectTypes[i] = ObjectTypes.SceneObj;
        }
        eventText.SetText(Color.yellow, "Bound " + selectedObjects.Count + " Objects to the Scene");
    }

    /// <summary>
    /// Use this to set the 'camera' as the object's parent.
    /// </summary>
    /// <param name="obj"></param>
    private void MoveObjectToCamera(GameObject obj)
    {
        obj.transform.SetParent(Camera);
        obj.transform.rotation = new Quaternion();
        obj.transform.localPosition = new Vector3(0, 0, 1.4f);
        obj.transform.GetChild(0).tag = "CameraObj";
        for (int i = 0; i < selectedObjects.Count; i++)
        {
            if (selectedObjects[i] == obj)
            {
                selectedObjectTypes[i] = ObjectTypes.CameraObj;
                break;
            }
        }
    }
    /// <summary>
    /// Helper method to child all selected objects to the camera.
    /// </summary>
    public void MoveSelectedObjectsToCamera()
    {
        for (int i = 0; i < selectedObjects.Count; i++)
        {
            GameObject obj = selectedObjects[i];
            obj.transform.SetParent(Camera);
            obj.transform.GetChild(0).tag = "CameraObj";
            selectedObjectTypes[i] = ObjectTypes.CameraObj;
        }
        eventText.SetText(Color.yellow, "Bound " + selectedObjects.Count + " Objects to the Camera");
    }

    /// <summary>
    /// Use this to set the 'item holder' as the object's parent.
    /// </summary>
    /// <param name="obj"></param>
    private void MoveObjectToHolder(GameObject obj)
    {
        obj.transform.SetParent(ItemHolder, false);
        obj.transform.rotation = new Quaternion();
        obj.transform.localPosition = new Vector3(0, 0.16f, 0);
        obj.transform.GetChild(0).tag = "NewObj";
    }
    #endregion
    

    /// <summary>
    /// Use this to close the application.
    /// </summary>
    public void CloseApplication()
    {
        Application.Quit();
    }
    #endregion
}