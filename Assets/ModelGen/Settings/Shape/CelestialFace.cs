using UnityEngine;

public abstract class CelestialFace
{
    protected Mesh mesh;
    protected int resolution;
    protected Vector3 localUp;
    protected Vector3 axisA;
    protected Vector3 axisB;

    public CelestialFace(Mesh mesh, int resolution, Vector3 localUp)
    {
        this.mesh = mesh;
        this.resolution = resolution;
        this.localUp = localUp;

        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        axisB = Vector3.Cross(localUp, axisA);
    }

    public abstract void ConstructMesh();
}