using UnityEngine;

public class InitCamera : MonoBehaviour
{
    AutoStereoCamera m_scriptInstance = null;

    [SerializeField]
    public Camera m_userDefinedMainCamera = null;

    [SerializeField]
    public bool m_ignoreTag = false;

    [SerializeField]
    public string m_tag = "MainCamera";

    [SerializeField]
    public bool m_takeCameraFromScene = false;

    [SerializeField]
    public Texture m_uiOverlayTexture = null;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        if (m_takeCameraFromScene == false)
        {
            // register the callback when enabling object
            Camera.onPreRender += RegisterMainCamera;
            Debug.Log("Registered AS3D camera callback");
        }
    }

    public AutoStereoCamera GetAS3DComponent()
    {
        return m_scriptInstance;
    }

    public void Reset()
    {
        if (m_scriptInstance)
        {
            Destroy(m_scriptInstance);
        }
        m_scriptInstance = null;
    }

    private void FixedUpdate()
    {
        if (m_userDefinedMainCamera != null)
        {
            RegisterMainCamera(m_userDefinedMainCamera);
        }
        else if(m_takeCameraFromScene)
        {
            Camera[] cameras = GameObject.FindObjectsOfType<Camera>();

            foreach (Camera cam in cameras)
            {
                RegisterMainCamera(cam);
            }
        }
    }

    private void RegisterMainCamera(Camera cam)
    {
        AutoStereoCamera script = cam.GetComponent<AutoStereoCamera>();

        if (cam.enabled == false && script != null)
        {
            Debug.Log("Disabled AutoStereoCamera " + cam.name + " with tag " + cam.tag);
            Destroy(script);
            m_scriptInstance = null;
            return;
        }

        if (m_ignoreTag || cam.tag == m_tag)
        {
            // add the script to the main camera if it does not have it already
            if (script == null)
            {
                m_scriptInstance = cam.gameObject.AddComponent<AutoStereoCamera>();
                Debug.Log("Initialized AutoStereoCamera " + cam.name + " with tag " + cam.tag);
            }
            else
            {
                m_scriptInstance = script;
            }

            m_scriptInstance.m_uiOverlayTexture = m_uiOverlayTexture;
        }
    }

    //public void OnDestory()
    //{
    //    // remove the callback when disabling object
    //    Camera.onPreRender -= RegisterMainCamera;
    //}
}
