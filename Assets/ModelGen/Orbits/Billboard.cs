using UnityEngine;

/// <summary>
/// Makes an object always face the camera
/// </summary>
public class Billboard : MonoBehaviour
{
    private Camera cam;
    
    void Start()
    {
        cam = Camera.main;
    }
    
    void Update()
    {
        if (cam != null)
        {
            transform.LookAt(cam.transform);
            transform.Rotate(0, 180, 0); // Flip to face camera properly
        }
    }
}