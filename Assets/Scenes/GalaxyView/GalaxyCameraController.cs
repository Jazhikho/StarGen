using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyCameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 50.0f;
    public float rotateSpeed = 100.0f;
    public float zoomSpeed = 500.0f;
    public float smoothing = 5.0f;
    
    [Header("Zoom Limits")]
    public float minZoomDistance = 10.0f;
    public float maxZoomDistance = 1000.0f;
    
    [Header("Galaxy Bounds")]
    public Vector3 galaxyCenter = Vector3.zero;
    public Vector3 galaxySize = new Vector3(100, 100, 100);
    
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    
    void Start()
    {
        targetPosition = transform.position;
        targetRotation = transform.rotation;
    }
    
    void Update()
    {
        HandleInputs();
        
        // Smoothly move the camera
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothing);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * smoothing);
    }
    
    private void HandleInputs()
    {
        // Camera movement with WASD/arrows
        Vector3 moveDirection = Vector3.zero;
        
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            moveDirection += transform.forward;
            
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            moveDirection -= transform.forward;
            
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            moveDirection -= transform.right;
            
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            moveDirection += transform.right;
            
        if (Input.GetKey(KeyCode.Q))
            moveDirection += Vector3.up;
            
        if (Input.GetKey(KeyCode.E))
            moveDirection -= Vector3.up;
        
        // Apply movement
        if (moveDirection.magnitude > 0.1f)
        {
            moveDirection.Normalize();
            targetPosition += moveDirection * moveSpeed * Time.deltaTime;
        }
        
        // Camera rotation with right mouse button
        if (Input.GetMouseButton(1))
        {
            float rotX = Input.GetAxis("Mouse Y") * rotateSpeed * Time.deltaTime;
            float rotY = Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime;
            
            // Apply rotation around local X and global Y
            targetRotation *= Quaternion.Euler(-rotX, rotY, 0);
        }
        
        // Zooming with scroll wheel
        float zoom = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed * Time.deltaTime;
        if (Mathf.Abs(zoom) > 0.01f)
        {
            // Calculate zoom direction and limit zoom distance
            targetPosition += transform.forward * zoom;
            
            // Constrain to min/max distance from galaxy center
            Vector3 directionToCenter = (galaxyCenter - targetPosition).normalized;
            float distanceToCenter = Vector3.Distance(galaxyCenter, targetPosition);
            
            if (distanceToCenter < minZoomDistance)
                targetPosition = galaxyCenter - directionToCenter * minZoomDistance;
            else if (distanceToCenter > maxZoomDistance)
                targetPosition = galaxyCenter - directionToCenter * maxZoomDistance;
        }
        
        // Reset view with Home key
        if (Input.GetKeyDown(KeyCode.Home))
        {
            ResetView();
        }
    }
    
    public void SetGalaxyBounds(Vector3 center, Vector3 size)
    {
        galaxyCenter = center;
        galaxySize = size;
        
        // Update max zoom distance based on galaxy size
        maxZoomDistance = Mathf.Max(size.x, size.y, size.z) * 2.0f;
        
        // Reset the camera view
        ResetView();
    }
    
    public void ResetView()
    {
        // Position the camera INSIDE the starfield, at a reasonable distance
        float maxDimension = Mathf.Max(galaxySize.x, galaxySize.y, galaxySize.z);
        float distance = maxDimension * 0.4f; // Much closer - inside the starfield
        distance = Mathf.Max(distance, 50f); // Ensure minimum distance
        
        // Position camera inside the galaxy bounds, looking towards center with some offset
        Vector3 offset = new Vector3(
            galaxySize.x * 0.3f,  // Offset to the side
            galaxySize.y * 0.2f,  // Slight elevation
            galaxySize.z * 0.3f   // Forward offset
        );
        
        targetPosition = galaxyCenter + offset;
        
        // Look at galaxy center
        Vector3 direction = (galaxyCenter - targetPosition).normalized;
        targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        
        // Apply immediately for initial setup
        transform.position = targetPosition;
        transform.rotation = targetRotation;
        
        Debug.Log($"Camera reset to position: {targetPosition}, looking at: {galaxyCenter}, galaxy size: {galaxySize}, distance from center: {Vector3.Distance(targetPosition, galaxyCenter)}");
    }
    
    public void LookAtPoint(Vector3 point)
    {
        // Keep the current position but look at the specified point
        Vector3 direction = (point - transform.position).normalized;
        targetRotation = Quaternion.LookRotation(direction, Vector3.up);
    }
}