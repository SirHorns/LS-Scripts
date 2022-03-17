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
    internal class EvelynnPassive : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER
        };

        IGameObject _owner;
        IBuff _revealedDebuff;
        IBuff _shadowWalk;
        IBuff _hateSpikeMarker;

        public IStatsModifier StatsModifier { get; private set; }

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            _owner = unit;
            _shadowWalk = AddBuff("ShadowWalk", 0f, 1, ownerSpell, unit, ownerSpell.CastInfo.Owner, true);
            _hateSpikeMarker = AddBuff("EvelynnHateSpikeMarker", 0f, 1, ownerSpell, unit, ownerSpell.CastInfo.Owner, true);
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {

        }

        public void OnDeath(IDeathData deathData)
        {

        }
        public void OnUpdate(float diff)
        {

        }
    }
}
