using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects.Stats;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Domain;

//*=========================================
/*
 * ValkyrieHorns
 * Lastupdated: 3/25/2022
 * 
 * TODOS:
 * 
 * Known Issues:
*/
//*========================================

namespace Buffs
{
    class ClockworkWinding : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER,
            BuffAddType = BuffAddType.STACKS_AND_RENEWS,
            MaxStacks = 1
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        private IObjAiBase _orianna;
        private ISpell _spell;
        private IAttackableUnit _previousTarger = null;
        private IAttackableUnit _currentTarget = null;
        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            _orianna = ownerSpell.CastInfo.Owner;
            _spell = ownerSpell;
            ApiEventManager.OnHitUnit.AddListener(this, _orianna, TargetExecute, false);

            ApiEventManager.OnLaunchAttack.AddListener(this, _orianna, OnLaunch, false);
        }

        private void OnLaunch(ISpell spell)
        {
            _currentTarget = spell.CastInfo.Targets[0].Unit;

            if (_previousTarger != null && _previousTarger == _currentTarget)
            {
                AddBuff("OrianaPowerDagger", 4f, 1, _spell, _orianna, _orianna);
            }
            else
            {
                _orianna.RemoveBuffsWithName("OrianaPowerDagger");
                _previousTarger = _currentTarget;
            }
        }

        private void TargetExecute(IDamageData data)
        {
            data.Target.TakeDamage(_orianna, CalculateDamage(), DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_ATTACK, false); 
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            ApiEventManager.OnHitUnit.RemoveListener(this, _orianna);
        }

        private float CalculateDamage()
        {
            var ownerLevel = _orianna.Stats.Level;
            var baseDamage = 0;

            if (ownerLevel >= 16)
            {
                baseDamage = 50;
            }
            else if (ownerLevel >= 13)
            {
                baseDamage = 42;
            }
            else if (ownerLevel >= 10)
            {
                baseDamage = 34;
            }
            else if (ownerLevel >= 7)
            {
                baseDamage = 26;
            }
            else if (ownerLevel >= 4)
            {
                baseDamage = 18;
            }
            else if (ownerLevel >= 1)
            {
                baseDamage = 10;
            }

            return baseDamage + (_orianna.Stats.AbilityPower.Total * .15f);
        }

        public void OnUpdate(float diff)
        {
        }
    }
}
