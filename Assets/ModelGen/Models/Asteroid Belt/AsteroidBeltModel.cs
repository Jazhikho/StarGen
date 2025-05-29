using UnityEngine;

public class AsteroidBeltModel : MonoBehaviour
{
    [Header("Asteroid Belt Settings")]
    public GameObject asteroidPrefab;
    public int beltDensity;
    public int seed;
    public float innerRadius;
    public float outerRadius;
    public float beltheight;
    public bool rotatingClockwise;

    [Header("Asteroid Settings")]
    public float minSize;
    public float maxSize;
    public float minSpeed;
    public float maxSpeed;
    public float minRotationSpeed;
    public float maxRotationSpeed;

    private Vector3 localPosition;
    private Vector3 worldOffset;
    private Vector3 worldPosition;
    private float randomRadius;
    private float randomRadian;
    private float beltx;
    private float belty;
    private float beltz;

    // Getting a random position in a circle given a radius and a radian
    // x = cx + r * cos(theta)
    // z = cy + r * sin(theta)

    private void Start()
    {
        Random.InitState(seed);

        for (int i = 0; i < beltDensity; i++)
        {
            do
            {
                float angle = Random.Range(0f, 360f);

                // Get a random radius
                randomRadius = Random.Range(innerRadius, outerRadius);

                // Get a random radian
                randomRadian = Random.Range(0f, 2 * Mathf.PI);

                belty = Random.Range(-beltheight / 2, beltheight / 2);
                beltx = Mathf.Cos(randomRadian) * randomRadius;
                beltz = Mathf.Sin(randomRadian) * randomRadius;


            } while (float.IsNaN(beltx) || float.IsNaN(beltz));

            localPosition = new Vector3(beltx, belty, beltz);
            worldOffset = transform.rotation * localPosition;
            worldPosition = transform.position + worldOffset;

            GameObject _asteroid = Instantiate(asteroidPrefab, worldPosition, Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)));
            _asteroid.AddComponent<AsteroidModel>().SetupBeltObject(gameObject, Random.Range(minSpeed, maxSpeed), rotatingClockwise, Random.Range(minRotationSpeed, maxRotationSpeed));
            _asteroid.transform.SetParent(transform);
        }
    }
}