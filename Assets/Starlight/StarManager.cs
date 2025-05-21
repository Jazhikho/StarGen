using System.Collections.Generic;
using UnityEngine;

namespace Starlight
{
    [ExecuteInEditMode]
    public class StarManager : MonoBehaviour
    {
        [Header("Appearance")]
        public Material starMaterial;
        public Mesh starMesh;
        public Color emissionTint = new Color(1,1,1);
        [Range(0f, 1f)]
        public float blurAmount = 0.5f;
        public float emissionEnergy = 2e+10f;
        [Range(0f, 90f)]
        public float billboardSizeDeg = 18f;
        public float luminosityCap = 4e+06f;
        public float metersPerLightyear = 100f;
        [Range(0f, 10f)]
        public float colorGamma = 3f;
        public float textureGamma = 1000f;
        public bool clampOutput = false;

        [Header("PSF Cropping Settings")]
        [Range(0f, 1f)]
        public float minSizeRatio = 0.003f;
        public float maxLuminosity = 100000f;
        [Range(0f, 2f)]
        public float scalingGamma = 0.45f;
        public bool debugShowRects = false;
        public Texture2D emissionTexture;
        
        [Header("Debug")]
        public bool logDebugInfo = true;
        
        [Header("Performance")]
        public bool enableLOD = true;
        public float lodNearDistance = 100f;
        public float lodFarDistance = 1000f;
        public int maxRenderedStars = 10000;
        public bool enableFrustumCulling = true;
        
        private List<Star> _stars = new List<Star>();
        private List<Matrix4x4> _matrices = new List<Matrix4x4>();
        private MaterialPropertyBlock _propertyBlock;
        private bool _dirty = true;
        private Texture2D _defaultTexture;
        
        // Public property to access stars
        public List<Star> Stars => _stars;
        public int StarCount => _stars.Count;
        
        private static readonly int EmissionTintProperty = Shader.PropertyToID("_EmissionTint");
        private static readonly int BlurAmountProperty = Shader.PropertyToID("_BlurAmount");
        private static readonly int EmissionEnergyProperty = Shader.PropertyToID("_EmissionEnergy");
        private static readonly int BillboardSizeDegProperty = Shader.PropertyToID("_BillboardSizeDeg");
        private static readonly int LuminosityCapProperty = Shader.PropertyToID("_LuminosityCap");
        private static readonly int MetersPerLightyearProperty = Shader.PropertyToID("_MetersPerLightyear");
        private static readonly int ColorGammaProperty = Shader.PropertyToID("_ColorGamma");
        private static readonly int TextureGammaProperty = Shader.PropertyToID("_TextureGamma");
        private static readonly int ClampOutputProperty = Shader.PropertyToID("_ClampOutput");
        private static readonly int MinSizeRatioProperty = Shader.PropertyToID("_MinSizeRatio");
        private static readonly int MaxLuminosityProperty = Shader.PropertyToID("_MaxLuminosity");
        private static readonly int ScalingGammaProperty = Shader.PropertyToID("_ScalingGamma");
        private static readonly int DebugShowRectsProperty = Shader.PropertyToID("_DebugShowRects");
        private static readonly int EmissionTextureProperty = Shader.PropertyToID("_EmissionTexture");
        
        // For star properties
        private static readonly int StarColorProperty = Shader.PropertyToID("_StarColor");
        private static readonly int StarLuminosityProperty = Shader.PropertyToID("_StarLuminosity");
        // Arrays for per-instance star properties
        private static readonly int StarColorsProperty = Shader.PropertyToID("_StarColors");
        private static readonly int StarLuminositiesProperty = Shader.PropertyToID("_StarLuminosities");
        
        void OnEnable()
        {
            _propertyBlock = new MaterialPropertyBlock();
            
            // Create a default white texture for fallback
            _defaultTexture = new Texture2D(2, 2);
            Color[] pixels = new Color[4] { Color.white, Color.white, Color.white, Color.white };
            _defaultTexture.SetPixels(pixels);
            _defaultTexture.Apply();
            
            // Initialize emission texture if null
            if (emissionTexture == null)
            {
                // Try to load from Resources
                emissionTexture = Resources.Load<Texture2D>("StarEmissionTexture");
                
                // If still null, create a simple radial gradient texture
                if (emissionTexture == null)
                {
                    CreateDefaultEmissionTexture();
                }
            }
            
            // Initialize star mesh if it's null
            if (starMesh == null)
            {
                starMesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx");
                if (logDebugInfo)
                    Debug.Log("Using default quad mesh for stars");
            }
            
            // Try to find the material if it's not assigned but exists in the project
            if (starMaterial == null)
            {
                starMaterial = Resources.Load<Material>("StarMaterial");
                
                // If still null, look for it in Resources folder
                if (starMaterial == null)
                {
                    // Try to find by shader
                    Material[] materials = Resources.FindObjectsOfTypeAll<Material>();
                    foreach (Material mat in materials)
                    {
                        if (mat.shader.name == "Starlight/Star")
                        {
                            starMaterial = mat;
                            Debug.Log("Found and assigned star material automatically");
                            break;
                        }
                    }
                    
                    // If still not found, use a new material with the star shader
                    if (starMaterial == null)
                    {
                        Shader starShader = Shader.Find("Starlight/Star");
                        if (starShader != null)
                        {
                            starMaterial = new Material(starShader);
                            starMaterial.name = "StarMaterial (Generated)";
                            Debug.Log("Created new star material with Starlight/Star shader");
                        }
                        else
                        {
                            Debug.LogError("Could not find Starlight/Star shader. Stars will not render correctly.");
                        }
                    }
                }
            }
            
            // Manually enable instancing on the material
            if (starMaterial != null)
            {
                starMaterial.enableInstancing = true;
            }
            
            UpdateMaterialProperties();
        }
        
        private void CreateDefaultEmissionTexture()
        {
            int size = 64;
            emissionTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];
            
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float maxDistance = size * 0.5f;
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pos = new Vector2(x, y);
                    float distance = Vector2.Distance(pos, center);
                    float normalizedDistance = distance / maxDistance;
                    
                    // Create radial gradient
                    float alpha = Mathf.Clamp01(1.0f - normalizedDistance);
                    alpha = Mathf.Pow(alpha, 2.0f); // Make falloff sharper
                    
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }
            
            emissionTexture.SetPixels(pixels);
            emissionTexture.Apply();
            emissionTexture.name = "Default Star Emission Texture";
            
            if (logDebugInfo)
                Debug.Log("Created default star emission texture");
        }
        
        void OnDestroy()
        {
            if (_defaultTexture != null)
            {
                if (Application.isPlaying)
                    Destroy(_defaultTexture);
                else
                    DestroyImmediate(_defaultTexture);
            }
        }
        
        public void SetStarList(List<Star> stars)
        {
            _stars = new List<Star>(stars);
            _dirty = true;
            
            if (logDebugInfo)
                Debug.Log($"Set star list with {stars.Count} stars");
        }
        
        private void UpdateMaterialProperties()
        {
            if (starMaterial == null)
            {
                if (logDebugInfo)
                    Debug.LogError("Star material is missing");
                return;
            }
            
            // Make sure instancing is enabled
            starMaterial.enableInstancing = true;
            
            starMaterial.SetColor(EmissionTintProperty, emissionTint);
            starMaterial.SetFloat(BlurAmountProperty, blurAmount);
            starMaterial.SetFloat(EmissionEnergyProperty, emissionEnergy);
            starMaterial.SetFloat(BillboardSizeDegProperty, billboardSizeDeg);
            starMaterial.SetFloat(LuminosityCapProperty, luminosityCap);
            starMaterial.SetFloat(MetersPerLightyearProperty, metersPerLightyear);
            starMaterial.SetFloat(ColorGammaProperty, colorGamma);
            starMaterial.SetFloat(TextureGammaProperty, textureGamma);
            starMaterial.SetFloat(ClampOutputProperty, clampOutput ? 1 : 0);
            starMaterial.SetFloat(MinSizeRatioProperty, minSizeRatio);
            starMaterial.SetFloat(MaxLuminosityProperty, maxLuminosity);
            starMaterial.SetFloat(ScalingGammaProperty, scalingGamma);
            starMaterial.SetFloat(DebugShowRectsProperty, debugShowRects ? 1 : 0);
            
            if (emissionTexture != null)
            {
                starMaterial.SetTexture(EmissionTextureProperty, emissionTexture);
            }
            else
            {
                if (logDebugInfo)
                    Debug.LogWarning("Star emission texture is missing, using default white texture");
                starMaterial.SetTexture(EmissionTextureProperty, _defaultTexture);
            }
        }
        
        void Update()
        {
            UpdateMaterialProperties();
            
            if (starMesh == null)
            {
                if (logDebugInfo)
                    Debug.LogError("Star mesh is missing");
                return;
            }
            
            if (_dirty)
                PrepareStars();
            
            RenderStars();
        }
        
        private void PrepareStars()
        {
            if (_stars == null || _stars.Count == 0)
            {
                if (logDebugInfo) 
                    Debug.Log("No stars to prepare");
                return;
            }
            
            _matrices.Clear();
            
            // Create transformation matrices for all stars
            foreach (var star in _stars)
            {
                Matrix4x4 matrix = Matrix4x4.TRS(
                    star.Position, 
                    Quaternion.identity, 
                    Vector3.one
                );
                
                _matrices.Add(matrix);
            }
            
            _dirty = false;
            
            if (logDebugInfo)
                Debug.Log($"Prepared {_matrices.Count} stars for rendering");
        }
        
        private void RenderStars()
        {
            if (_stars == null || _stars.Count == 0 || _matrices.Count == 0 || starMaterial == null || starMesh == null)
            {
                if (logDebugInfo)
                    Debug.LogWarning($"Cannot render stars: Stars:{_stars != null}, Count:{_stars?.Count ?? 0}, Matrices:{_matrices.Count}, Material:{starMaterial != null}, Mesh:{starMesh != null}");
                return;
            }
                
            // Make sure instancing is enabled
            if (!starMaterial.enableInstancing)
            {
                starMaterial.enableInstancing = true;
                Debug.LogWarning("Enabling instancing on star material");
            }
            
            // Group stars into batches (Unity limit is 1023 instances per batch)
            int batchSize = 1000;
            
            for (int i = 0; i < _matrices.Count; i += batchSize)
            {
                int count = Mathf.Min(batchSize, _matrices.Count - i);
                Matrix4x4[] batch = new Matrix4x4[count];
                
                // Reset the property block for each batch
                _propertyBlock = new MaterialPropertyBlock();
                
                // Prepare per-instance data arrays
                Vector4[] colors = new Vector4[count];
                float[] luminosities = new float[count];
                
                // Set per-instance properties for each star in this batch
                for (int j = 0; j < count; j++)
                {
                    batch[j] = _matrices[i + j];
                    Star star = _stars[i + j];
                    
                    // Get the star's color based on temperature
                    Color starColor = BlackbodyToRGB(star.Temperature);
                    colors[j] = new Vector4(starColor.r, starColor.g, starColor.b, starColor.a);
                    
                    // Normalize luminosity to prevent extreme brightness
                    float normalizedLuminosity = Mathf.Clamp(star.Luminosity, 0.01f, luminosityCap);
                    luminosities[j] = normalizedLuminosity;
                }
                
                // Set per-instance data arrays in the property block
                _propertyBlock.SetVectorArray(StarColorsProperty, colors);
                _propertyBlock.SetFloatArray(StarLuminositiesProperty, luminosities);
                
                // Ensure textures are properly set
                if (emissionTexture != null)
                {
                    _propertyBlock.SetTexture(EmissionTextureProperty, emissionTexture);
                }
                else
                {
                    _propertyBlock.SetTexture(EmissionTextureProperty, _defaultTexture);
                }
                
                // Apply other shader properties to the property block
                _propertyBlock.SetColor(EmissionTintProperty, emissionTint);
                _propertyBlock.SetFloat(BlurAmountProperty, blurAmount);
                _propertyBlock.SetFloat(EmissionEnergyProperty, emissionEnergy);
                _propertyBlock.SetFloat(BillboardSizeDegProperty, billboardSizeDeg);
                _propertyBlock.SetFloat(LuminosityCapProperty, luminosityCap);
                _propertyBlock.SetFloat(MetersPerLightyearProperty, metersPerLightyear);
                _propertyBlock.SetFloat(ColorGammaProperty, colorGamma);
                _propertyBlock.SetFloat(TextureGammaProperty, textureGamma);
                _propertyBlock.SetFloat(ClampOutputProperty, clampOutput ? 1 : 0);
                _propertyBlock.SetFloat(MinSizeRatioProperty, minSizeRatio);
                _propertyBlock.SetFloat(MaxLuminosityProperty, maxLuminosity);
                _propertyBlock.SetFloat(ScalingGammaProperty, scalingGamma);
                _propertyBlock.SetFloat(DebugShowRectsProperty, debugShowRects ? 1 : 0);
                
                // Draw this batch of stars
                Graphics.DrawMeshInstanced(
                    starMesh,
                    0,
                    starMaterial,
                    batch,
                    count,
                    _propertyBlock,
                    UnityEngine.Rendering.ShadowCastingMode.Off,
                    false,
                    gameObject.layer
                );
            }
        }
        
        public static Color BlackbodyToRGB(float kelvin)
        {
            // Ensure temperature is within a valid range
            kelvin = Mathf.Clamp(kelvin, 1000f, 40000f);
            
            float temperature = kelvin / 100.0f;
            float red, green, blue;
            
            // Red calculation
            if (temperature < 66.0f)
            {
                red = 255;
            }
            else
            {
                red = temperature - 55.0f;
                red = 351.97690566805693f + 0.114206453784165f * red - 40.25366309332127f * Mathf.Log(Mathf.Max(1f, red));
                red = Mathf.Clamp(red, 0, 255);
            }
            
            // Green calculation
            if (temperature < 66.0f)
            {
                green = temperature - 2;
                // Prevent log of negative number
                if (green <= 0) green = 0.1f;
                green = -155.25485562709179f - 0.44596950469579133f * green + 104.49216199393888f * Mathf.Log(green);
                green = Mathf.Clamp(green, 0, 255);
            }
            else
            {
                green = temperature - 50.0f;
                // Prevent log of negative number
                if (green <= 0) green = 0.1f;
                green = 325.4494125711974f + 0.07943456536662342f * green - 28.0852963507957f * Mathf.Log(green);
                green = Mathf.Clamp(green, 0, 255);
            }
            
            // Blue calculation
            if (temperature >= 66.0f)
            {
                blue = 255;
            }
            else if (temperature <= 20.0f)
            {
                blue = 0;
            }
            else
            {
                blue = temperature - 10;
                // Prevent log of negative number
                if (blue <= 0) blue = 0.1f;
                blue = -254.76935184120902f + 0.8274096064007395f * blue + 115.67994401066147f * Mathf.Log(blue);
                blue = Mathf.Clamp(blue, 0, 255);
            }
            
            // Convert to Unity's color range (0-1)
            return new Color(red / 255.0f, green / 255.0f, blue / 255.0f, 1.0f);
        }
        
        private List<int> GetVisibleStarIndices()
        {
            List<int> visibleIndices = new List<int>();
            
            if (!Camera.main)
                return visibleIndices;
            
            Vector3 cameraPos = Camera.main.transform.position;
            Plane[] frustumPlanes = enableFrustumCulling ? GeometryUtility.CalculateFrustumPlanes(Camera.main) : null;
            
            for (int i = 0; i < _stars.Count; i++)
            {
                Star star = _stars[i];
                
                // Frustum culling
                if (enableFrustumCulling && frustumPlanes != null)
                {
                    if (!GeometryUtility.TestPlanesAABB(frustumPlanes, new Bounds(star.Position, Vector3.one * 0.1f)))
                        continue;
                }
                
                // Distance culling for performance
                float distance = Vector3.Distance(cameraPos, star.Position);
                if (distance > lodFarDistance * 2f)
                    continue;
                
                visibleIndices.Add(i);
                
                // Limit total rendered stars for performance
                if (visibleIndices.Count >= maxRenderedStars)
                    break;
            }
            
            return visibleIndices;
        }
        
        private float CalculateLODFactor(float distance)
        {
            if (distance <= lodNearDistance)
                return 1.0f;
            else if (distance >= lodFarDistance)
                return 0.1f;
            else
            {
                float t = (distance - lodNearDistance) / (lodFarDistance - lodNearDistance);
                return Mathf.Lerp(1.0f, 0.1f, t);
            }
        }
    }
}