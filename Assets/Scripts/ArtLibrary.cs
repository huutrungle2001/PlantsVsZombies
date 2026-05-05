using System.Collections.Generic;
using UnityEngine;

public static class ArtLibrary
{
    private const string AnimRoot = "Animations";

    private static readonly string[] DefaultZombies =
    {
        "NormalZombie",
        "ConeheadZombie",
        "BucketheadZombie",
        "FlagZombie"
    };

    private static readonly Dictionary<string, RuntimeAnimatorController> ControllerCache =
        new Dictionary<string, RuntimeAnimatorController>();

    public static RuntimeAnimatorController GetPlantController(string plantName)
    {
        return LoadController("Plants/" + plantName);
    }

    public static RuntimeAnimatorController GetRandomZombieController()
    {
        if (DefaultZombies.Length == 0)
        {
            return null;
        }

        var index = Random.Range(0, DefaultZombies.Length);
        return LoadController("Zombies/" + DefaultZombies[index]);
    }

    private static RuntimeAnimatorController LoadController(string subPath)
    {
        var resourcePath = AnimRoot + "/" + subPath;

        if (ControllerCache.TryGetValue(resourcePath, out var cached))
        {
            return cached;
        }

        var controller = Resources.Load<RuntimeAnimatorController>(resourcePath);
        if (controller != null)
        {
            ControllerCache[resourcePath] = controller;
        }

        return controller;
    }
}
