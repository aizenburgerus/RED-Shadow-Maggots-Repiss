using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Stat Change Effect", menuName = "ScriptableObjects/Game Effects/Stat Change Effect", order = 1)]
public class StatChangeEffect : BaseGameEffect
{
    [SerializeReference, SubclassSelector] public BaseGameStat stat;

    public override bool Activate(AppliedEffect effect)
    {
        // ЗАЩИТА: Проверяем критически важные ссылки
        if (effect == null) return false;

        if (stat == null)
        {
            Debug.LogError($"[StatChangeEffect] Ошибка: Поле 'stat' не инициализировано или стерлось при загрузке на {effect.Target?.name}!");
            return false;
        }

        if (effect.cachedValues == null)
        {
            effect.cachedValues = new List<CachedValue>();
        }

        if (effect.cachedValues.Count == 0)
        {
            // ЗАЩИТА: Проверяем, есть ли откуда считывать значение
            if (effect.value == null)
            {
                Debug.LogError($"[StatChangeEffect] Ошибка: 'effect.value' равен null при попытке активировать эффект на {effect.Target?.name}!");
                return false;
            }

            var value = effect.value.GetValue(effect.targetProps);

            if (effect.Target != null)
            {
                stat.SetGameStat(effect.Target, value);
            }

            effect.cachedValues.Add(new CachedValue { Value = value, Type = effect.value.ValueType });
        }
        else
        {
            var amount = effect.cachedValues[0];
            if (effect.Target != null && amount != null)
            {
                stat.SetGameStat(effect.Target, amount.Value);
            }
        }

        return base.Activate(effect);
    }


    public override void OnExpire(AppliedEffect effect)
    {
        // Проверяем, есть ли сохраненное значение, чтобы избежать краша
        if (effect.cachedValues != null && effect.cachedValues.Count > 0)
        {
            stat.SetGameStat(effect.Target, -effect.cachedValues[0].Value);
        }
        else
        {
            Debug.LogWarning($"[StatChangeEffect] Попытка удалить эффект {name}, но cachedValues пуст!");
        }

        base.OnExpire(effect);
    }

    public override string GetEffectDescription(AppliedEffect effect)
    {
        var d = base.GetEffectDescription(effect);

        // 1. Безопасно заменяем имя стата
        if (stat != null)
        {
            d = d.Replace("$STAT", stat.Name);
        }

        // 2. Безопасно заменяем значение $VALUE, проверяя стат и кэш
        if (effect.value != null && stat != null)
        {
            if (effect.cachedValues != null && effect.cachedValues.Count > 0)
            {
                d = d.Replace("$VALUE", effect.cachedValues[0].Value.FormatTo(stat.ValueType));
            }
            else if (effect.targetProps != null)
            {
                // Фоллбек: если кэша нет, пробуем рассчитать базовое значение «на лету»
                var fallbackValue = effect.value.GetValue(effect.targetProps);
                d = d.Replace("$VALUE", fallbackValue.FormatTo(stat.ValueType));
            }
        }

        // 3. Безопасно обрабатываем описание умения
        if (effect.value != null)
        {
            d = effect.value.ProcessSkillDescription(effect.effectTarget, d);
        }

        return d;
    }

    public override string GetSkillDescription(EffectGroup eg)
    {
        var d = base.GetSkillDescription(eg);

        var key = "$STAT";
        var statName = stat != null ? stat.Name : "NO STAT";
        d = d.Replace(key, statName);

        //if (props.value != null)
        //{
        //    d = props.value.ProcessSkillDescription(d);
        //}

        return d;
    }
}