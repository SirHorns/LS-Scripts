using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Collections.Generic;


//*=========================================
/*
 * More than likely this buff only handles logic relating to Fishbones AA
*/
//*========================================

namespace Buffs
{
    class JinxQ : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER,
            BuffAddType = BuffAddType.REPLACE_EXISTING,
            MaxStacks = 1,
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        IObjAiBase _owner;
        
        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            _owner = unit as IObjAiBase;

            //Require method for swaping spell icons: _owner.SetSpellIcon("JinxQ", 0, 0, false);
            ownerSpell.SetSpellToggle(true);
            //_owner.SetAutoAttackSpell("JinxQAttack", false);
            StatsModifier.Range.FlatBonus = CalculateRangeBonus(ownerSpell.CastInfo.SpellLevel);
            StatsModifier.AttackSpeed.PercentBonus = -.1f;

            unit.AddStatModifier(StatsModifier);
        }

        private float CalculateRangeBonus(int spellLevel)
        {
            float bRange;

            switch (spellLevel)
            {
                case 1:
                    bRange = 75f;
                    break;
                case 2:
                    bRange = 100f;
                    break;
                case 3:
                    bRange = 125;
                    break;
                case 4:
                    bRange = 150f;
                    break;
                case 5:
                    bRange = 175f;
                    break;
                default:
                    bRange = 0.0f;
                    break;
            }
            return bRange;
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            ownerSpell.SetSpellToggle(false);
        }

        public void OnUpdate(float diff)
        {
        }
    }
}
