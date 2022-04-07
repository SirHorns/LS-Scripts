using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Domain.GameObjects.Spell.Sector;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using System;

namespace Spells
{
    public class EvelynnE : ISpellScript
    {

        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {

            TriggersSpellCasts = true,
            IsDamagingSpell = true,
            NotSingleTargetSpell = false,
            AutoFaceDirection = true,
            CastingBreaksStealth = true,

            // TODO
        };

        IAttackableUnit _target;
        ISpell _spell;
        IObjAiBase _owner;

        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
            _owner = owner;
            _spell = spell;
            //ApiEventManager.OnSpellHit.AddListener(this, spell, TargetExecute, false);
        }

        public void TargetExecute(ISpell spell, IAttackableUnit target, ISpellMissile missile, ISpellSector sector)
        {
            
        }


        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {

        }
        

        public void OnSpellPreCast(IObjAiBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
            var flat = new[] { 35, 55, 75, 95, 112 }[spell.CastInfo.SpellLevel - 1];
            var ad = owner.Stats.AttackDamage.FlatBonus * .5f;
            var ap = owner.Stats.AbilityPower.Total * .5f;
            var damage = flat + ad + ap;

            target.TakeDamage(_owner, damage, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);

            target.TakeDamage(_owner, damage, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
        }

        public void OnSpellCast(ISpell spell)
        {
            
        }

        public void OnSpellPostCast(ISpell spell)
        {
            AddBuff("EvelynnEActive", 3f, 1, spell, spell.CastInfo.Targets[0].Unit, spell.CastInfo.Owner);
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, ISpellMissile missile)
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
        }
    }
}
