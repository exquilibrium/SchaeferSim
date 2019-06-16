using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DisplayDesc
{
    public string m_name = "";
    public List<string> m_deviceIds = new List<string>();
    public int m_renderViewCount = 8;
    public int m_viewShift = 4;
    public float m_lenticularSlopeY = 2f;
    public float m_lenticularSlopeX = 3f;
    public bool m_bgrDisplay = false;
    public bool m_invertViewport = true;
    public bool m_rightToLeft = true;
}

[RequireComponent(typeof(Camera))]
public class AutoStereoCamera : MonoBehaviour
{
    private const int m_maxRenderViews = 8;

    // input
    public const KeyCode m_toggleUIKey1 = KeyCode.F1;
    public const KeyCode m_toggleUIKey2 = KeyCode.F3;

    public const KeyCode m_toogleJoJo = KeyCode.J;

    public const KeyCode m_resScalePlus = KeyCode.PageUp;
    public const KeyCode m_resScaleMinus = KeyCode.PageDown;

    public const KeyCode m_cameraAnglePlus = KeyCode.Home;
    public const KeyCode m_cameraAngleMinus = KeyCode.End;

    public const KeyCode m_focusDistancePlus = KeyCode.Insert;
    public const KeyCode m_focusDistanceMinus = KeyCode.Delete;


    public const KeyCode m_focusModeSwitch = KeyCode.Tab;

    // autostereo data
    public DisplayDesc m_screenSettings = new DisplayDesc();
    [SerializeField]
    public float m_eyeSeparation = 0.045f;
    [SerializeField]
    public float m_focusDistance = 2.3f;
    [SerializeField]
    public int m_resolutionScale = 1;
    [SerializeField]
    public bool m_jojoEffect = false;

    private string m_configDirPath = null;

    private int m_depthResolutionBits = 32;

    private bool m_reconstructView = false;

    public Texture m_uiOverlayTexture = null;

    private List<Camera> m_renderCameras = new List<Camera>();
    private RenderTexture[] m_colorTargets;

    // reconstruction:
    private RenderTexture m_depthTarget = null;

    private Material m_material;

    private Camera m_sourceCamera;
    private CameraClearFlags m_originalClearFlag;

    private bool m_originalCursorVisibility;
    private CursorLockMode m_originalLockMode;
    private GameObject autoStereoUI;

    private Matrix4x4 m_projectionMat;

    public bool m_showGui = false;
    public GameObject m_focusGizmo;
    public GameObject m_zoomFocusGizmo;
    public bool m_useFocusGizmo = true;

    private void Start()
    {
        if (m_uiOverlayTexture == null)
        {
            Debug.LogError("No overlay texture was set");
        }

        m_focusGizmo = GameObject.Find("AS3D_FOCUS_GIZMO");
        m_zoomFocusGizmo = Instantiate((GameObject)Resources.Load("AS3D/AS3D_FOCUS_GIZMO", typeof(GameObject)), transform);
        m_zoomFocusGizmo.transform.localPosition = Vector3.forward * m_focusDistance;
        m_zoomFocusGizmo.name = "AS3D_ZOOM_FOCUS_GIZMO";

        if (m_focusGizmo != null)
        {
            m_focusGizmo.GetComponent<SpriteRenderer>().enabled = false;
            m_useFocusGizmo = true;
        }
        else
        {
            m_useFocusGizmo = false;
        }

        m_zoomFocusGizmo.GetComponent<SpriteRenderer>().enabled = false;

        m_configDirPath = $"{Application.persistentDataPath}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}AS3D_Screen_Settings";
        Directory.CreateDirectory(m_configDirPath); // create directory for jsons to be saved in
        WriteDefaultJsonSettings();

        List<string> displayIds = DisplayIdentifier.GetDisplayIDs();
        Debug.Log("Detected displays: " + string.Join(", ", displayIds));

        bool configFound = false;
        List<DisplayDesc> configs = LoadAllConfigs();
        foreach (string ID in displayIds)
        {
            foreach (DisplayDesc config in configs)
            {
                if (config.m_deviceIds.Contains(ID))
                {
                    m_screenSettings = config;
                    configFound = true;
                    Debug.Log($"Found config '{config.m_name}' for monitor '{ID}'");
                    break;
                }
            }
        }

        if (!configFound)
        {
            Debug.LogWarning("Could not detect AS3D display or configuration file.");
        }

        m_originalCursorVisibility = Cursor.visible;
        m_originalLockMode = Cursor.lockState;
        autoStereoUI = GameObject.Find("AutoStereoUI");
        if (autoStereoUI == null)
        {
            Debug.LogWarning("AutoStereoUI not found in scene");
        }
        else
        {
            autoStereoUI.SetActive(false);
            if (m_useFocusGizmo)
            {
                autoStereoUI.GetComponentInChildren<UIBehaviour>().m_focusLengthSlider.interactable = false;
                Debug.Log("Override focus length slider with AS3D_FOCUS_GIZMO in scene");
            }
        }

        m_sourceCamera = GetComponent<Camera>();

        if (m_reconstructView == false)
        {
            m_sourceCamera.cullingMask = 0; // main camera rendering of the scene is disabled
            m_originalClearFlag = m_sourceCamera.clearFlags;
            m_sourceCamera.clearFlags = CameraClearFlags.Nothing;
        }

        Shader as3dShader = Shader.Find("Autostereo");

        if (as3dShader != null)
        {
            m_material = new Material(as3dShader);
        }
        else
        {
            Debug.Assert(false, "Autostereo shader could not be found.");
            return;
        }

        m_colorTargets = new RenderTexture[m_maxRenderViews];

        CreateRenderCameras();
    }

    private void Update()
    {
        GameObject focusGizmo = m_useFocusGizmo ? m_focusGizmo : m_zoomFocusGizmo;
        m_focusDistance = Vector3.Distance(transform.position, focusGizmo.transform.position);

        // Keyboard Input

        if ((Input.GetKeyDown(m_toggleUIKey1) && Input.GetKey(m_toggleUIKey2)) || (Input.GetKey(m_toggleUIKey1) && Input.GetKeyDown(m_toggleUIKey2)))
        {
            if (autoStereoUI == null)
            {
                Debug.LogWarning("AutoStereoUI not found in scene");
                return;
            }

            m_showGui = !m_showGui;
            Cursor.lockState = m_showGui ? CursorLockMode.None : m_originalLockMode;
            Cursor.visible = m_showGui ? true : m_originalCursorVisibility;
            focusGizmo.GetComponent<SpriteRenderer>().enabled = m_showGui;
            autoStereoUI.SetActive(m_showGui);
        }

        if (Input.GetKeyDown(m_resScalePlus))
        {
            m_resolutionScale = Mathf.Clamp(m_resolutionScale + 1, 1, 8);
            CreateRenderTargets();
        }

        if (Input.GetKeyDown(m_resScaleMinus))
        {
            m_resolutionScale = Mathf.Clamp(m_resolutionScale - 1, 1, 8);
            CreateRenderTargets();
        }

        if (Input.GetKeyDown(m_cameraAnglePlus))
        {
            m_eyeSeparation = Mathf.Clamp(m_eyeSeparation + 0.005f, 0, 0.07f);
        }

        if (Input.GetKeyDown(m_cameraAngleMinus))
        {
            m_eyeSeparation = Mathf.Clamp(m_eyeSeparation - 0.005f, 0, 0.07f);
        }

        if (Input.GetKey(m_focusDistancePlus))
        {
            m_focusDistance = Mathf.Min(m_focusDistance + 0.1f, 15);
        }

        if (Input.GetKeyDown(m_toogleJoJo))
        {
            m_jojoEffect = !m_jojoEffect;
        }

        if (Input.GetKey(m_focusDistanceMinus))
        {
            m_focusDistance = Mathf.Max(m_focusDistance - 0.1f, 0.1f);
        }

        if(Input.GetKeyDown(m_focusModeSwitch))
        {
            if (m_focusGizmo != null)
            {
                m_useFocusGizmo = !m_useFocusGizmo;
                autoStereoUI.GetComponentInChildren<UIBehaviour>().m_useFocusGizmoToggle.isOn = m_useFocusGizmo;
                autoStereoUI.GetComponentInChildren<UIBehaviour>().m_focusLengthSlider.interactable = !m_useFocusGizmo;
                m_focusGizmo.GetComponent<SpriteRenderer>().enabled = m_useFocusGizmo && m_showGui;
                m_zoomFocusGizmo.GetComponent<SpriteRenderer>().enabled = !m_useFocusGizmo && m_showGui;
            }
        }
    }

    private void OnPreRender()
    {
        //DrawCameraRays(0.1f); // draw look direction rays for active render cameras

        if (m_uiOverlayTexture != null)
        {
            m_material.EnableKeyword("UI_TEXTURE_ON");
            m_material.SetTexture("uiOverlayTexture", m_uiOverlayTexture);
        }
        else
        {
            m_material.DisableKeyword("UI_TEXTURE_ON");
        }

        if (m_reconstructView)
        {
            m_material.EnableKeyword("RECONSTRUCT");

            UpdateRenderReconstructCamera();
        }
        else
        {
            m_material.DisableKeyword("RECONSTRUCT");

            UpdateRenderCameras();
        }

        //Update JojoMap | Returns identity if m_jojoEffect is false
        int[] jojoMap = getJojoMapAndManageCameras(m_screenSettings);
        //todo float list to int
        List<float> jojoFloats = new List<float>();
        foreach (float value in jojoMap)
        {
            jojoFloats.Add(value);
        }


        m_material.SetFloatArray("u_jojoMap", jojoFloats);
        m_material.SetInt("u_textureXSize", Screen.width);
        m_material.SetInt("u_viewshift", m_screenSettings.m_viewShift);
        m_material.SetFloat("u_lenticularSlope", m_screenSettings.m_lenticularSlopeY / m_screenSettings.m_lenticularSlopeX);
        m_material.SetInt("u_viewportInvert", m_screenSettings.m_invertViewport ? 1 : 0);
        m_material.SetInt("u_bgrDisplay", m_screenSettings.m_bgrDisplay ? 1 : 0);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, m_material);
    }

    public void CreateRenderTargets()
    {
        for (int i = 0; i < m_colorTargets.Length; ++i)
        {
            if (m_colorTargets[i] != null)
            {
                m_colorTargets[i].Release();
            }
            m_colorTargets[i] = new RenderTexture(Screen.width / m_resolutionScale, Screen.height / m_resolutionScale, 0, RenderTextureFormat.Default);

            m_colorTargets[i].filterMode = FilterMode.Point;
            m_colorTargets[i].useMipMap = false;
            m_colorTargets[i].dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
            m_colorTargets[i].Create();

            m_material.SetTexture("colorView_" + i, m_colorTargets[i]);

            if (m_reconstructView)
            {
                //m_sourceCamera.targetTexture = m_colorTargets[i];
                break;
            }

            m_renderCameras[i].targetTexture = m_colorTargets[i];
        }

        if (m_reconstructView)
        {
            if (m_depthTarget != null)
            {
                m_depthTarget.Release();
            }

            m_depthTarget = new RenderTexture(Screen.width / m_resolutionScale, Screen.height / m_resolutionScale, m_depthResolutionBits, RenderTextureFormat.Depth); //  RenderTextureFormat.RFloat
            m_depthTarget.filterMode = FilterMode.Point;
            m_depthTarget.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
            m_depthTarget.useMipMap = false;
            m_depthTarget.hideFlags = HideFlags.DontSave;
            m_depthTarget.Create();

            m_sourceCamera.SetTargetBuffers(m_colorTargets[0].colorBuffer, m_depthTarget.depthBuffer);

            m_material.SetTexture("depthView_0", m_depthTarget);
        }

        //Debug.Log("Render Target Res: " + m_colorTargets[0].width + "x" + m_colorTargets[0].height);
    }

    private void CreateRenderCameras()
    {
        m_renderCameras.Clear();

        for (int i = 0; i < m_maxRenderViews; ++i)
        {
            Camera camera = new GameObject("Camera_" + i).AddComponent<Camera>();
            camera.enabled = false;
            camera.transform.SetParent(transform, false);

            if (m_sourceCamera != null)
            {
                camera.clearFlags = m_originalClearFlag;
                camera.backgroundColor = m_sourceCamera.backgroundColor;
                camera.nearClipPlane = m_sourceCamera.nearClipPlane;
                camera.farClipPlane = m_sourceCamera.farClipPlane;
                camera.fieldOfView = m_sourceCamera.fieldOfView;
                camera.usePhysicalProperties = m_sourceCamera.usePhysicalProperties;
                camera.focalLength = m_sourceCamera.focalLength;
                camera.sensorSize = m_sourceCamera.sensorSize;
                camera.depth = m_sourceCamera.depth - 1; // render before main camera
            }

            m_renderCameras.Add(camera);
            m_projectionMat = camera.projectionMatrix;
        }

        if (m_reconstructView)
        {
            m_sourceCamera.enabled = true;
        }

        CreateRenderTargets();
    }

    private void UpdateRenderReconstructCamera()
    {
        m_material.SetInt("u_numRenderViews", m_screenSettings.m_renderViewCount);
        m_sourceCamera.enabled = true;

        Matrix4x4 sourceInvViewProj = (m_sourceCamera.projectionMatrix * m_sourceCamera.worldToCameraMatrix).inverse;
        m_material.SetMatrix("u_sourceInvViewProj", sourceInvViewProj);

        Matrix4x4[] targetViewProjs = new Matrix4x4[m_screenSettings.m_renderViewCount];

        float firstOffset = (m_eyeSeparation / m_screenSettings.m_renderViewCount) * (m_screenSettings.m_renderViewCount - 1) * 0.5f;

        for (int i = 0; i < m_screenSettings.m_renderViewCount; ++i) // enable and position all currently needed render cameras
        {
            Camera currentRenderCam = m_renderCameras[i];

            currentRenderCam.transform.localPosition = new Vector3(firstOffset - i * (m_eyeSeparation / m_screenSettings.m_renderViewCount), 0, 0);

            float shearFactor = -currentRenderCam.transform.localPosition.x / m_focusDistance;

            Matrix4x4 shearMatrix = Matrix4x4.identity;
            shearMatrix[0, 2] = shearFactor;
            currentRenderCam.projectionMatrix = m_projectionMat * shearMatrix;

            targetViewProjs[i] = currentRenderCam.projectionMatrix * currentRenderCam.worldToCameraMatrix;

            currentRenderCam.enabled = false;
        }

        m_material.SetMatrixArray("u_targetViewProj", targetViewProjs);
    }

    private void UpdateRenderCameras()
    {
        m_material.SetInt("u_numRenderViews", m_screenSettings.m_renderViewCount);

        float firstOffset = (m_eyeSeparation / m_screenSettings.m_renderViewCount) * (m_screenSettings.m_renderViewCount - 1) * 0.5f;

        int i = 0;
        for (; i < m_screenSettings.m_renderViewCount; ++i) // enable and position all currently needed render cameras
        {
            Camera currentRenderCam = m_renderCameras[i];

            currentRenderCam.transform.localPosition = new Vector3(firstOffset - i * (m_eyeSeparation / m_screenSettings.m_renderViewCount), 0, 0);

            float shearFactor = -currentRenderCam.transform.localPosition.x / m_focusDistance;

            // focus distance is in view space. Writing directly into projection matrix would require focus distance to be in projection space
            Matrix4x4 shearMatrix = Matrix4x4.identity;
            shearMatrix[0, 2] = shearFactor;
            currentRenderCam.projectionMatrix = m_projectionMat * shearMatrix;
            currentRenderCam.enabled = true;
        }

        for (; i < m_renderCameras.Count; ++i) // disable all currently not needed render cameras
        {
            m_renderCameras[i].enabled = false;
        }
    }

    private void DrawCameraRays(float duration)
    {
        for (int i = 0; i < m_screenSettings.m_renderViewCount; ++i)
        {
            Debug.DrawRay(m_renderCameras[i].transform.position, m_renderCameras[i].transform.position + m_renderCameras[i].transform.forward * 100f, Color.red, duration);
        }
    }

    private void WriteDefaultJsonSettings()
    {
        TextAsset[] defaultSettings = Resources.LoadAll<TextAsset>("AS3D" + Path.DirectorySeparatorChar + "DefaultSettings");
        for (int i = 0; i < defaultSettings.Length; ++i)
        {
            File.WriteAllText(m_configDirPath + Path.DirectorySeparatorChar + defaultSettings[i].name + ".json", defaultSettings[i].text);
        }
    }

    public void WriteConfig(string fileName)
    {
        TextAsset[] defaultConfigs = Resources.LoadAll<TextAsset>("AS3D" + Path.DirectorySeparatorChar + "DefaultSettings");

        foreach (var defaultConfig in defaultConfigs)
        {
            if (defaultConfig.name == fileName)
            {
                Debug.LogWarning("Overwriting default config files is prohibited. Filename: " + fileName);
                return;
            }
        }

        string jsonScreenSettings = JsonUtility.ToJson(m_screenSettings, true);
        string jsonFilePath = m_configDirPath + Path.DirectorySeparatorChar + fileName + ".json";
        File.WriteAllText(jsonFilePath, jsonScreenSettings);
        Debug.Log("Wrote Screen Settings json to: " + jsonFilePath);
    }

    // used for initial automatic screen detection
    private List<DisplayDesc> LoadAllConfigs()
    {
        List<DisplayDesc> configs = new List<DisplayDesc>();

        foreach (string configFilename in Directory.EnumerateFiles(m_configDirPath, "*.json"))
        {
            string configFile = File.ReadAllText(configFilename);
            DisplayDesc config = JsonUtility.FromJson<DisplayDesc>(configFile);
            if (string.IsNullOrWhiteSpace(config.m_name))
            {
                FileInfo fileInfo = new FileInfo(configFilename);
                config.m_name = fileInfo.Name.Substring(0, fileInfo.Name.LastIndexOf("."));
            }
            configs.Add(config);
        }

        return configs;
    }

    // used for loading at runtime from menu
    public void LoadConfig(string fileName)
    {
        string jsonFileName = fileName + ".json";
        string jsonFilePath = m_configDirPath + Path.DirectorySeparatorChar + jsonFileName;
        m_screenSettings = JsonUtility.FromJson<DisplayDesc>(File.ReadAllText(jsonFilePath));
        Debug.Log("Read json file from path: " + jsonFilePath);

    }

    public List<string> GetSettingsList()
    {
        FileInfo[] fileInfos = new DirectoryInfo(m_configDirPath).GetFiles();
        List<string> fileNames = new List<string>();
        for (int i = 0; i < fileInfos.Length; ++i)
        {
            string fileInfoName = fileInfos[i].Name;
            if (!fileInfoName.EndsWith(".json"))
            {
                continue;
            }
            fileInfoName = fileInfoName.Substring(0, fileInfoName.LastIndexOf(".json"));
            fileNames.Add(fileInfoName);
        }
        return fileNames;
    }

    public int[] getJojoMapAndManageCameras(DisplayDesc desc)
    {
        int[] map = GetJojoMap(desc);
        DeactivateUnusedCameras(desc.m_renderViewCount, map);
        return map;
    }

    private int[] GetJojoMap (DisplayDesc desc)
    {
        int rightShift = RightShift(desc.m_renderViewCount);
        int numRenderViews = desc.m_renderViewCount;
        int[] map = new int[numRenderViews];
        if (m_jojoEffect)
        {
            int jojo = 0;
            for (int i = 0; i < numRenderViews; i++)
            {
                //pattern for 8 cameras -> [0,1,2,3,4,3,2,1]
                int x = (i < numRenderViews / 2) ? +1 : -1;
                map[i] = jojo + rightShift;
                jojo += x;
            }
        }
        else
        {
            for (int i = 0; i < numRenderViews; i++)
            {
                map[i] = i; //with the jojo deactivated, there is no maping, the inidices are their identities
            }
        }
        return map;
    }

    private int RightShift (int num_Renderviews)
    {
        int shift = 5;
        for (int iterator = 2; iterator <= num_Renderviews; iterator += 2)
        {
            shift--;
        }
        return num_Renderviews < 4 ? shift - 1 : shift;
    }

    private void DeactivateUnusedCameras(int num_Renderviews, int[] jojoMap)
    {
        for(int i = 0; i < num_Renderviews; i++)
        {
            if (!m_jojoEffect)
                m_renderCameras[i].gameObject.SetActive(true);
            else
            {
                m_renderCameras[i].gameObject.SetActive(false);
                foreach (var activatedIndex in jojoMap)
                {
                    if(activatedIndex == i)
                        m_renderCameras[i].gameObject.SetActive(true);
                }
            }
        }
    }

}
