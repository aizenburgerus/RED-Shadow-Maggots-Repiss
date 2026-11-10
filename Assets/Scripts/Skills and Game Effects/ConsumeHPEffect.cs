using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Consume HP Effect", menuName = "ScriptableObjects/Game Effects/Consume HP", order = 1)]
public class ConsumeHPEffect : BaseDamageEffect
{
    public override bool Activate(AppliedEffect effect)
    {
        // ЗАЩИТА: Проверяем, что эффект, его цель и формула урона существуют
        if (effect == null || effect.Target == null)
        {
            Debug.LogWarning("[ConsumeHPEffect] Эффект или его цель равны null.");
            return false;
        }

        if (effect.value == null)
        {
            Debug.LogError($"[ConsumeHPEffect] Ошибка: Не назначено 'value' (формула/значение) у эффекта на персонаже {effect.Target.name}!");
            return false;
        }

        // Рассчитываем значение урона/поглощения
        var value = effect.value.GetValue(effect.targetProps);

        // Вызываем базовый метод поглощения здоровья
        base.ConsumeHealth(value, effect.Target);

        return base.Activate(effect);
    }

}