using UnityEngine;

namespace Starlight
{
    [RequireComponent(typeof(StarManager))]
    public class StarClickDetector : MonoBehaviour
    {
        [Header("Click Settings")]
        public Camera clickCamera;
        public float clickRadius = 0.01f; // As fraction of screen height
        
        [Header("Debug")]
        public bool debugClicks = true;
        
        // Events
        public System.Action<Star, Vector3> OnStarClicked;
        public System.Action<Star, Vector3> OnStarDoubleClicked;
        
        private StarManager _starManager;
        private float _lastClickTime;
        private Star _lastClickedStar;
        private const float DOUBLE_CLICK_TIME = 0.3f; // seconds
        
        private void Awake()
        {
            _starManager = GetComponent<StarManager>();
            
            if (clickCamera == null)
                clickCamera = Camera.main;
        }
        
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                CheckStarClick(Input.mousePosition);
            }
        }
        
        private void CheckStarClick(Vector3 screenPos)
        {
            if (clickCamera == null || _starManager == null || _starManager.StarCount == 0)
                return;
                
            Vector3 mouseRay = clickCamera.ScreenPointToRay(screenPos).direction.normalized;
            float closestDistance = float.MaxValue;
            Star closestStar = null;
            Vector3 closestStarScreenPos = Vector3.zero;
            
            // Find the closest star to the mouse cursor
            foreach (var star in _starManager.Stars)
            {
                // Convert star position to screen space
                Vector3 starScreenPos = clickCamera.WorldToScreenPoint(star.Position);
                
                // Skip stars behind the camera
                if (starScreenPos.z < 0)
                    continue;
                    
                // Calculate 2D screen distance
                float distInPixels = Vector2.Distance(
                    new Vector2(screenPos.x, screenPos.y),
                    new Vector2(starScreenPos.x, starScreenPos.y)
                );
                
                // Check if within clickable radius
                float clickableRadius = Screen.height * clickRadius;
                if (distInPixels <= clickableRadius && starScreenPos.z < closestDistance)
                {
                    closestDistance = starScreenPos.z;
                    closestStar = star;
                    closestStarScreenPos = starScreenPos;
                }
            }
            
            if (closestStar != null)
            {
                if (debugClicks)
                {
                    // Calculate color from temperature for debugging
                    Color starColor = StarManager.BlackbodyToRGB(closestStar.Temperature);
                    Debug.Log($"Clicked star at {closestStar.Position}\n" +
                              $"Temperature: {closestStar.Temperature}K\n" + 
                              $"Luminosity: {closestStar.Luminosity} L☉\n" +
                              $"Color: RGB({starColor.r:F2}, {starColor.g:F2}, {starColor.b:F2})\n" +
                              $"Distance from camera: {Vector3.Distance(clickCamera.transform.position, closestStar.Position):F2} units");
                }
                
                // Check for double click
                bool isDoubleClick = (Time.time - _lastClickTime < DOUBLE_CLICK_TIME) && 
                                     (_lastClickedStar == closestStar);
                
                if (isDoubleClick)
                {
                    OnStarDoubleClicked?.Invoke(closestStar, closestStarScreenPos);
                    if (debugClicks)
                        Debug.Log("Double-click detected!");
                }
                else
                {
                    OnStarClicked?.Invoke(closestStar, closestStarScreenPos);
                }
                
                _lastClickTime = Time.time;
                _lastClickedStar = closestStar;
            }
        }
    }
}