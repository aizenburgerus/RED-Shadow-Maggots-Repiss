using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Facade;

namespace SuperCrits
{
    public class BaseSuperCrit : BaseCharacterPassive
    {
        BaseAbilityObject SuperCrit => baseCharacter.Reference.superCritical;

        protected override void Init()
        {
            baseCharacter.OnBeginSuperCrit += ResetEffects;
        }

        protected override void Cleanup()
        {
            baseCharacter.OnBeginSuperCrit -= ResetEffects;
        }

        void ResetEffects() => effectsApplied = 0;

        public void OnBeginSuperCrit(int startIndex)
        {
            if (BaseCharacter.IncomingAttack.Targets.Caster != baseCharacter) return;
            if (SuperCrit != null && SuperCrit.effects.Length > 0 && SuperCrit.effects[0].attackProps == null)
            {
                effectsApplied = 0;
                while (effectsApplied < Mathf.Min(startIndex, SuperCrit.effects.Length))
                    ApplyNextSuperCritEffect();
            }
            effectsApplied = Mathf.Max(0, startIndex);
        }

        int effectsApplied = 0;
        public void ApplyNextSuperCritEffect()
        {
            if (BaseCharacter.IncomingAttack.Targets.Caster != baseCharacter || SuperCrit == null ||
                SuperCrit.effects == null || effectsApplied >= SuperCrit.effects.Length) return;
            var group = SuperCrit.effects[effectsApplied++].Copy();
            var targets = group.effectTarget.GetTargets(baseCharacter, battleSystem.OpposingCharacter);
            if (targets == null || targets.Length == 0) return;
            StartCoroutine(group.appStyle.Apply(group, baseCharacter, targets[0]));
        }
    }
}
