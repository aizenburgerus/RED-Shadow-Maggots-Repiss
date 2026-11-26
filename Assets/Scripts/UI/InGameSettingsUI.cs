using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JSAM;
using static Facade;

public class InGameSettingsUI : BaseGameUI
{
    [SerializeField] OptimizedCanvas[] uiCanvases;

    [Header("Graphics")]
    [SerializeField] Slider qualitySlider;
    [SerializeField] Toggle postProcessingToggle;
    [SerializeField] Toggle antiAliasingToggle;

    [Header("Volume")]
    [SerializeField] Slider masterSlider;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider soundSlider;
    [SerializeField] Slider voiceSlider;

    private void OnEnable()
    {
        AudioManager.OnMasterVolumeChanged += UpdateMaster;
        AudioManager.OnMusicVolumeChanged += UpdateMusic;
        AudioManager.OnSoundVolumeChanged += UpdateSound;
        AudioManager.OnVoiceVolumeChanged += UpdateVoice;

        qualitySlider.minValue = 0;
        qualitySlider.maxValue = QualitySettings.names.Length - 1;
        qualitySlider.wholeNumbers = true;
        qualitySlider.onValueChanged.AddListener(SetQualityLevel);
        optimizedCanvas.onCanvasShow.AddListener(RefreshSettingsUI);
        optimizedCanvas.onCanvasStartHide.AddListener(SaveSettings);
    }

    private void OnDisable()
    {
        AudioManager.OnMasterVolumeChanged -= UpdateMaster;
        AudioManager.OnMusicVolumeChanged -= UpdateMusic;
        AudioManager.OnSoundVolumeChanged -= UpdateSound;
        AudioManager.OnVoiceVolumeChanged -= UpdateVoice;
        qualitySlider.onValueChanged.RemoveListener(SetQualityLevel);
        optimizedCanvas.onCanvasShow.RemoveListener(RefreshSettingsUI);
        optimizedCanvas.onCanvasStartHide.RemoveListener(SaveSettings);
        SaveSettings();
    }

    void OnApplicationPause(bool paused)
    {
        if (paused) SaveSettings();
    }

    void OnApplicationFocus(bool focused)
    {
        if (!focused) SaveSettings();
    }

    void SaveSettings()
    {
        if (!AudioManager.Instance) return;
        var audio = AudioManager.InternalInstance;
        if (audio) audio.SaveVolumeSettings();
    }

    void RefreshSettingsUI()
    {
        qualitySlider.SetValueWithoutNotify(QualitySettings.GetQualityLevel());
        var graphics = graphicsSettings;
        bool hasPostProcessing = graphics && graphics.PostProcessing;
        postProcessingToggle.interactable = hasPostProcessing;
        antiAliasingToggle.interactable = hasPostProcessing;
        if (hasPostProcessing)
        {
            postProcessingToggle.SetIsOnWithoutNotify(graphics.PostProcessing.enabled);
            antiAliasingToggle.SetIsOnWithoutNotify(graphics.PostProcessing.antialiasingMode !=
                UnityEngine.Rendering.PostProcessing.PostProcessLayer.Antialiasing.None);
        }
        UpdateVolumeSliders();
    }

    void SetQualityLevel(float level)
    {
        if (!optimizedCanvas.IsVisible || !graphicsSettings) return;
        graphicsSettings.SetGraphicsLevel(level);
    }

    public override void ShowUI()
    {
        optimizedCanvas.Show();
        playerControlManager.SetControlMode(PlayerControlMode.InSettings);
    }

    public override void HideUI()
    {
        optimizedCanvas.Hide();
        playerControlManager.ReturnControl();
    }

    public void HideAllUIExcept(int ignoreIndex = -1)
    {
        for (int i = 0; i < uiCanvases.Length; i++)
        {
            //uiCanvases[i].SetActive(i == ignoreIndex);
            uiCanvases[i].gameObject.SetActive(i == ignoreIndex);
        }
    }

    public void TogglePostProcessing()
    {
        if (!optimizedCanvas.IsVisible || !graphicsSettings) return;
        graphicsSettings.SetPostProcessing(postProcessingToggle.isOn);
    }

    public void ToggleAntiAliasing()
    {
        if (!optimizedCanvas.IsVisible || !graphicsSettings) return;
        graphicsSettings.SetAntiAliasing(antiAliasingToggle.isOn);
    }

    public void ShowGraphicsUI()
    {
        HideAllUIExcept(0);
    }
    
    public void ShowVolumeUI()
    {
        HideAllUIExcept(1);
    }

    void UpdateMaster(float vol) => masterSlider.SetValueWithoutNotify(vol);
    void UpdateMusic(float vol) => musicSlider.SetValueWithoutNotify(vol);
    void UpdateSound(float vol) => soundSlider.SetValueWithoutNotify(vol);
    void UpdateVoice(float vol) => voiceSlider.SetValueWithoutNotify(vol);

    void UpdateVolumeSliders()
    {
        UpdateMaster(AudioManager.MasterVolume);
        UpdateMusic(AudioManager.MusicVolume);
        UpdateSound(AudioManager.SoundVolume);
        UpdateVoice(AudioManager.VoiceVolume);
    }

    public void SetMasterVolume(float val)
    {
        if (!optimizedCanvas.IsVisible) return;
        AudioManager.MasterVolume = val;
    }

    public void SetSoundVolume(float val)
    {
        if (!optimizedCanvas.IsVisible) return;
        AudioManager.SoundVolume = val;
    }

    public void SetMusicVolume(float val)
    {
        if (!optimizedCanvas.IsVisible) return;
        AudioManager.MusicVolume = val;
    }

    public void SetVoiceVolume(float val)
    {
        if (!optimizedCanvas.IsVisible) return;
        AudioManager.VoiceVolume = val;
    }
}
