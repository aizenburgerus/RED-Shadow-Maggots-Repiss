using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class GraphicsSettings : BasicSingleton<GraphicsSettings>
{
    const string QualityLevelKey = "RSM_GRAPHICS_QUALITY";
    const string PostProcessingKey = "RSM_POST_PROCESSING";
    const string AntiAliasingKey = "RSM_ANTI_ALIASING";

    [SerializeField] PostProcessLayer cameraPostProcess;
    public PostProcessLayer PostProcessing { get { return cameraPostProcess; } }

    [SerializeField] PostProcessVolume postProcessVolume;
    public PostProcessVolume MainPostProcessVolume { get { return postProcessVolume; } }

    int shadowLevel = 5;
    public int ShadowLevel
    {
        get { return Mathf.Min(QualitySettings.GetQualityLevel(), shadowLevel); }
    }
    public static System.Action<int> OnQualityLevelChanged;

    void Awake()
    {
        int level = PlayerPrefs.GetInt(QualityLevelKey, QualitySettings.GetQualityLevel());
        level = Mathf.Clamp(level, 0, QualitySettings.names.Length - 1);
        QualitySettings.SetQualityLevel(level);
        OnQualityLevelChanged?.Invoke(level);

        if (cameraPostProcess)
        {
            cameraPostProcess.enabled = PlayerPrefs.GetInt(PostProcessingKey,
                cameraPostProcess.enabled ? 1 : 0) != 0;
            int antiAliasing = PlayerPrefs.GetInt(AntiAliasingKey,
                (int)cameraPostProcess.antialiasingMode);
            if (System.Enum.IsDefined(typeof(PostProcessLayer.Antialiasing), antiAliasing))
            {
                cameraPostProcess.antialiasingMode = (PostProcessLayer.Antialiasing)antiAliasing;
            }
        }
    }

    public void SetGraphicsLevel(float newLevel)
    {
        int level = Mathf.Clamp((int)newLevel, 0, QualitySettings.names.Length - 1);
        QualitySettings.SetQualityLevel(level);
        PlayerPrefs.SetInt(QualityLevelKey, level);
        PlayerPrefs.Save();
        OnQualityLevelChanged?.Invoke(level);
    }

    public void SetPostProcessing(bool enabled)
    {
        if (!cameraPostProcess) return;
        cameraPostProcess.enabled = enabled;
        PlayerPrefs.SetInt(PostProcessingKey, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetAntiAliasing(bool enabled)
    {
        if (!cameraPostProcess) return;
        cameraPostProcess.antialiasingMode = enabled
            ? PostProcessLayer.Antialiasing.FastApproximateAntialiasing
            : PostProcessLayer.Antialiasing.None;
        PlayerPrefs.SetInt(AntiAliasingKey, (int)cameraPostProcess.antialiasingMode);
        PlayerPrefs.Save();
    }

    [ContextMenu(nameof(DebugTest))]
    void DebugTest()
    {
        Debug.Log(QualitySettings.names[QualitySettings.GetQualityLevel()]);
    }
}
