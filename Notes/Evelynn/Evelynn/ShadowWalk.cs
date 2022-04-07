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
            BuffType =  BuffType.AURA,
            BuffAddType = BuffAddType.STACKS_AND_CONTINUE
        };

        public IStatsModifier StatsModifier { get; private set; }

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            LogDebug("EvelynnShadoWalkBUFF: Activated");
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            LogDebug("EvelynnShadoWalkBUFF: Deactivated");
        }

        public void OnDeath(IDeathData deathData)
        {

        }
        public void OnUpdate(float diff)
        {

        }
    }
}
