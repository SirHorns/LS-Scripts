using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects.Stats;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Domain;


using System;

//*=========================================
/*
 * Unknown what this Buff is used for, I suspect it partially handles Jinx's Q logic.
 * Specifically the icon swapping and possibly other unseen buffs
 *
 * Not sure if it is used to handle both icon swaps or just when she switches back to PowPow
 *
 * In testing I created a function in ObjAIBase that changed Spell Icons since there didn't seem to be anything made that handles that
 * Commented out the IconSwap Line
*/
//*========================================

namespace Buffs
{
    class JinxQIcon : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER,
            BuffAddType = BuffAddType.REPLACE_EXISTING,
            MaxStacks = 1,
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        private IObjAiBase _owner;
        
        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            _owner = ownerSpell.CastInfo.Owner;

            //Requires method for swapping spell icons:
            /*
             * Spell: JinxQ
             * byte Slot: 0
             * byte IconIndex: 1
             */

            //_owner.SetAutoAttackSpell("JinxBasicAttack", false);
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
        }

        public void OnUpdate(float diff)
        {
        }
    }
}
