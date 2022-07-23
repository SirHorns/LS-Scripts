using System.Numerics;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using System;
using System.Collections.Generic;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.Domain.GameObjects.Spell.Sector;

//*=========================================
/*
 * Spell has an issue with not truly being global,i.e Launching from fountain will reach somewhere around the enemy nexus before the missile poofs
 * Either there is a packet usage or something else that fixes this as finding a match where a Jinx tries a fountain to fountain kill would be difficult.
*/
//*========================================

namespace Spells
{
    public class JinxR : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true,
            MissileParameters = new MissileParameters()
            {
                Type = MissileType.Circle,
            },
        };

        IObjAiBase _owner;
        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
            _owner = owner;
            ApiEventManager.OnSpellHit.AddListener(this, spell, TargetExecute, false);
        }

        private void TargetExecute(ISpell spell, IAttackableUnit target, ISpellMissile missile, ISpellSector sector)
        {
            if ((target is IChampion))
            {
                AddBuff("JinxWSight", 2f, 1, spell, target, _owner);
                var owner = spell.CastInfo.Owner;
                var ad = owner.Stats.AttackDamage.Total * spell.SpellData.AttackDamageCoefficient;
                var damage = spell.CastInfo.SpellLevel * 10 + ad;

                target.TakeDamage(owner, 100f, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_ATTACK, false);
                
                //She has several confusing partical names so was using this as a tmp stand in
                ///I think she actually applies several particals to a target but will need too check.
                AddParticleTarget(owner, target, "Ezreal_mysticshot_tar", target);

                missile.SetToRemove();
            }

            
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnSpellPreCast(IObjAiBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
        }

        public void OnSpellCast(ISpell spell)
        {
        }

        public void OnSpellPostCast(ISpell spell)
        {
        }

        public void OnSpellChannel(ISpell spell)
        {
        }

        public void OnSpellChannelCancel(ISpell spell, ChannelingStopSource reason)
        {
        }

        public void OnSpellPostChannel(ISpell spell)
        {
        }

        public void OnUpdate(float diff)
        {
            SetSpellToolTipVar(_owner, 0, _owner.Stats.AttackDamage.Total - _owner.Stats.AttackDamage.BaseValue, SpellbookType.SPELLBOOK_CHAMPION, 3, SpellSlotType.SpellSlots);
        }
    }
}
