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
    class EvelynnWPassive : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.HASTE,
            BuffAddType = BuffAddType.STACKS_AND_RENEWS,
            MaxStacks = 4
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        IParticle p0;
        IParticle p1;

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            p0 = AddParticleTarget(ownerSpell.CastInfo.Owner, unit, "Evelynn_W_cas", unit, buff.Duration, bone: "BUFFBONE_CSTM_SHIELD_TOP");

            var percentMS = 0f;
            var particleStack = "";
            switch (buff.StackCount) {
                case 1:
                    particleStack = "Evelynn_W_passive_01";
                    percentMS = .3f;
                    break;
                case 2:
                    particleStack = "Evelynn_W_passive_02";
                    percentMS = .4f;
                    break;
                case 3:
                    particleStack = "Evelynn_W_passive_03";
                    percentMS = .5f;
                    break;
                case 4:
                    particleStack = "Evelynn_W_passive_04";
                    percentMS = .6f;
                    break;
            }

            p1 = AddParticleTarget(ownerSpell.CastInfo.Owner, unit, particleStack, unit, buff.Duration, bone: "root");
            StatsModifier.MoveSpeed.PercentBonus += percentMS;
            unit.AddStatModifier(StatsModifier);
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            RemoveParticle(p0);
            RemoveParticle(p1);
        }

        public void OnPreAttack(ISpell spell)
        {

        }

        public void OnUpdate(float diff)
        {
        }
    }
}
