using System.Numerics;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects.Stats;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    class EvelynnWActive : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.HASTE,
            BuffAddType = BuffAddType.RENEW_EXISTING,
            MaxStacks = 1
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            AddParticleTarget(ownerSpell.CastInfo.Owner, unit, "Evelynn_W_cas", unit, buff.Duration, bone: "BUFFBONE_CSTM_SHIELD_TOP");
            AddParticleTarget(ownerSpell.CastInfo.Owner, unit, "Evelynn_W_active_buf", unit, buff.Duration, bone: "BUFFBONE_GLB_GROUND_LOC");

            var percentMS = new[] { .3f, .4f, .5f, .6f, .7f}[ownerSpell.CastInfo.SpellLevel - 1];
            StatsModifier.MoveSpeed.PercentBonus += percentMS;
            unit.AddStatModifier(StatsModifier);
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
        }

        public void OnPreAttack(ISpell spell)
        {

        }

        public void OnUpdate(float diff)
        {
        }
    }
}
