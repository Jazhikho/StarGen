using UnityEngine;

[CreateAssetMenu()]
public class StarSettings : ScriptableObject
{
    [Header("Shape")]
    public float starRadius = 1f;
    public bool useSurfaceNoise = false;
    public NoiseSettings surfaceNoise;
    public float surfaceVariation = 0.05f;

    [Header("Appearance")]
    public Material starMaterial;
    public Color starColor = Color.yellow;
    public float emissionStrength = 2f;

    [Header("Corona")]
    public bool hasCorona = true;
    public Color coronaColor = new Color(1f, 0.5f, 0f, 0.3f);
    public float coronaSize = 1.5f;

    [Header("Lighting")]
    public float lightRange = 100f;
    public float lightIntensity = 2f;

    [Header("Animation")]
    public bool animateSurface = true;
    public float activitySpeed = 1f;
    public AnimationCurve intensityFlicker;
}