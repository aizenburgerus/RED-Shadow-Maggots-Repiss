using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New Battle Object", menuName = "ScriptableObjects/Battle Object", order = 1)]
public class BattleObject : ScriptableObject
{
    public List<WaveObject> waves;

    // Build a runtime encounter without changing the shared battle/wave assets.
    public BattleObject CreateEncounter(bool isBossBattle)
    {
        var encounter = Instantiate(this);
        encounter.waves = new List<WaveObject>();
        foreach (var template in waves)
        {
            var wave = Instantiate(template);
            var candidates = template.Enemies
                .Where(enemy => enemy != null && !string.IsNullOrEmpty(enemy.AssetGUID))
                .ToArray();
            if (candidates.Length == 0)
            {
                Destroy(wave);
                foreach (var createdWave in encounter.waves) Destroy(createdWave);
                Destroy(encounter);
                throw new System.InvalidOperationException($"Wave '{template.name}' has no enemies.");
            }

            int count = isBossBattle ? 1 : Random.Range(1, 5);
            wave.Enemies = new UnityEngine.AddressableAssets.AssetReference[count];
            for (int i = 0; i < count; i++)
            {
                wave.Enemies[i] = candidates[isBossBattle ? 0 : Random.Range(0, candidates.Length)];
            }
            encounter.waves.Add(wave);
        }
        return encounter;
    }
}
