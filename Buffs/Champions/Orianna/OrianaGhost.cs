using System.Numerics;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects.Stats;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System;
namespace Buffs
{
    class OrianaGhost : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            MaxStacks = 1,
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier ()
        {
        };
        IParticle _bind;
        IParticle _ring;
        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            var spellLevel = ownerSpell.CastInfo.SpellLevel - 1;
            var bonusResistances = new[] { 6, 12, 18, 24, 30 }[spellLevel];
            StatsModifier.Armor.FlatBonus = bonusResistances;
            StatsModifier.MagicResist.FlatBonus = bonusResistances;
            unit.AddStatModifier(StatsModifier);

            _bind = AddParticleTarget(ownerSpell.CastInfo.Owner, unit, "Oriana_Ghost_bind", unit, 2300f, flags: FXFlags.TargetDirection);
            _ring = AddParticleTarget(ownerSpell.CastInfo.Owner, unit, "OriannaEAllyRangeRing", unit, 2300f, flags: FXFlags.TargetDirection,teamOnly: ownerSpell.CastInfo.Owner.Team);
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            _bind.SetToRemove();
            _ring.SetToRemove();
        }

        public void OnPreAttack(ISpell spell)
        {
        }

        public void OnUpdate(float diff)
        {
        }
    }
}
