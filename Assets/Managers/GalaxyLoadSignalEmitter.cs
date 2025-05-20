using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Emits a signal when a galaxy is loaded to notify other components
/// </summary>
public class GalaxyLoadSignalEmitter : MonoBehaviour
{
    // Define event with string parameter (the save name)
    [System.Serializable]
    public class GalaxyLoadedUnityEvent : UnityEvent<string> { }
    
    // Instance-based Unity event for inspector connections
    public GalaxyLoadedUnityEvent GalaxyLoaded = new GalaxyLoadedUnityEvent();
    
    // Static event for code-based subscribers
    public delegate void GalaxyLoadedDelegate(string galaxyName);
    public static event GalaxyLoadedDelegate GalaxyLoadedEvent;
    
    public void EmitGalaxyLoaded(string saveName)
    {
        // Invoke both the Unity event and static event
        GalaxyLoaded.Invoke(saveName);
        GalaxyLoadedEvent?.Invoke(saveName);
        
        // Self-destruct after emitting the signal
        Destroy(gameObject, 0.1f);
    }
    
    /// <summary>
    /// Create a singleton instance on demand
    /// </summary>
    public static GalaxyLoadSignalEmitter GetInstance()
    {
        GalaxyLoadSignalEmitter instance = GameObject.FindAnyObjectByType<GalaxyLoadSignalEmitter>();
        
        if (instance == null)
        {
            GameObject emitterObj = new GameObject("GalaxyLoadSignalEmitter");
            instance = emitterObj.AddComponent<GalaxyLoadSignalEmitter>();
        }
        
        return instance;
    }
}