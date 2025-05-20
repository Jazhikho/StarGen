using UnityEngine;

public class GalaxyDataSingleton : MonoBehaviour
{
    // Singleton instance
    private static GalaxyDataSingleton _instance;
    
    public static GalaxyDataSingleton Instance
    {
        get
        {
            if (_instance == null)
            {
                // Check if an instance already exists in the scene
                _instance = FindAnyObjectByType<GalaxyDataSingleton>();
                
                // If not, create a new GameObject with the component
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("GalaxyDataSingleton");
                    _instance = singletonObject.AddComponent<GalaxyDataSingleton>();
                    DontDestroyOnLoad(singletonObject);
                }
            }
            
            return _instance;
        }
    }
    
    private void Awake()
    {
        // Ensure single instance
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
}