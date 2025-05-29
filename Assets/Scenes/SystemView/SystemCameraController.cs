using UnityEngine;

public class SystemCameraController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 10f;
    public float fastMoveMultiplier = 3f;
    public float rotateSpeed = 2f;
    
    [Header("Zoom")]
    public float zoomSpeed = 5f;
    public float minZoom = 1f;
    public float maxZoom = 1000f;
    
    private Vector3 _systemCenter = Vector3.zero;
    private Vector3 _systemSize = Vector3.one;
    private Camera _camera;
    private Vector3 _lastMousePosition;
    private bool _isRotating;
    private bool _isInitialized = false;
    
    private void Awake()
    {
        _camera = GetComponent<Camera>();
        if (_camera == null)
        {
            Debug.LogError("SystemCameraController requires a Camera component!");
        }
    }
    
    private void Update()
    {
        if (_isInitialized)
        {
            HandleInput();
        }
    }
    
    private void HandleInput()
    {
        // Mouse rotation
        if (Input.GetMouseButtonDown(1)) // Right click
        {
            _isRotating = true;
            _lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            _isRotating = false;
        }
        
        if (_isRotating)
        {
            Vector3 deltaMousePosition = Input.mousePosition - _lastMousePosition;
            
            // Rotate around the system center
            transform.RotateAround(_systemCenter, Vector3.up, deltaMousePosition.x * rotateSpeed);
            transform.RotateAround(_systemCenter, transform.right, -deltaMousePosition.y * rotateSpeed);
            
            _lastMousePosition = Input.mousePosition;
        }
        
        // WASD movement
        Vector3 moveDirection = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) moveDirection += transform.forward;
        if (Input.GetKey(KeyCode.S)) moveDirection -= transform.forward;
        if (Input.GetKey(KeyCode.A)) moveDirection -= transform.right;
        if (Input.GetKey(KeyCode.D)) moveDirection += transform.right;
        
        float currentMoveSpeed = Input.GetKey(KeyCode.LeftShift) ? 
            moveSpeed * fastMoveMultiplier : moveSpeed;
        
        transform.position += moveDirection * currentMoveSpeed * Time.deltaTime;
        
        // Scroll wheel zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            Vector3 moveTowards = _systemCenter - transform.position;
            float distance = moveTowards.magnitude;
            
            if (distance > 0.001f) // Prevent division by zero
            {
                // Move towards/away from system center
                float zoomAmount = scroll * zoomSpeed * distance * 0.1f;
                transform.position += moveTowards.normalized * zoomAmount;
                
                // Clamp distance to system center
                float newDistance = (_systemCenter - transform.position).magnitude;
                newDistance = Mathf.Clamp(newDistance, minZoom, maxZoom);
                
                Vector3 directionToCenter = (_systemCenter - transform.position).normalized;
                transform.position = _systemCenter - directionToCenter * newDistance;
            }
        }
    }
    
    public void SetSystemBounds(Vector3 center, Vector3 size)
    {
        // Validate inputs
        if (!IsValidVector3(center) || !IsValidVector3(size))
        {
            Debug.LogWarning("Invalid system bounds provided to camera!");
            return;
        }
        
        _systemCenter = center;
        _systemSize = size;
        
        // Set initial camera position
        float maxDimension = Mathf.Max(size.x, size.y, size.z, 1f); // Ensure minimum size
        float distance = maxDimension * 1.5f; // Give some breathing room
        
        transform.position = center + new Vector3(distance * 0.3f, distance * 0.2f, -distance * 0.7f);
        transform.LookAt(center);
        
        // Update zoom limits based on system size
        minZoom = maxDimension * 0.01f;
        maxZoom = maxDimension * 3f;
        
        _isInitialized = true;
        
        Debug.Log($"Camera initialized - Center: {center}, Size: {size}, Distance: {distance}");
    }
    
    public void FocusOnBarycenter(Vector3 barycenterPos, float maxOrbitRadius)
    {
        if (!IsValidVector3(barycenterPos) || maxOrbitRadius <= 0)
        {
            Debug.LogWarning("Invalid barycenter focus parameters!");
            return;
        }
        
        _systemCenter = barycenterPos;
        
        // Distance to see both objects (convert from 1 AU = 10 Unity units)
        float viewDistance = maxOrbitRadius * 3f; // Back off enough to see full orbits
        
        transform.position = barycenterPos + new Vector3(viewDistance * 0.5f, viewDistance * 0.3f, -viewDistance * 0.8f);
        transform.LookAt(barycenterPos);
        
        // Update zoom limits
        minZoom = maxOrbitRadius * 0.1f;
        maxZoom = maxOrbitRadius * 5f;
        
        _isInitialized = true;
        
        Debug.Log($"Camera focused on barycenter at {barycenterPos}, orbit radius: {maxOrbitRadius}");
    }
    
    public void FocusOnStar(Vector3 starPos, float starRadius)
    {
        if (!IsValidVector3(starPos) || starRadius <= 0)
        {
            Debug.LogWarning("Invalid star focus parameters!");
            return;
        }
        
        _systemCenter = starPos;
        
        float viewDistance = starRadius * 5f; // Good viewing distance for a star
        
        transform.position = starPos + new Vector3(viewDistance * 0.4f, viewDistance * 0.2f, -viewDistance * 0.6f);
        transform.LookAt(starPos);
        
        minZoom = starRadius * 0.5f;
        maxZoom = starRadius * 20f;
        
        _isInitialized = true;
        
        Debug.Log($"Camera focused on star at {starPos}, radius: {starRadius}");
    }
    
    private bool IsValidVector3(Vector3 vec)
    {
        return !float.IsNaN(vec.x) && !float.IsNaN(vec.y) && !float.IsNaN(vec.z) &&
               !float.IsInfinity(vec.x) && !float.IsInfinity(vec.y) && !float.IsInfinity(vec.z);
    }
    
    public void ResetView()
    {
        if (_isInitialized && IsValidVector3(_systemCenter))
        {
            SetSystemBounds(_systemCenter, _systemSize);
        }
    }
    
    public void LookAtPoint(Vector3 point)
    {
        if (IsValidVector3(point))
        {
            transform.LookAt(point);
        }
    }
}