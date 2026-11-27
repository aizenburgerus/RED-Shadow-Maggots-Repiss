using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class BaseEffectValue
{
    public ValueType ValueType;

    public abstract float GetValue(TargetProps targetProps);

    public abstract string ProcessSkillDescription(BaseEffectTarget target, string description);

    public abstract string Descriptor { get; }

    public abstract BattleState.SerializedValue Serialize();

    public virtual string GetEffectDescription(AppliedEffect effect)
    {
        //var value = GetValue(effect.targetProps);
        return effect.cachedValues[0].Value.FormatTo(effect.cachedValues[0].Type);
    }

    /// <summary>
    /// Shallow-Copy
    /// </summary>
    /// <returns></returns>
    public virtual BaseEffectValue Clone()
    {
        return MemberwiseClone() as BaseEffectValue;
    }
}

[System.Serializable]
public class FlatValue : BaseEffectValue
{
    public float Flat;

    public override float GetValue(TargetProps targetProps)
    {
        return Flat;
    }

    public override string Descriptor => Flat.FormatTo(ValueType);

    public override string ProcessSkillDescription(BaseEffectTarget target, string description)
    {
        description = description.Replace("$VALUE", Descriptor);

        return description;
    }

    public override BattleState.SerializedValue Serialize()
    {
        return new()
        { 
            Type = nameof(FlatValue),
            Values = new[] { Flat.ToString(), ((int)ValueType).ToString() }
        };
    }
}

// Kept under its original name so existing SerializeReference ability data can load.
[System.Serializable]
public class StatScaledValue : BaseEffectValue
{
    [SerializeReference] public BaseEffectValue Value;
    [SerializeReference] public BaseGameStat Stat;
    [SerializeReference] public BaseEffectTarget StatSource;

    public override float GetValue(TargetProps targetProps)
    {
        var targets = StatSource.GetTargets(targetProps.Caster, targetProps.Targets[0]);
        return Value.GetValue(targetProps) * Stat.GetValue(new TargetProps { Caster = targetProps.Caster, Targets = targets });
    }

    public override string Descriptor => Value.Descriptor + " of " + Stat.Name;

    public override string ProcessSkillDescription(BaseEffectTarget target, string description) =>
        description.Replace("$VALUE", Descriptor);

    public override BattleState.SerializedValue Serialize() => new BattleState.SerializedValue
    {
        Type = nameof(StatScaledValue),
        Values = new[] { ((int)ValueType).ToString(), Stat.GetType().FullName, StatSource.GetType().FullName },
        NestedValues = new[] { Value.Serialize() }
    };

    public static StatScaledValue Deserialize(BattleState.SerializedValue value) => new StatScaledValue
    {
        Value = BattleState.SerializedValue.Deserialize(value.NestedValues[0]),
        ValueType = (ValueType)System.Convert.ToInt32(value.Values[0]),
        Stat = (BaseGameStat)System.Activator.CreateInstance(System.Type.GetType(value.Values[1])),
        StatSource = (BaseEffectTarget)System.Activator.CreateInstance(System.Type.GetType(value.Values[2]))
    };
}

public static class ValueExtensions
{
    public static string FormatTo(this float value, ValueType valueType)
    {
        string valueString = "";
        switch (valueType)
        {
            case ValueType.Percentage:
                {
                    var abs = Mathf.Abs(value);
                    valueString = abs.FormatPercentage();
                }
                break;
            case ValueType.Value:
                var val = Mathf.FloorToInt(value);
                valueString = Mathf.Abs(val).ToString();
                break;
            case ValueType.Decimal:
                {
                    var abs = Mathf.Abs(value);
                    valueString = abs.FormatToDecimal();
                }
                break;
        }
        return valueString;
    }
}
