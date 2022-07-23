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
 * ValkyrieHorns
 * Lastupdated: 4/7/2022
 * 
 * TODOS:
 * 
 * Known Issues:
*/
//*========================================

namespace Buffs
{
    class JinxQRamp : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER,
            BuffAddType = BuffAddType.STACKS_AND_DECAYS,
            MaxStacks = 3
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        private IObjAiBase _owner;
        private ISpell _spell;
        private IAttackableUnit _previousTarger = null;
        private IAttackableUnit _currentTarget = null;
        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            _owner = ownerSpell.CastInfo.Owner;
            _spell = ownerSpell;

            StatsModifier.AttackSpeed.PercentBonus = .2f;//CalculateBonusAttackSpeed(_owner.GetSpell(0).CastInfo.SpellLevel);
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
                    bAS = .5f;
                    break;
                case 2:
                    bAS = .7f;
                    break;
                case 3:
                    bAS = .9f;
                    break;
                case 4:
                    bAS = 1.1f;
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
