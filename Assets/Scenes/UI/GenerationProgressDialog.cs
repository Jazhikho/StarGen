using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Collections;

public class GenerationProgressDialog : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text statusLabel;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text progressDetails;
    [SerializeField] private TMP_Text statsLabel;
    [SerializeField] private Button cancelButton;

    // Events
    public event Action GenerationCanceled;
    public event Action GenerationCompleted;

    // Progress tracking variables
    private int _totalSectors = 0;
    private int _currentSector = 0;
    private int _starCount = 0;
    private int _systemCount = 0;
    private int _planetCount = 0;
    private Dictionary<string, object> _generationParams = null;
    
    // Data collections
    private List<Sector> _generatedSectors = new List<Sector>();
    private List<StarSystem> _generatedSystems = new List<StarSystem>();
    
    // Generator instance
    private SectorGenerator _sectorGenerator = new SectorGenerator();
    
    // Generation control
    private Coroutine _generationCoroutine;
    private bool _isCanceled = false;

    void Start()
    {
        cancelButton.onClick.AddListener(OnCancelButtonPressed);
    }

    public void Initialize(Dictionary<string, object> parameters)
    {
        _generationParams = parameters;
        
        // Get dimensions
        int sectorSizeX = (int)parameters["sector_size_x"];
        int sectorSizeY = (int)parameters["sector_size_y"];
        int sectorSizeZ = (int)parameters["sector_size_z"];
        
        _totalSectors = sectorSizeX * sectorSizeY * sectorSizeZ;
        _currentSector = 0;
        _starCount = 0;
        _systemCount = 0;
        _planetCount = 0;
        _isCanceled = false;
        
        // Clear previous data
        _generatedSectors.Clear();
        _generatedSystems.Clear();

        // Initialize the sector generator with parameters
        _sectorGenerator.Initialize(parameters);

        // Update UI
        statusLabel.text = $"Preparing to generate {_totalSectors} sectors";
        progressBar.value = 0;
        progressBar.maxValue = _totalSectors;
        progressDetails.text = $"Sector: 0 of {_totalSectors}";
        statsLabel.text = "Stars: 0 | Systems: 0 | Planets: 0";

        // Start generation process
        _generationCoroutine = StartCoroutine(GenerateSectorsCoroutine());
    }

    private IEnumerator GenerateSectorsCoroutine()
    {
        // Small delay before starting
        yield return new WaitForSeconds(0.1f);

        // Get sector dimensions
        int sectorSizeX = (int)_generationParams["sector_size_x"];
        int sectorSizeY = (int)_generationParams["sector_size_y"];
        int sectorSizeZ = (int)_generationParams["sector_size_z"];

        // Calculate the starting coordinates so that (0,0,0) is at the center
        int startX = -sectorSizeX / 2;
        int startY = -sectorSizeY / 2;
        int startZ = -sectorSizeZ / 2;

        // Generate sectors in a 3D grid pattern
        for (int z = 0; z < sectorSizeZ; z++)
        {
            for (int y = 0; y < sectorSizeY; y++)
            {
                for (int x = 0; x < sectorSizeX; x++)
                {
                    if (_isCanceled)
                    {
                        yield break;
                    }

                    _currentSector++;
                    
                    // Create sector coordinates
                    Vector3Int sectorCoords = new Vector3Int(
                        startX + x,
                        startY + y,
                        startZ + z
                    );
                    
                    // Generate the sector
                    yield return GenerateSector(sectorCoords);
                    
                    // Update UI
                    UpdateProgress(_currentSector, _starCount, _systemCount, _planetCount);
                    
                    // Small delay to allow UI to update
                    yield return null;
                }
            }
        }

        Complete();
    }

    private IEnumerator GenerateSector(Vector3Int sectorCoords)
    {
        // Generate the sector using SectorGenerator
        Sector sector = _sectorGenerator.GenerateSector(sectorCoords);
        
        // Add the sector to our collection
        _generatedSectors.Add(sector);
        
        // Add all systems from this sector to our master list
        if (sector.Systems != null && sector.Systems.Count > 0)
        {
            _generatedSystems.AddRange(sector.Systems);
            _systemCount += sector.Systems.Count;
            
            // Count stars and planets
            foreach (var system in sector.Systems)
            {
                // Count stars
                if (system.Stars != null)
                    _starCount += system.Stars.Count;
                
                // Count planets
                // 1. Count planets orbiting individual stars
                foreach (var star in system.Stars)
                {
                    if (star.Planets != null)
                        _planetCount += star.Planets.Count;
                }
                
                // 2. Count circumbinary planets (planets orbiting binary pairs)
                foreach (var binaryPair in system.BinaryPairs)
                {
                    if (binaryPair.CircumbinaryPlanets != null)
                        _planetCount += binaryPair.CircumbinaryPlanets.Count;
                }
            }
        }
        
        yield return null;
    }

    private void UpdateProgress(int sectorNum, int stars, int systems, int planets)
    {
        _currentSector = sectorNum;
        _starCount = stars;
        _systemCount = systems;
        _planetCount = planets;

        statusLabel.text = $"Generating Sector {_currentSector}...";
        progressBar.value = _currentSector;
        progressDetails.text = $"Sector: {_currentSector} of {_totalSectors}";
        statsLabel.text = $"Stars: {_starCount} | Systems: {_systemCount} | Planets: {_planetCount}";
    }

    private void Complete()
    {
        statusLabel.text = "Generation Complete!";
        progressBar.value = _totalSectors;
        cancelButton.GetComponentInChildren<TMP_Text>().text = "Continue";

        // Store data in the global singleton
        GalaxyDataStore dataStore = GalaxyDataStore.Instance;

        if (dataStore != null)
        {
            // Clear any existing data
            dataStore.ClearGalaxyData();

            // Set galaxy metadata
            dataStore.GalaxyName = "Generated Galaxy";
            dataStore.GenerationSeed = _generationParams.ContainsKey("random_seed") ? 
                _generationParams["random_seed"].ToString() : "0";
            dataStore.GenerationParameters = _generationParams;
            dataStore.CreationDate = DateTime.Now;
            
            // Add all sectors
            foreach (var sector in _generatedSectors)
            {
                dataStore.AddSector(sector);
            }
            
            // Add all systems
            foreach (var system in _generatedSystems)
            {
                dataStore.AddStarSystem(system);
            }
            
            Debug.Log($"Galaxy generation complete - Sectors: {_generatedSectors.Count}, " +
                     $"Systems: {_generatedSystems.Count}, Stars: {_starCount}, " +
                     $"Planets: {_planetCount}");

            // Invoke the completion event
            GenerationCompleted?.Invoke();
        }
        else
        {
            Debug.LogError("Failed to get GalaxyDataStore singleton");
        }
    }

    private void OnCancelButtonPressed()
    {
        if (_currentSector >= _totalSectors)
        {
            // Generation is complete, just close and continue
            GenerationCompleted?.Invoke();
            Destroy(gameObject);
            return;
        }

        // Cancel the generation
        _isCanceled = true;
        
        if (_generationCoroutine != null)
        {
            StopCoroutine(_generationCoroutine);
        }
        
        GenerationCanceled?.Invoke();
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (_generationCoroutine != null)
        {
            StopCoroutine(_generationCoroutine);
        }
    }
}