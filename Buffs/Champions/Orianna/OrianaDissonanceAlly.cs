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

//*=========================================
/*
 * ValkyrieHorns
 * Lastupdated: 3/26/2022
 * 
 * TODOS:
 * Add particles for orianna  W ms boost
 * Add in Decaying buff component
 * 
 * Known Issues:
*/
//*========================================

namespace Buffs
{
    class OrianaHaste : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.HASTE,
            BuffAddType = BuffAddType.RENEW_EXISTING,
            MaxStacks = 1
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier ()
        {
        };

        float _currentPercentBonus;
        float _totalBonus;
        bool _decay = false;
        IAttackableUnit _buffHolder;
        float _r;
        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            

            _buffHolder = unit;

            _decay = false;
            _currentPercentBonus = new[] { .2f, .25f, .3f, .35f, .4f }[ownerSpell.CastInfo.SpellLevel - 1];
            _totalBonus = _buffHolder.Stats.MoveSpeed.Total * _currentPercentBonus;
            _r = _totalBonus / 2;

            StatsModifier.MoveSpeed.PercentBonus = _currentPercentBonus;

            unit.AddStatModifier(StatsModifier);

            _decay = true;
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
        }

        //forumla f(t) = C - r*t
        //[Remaining Bonus MS] - r * 2
        // r = CBMS / 2 = 
        float _decayTime = 0.0f; 
        public void OnUpdate(float diff)
        {
        }
    }
}
