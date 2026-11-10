using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Damage Per Turn Effect", menuName = "ScriptableObjects/Game Effects/Take Damage Per Turn", order = 1)]
public class DamagePerTurnEffect : BaseDamageEffect
{
    public override float TickAnimationTime => -1f;

    public override bool Activate(AppliedEffect effect)
    {
        if (effect.cachedValues.Count == 0)
        {
            var value = effect.value.GetValue(effect.targetProps);
            effect.cachedValues.Add(new() { Value = value, Type = effect.value.ValueType});
        }

        return base.Activate(effect);
    }

    public override string GetEffectDescription(AppliedEffect effect)
    {
        var d = base.GetEffectDescription(effect);

        // ЗАЩИТА: Проверяем, что эффект и его формула не null
        if (effect == null || effect.value == null) return d;

        float actualValue = 0f;

        // 1. Если кэш уже создан, берем значение оттуда
        if (effect.cachedValues != null && effect.cachedValues.Count > 0)
        {
            actualValue = effect.cachedValues[0].Value;
        }
        // 2. Если кэша еще нет, рассчитываем его динамически из формулы
        else if (effect.targetProps != null)
        {
            actualValue = effect.value.GetValue(effect.targetProps);
        }

        // Безопасно форматируем и вставляем значение в описание
        d = d.Replace("$VALUE", actualValue.FormatToDecimal());

        return d;
    }

}