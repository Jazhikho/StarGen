using UnityEngine;

public abstract class CelestialBodyModel : MonoBehaviour
{
    [Range(2, 256)]
    public int resolution = 10;
    public bool autoUpdate = true;
    public enum FaceRenderMask { All, Top, Bottom, Left, Right, Front, Back };
    public FaceRenderMask faceRenderMask;

    [SerializeField, HideInInspector]
    protected MeshFilter[] meshFilters;
    protected CelestialFace[] celestialFaces;

    protected virtual void OnValidate()
    {
        Initialize();
        GenerateMesh();
    }

    protected abstract void Initialize();
    protected abstract void GenerateMesh();
    
    public abstract void GenerateCelestialBody();

    public void InitializeMeshComponents()
    {
        if (meshFilters == null || meshFilters.Length == 0)
        {
            meshFilters = new MeshFilter[6];
        }

        for (int i = 0; i < 6; i++)
        {
            if (meshFilters[i] == null)
            {
                GameObject meshObj = new GameObject($"Face_{i}");
                meshObj.transform.parent = transform;

                meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
                meshFilters[i].sharedMesh.name = $"Face_{i}_Mesh";

                // Add MeshRenderer
                MeshRenderer renderer = meshObj.AddComponent<MeshRenderer>();
                // Material will be set later in the specific model classes
            }
        }
    }

    protected void SetFaceVisibility()
    {
        for (int i = 0; i < 6; i++)
        {
            bool renderFace = faceRenderMask == FaceRenderMask.All || (int)faceRenderMask - 1 == i;
            meshFilters[i].gameObject.SetActive(renderFace);
        }
    }
}