using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class PixelationPosterizeOutlineEffect : MonoBehaviour
{
    [Header("Shaders")]
    public Shader effectShader;

    [Header("Pixelation")]
    public bool enablePixelation = true;
    [Range(1, 256)]
    public float pixelSize = 64;

    [Header("Posterization")]
    public bool enablePosterize = true;
    [Range(2, 16)]
    public float posterizeLevels = 4;

    [Header("Outline")]
    public bool enableOutline = true;
    public Color outlineColor = Color.black;
    [Range(1, 5)]
    public float outlineThickness = 1.0f;
    [Range(0.05f, 1.0f)]
    public float outlineThreshold = 0.2f;

    [Header("Desaturation")]
    public bool enableDesaturation = false;
    [Range(0, 1)]
    public float desaturationAmount = 0.5f;

    [Header("Contrast")]
    public bool enableContrast = false;
    [Range(0, 3)]
    public float contrastAmount = 1.0f;

    private Material effectMaterial;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();

        // Enable depth texture on the camera
        cam.depthTextureMode |= DepthTextureMode.Depth;

        if (effectShader == null)
        {
            effectShader = Shader.Find("Hidden/PixelationPosterizeOutlineShader");
        }

        if (effectShader != null && effectShader.isSupported)
        {
            effectMaterial = new Material(effectShader);
        }
        else
        {
            Debug.LogError("Shader not found or not supported.");
            enabled = false;
        }

        // Initialize shader properties to ensure they are set correctly at start
        InitializeShaderProperties();
    }

    void InitializeShaderProperties()
    {
        if (effectMaterial == null) return;

        // Pixelation
        effectMaterial.SetFloat("_EnablePixelation", enablePixelation ? 1.0f : 0.0f);
        effectMaterial.SetFloat("_PixelSize", pixelSize);

        // Posterization
        effectMaterial.SetFloat("_EnablePosterize", enablePosterize ? 1.0f : 0.0f);
        effectMaterial.SetFloat("_PosterizeLevels", posterizeLevels);

        // Outline
        effectMaterial.SetFloat("_EnableOutline", enableOutline ? 1.0f : 0.0f);
        effectMaterial.SetColor("_OutlineColor", outlineColor);
        effectMaterial.SetFloat("_OutlineThickness", outlineThickness);
        effectMaterial.SetFloat("_OutlineThreshold", outlineThreshold);

        // Desaturation
        effectMaterial.SetFloat("_EnableDesaturation", enableDesaturation ? 1.0f : 0.0f);
        effectMaterial.SetFloat("_DesaturationAmount", desaturationAmount);

        // Contrast
        effectMaterial.SetFloat("_EnableContrast", enableContrast ? 1.0f : 0.0f);
        effectMaterial.SetFloat("_ContrastAmount", contrastAmount);
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (effectMaterial != null)
        {
            // Update shader properties in case they've changed in the Inspector
            effectMaterial.SetFloat("_EnablePixelation", enablePixelation ? 1.0f : 0.0f);
            effectMaterial.SetFloat("_PixelSize", pixelSize);

            effectMaterial.SetFloat("_EnablePosterize", enablePosterize ? 1.0f : 0.0f);
            effectMaterial.SetFloat("_PosterizeLevels", posterizeLevels);

            effectMaterial.SetFloat("_EnableOutline", enableOutline ? 1.0f : 0.0f);
            effectMaterial.SetColor("_OutlineColor", outlineColor);
            effectMaterial.SetFloat("_OutlineThickness", outlineThickness);
            effectMaterial.SetFloat("_OutlineThreshold", outlineThreshold);

            effectMaterial.SetFloat("_EnableDesaturation", enableDesaturation ? 1.0f : 0.0f);
            effectMaterial.SetFloat("_DesaturationAmount", desaturationAmount);

            effectMaterial.SetFloat("_EnableContrast", enableContrast ? 1.0f : 0.0f);
            effectMaterial.SetFloat("_ContrastAmount", contrastAmount);

            Graphics.Blit(source, destination, effectMaterial);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }

    void OnValidate()
    {
        // Clamp values to their ranges
        pixelSize = Mathf.Clamp(pixelSize, 1, 256);
        posterizeLevels = Mathf.Clamp(posterizeLevels, 2, 16);
        outlineThickness = Mathf.Clamp(outlineThickness, 1, 5);
        outlineThreshold = Mathf.Clamp(outlineThreshold, 0.05f, 1.0f);
        desaturationAmount = Mathf.Clamp(desaturationAmount, 0, 1);
        contrastAmount = Mathf.Clamp(contrastAmount, 0, 3);

        // Initialize shader properties when values change in the Inspector
        InitializeShaderProperties();
    }
}
