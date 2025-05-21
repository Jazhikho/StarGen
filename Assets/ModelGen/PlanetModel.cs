using UnityEngine;

public class PlanetModel : CelestialBodyModel
{
    public ShapeSettings shapeSettings;
    public ColorSettings colorSettings;

    [HideInInspector]
    public bool shapeSettingsFoldout;
    [HideInInspector]
    public bool colorSettingsFoldout;

    ShapeGenerator shapeGenerator = new ShapeGenerator();
    ColorGenerator colorGenerator = new ColorGenerator();
    TerrainFace[] terrainFaces;

    protected override void Initialize()
    {
        InitializeMeshComponents();
        
        shapeGenerator.UpdateSettings(shapeSettings);
        colorGenerator.UpdateSettings(colorSettings);

        terrainFaces = new TerrainFace[6];
        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        for (int i = 0; i < 6; i++)
        {
            meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial = colorSettings.planetMaterial;
            terrainFaces[i] = new TerrainFace(shapeGenerator, meshFilters[i].sharedMesh, resolution, directions[i]);
        }

        SetFaceVisibility();
    }

    public override void GenerateCelestialBody()
    {
        GeneratePlanet();
    }

    public void GeneratePlanet()
    {
        Initialize();
        GenerateMesh();
        GenerateColors();
    }

    public void OnShapeSettingsUpdated()
    {
        if (autoUpdate)
        {
            Initialize();
            GenerateMesh();
        }
    }

    public void OnColorSettingsUpdated()
    {
        if (autoUpdate)
        {
            Initialize();
            GenerateColors();
        }
    }

    protected override void GenerateMesh()
    {
        for (int i = 0; i < 6; i++)
        {
            if (meshFilters[i].gameObject.activeSelf)
            {
                terrainFaces[i].ConstructMesh();
            }
        }

        colorGenerator.UpdateElevation(shapeGenerator.elevationMinMax);
    }

    void GenerateColors()
    {
        colorGenerator.UpdateColors();
        for (int i = 0; i < 6; i++)
        {
            if (meshFilters[i].gameObject.activeSelf)
            {
                terrainFaces[i].UpdateUVs(colorGenerator);
            }
        }
    }
}