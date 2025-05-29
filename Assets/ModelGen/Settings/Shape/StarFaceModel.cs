using UnityEngine;

public class StarFaceModel : CelestialFace
{
    StarSettings settings;

    public StarFaceModel(Mesh mesh, int resolution, Vector3 localUp, StarSettings settings) 
        : base(mesh, resolution, localUp)
    {
        this.settings = settings;
    }

    public override void ConstructMesh()
    {
        // Return early if mesh or settings is null
        if (mesh == null || settings == null)
        {
            Debug.LogWarning("Cannot construct star mesh: mesh or settings is null");
            return;
        }
        
        Vector3[] vertices = new Vector3[resolution * resolution];
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];
        int triIndex = 0;
        Vector2[] uv = new Vector2[vertices.Length];

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = x + y * resolution;
                Vector2 percent = new Vector2(x, y) / (resolution - 1);
                Vector3 pointOnUnitCube = localUp + (percent.x - .5f) * 2 * axisA + (percent.y - .5f) * 2 * axisB;
                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;

                // Apply basic star radius with optional surface variation
                float elevation = settings.starRadius;
                if (settings.surfaceNoise != null && settings.useSurfaceNoise)
                {
                    float noiseValue = NoiseFilterFactory.CreateNoiseFilter(settings.surfaceNoise).Evaluate(pointOnUnitSphere);
                    elevation += noiseValue * settings.surfaceVariation;
                }

                vertices[i] = pointOnUnitSphere * elevation;
                uv[i] = percent; // Simple UV for potential texture mapping

                if (x != resolution - 1 && y != resolution - 1)
                {
                    triangles[triIndex] = i;
                    triangles[triIndex + 1] = i + resolution + 1;
                    triangles[triIndex + 2] = i + resolution;

                    triangles[triIndex + 3] = i;
                    triangles[triIndex + 4] = i + 1;
                    triangles[triIndex + 5] = i + resolution + 1;
                    triIndex += 6;
                }
            }
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.uv = uv;
    }
}