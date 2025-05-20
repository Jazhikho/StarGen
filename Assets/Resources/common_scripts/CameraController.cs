using UnityEngine;
using System.Collections;

/// <summary>
/// Controls camera movement and zoom in the galaxy view
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float PanSpeed = 30f;
    public float RotationSpeed = 50f;
    public float ZoomSpeed = 500f;
    
    [Header("Limits")]
    public float MinZoom = 10f;
    public float MaxZoom = 1000f;
    public float BoundaryMargin = 50f;
    
    // Galaxy bounds
    private Vector3 _galaxyCenter = Vector3.zero;
    private Vector3 _galaxySize = new Vector3(100, 100, 100);
    private float _maxGalaxyExtent = 100f;
    
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;
    
    void Start()
    {
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;
    }
    
    void Update()
    {
        HandleInput();
        EnforceBoundaries();
    }
    
    /// <summary>
    /// Sets the galaxy boundaries for camera movement
    /// </summary>
    /// <param name="center">Center position of the galaxy</param>
    /// <param name="size">Size of the galaxy</param>
    public void SetGalaxyBounds(Vector3 center, Vector3 size)
    {
        _galaxyCenter = center;
        _galaxySize = size;
        _maxGalaxyExtent = Mathf.Max(size.x, Mathf.Max(size.y, size.z));
        
        // Position camera to view full galaxy
        PositionCameraToViewGalaxy();
    }
    
    /// <summary>
    /// Positions camera to view the full galaxy
    /// </summary>
    private void PositionCameraToViewGalaxy()
    {
        // Calculate distance needed to see the full galaxy
        float distance = _maxGalaxyExtent * 2.0f;
        
        // Position camera
        transform.position = _galaxyCenter + new Vector3(0, distance * 0.3f, distance);
        transform.LookAt(_galaxyCenter);
        
        // Store as initial position/rotation
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;
    }
    
    /// <summary>
    /// Handles keyboard/mouse input for camera control
    /// </summary>
    private void HandleInput()
    {
        // Panning with WASD
        Vector3 panMovement = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) panMovement.z += 1;
        if (Input.GetKey(KeyCode.S)) panMovement.z -= 1;
        if (Input.GetKey(KeyCode.A)) panMovement.x -= 1;
        if (Input.GetKey(KeyCode.D)) panMovement.x += 1;
        
        if (panMovement != Vector3.zero)
        {
            // Move along camera's local axes
            transform.Translate(panMovement * PanSpeed * Time.deltaTime, Space.Self);
        }
        
        // Rotation with Q/E or right mouse button
        float rotation = 0;
        if (Input.GetKey(KeyCode.Q)) rotation -= 1;
        if (Input.GetKey(KeyCode.E)) rotation += 1;
        
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            
            transform.RotateAround(transform.position, Vector3.up, mouseX * RotationSpeed * Time.deltaTime);
            transform.RotateAround(transform.position, transform.right, -mouseY * RotationSpeed * Time.deltaTime);
        }
        else if (rotation != 0)
        {
            transform.RotateAround(transform.position, Vector3.up, rotation * RotationSpeed * Time.deltaTime);
        }
        
        // Zoom with mouse wheel
        float zoom = Input.GetAxis("Mouse ScrollWheel");
        if (zoom != 0)
        {
            // Zoom by moving forward/backward
            Vector3 zoomMovement = transform.forward * zoom * ZoomSpeed * Time.deltaTime;
            transform.Translate(zoomMovement, Space.World);
        }
        
        // Reset camera with R
        if (Input.GetKeyDown(KeyCode.R))
        {
            transform.position = _initialPosition;
            transform.rotation = _initialRotation;
        }
    }
    
    /// <summary>
    /// Ensures camera stays within reasonable boundaries
    /// </summary>
    private void EnforceBoundaries()
    {
        // Calculate allowed movement bounds
        float boundaryExtent = _maxGalaxyExtent + BoundaryMargin;
        Vector3 minBounds = _galaxyCenter - new Vector3(boundaryExtent, boundaryExtent, boundaryExtent);
        Vector3 maxBounds = _galaxyCenter + new Vector3(boundaryExtent, boundaryExtent, boundaryExtent);
        
        // Clamp position
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minBounds.x, maxBounds.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minBounds.y, maxBounds.y);
        clampedPosition.z = Mathf.Clamp(clampedPosition.z, minBounds.z, maxBounds.z);
        
        transform.position = clampedPosition;
        
        // Ensure minimum and maximum distance from galaxy center
        float currentDistance = Vector3.Distance(transform.position, _galaxyCenter);
        if (currentDistance < MinZoom)
        {
            Vector3 direction = (transform.position - _galaxyCenter).normalized;
            transform.position = _galaxyCenter + direction * MinZoom;
        }
        else if (currentDistance > MaxZoom)
        {
            Vector3 direction = (transform.position - _galaxyCenter).normalized;
            transform.position = _galaxyCenter + direction * MaxZoom;
        }
    }
}