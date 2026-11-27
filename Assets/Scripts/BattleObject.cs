using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New Battle Object", menuName = "ScriptableObjects/Battle Object", order = 1)]
public class BattleObject : ScriptableObject
{
    public List<WaveObject> waves;

    // Build a runtime encounter without changing the shared battle/wave assets.
    public BattleObject CreateEncounter(bool isBossBattle, IEnumerable<BattleObject> enemyPool = null)
    {
        var encounter = Instantiate(this);
        encounter.waves = new List<WaveObject>();
        foreach (var template in waves)
        {
            var wave = Instantiate(template);
            var candidates = (isBossBattle || enemyPool == null ? template.Enemies :
                enemyPool.SelectMany(battle => battle.waves).SelectMany(poolWave => poolWave.Enemies))
                .Where(enemy => enemy != null && !string.IsNullOrEmpty(enemy.AssetGUID))
                .GroupBy(enemy => enemy.AssetGUID).Select(group => group.First())
                .ToArray();
            if (candidates.Length == 0)
            {
                Destroy(wave);
                foreach (var createdWave in encounter.waves) Destroy(createdWave);
                Destroy(encounter);
                throw new System.InvalidOperationException($"Wave '{template.name}' has no enemies.");
            }

            int count = isBossBattle ? 1 : Random.Range(1, Mathf.Min(4, candidates.Length) + 1);
            wave.Enemies = new UnityEngine.AddressableAssets.AssetReference[count];
            for (int i = 0; i < count; i++)
            {
                int selected = isBossBattle ? 0 : Random.Range(i, candidates.Length);
                var enemy = candidates[selected];
                candidates[selected] = candidates[i];
                candidates[i] = enemy;
                wave.Enemies[i] = enemy;
            }
            encounter.waves.Add(wave);
        }
        return encounter;
    }
}
