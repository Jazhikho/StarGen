# StarGen Project TODO List

## MUST DO (Essential Functionality)

### Database Issues
- **GalaxyDataStore/SaveSystem Consistency**: Although not a dual saving system, the codebase has two serialization implementations (GalaxyDataStore methods and SaveSystem) that could get out of sync. Need to clean up older serialization methods if they're not being used.

### Missing Generators
- Create PopulationGenerator to generate population data for habitable planets - Data model exists but generator is missing
- Implement proper linking between SectorGenerator and GalaxyView

### Data Relationship Fixes
- Implement proper parent-child relationships across all generators
- Ensure all objects properly link to parent systems

### Core Visualization
- ✅ Handle AsteroidBelt → CelestialBodyData (as Asteroid type)
- ✅ Add support for asteroid belt visualization
- Add support for ring system visualization
- Fix mass lookup system to use actual data - Partially implemented

## GOOD TO DO (Releasable Version)

### Generation Improvements
- Implement anomaly generation in SectorGenerator.cs - Code stub exists but unimplemented
- Create a centralized generation process manager
- Add support for partial galaxy generation and expansion

### Biology and Population
- Generate biospheres for habitable planets
- Create population centers with realistic distributions

### UI and Scene Management
- Implement real-time generation statistics display
- Create more informative progress indicators during generation
- Implement better error messages and recovery options
- Improve the SaveLoadManager to include metadata display

### Generation Flow Improvements
- Implement asynchronous sector generation for better UI responsiveness

### Visualization Enhancements
- ✅ Create particle system or instanced mesh renderer for asteroids
- Create ring mesh generator
- Support multiple ring bands
- Add gap rendering
- Support opacity and color variation in rings
- Add atmosphere rendering for planets
- Use biome/atmosphere data for coloring planets - Partially implemented

## FUTURE TODO (Future Features)

### Advanced Generation
- Create GalaxyGenerator class to manage the generation of multiple sectors as a cohesive galaxy
- Generate civilization histories
- Implement inter-civilization relationships
- Add technological advancement simulation
- Add navigation paths and trade routes between populated systems
- Create a more robust spatial indexing system for faster lookups
- Add step-by-step generation with progress saving
- Add priority-based generation (generate inhabited systems first)
- Create generation templates for different galaxy types

### Advanced Visualization
- Add corona/atmosphere effects for different star types
- Add cloud layers for certain planets
- ✅ Support different density regions in asteroid belts
- ✅ Add notable object markers in asteroid belts
- ✅ Support thickness variation in asteroid belts
- Add LOD system for performance
- Add galaxy metadata editor in UI

### Performance Optimizations
- Implement procedural generation instead of storing all data
- Add demand-loading for sectors and systems
- Optimize memory usage for large galaxy generation
- Add multi-threaded generation support
- Add background preloading for scene transitions 