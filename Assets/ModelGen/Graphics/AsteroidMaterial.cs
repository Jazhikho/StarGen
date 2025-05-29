using UnityEngine;

/// <summary>
/// Creates and manages a material for rendering asteroids with noise-based variations
/// </summary>
[ExecuteInEditMode]
public class AsteroidMaterial : MonoBehaviour
{
    [Header("Material Settings")]
    [SerializeField] private Color baseColor = new Color(0.6f, 0.6f, 0.6f);
    [SerializeField] private float noiseScale = 10f;
    [SerializeField] private float noiseStrength = 0.3f;
    [SerializeField] private float roughness = 0.7f;
    [SerializeField] private float metallic = 0.1f;
    [SerializeField] private Texture2D noiseTex;
    
    [Header("Reference")]
    [SerializeField] private Renderer targetRenderer;
    
    private Material material;
    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    private static readonly int NoiseScaleID = Shader.PropertyToID("_NoiseScale");
    private static readonly int NoiseStrengthID = Shader.PropertyToID("_NoiseStrength");
    private static readonly int RoughnessID = Shader.PropertyToID("_Roughness");
    private static readonly int MetallicID = Shader.PropertyToID("_Metallic");
    private static readonly int NoiseTexID = Shader.PropertyToID("_NoiseTex");
    
    private void OnEnable()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }
        
        if (targetRenderer != null)
        {
            // Create a new material instance if needed
            if (material == null)
            {
                // Use standard shader for flexibility
                material = new Material(Shader.Find("Standard"));
                targetRenderer.material = material;
            }
            
            UpdateMaterial();
        }
    }
    
    private void OnDisable()
    {
        if (material != null && Application.isEditor && !Application.isPlaying)
        {
            DestroyImmediate(material);
        }
    }
    
    public void UpdateMaterial()
    {
        if (material != null)
        {
            // Set basic material properties
            material.color = baseColor;
            material.SetFloat(RoughnessID, roughness);
            material.SetFloat(MetallicID, metallic);
            
            if (noiseTex != null)
            {
                material.SetTexture(NoiseTexID, noiseTex);
                material.SetFloat(NoiseScaleID, noiseScale);
                material.SetFloat(NoiseStrengthID, noiseStrength);
                material.EnableKeyword("_NORMALMAP");
            }
        }
    }
    
    public void SetColor(Color color)
    {
        baseColor = color;
        UpdateMaterial();
    }
    
    public Material GetMaterial()
    {
        return material;
    }
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        UpdateMaterial();
    }
#endif
} 