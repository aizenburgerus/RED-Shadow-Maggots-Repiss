using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Revive Effect", menuName = "ScriptableObjects/Game Effects/On Death/Revive", order = 1)]
public class ReviveEffect : BaseGameEffect
{
    public override bool IncludesExplainer => true;
    public override string ExplainerName => "Revive";
    public override string ExplainerDescription =>
        "When " + RSMConstants.Keywords.Short.HEALTH + " reaches 0, " +
        "prevents death and regain " + RSMConstants.Keywords.Short.HEALTH + ".";

    public override bool Activate(AppliedEffect effect)
    {
        if (effect == null) return false;

        // Инициализируем список кэша, если он почему-то null
        if (effect.cachedValues == null)
        {
            effect.cachedValues = new List<CachedValue>();
        }

        if (effect.cachedValues.Count == 0)
        {
            // ЗАЩИТА: Если формула value стерлась при загрузке, ставим временное значение 0
            float value = 0f;

            if (effect.value != null)
            {
                value = effect.value.GetValue(effect.targetProps);
            }
            else
            {
                Debug.LogWarning($"[ReviveEffect] Предупреждение: effect.value равен null при загрузке на {effect.Target?.name}. Значение установлено в 0.");
            }

            effect.cachedValues.Add(new() { Value = value, Type = ValueType.Value });
        }

        return base.Activate(effect);
    }


    public override void OnDeath(AppliedEffect effect)
    {
        effect.Target.Heal(effect.cachedValues[0].Value);
    }

    //public override string GetSkillDescription(TargetMode targetMode, EffectProperties props)
    //{
    //    var target = TargetModeDescriptor(targetMode);
    //
    //    string d = "";
    //
    //    if (target == "")
    //    {
    //        d += "Receive a ";
    //    }
    //    else
    //    {
    //        d += target + "receive a ";
    //    }
    //
    //    d += "<u>" + ExplainerName + "</u> status";
    //
    //    d += " that recovers " + RSMConstants.Keywords.Short.HEALTH + " equal to ";
    //
    //    d += EffectValueDescriptor(props.effectValues[0], stat) +
    //        DurationAndActivationDescriptor(props.effectDuration, props.activationLimit);
    //
    //    return d;
    //}

    //public override string GetEffectDescription(AppliedEffect effect)
    //{
    //    string d = ExplainerDescription;
    //    var index = d.IndexOf(RSMConstants.Keywords.Short.HEALTH);
    //    return d.Insert(index, effect.cachedValues[0] + " ");
    //}
}