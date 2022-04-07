using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.API;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    internal class EvelynnStealthMana : IBuffGameScript
    {
        //TODO: Add mana regen
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER
        };

        public IStatsModifier StatsModifier { get; private set; }

        IParticle p0;
        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            LogDebug("StealthManaActivated");

            StatsModifier.ManaRegeneration.PercentBonus += unit.Stats.ManaPoints.Total * 0.01f;
            unit.AddStatModifier(StatsModifier);
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {            
            RemoveParticle(p0);
        }

        public void OnDeath(IDeathData deathData)
        {

        }
        public void OnUpdate(float diff)
        {

        }
    }
}
