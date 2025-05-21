using UnityEngine;

public class StarModel : CelestialBodyModel
{
    public StarSettings starSettings;

    [HideInInspector]
    public bool starSettingsFoldout;

    StarFaceModel[] starFaces;
    private Light starLight;
    private float animationTime;

    protected override void Initialize()
    {
        InitializeMeshComponents();

        starFaces = new StarFaceModel[6];
        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        for (int i = 0; i < 6; i++)
        {
            starFaces[i] = new StarFaceModel(meshFilters[i].sharedMesh, resolution, directions[i], starSettings);
            UpdateStarMaterial(meshFilters[i].GetComponent<MeshRenderer>());
        }

        SetFaceVisibility();
        SetupStarLight();
    }

    public override void GenerateCelestialBody()
    {
        GenerateStar();
    }

    public void GenerateStar()
    {
        Initialize();
        GenerateMesh();
    }

    public void OnStarSettingsUpdated()
    {
        if (autoUpdate)
        {
            GenerateStar();
        }
    }

    protected override void GenerateMesh()
    {
        for (int i = 0; i < 6; i++)
        {
            if (meshFilters[i].gameObject.activeSelf)
            {
                starFaces[i].ConstructMesh();
            }
        }
    }

    private void UpdateStarMaterial(MeshRenderer renderer)
    {
        renderer.sharedMaterial = starSettings.starMaterial;

        // Set emission properties
        MaterialPropertyBlock props = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(props);
        props.SetColor("_EmissionColor", starSettings.starColor * starSettings.emissionStrength);
        renderer.SetPropertyBlock(props);
    }

    private void SetupStarLight()
    {
        if (starLight == null)
        {
            starLight = GetComponent<Light>();
            if (starLight == null)
            {
                starLight = gameObject.AddComponent<Light>();
            }
        }

        starLight.type = LightType.Point;
        starLight.range = starSettings.lightRange;
        starLight.intensity = starSettings.lightIntensity;
        starLight.color = starSettings.starColor;
    }

    void Update()
    {
        if (starSettings != null && starSettings.animateSurface && Application.isPlaying)
        {
            animationTime += Time.deltaTime * starSettings.activitySpeed;

            // Animate light intensity
            if (starSettings.intensityFlicker != null && starLight != null)
            {
                float flicker = starSettings.intensityFlicker.Evaluate(animationTime % 1f);
                starLight.intensity = starSettings.lightIntensity * (1f + flicker * 0.1f);
            }

            // Could add surface animation here if needed
        }
    }
}