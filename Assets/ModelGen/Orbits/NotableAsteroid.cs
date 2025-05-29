using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Represents a notable object in an asteroid belt, such as a dwarf planet or large asteroid
/// </summary>
public class NotableAsteroid : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private Renderer objectRenderer;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private float rotationSpeed = 20f;
    
    [Header("UI Elements")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI sizeText;
    [SerializeField] private TextMeshProUGUI compositionText;
    
    private Color originalColor;
    private bool isSelected = false;
    private string asteroidName;
    private float radius;
    private Dictionary<string, float> composition;
    
    private void Awake()
    {
        if (objectRenderer == null)
        {
            objectRenderer = GetComponent<Renderer>();
        }
        
        if (objectRenderer != null)
        {
            originalColor = objectRenderer.material.color;
        }
        
        // Hide info panel by default
        if (infoPanel != null)
        {
            infoPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Set the data for this notable asteroid
    /// </summary>
    public void SetData(string name, float radiusKm, Dictionary<string, float> asteroidComposition)
    {
        asteroidName = name;
        radius = radiusKm;
        composition = asteroidComposition;
        
        UpdateInfoPanel();
    }
    
    private void Update()
    {
        // Rotate around Y axis
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        
        // Update info panel position if active
        if (isSelected && infoPanel != null && infoPanel.activeSelf)
        {
            // Position panel above the asteroid in screen space
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            infoPanel.transform.position = screenPos + new Vector3(0, 100, 0);
        }
    }
    
    private void OnMouseDown()
    {
        isSelected = !isSelected;
        
        if (objectRenderer != null)
        {
            objectRenderer.material.color = isSelected ? highlightColor : originalColor;
        }
        
        if (infoPanel != null)
        {
            infoPanel.SetActive(isSelected);
        }
    }
    
    private void UpdateInfoPanel()
    {
        if (nameText != null)
        {
            nameText.text = asteroidName;
        }
        
        if (sizeText != null)
        {
            sizeText.text = $"Radius: {radius:F1} km";
        }
        
        if (compositionText != null && composition != null)
        {
            string compText = "Composition:\n";
            foreach (var kvp in composition)
            {
                compText += $"{kvp.Key}: {kvp.Value:P0}\n";
            }
            compositionText.text = compText;
        }
    }
} 