using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects.Stats;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Domain;

using System;

//*=========================================
/*
 * This spell purely handles her ramping AS buff "Rev'd"
 * Issues with this buff removing incrementally and only removing its respective AS boost instead of every instance of it or not at all.
 *
 * Jinx's Q mechanic interacts with her auto-attacks in a lot oof wierd ways that need to be listed out.
 * In the case of this buff one oddity is how she keeps these buff stacks for one AA after switching to Fishbones then it removes all stacks
 * Otherwise they naturally decay.
 *
 * There are other odd mechanics with her Q that have too be implemented or checked for.
*/
//*========================================

namespace Buffs
{
    class JinxQRamp : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER,
            BuffAddType = BuffAddType.STACKS_AND_CONTINUE,
            MaxStacks = 3
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        private IObjAiBase _owner;
        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            _owner = ownerSpell.CastInfo.Owner;

            StatsModifier.AttackSpeed.PercentBaseBonus = CalculateBonusAttackSpeed(_owner.GetSpell(0).CastInfo.SpellLevel);

            _owner.AddStatModifier(StatsModifier);
        }
        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            //_owner.RemoveStatModifier(StatsModifier);
        }

        private float CalculateBonusAttackSpeed(int spellLevel)
        {
            float bAS;

            switch (spellLevel)
            {
                case 1:
                    bAS = .3f;
                    break;
                case 2:
                    bAS = .55f;
                    break;
                case 3:
                    bAS = .80f;
                    break;
                case 4:
                    bAS = 1.05f;
                    break;
                case 5:
                    bAS = 1.3f;
                    break;
                default:
                    bAS = 0.0f;
                    break;
            }
            return bAS;
        }

        public void OnUpdate(float diff)
        {
        }
    }
}
