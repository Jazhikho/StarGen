using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SystemInfoPanel : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text systemIdLabel;
    public TMP_Text systemStatsLabel;
    public TMP_Text primaryStarLabel;
    public TMP_Text starCountLabel;
    public TMP_Text planetCountLabel;
    public TMP_Text asteroidBeltsLabel;
    public Button closeButton;
    
    private StarSystem _currentSystem;
    
    void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HidePanel);
        }
        
        // Initially hide the panel
        gameObject.SetActive(false);
    }
    
    public void ShowSystemInfo(StarSystem system)
    {
        if (system == null)
        {
            ClearPanel("No system selected");
            return;
        }
        
        _currentSystem = system;
        
        // Update system ID
        if (systemIdLabel != null)
            systemIdLabel.text = $"ID: {system.ID}";
        
        // Show additional system statistics
        if (systemStatsLabel != null)
            systemStatsLabel.text = $"Population: {system.SystemPopulation:N0}";
        
        // Primary star information
        if (primaryStarLabel != null)
        {
            if (system.PrimaryStar != null)
            {
                var star = system.PrimaryStar;
                primaryStarLabel.text = $"Primary Star: {GetStarDescription(star)}";
            }
            else
            {
                primaryStarLabel.text = "Primary Star: None";
            }
        }
        
        // Count stars in the system
        if (starCountLabel != null)
        {
            int starCount = system.Stars.Count;
            starCountLabel.text = $"Total Stars: {starCount}";
        }
        
        // Count planets and asteroid belts
        if (planetCountLabel != null || asteroidBeltsLabel != null)
        {
            int planetCount = 0;
            int asteroidBeltCount = 0;
            
            foreach (var star in system.Stars)
            {
                planetCount += star.Planets.Count;
                asteroidBeltCount += star.AsteroidBelts.Count;
            }
            
            // Count circumbinary planets if in binary system
            foreach (var binaryPair in system.BinaryPairs)
            {
                planetCount += binaryPair.CircumbinaryPlanets?.Count ?? 0;
                asteroidBeltCount += binaryPair.CircumbinaryAsteroidBelts?.Count ?? 0;
            }
            
            if (planetCountLabel != null)
                planetCountLabel.text = $"Planets: {planetCount}";
                
            if (asteroidBeltsLabel != null)
                asteroidBeltsLabel.text = $"Asteroid Belts: {asteroidBeltCount}";
        }
        
        // Make panel visible
        gameObject.SetActive(true);
    }
    
    private string GetStarDescription(StarStructure star)
    {
        if (star == null) return "Unknown";
        
        string spectralClass = star.SpectralClass ?? "Unknown";
        float mass = star.Mass;
        float luminosity = star.Luminosity;
        
        // Get star type description from spectral class
        string typeDesc = GetStarTypeDescription(spectralClass);
        
        return $"{typeDesc} (Mass: {mass:F2} M☉, Luminosity: {luminosity:F2} L☉)";
    }
    
    private string GetStarTypeDescription(string spectralClass)
    {
        if (string.IsNullOrEmpty(spectralClass))
            return "Unknown";
            
        char type = spectralClass[0];
        string temperatureDesc;
        
        switch (type)
        {
            case 'O': temperatureDesc = "Blue"; break;
            case 'B': temperatureDesc = "Blue-White"; break;
            case 'A': temperatureDesc = "White"; break;
            case 'F': temperatureDesc = "Yellow-White"; break;
            case 'G': temperatureDesc = "Yellow"; break;
            case 'K': temperatureDesc = "Orange"; break;
            case 'M': temperatureDesc = "Red"; break;
            case 'W':
                if (spectralClass.StartsWith("WD")) return "White Dwarf";
                if (spectralClass.StartsWith("WR")) return "Wolf-Rayet";
                temperatureDesc = "Unknown";
                break;
            case 'C': temperatureDesc = "Carbon"; break;
            default: temperatureDesc = "Unknown"; break;
        }
        
        return $"{spectralClass} ({temperatureDesc})";
    }
    
    private void ClearPanel(string message = "No system selected")
    {
        _currentSystem = null;
        
        if (systemIdLabel != null)
            systemIdLabel.text = "ID: ---";
            
        if (systemStatsLabel != null)
            systemStatsLabel.text = message;
            
        if (primaryStarLabel != null)
            primaryStarLabel.text = "Primary Star: ---";
            
        if (starCountLabel != null)
            starCountLabel.text = "Total Stars: 0";
            
        if (planetCountLabel != null)
            planetCountLabel.text = "Planets: 0";
            
        if (asteroidBeltsLabel != null)
            asteroidBeltsLabel.text = "Asteroid Belts: 0";
            
        gameObject.SetActive(true);
    }
    
    public void HidePanel()
    {
        gameObject.SetActive(false);
    }
}