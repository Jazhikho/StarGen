using UnityEngine;

public class AsteroidModel : MonoBehaviour
{
    [SerializeField]
    private GameObject parent;
    [SerializeField]
    private float orbitSpeed;
    [SerializeField]
    private bool rotationClockwise;
    [SerializeField]
    private float rotationSpeed;
    [SerializeField]
    private Vector3 rotationDirection;

    public void SetupBeltObject(GameObject _parent, float _orbitSpeed, bool _rotationClockwise, float _rotationSpeed)
    {
        parent = _parent;
        orbitSpeed = _orbitSpeed;
        rotationClockwise = _rotationClockwise;
        rotationSpeed = _rotationSpeed;
        rotationDirection = new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
    }

    private void Update()
    {
        // Update the position of the asteroid
        if (rotationClockwise)
        {
            transform.RotateAround(parent.transform.position, parent.transform.up, orbitSpeed * Time.deltaTime);
        }
        else
        {
            transform.RotateAround(parent.transform.position, -parent.transform.up, orbitSpeed * Time.deltaTime);
        }
        transform.Rotate(rotationDirection, rotationSpeed * Time.deltaTime);
    }
}
