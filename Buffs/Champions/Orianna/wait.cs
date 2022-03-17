﻿using System.Numerics;
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
    class Wait : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER,
            BuffAddType = BuffAddType.RENEW_EXISTING,
            MaxStacks = 1
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier ();

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            var spellLevel = ownerSpell.CastInfo.SpellLevel;
            var bonusResistance = new[] { 6f, 12f, 19f, 24f, 30f }[spellLevel];
            StatsModifier.Armor.FlatBonus = bonusResistance;
            StatsModifier.MagicResist.FlatBonus = bonusResistance;
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