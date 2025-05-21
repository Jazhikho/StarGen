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

    protected void InitializeMeshComponents()
    {
        if (meshFilters == null || meshFilters.Length == 0)
        {
            meshFilters = new MeshFilter[6];
        }

        for (int i = 0; i < 6; i++)
        {
            if (meshFilters[i] == null)
            {
                GameObject meshObj = new GameObject("mesh");
                meshObj.transform.parent = transform;

                meshObj.AddComponent<MeshRenderer>();
                meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
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