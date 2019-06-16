#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class EditorPlugin : Editor
{
    static GameObject uiCanvas = null;
    static GameObject focusGizmo = null;

    [MenuItem("AS3D/Initialize")]
    static void Initialize()
    {
        GameObject cameraHost = GameObject.Find("AS3D_CAMERA_HOST_OBJECT");

        if (cameraHost == null)
        {
            cameraHost = new GameObject("AS3D_CAMERA_HOST_OBJECT");
            InitCamera initScript = cameraHost.AddComponent<InitCamera>();
            initScript.m_uiOverlayTexture = Resources.Load<Texture2D>("AS3D/Textures/watermark");
            Debug.Log("Added Camera host");
        }
    }

    [MenuItem("AS3D/Create UI")]
    static void CreateUI()
    {
        GameObject cameraHost = GameObject.Find("AS3D_CAMERA_HOST_OBJECT");

        if (cameraHost == null)
        {
            Debug.Assert(false, "Camera host must be initialized first [AS3D / Initialize]");
            return;
        }

        if (GameObject.Find("AutoStereoUI") == null)
        {
            uiCanvas = (GameObject)Resources.Load("AS3D/AutoStereoUI", typeof(GameObject));
            if (uiCanvas != null)
            {
                GameObject instance = Instantiate(uiCanvas);
                instance.name = "AutoStereoUI";
                Debug.Log("Imported UI");
            }
            else
            {
                Debug.Assert(false, "Failed to import UI");
            }
        }
        else
        {
            Debug.LogWarning("AutoStereoUI already in scene");
        }
    }

    [MenuItem("AS3D/Create Focus Gizmo")]
    static void CreateFocusGizmo()
    {
        if (GameObject.Find("AS3D_FOCUS_GIZMO") == null)
        {
            focusGizmo = (GameObject)Resources.Load("AS3D/AS3D_FOCUS_GIZMO", typeof(GameObject));
            if (focusGizmo != null)
            {
                GameObject instance = Instantiate(focusGizmo);
                instance.name = "AS3D_FOCUS_GIZMO";
                Debug.Log("Imported AS3D_FOCUS_GIZMO");
            }
            else
            {
                Debug.Assert(false, "Failed to import AS3D_FOCUS_GIZMO");
            }
        }
        else
        {
            Debug.LogWarning("AS3D_FOCUS_GIZMO already in scene");
        }
    }
}
#endif