using UnityEngine;
using Vuforia;

public class VBTabHandler : MonoBehaviour, IVirtualButtonEventHandler
{

    public GameObject Shapes;
    public GameObject Colours;
    public GameObject Textures;

    public InputHandler inputHandler;

    void Start()
    {
        // Register with the virtual buttons TrackableBehaviour
        VirtualButtonBehaviour[] vbs = GetComponentsInChildren<VirtualButtonBehaviour>();
        for (int i = 0; i < vbs.Length; ++i)
        {
            vbs[i].RegisterEventHandler(this);
        }
    }

    public void OnButtonPressed(VirtualButtonAbstractBehaviour vb)
    {
        Debug.Log("OnButtonPressed: " + vb.VirtualButtonName);

        switch (vb.VirtualButtonName)
        {
            case "Shape":
                Shapes.SetActive(true);
                Colours.SetActive(false);
                Textures.SetActive(false);
                break;

            case "Colour":
                inputHandler.SetupColour();
                Shapes.SetActive(false);
                Colours.SetActive(true);
                Textures.SetActive(false);
                break;

            case "Texture":
                Shapes.SetActive(false);
                Colours.SetActive(false);
                Textures.SetActive(true);
                break;

            case "Create":
                inputHandler.CreateNewObject();
                break;

            case "Delete":
                inputHandler.DeleteNewObject();
                break;

            default:
                break;

        }
    }

    public void OnButtonReleased(VirtualButtonAbstractBehaviour vb)
    {
        Debug.Log("OnButtonReleased");
    }

}
