# Starlight for Unity

[![MIT License](https://img.shields.io/github/license/tiffany352/godot-starlight)](https://github.com/tiffany352/godot-starlight)

Starlight is a Unity implementation of the [Starlight for Godot](https://github.com/tiffany352/godot-starlight) addon, capable of rendering 100,000+ stars in realtime with low performance cost. It's an alternative to using a skybox (or can be used in conjunction with one) by creating a 3D starfield, and may be particularly useful for space games or astronomical visualizations.

This is a C# port of the original GDScript implementation, with adaptations for Unity's rendering pipeline.

![Screenshot](https://github.com/tiffany352/godot-starlight/raw/main/docs/screenshot.jpg)
(Original screenshot from the Godot version)

## Features

- Stars are rendered positionally in 3D, allowing you to fly around and see stars go by
- Exact position, luminosity, and temperature of each star can be configured
- Physically based light model: Using a Point Spread Function (PSF), rather than a texture that grows or shrinks with distance/brightness
- Based on GPU instancing for performance
- Uses a trick to avoid being clipped by the far plane, to let stars be very far away
- Comes with a random star generator based on main sequence stars (classes M through O).

## Installation

1. Ensure you're using Unity 2020.3 or later
2. Download this package and import it into your Unity project
3. Check out the demonstration scene for an example setup

## Getting Started

### Quick Setup

1. Create an empty GameObject in your scene
2. Add the `StarManager` component to the GameObject
3. Add the `StarGenerator` component to the same GameObject
4. Configure the star material and PSF texture:
   - For the material, create one from the Star shader (Shader: "Starlight/Star")
   - For the PSF texture, you'll need a texture that represents a point spread function
5. Configure the star generation parameters on the `StarGenerator` component
6. Press Play to see your stars!

### Manual Setup

If you want to manually control the stars:

```csharp
// Create a new list of stars
List<Star> stars = new List<Star>();

// Add stars with position, luminosity, and temperature
stars.Add(new Star(new Vector3(0, 0, 0), 1.0f, 5778f));  // Sun-like star at origin
stars.Add(new Star(new Vector3(10, 5, 20), 0.5f, 3500f)); // Red dwarf
stars.Add(new Star(new Vector3(-15, 8, -30), 15f, 9500f)); // Blue giant

// Get the star manager and set the star list
StarManager starManager = GetComponent<StarManager>();
starManager.SetStarList(stars);
```

### Using StarfieldData

For more complex scenarios, you can use the `StarfieldData` class to manage your star catalog:

```csharp
// Create a new starfield data object
StarfieldData starfieldData = new StarfieldData();

// Add stars to the starfield data
starfieldData.AddStar(new Star(new Vector3(0, 0, 0), 1.0f, 5778f));
starfieldData.AddStar(new Star(new Vector3(10, 5, 20), 0.5f, 3500f));

// Pass the star list to the star manager
StarManager starManager = GetComponent<StarManager>();
starManager.SetStarList(starfieldData.Stars);
```

## Configuration

### Star Manager Properties

The `StarManager` component exposes these properties:

- **Star Material**: The material used for rendering stars (must use the Star shader)
- **Star Mesh**: The mesh used for stars (typically a quad)
- **Emission Tint**: Color tint applied to the emission texture
- **Blur Amount**: How much to blur the PSF texture
- **Emission Energy**: Multiplier for star brightness
- **Billboard Size Deg**: Controls how much screen space the PSF takes up, in degrees
- **Luminosity Cap**: Maximum brightness a star can have
- **Meters Per Lightyear**: Scaling factor for star distances
- **Color Gamma**: Controls how strongly star colors show through
- **Texture Gamma**: Gamma correction for the PSF texture
- **Clamp Output**: Clamps the output color from 0 to 1 when enabled
- **Min Size Ratio**: Minimum size that a star can render at
- **Max Luminosity**: Point at which the full PSF texture is used
- **Scaling Gamma**: Controls falloff of diffraction spikes
- **Debug Show Rects**: Shows visualization for debugging cropping settings
- **Emission Texture**: The Point Spread Function texture

### Star Generator Properties

- **Size**: Radius of the sphere in which to place stars
- **Star Count**: Number of stars to generate
- **Seed**: Random seed for generation
- **Generate At Origin**: If enabled, places a Sun-like star at (0,0,0)
- **Generate On Start**: Automatically generate stars when the scene starts

## Performance Tips

- The PSF texture needs to be cropped based on star brightness for good performance
- The majority of stars only appear as a couple of pixels, so efficient cropping dramatically reduces overdraw
- Find the right balance with the `Min Size Ratio`, `Max Luminosity`, and `Scaling Gamma` parameters
- Start with lower star counts (1,000-10,000) while tweaking these values
- Use the `Debug Show Rects` option to visualize the cropping

## Creating Your Own PSF Texture

The Point Spread Function (PSF) texture is critical for realistic star rendering. You might want to create your own for:

1. Different telescopes/instruments
2. Stylized star appearances
3. Lower performance requirements

A good PSF texture should:
- Be high resolution (1024x1024 or higher)
- Have a bright central point that falls off gradually
- Possibly include diffraction patterns for realism

There are four PSF's provided for you in the `/psf-textures` file; using the JWST version is highly recommended for Unity,
as it looked best on my system, but you may have different results.

## Credits

This project is a C# port of [Starlight for Godot](https://github.com/tiffany352/godot-starlight) by Tiffany Bennett.

Original code is released under MIT license.

## License

This code is released under [MIT license](./LICENSE).
