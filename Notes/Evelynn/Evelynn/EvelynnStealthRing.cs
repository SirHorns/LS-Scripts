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
    internal class EvelynnStealthRing : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.INTERNAL
        };

        public IStatsModifier StatsModifier { get; private set; }

        IGameObject _owner;
        IParticle p0;
        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            _owner = unit;
            p0 = AddParticlePos(_owner, "evelynn_ring_green", _owner.Position, _owner.Position);
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
