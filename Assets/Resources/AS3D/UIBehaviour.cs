using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBehaviour : MonoBehaviour
{
    [SerializeField]
    private AutoStereoCamera m_controller = null;
    private InitCamera m_host = null;

    [SerializeField]
    private Toggle m_bgrToggle;
    [SerializeField]
    private Toggle m_invertViewToggle;
    [SerializeField]
    private Toggle m_rightToLeftToggle;
    [SerializeField]
    public Toggle m_useFocusGizmoToggle;

    [SerializeField]
    private Text m_renderViewsText;
    [SerializeField]
    private Slider m_renderViewsSlider;
    [SerializeField]
    private Text m_eyeSeparationText;
    [SerializeField]
    private Slider m_eyeSeparationSlider;
    [SerializeField]
    private Text m_focusLengthText;
    [SerializeField]
    public Slider m_focusLengthSlider;
    [SerializeField]
    private Text m_viewShiftText;
    [SerializeField]
    private Slider m_viewShiftSlider;
    [SerializeField]
    private InputField m_lenticularSlopeFieldY;
    [SerializeField]
    private InputField m_lenticularSlopeFieldX;
    [SerializeField]
    private Text m_resolutionFractionText;
    [SerializeField]
    private Slider m_resolutionFractionSlider;

    [SerializeField]
    private InputField m_jsonFileNameInput;
    [SerializeField]
    private Dropdown m_settingsDropdown;

    private Canvas m_canvas;

    public void Awake()
    {
        DontDestroyOnLoad(transform.root);

        if(m_controller == null)
        {
            GameObject hostObj = GameObject.Find("AS3D_CAMERA_HOST_OBJECT");
            Debug.Assert(hostObj != null, "Camera host must be initialized first [AS3D / Initialize]");
            m_host = hostObj.GetComponent<InitCamera>();
            Debug.Assert(m_host != null, "InitCamera component not present");
        }

        m_canvas = GetComponent<Canvas>();
        m_canvas.enabled = false;
    }

    private void InitializeUI()
    {
        m_bgrToggle.isOn = m_controller.m_screenSettings.m_bgrDisplay;
        m_invertViewToggle.isOn = m_controller.m_screenSettings.m_invertViewport;
        m_rightToLeftToggle.isOn = m_controller.m_screenSettings.m_rightToLeft;
        m_useFocusGizmoToggle.isOn = m_controller.m_useFocusGizmo;
        if(m_controller.m_focusGizmo == null)
        {
            m_useFocusGizmoToggle.interactable = false;
        }
        else
        {
            m_useFocusGizmoToggle.interactable = true;
        }

        m_renderViewsText.text = m_controller.m_screenSettings.m_renderViewCount.ToString();
        m_renderViewsSlider.value = m_controller.m_screenSettings.m_renderViewCount;
        m_eyeSeparationText.text = m_controller.m_eyeSeparation.ToString();
        m_eyeSeparationSlider.value = m_controller.m_eyeSeparation;
        m_focusLengthText.text = m_controller.m_focusDistance.ToString();
        m_focusLengthSlider.value = m_controller.m_focusDistance;
        m_viewShiftText.text = m_controller.m_screenSettings.m_viewShift.ToString();
        m_viewShiftSlider.value = m_controller.m_screenSettings.m_viewShift;
        m_lenticularSlopeFieldY.text = m_controller.m_screenSettings.m_lenticularSlopeY.ToString();
        m_lenticularSlopeFieldX.text = m_controller.m_screenSettings.m_lenticularSlopeX.ToString();
        m_resolutionFractionText.text = m_controller.m_resolutionScale == 1 ? "" : "1/";
        m_resolutionFractionText.text += m_controller.m_resolutionScale.ToString();
        m_resolutionFractionSlider.value = m_controller.m_resolutionScale;

        for(int i = 0; i < m_settingsDropdown.options.Count; ++i)
        {
            if(m_settingsDropdown.options[i].text == m_controller.m_screenSettings.m_name)
            {
                m_settingsDropdown.value = i;
            }
        }
    }

    public void FixedUpdate()
    {
        if(m_host != null)
        {
            m_controller = m_host.GetAS3DComponent();
        }

        if(m_controller == null)
        {
            return;
        }

        List<string> options = m_controller.GetSettingsList();

        if (m_settingsDropdown.options.Count != options.Count)
        {
            m_settingsDropdown.ClearOptions();
            m_settingsDropdown.AddOptions(options);
        }

        InitializeUI();

        m_canvas.enabled = m_controller != null;
    }

    public void WriteJson()
    {
        if(m_jsonFileNameInput.text != "")
        {
            m_controller.WriteConfig(m_jsonFileNameInput.text);
        }
        else
        {
            Debug.LogWarning("No name for configuration specified");
        }
    }

    public void LoadJson(Dropdown state)
    {
        string jsonFileName = state.options[state.value].text;
        m_controller.LoadConfig(jsonFileName);
    }

    public void BGRToggle(Toggle state)
    {
        m_controller.m_screenSettings.m_bgrDisplay = state.isOn;
    }

    public void InvertViewToggle(Toggle state)
    {
        m_controller.m_screenSettings.m_invertViewport = state.isOn;
    }

    public void RightToLeft(Toggle state)
    {
        m_controller.m_screenSettings.m_rightToLeft = state.isOn;
    }

    public void UseFocusGizmo(Toggle state)
    {
        if(m_controller.m_focusGizmo != null)
        {
            m_controller.m_useFocusGizmo = state.isOn;
            m_focusLengthSlider.interactable = !state.isOn;
            m_controller.m_focusGizmo.GetComponent<SpriteRenderer>().enabled = state.isOn && m_controller.m_showGui;
            m_controller.m_zoomFocusGizmo.GetComponent<SpriteRenderer>().enabled = !state.isOn && m_controller.m_showGui;
        }
    }

    public void RenderViews(Slider state)
    {
        m_renderViewsText.text = ((int)state.value).ToString();
        m_controller.m_screenSettings.m_renderViewCount = (int)state.value;
    }

    public void EyeSperaration(Slider state)
    {
        state.value = (float)Math.Round(state.value, 3);
        m_eyeSeparationText.text = (state.value).ToString();
        m_controller.m_eyeSeparation = state.value;
    }

    public void FocusLength(Slider state)
    {
        if (!m_controller.m_useFocusGizmo)
        {
            state.value = (float)Math.Round(state.value, 1);
            m_focusLengthText.text = (state.value).ToString();
            m_controller.m_focusDistance = state.value;
            m_controller.m_zoomFocusGizmo.transform.localPosition = Vector3.forward * m_controller.m_focusDistance;
        }
    }

    public void ViewShift(Slider state)
    {
        m_viewShiftText.text = ((int)state.value).ToString();
        m_controller.m_screenSettings.m_viewShift = (int)state.value;
    }

    public void LenticularSlopeY(InputField state)
    {
        if (state.text != "")
        {
            m_controller.m_screenSettings.m_lenticularSlopeY = Mathf.Abs(Convert.ToSingle(state.text));
        }
    }

    public void LenticularSlopeX(InputField state)
    {
        if (state.text != "")
        {
            m_controller.m_screenSettings.m_lenticularSlopeX = Mathf.Abs(Convert.ToSingle(state.text));
        }
    }

    public void ResolutionScale(Slider state)
    {
        m_controller.m_resolutionScale = (int)state.value;
        m_controller.CreateRenderTargets();
    }
}
