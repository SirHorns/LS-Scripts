using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.API;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System;
using System.Numerics;
using System.Collections.Generic;

namespace Buffs
{
    internal class ShadowWalk  : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.INTERNAL
            //BuffType =  BuffType.COMBAT_ENCHANCER,
            //BuffAddType = BuffAddType.STACKS_AND_CONTINUE
        };

        public IStatsModifier StatsModifier { get; private set; }

        IParticle p0;
        IBuff _stealth;
        IBuff _stealthRing;

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            _stealth = AddBuff("EvelynnStealth", 0f, 1, ownerSpell, unit, ownerSpell.CastInfo.Owner, true);
            _stealthRing = AddBuff("EvelynnStealthRing", 25000f, 1, ownerSpell, unit, ownerSpell.CastInfo.Owner, true);
            unit.SetAnimStates(new Dictionary<string, string> { { "evelynn_run", "evelynn_run_sneak" } });
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            _stealthRing.DeactivateBuff();
            unit.SetAnimStates(new Dictionary<string, string> {});
        }

        public void OnDeath(IDeathData deathData)
        {

        }
        public void OnUpdate(float diff)
        {

        }
    }
}
