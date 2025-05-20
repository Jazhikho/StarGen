using UnityEngine;

namespace Starlight
{
    /// <summary>
    /// Demo script for the Starlight system
    /// </summary>
    public class StarDemo : MonoBehaviour
    {
        [Header("Camera Controls")]
        public float moveSpeed = 100f;
        public float lookSpeed = 3f;
        
        private float yaw = 0f;
        private float pitch = 0f;
        private Camera cam;
        
        private void Start()
        {
            cam = Camera.main;
            if (cam == null)
            {
                cam = GetComponent<Camera>();
            }
            
            // Set camera clear color to black
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Color.black;
            }
        }
        
        private void Update()
        {
            HandleMovement();
            HandleRotation();
        }
        
        private void HandleMovement()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            float up = Input.GetKey(KeyCode.E) ? 1f : 0f;
            float down = Input.GetKey(KeyCode.Q) ? -1f : 0f;
            
            Vector3 movement = new Vector3(horizontal, up + down, vertical) * moveSpeed * Time.deltaTime;
            transform.Translate(movement);
        }
        
        private void HandleRotation()
        {
            if (Input.GetMouseButton(1)) // Right mouse button pressed
            {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");
                
                yaw += mouseX * lookSpeed;
                pitch -= mouseY * lookSpeed;
                pitch = Mathf.Clamp(pitch, -89f, 89f);
                
                transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
            }
        }
    }
} 