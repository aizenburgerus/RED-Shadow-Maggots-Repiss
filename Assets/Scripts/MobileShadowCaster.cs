using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static Facade;

public class MobileShadowCaster : MonoBehaviour
{
    [SerializeField] Renderer[] defaultRenderers;

    [SerializeField] Renderer shadowOrigin;
    [SerializeField] float distanceOffset = 0;
    [SerializeField] float shadowOffset = 0.01f;
    [SerializeField] float maxDistance = 5;
    [SerializeField] LayerMask layerMask;
    [SerializeField] new SpriteRenderer renderer;
    //[SerializeField] new Renderer renderer;
    //Material material;
    //int colorProp;

    [SerializeField] Vector2 shadowOpacity = new Vector2(0, 200);

    bool lowQuality = false;

    private void OnEnable()
    {
        GraphicsSettings.OnQualityLevelChanged += UpdateShadowCasting;
    }

    private void OnDisable()
    {
        GraphicsSettings.OnQualityLevelChanged -= UpdateShadowCasting;
    }

    private void Start()
    {
        //colorProp = Shader.PropertyToID("_Color");
        //material = renderer.material;

        UpdateShadowCasting(QualitySettings.GetQualityLevel());
    }

    void UpdateShadowCasting(int level)
    {
        lowQuality = level < 3;

        bool useSpriteShadow = lowQuality && shadowOrigin && renderer;
        var castMode = useSpriteShadow ? ShadowCastingMode.Off : ShadowCastingMode.On;

        for (int i = 0; i < defaultRenderers.Length; i++)
        {
            if (defaultRenderers[i]) defaultRenderers[i].shadowCastingMode = castMode;
        }

        if (renderer) renderer.enabled = useSpriteShadow;
    }

    private void Update()
    {
        if (!lowQuality || !renderer) return;
        if (!shadowOrigin)
        {
            renderer.enabled = false;
            return;
        }

        Ray ray = new Ray();
        ray.direction = Vector3.down;
        ray.origin = shadowOrigin.bounds.center;
        RaycastHit hit = new RaycastHit();

        if (Physics.Raycast(ray, out hit, maxDistance + distanceOffset, layerMask))
        {
            transform.position = hit.point + Vector3.up * shadowOffset;
            transform.forward = Vector3.up;
            float distance = hit.distance - distanceOffset;
            byte alpha = (byte)Mathf.Lerp(shadowOpacity.y, shadowOpacity.x, distance / maxDistance);
            Color newColor = new Color32(0, 0, 0, alpha);
            renderer.color = newColor;
            //material.SetColor(colorProp, newColor);
        }
        renderer.enabled = hit.collider;
    }
}
