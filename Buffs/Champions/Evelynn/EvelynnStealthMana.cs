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
            BuffType = BuffType.INTERNAL
        };

        public IStatsModifier StatsModifier { get; private set; }

        IParticle pbuff;
        IParticle p0;
        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            pbuff = AddParticleTarget(ownerSpell.CastInfo.Owner, unit, "evelynnmana", unit, buff.Duration, bone: "BUFFBONE_CSTM_SHIELD_TOP");
            //Mana is busted for some reason?
            //StatsModifier.ManaRegeneration.PercentBonus += unit.Stats.ManaPoints.Total * 0.01f;
            //unit.AddStatModifier(StatsModifier);
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {            
            RemoveParticle(p0);
            //unit.RemoveStatModifier(StatsModifier);
        }

        public void OnDeath(IDeathData deathData)
        {

        }
        public void OnUpdate(float diff)
        {

        }
    }
}
