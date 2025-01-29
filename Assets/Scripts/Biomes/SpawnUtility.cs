using UnityEngine;
using System.Collections.Generic;

public static class SpawnUtility
{
    public static EnemySpawnOption WeightedRandomPick(List<EnemySpawnOption> options)
    {
        if (options == null || options.Count == 0)
        {
            Debug.LogWarning("WeightedRandomPick chiamato con lista vuota o nulla.");
            return null;
        }

        float totalWeight = 0f;

        foreach (var opt in options)
        {
            if (opt.weight <= 0)
            {
                Debug.LogWarning($"Opzione {opt.enemyPrefab?.name ?? "null"} ha un peso non valido: {opt.weight}");
                continue;
            }
            totalWeight += opt.weight;
        }

        if (totalWeight <= 0)
        {
            Debug.LogWarning("Tutti i pesi delle opzioni sono non validi o zero.");
            return null;
        }

        float rand = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var opt in options)
        {
            cumulative += opt.weight;
            if (rand <= cumulative)
            {
                Debug.Log($"Selezionato: {opt.enemyPrefab.name} con peso {opt.weight}");
                return opt;
            }
        }

        Debug.LogWarning("WeightedRandomPick ha raggiunto il fallback. Questo non dovrebbe succedere!");
        return options[options.Count - 1];
    }
}