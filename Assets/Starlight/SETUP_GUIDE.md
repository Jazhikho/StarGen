# Starlight for Unity: Complete Setup Guide

This guide will walk you through setting up a complete starfield in Unity using the Starlight system. It covers everything from basic setup to advanced customization.

## Prerequisites

- Unity 2020.3 or later (this implementation was designed for the Universal Render Pipeline but can be adapted for Built-in or HDRP)
- Basic understanding of Unity Editor and C#

## Installation

1. Create a new folder in your project's Assets folder named "Starlight"
2. Copy all the provided scripts and shaders into that folder:
   - Star.cs
   - StarManager.cs
   - StarGenerator.cs
   - StarfieldData.cs
   - ComputeShaderSupport.cs
   - StarlightDemo.cs
   - Star.shader

## Creating Required Assets

### 1. Create a Point Spread Function Texture

The PSF texture is critical for realistic star rendering. For this guide, we'll create a simple one:

1. In Unity, right-click the Project window and select Create > Texture > 2D Texture
2. Name it "StarPSF"
3. Set the settings:
   - Size: 1024x1024
   - Format: RGBA 32bit
   - Wrap Mode: Clamp
   - Filter Mode: Bilinear
4. Double-click to open the texture editor
5. Create a bright point in the center that fades outward with an optional diffraction pattern
6. Save the texture

*Alternatively, you can import an existing PSF image if you have one.*

### 2. Create the Star Material

1. In your Project window, right-click and select Create > Material
2. Name it "StarMaterial"
3. In the Inspector, change the Shader to "Starlight/Star" (you may need to search for it)
4. Set the material properties:
   - Emission Texture: Assign the StarPSF texture you created
   - Emission Tint: (0.252, 0.157, 0.292) or any color you prefer
   - Blur Amount: 0.5
   - Emission Energy: 2e+10
   - Billboard Size Deg: 45
   - Luminosity Cap: 4e+06
   - Meters Per Lightyear: 100
   - Color Gamma: 3.0
   - Texture Gamma: 1000.0
   - Min Size Ratio: 0.003
   - Max Luminosity: 100000.0
   - Scaling Gamma: 0.45

## Setting Up The Starfield

### Method 1: Using the Built-in Star Generator

1. Create an empty GameObject in your scene
   - In the Hierarchy window, right-click and select Create Empty
   - Name it "Starfield"

2. Add the StarManager component:
   - With the Starfield object selected, click Add Component in the Inspector
   - Search for "StarManager" and add it

3. Configure the StarManager:
   - Star Material: Assign the StarMaterial you created
   - Star Mesh: Create or assign a simple quad mesh
   - Emission Texture: Assign the StarPSF texture

4. Add the StarGenerator component:
   - Click Add Component again and search for "StarGenerator"
   - Configure its properties:
     - Size: 5000
     - Star Count: 10000 (start lower and increase as needed)
     - Seed: Any number (changing this gives different star patterns)
     - Generate At Origin: Check if you want a sun-like star at center
     - Generate On Start: Check to automatically generate on scene load

5. Press Play to see your starfield!

### Method 2: Manual Star Creation with Code

1. Set up the StarManager component as described in Method 1

2. Instead of adding StarGenerator, create a new script (e.g., "CustomStarfield.cs") with this code:

```csharp
using UnityEngine;
using System.Collections.Generic;
using Starlight;

public class CustomStarfield : MonoBehaviour
{
    private StarManager starManager;

    void Start()
    {
        starManager = GetComponent<StarManager>();
        if (starManager == null) return;

        // Create a custom list of stars
        List<Star> stars = new List<Star>();

        // Add some specific stars
        stars.Add(new Star(Vector3.zero, 1.0f, 5778f)); // Sun at origin
        stars.Add(new Star(new Vector3(500, 0, 0), 0.1f, 3500f)); // Red dwarf
        stars.Add(new Star(new Vector3(0, 500, 0), 25f, 9500f)); // Blue star
        stars.Add(new Star(new Vector3(0, 0, 500), 5f, 6000f)); // F-class star

        // Add lots of random stars
        for (int i = 0; i < 10000; i++)
        {
            Vector3 pos = Random.insideUnitSphere * 5000f;
            float lum = Random.Range(0.01f, 5f); 
            float temp = Random.Range(2400f, 10000f);
            stars.Add(new Star(pos, lum, temp));
        }

        // Set the star list on the manager
        starManager.SetStarList(stars);
    }
}
```

3. Add this script to your Starfield GameObject and press Play

## Additional Options and Customization

### Camera Setup Recommendations

For optimal starfield rendering:

1. Set your camera's Clear Flags to "Solid Color" with a black background
2. Set the camera's far clip plane to a large value (10000+)
3. For space scenes, consider adding a simple skybox for distant nebulae

### Performance Tweaking

If you experience performance issues:

1. Reduce the star count
2. Increase Min Size Ratio (0.005 to 0.01)
3. Decrease Max Luminosity
4. Make sure your PSF texture is efficiently cropped

### Runtime Modifications

You can modify the starfield at runtime:

```csharp
// Get references
StarManager starManager = GetComponent<StarManager>();
StarGenerator starGenerator = GetComponent<StarGenerator>();

// Change generator properties and regenerate
starGenerator.seed = 12345;
starGenerator.starCount = 5000;
starGenerator.RegenerateStars();

// Or manually update some stars
List<Star> stars = new List<Star>(starManager.GetStarList());
stars[0].Position = new Vector3(0, 100, 0);
starManager.SetStarList(stars);
```

## Troubleshooting

### Stars Not Visible

1. Check that your camera is positioned within range of the stars
2. Verify that the StarManager has a valid material and mesh
3. Ensure Emission Energy is high enough
4. Check that the star shader is compatible with your render pipeline

### Performance Issues

1. Reduce star count
2. Check GPU profiler for overdraw issues
3. Optimize PSF texture cropping settings
4. Ensure Min Size Ratio isn't too low

### Rendering Artifacts

1. Adjust Billboard Size Deg
2. Check Z-buffer settings
3. Increase Luminosity Cap for very bright stars
4. Adjust Scaling Gamma to prevent texture cropping artifacts

## Advanced: Adding Custom Star Catalogs

For astronomical applications, you might want to use real star data:

```csharp
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Starlight;
using System.Globalization;

public class RealStarCatalog : MonoBehaviour
{
    public TextAsset starCatalogFile;
    private StarManager starManager;
    
    void Start()
    {
        starManager = GetComponent<StarManager>();
        if (starCatalogFile != null)
        {
            LoadStarCatalog();
        }
    }
    
    void LoadStarCatalog()
    {
        List<Star> stars = new List<Star>();
        string[] lines = starCatalogFile.text.Split('\n');
        
        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
            
            string[] fields = line.Split(',');
            if (fields.Length >= 5)
            {
                try
                {
                    // Parse coordinates (usually in light years)
                    float x = float.Parse(fields[0], CultureInfo.InvariantCulture);
                    float y = float.Parse(fields[1], CultureInfo.InvariantCulture);
                    float z = float.Parse(fields[2], CultureInfo.InvariantCulture);
                    
                    // Parse luminosity and temperature
                    float luminosity = float.Parse(fields[3], CultureInfo.InvariantCulture);
                    float temperature = float.Parse(fields[4], CultureInfo.InvariantCulture);
                    
                    stars.Add(new Star(new Vector3(x, y, z), luminosity, temperature));
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error parsing line: {line}. Error: {e.Message}");
                }
            }
        }
        
        starManager.SetStarList(stars);
    }
}
```

## Conclusion

You should now have a fully functional starfield in your Unity scene. The system is designed to be flexible, allowing you to create either random procedural starfields or accurately represented astronomical data.

For further customization, feel free to modify the shaders and scripts to meet your specific needs.
