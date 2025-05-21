using UnityEngine;

public static class CelestialBodyFactory
{
    public enum CelestialBodyType
    {
        Planet,
        Star,
        Moon,
        Asteroid
    }

    public static CelestialBodyModel CreateCelestialBody(CelestialBodyType type, GameObject parent = null)
    {
        GameObject celestialObject = new GameObject();
        CelestialBodyModel body = null;

        switch (type)
        {
            case CelestialBodyType.Planet:
                celestialObject.name = "PlanetModel";
                body = celestialObject.AddComponent<PlanetModel>();
                break;

            case CelestialBodyType.Star:
                celestialObject.name = "StarModel";
                body = celestialObject.AddComponent<StarModel>();
                break;

            case CelestialBodyType.Moon:
                celestialObject.name = "MoonModel";
                body = celestialObject.AddComponent<PlanetModel>();
                break;

            case CelestialBodyType.Asteroid:
                celestialObject.name = "AsteroidModel";
                body = celestialObject.AddComponent<PlanetModel>();
                break;
        }

        if (parent != null)
        {
            celestialObject.transform.SetParent(parent.transform);
        }

        return body;
    }
}