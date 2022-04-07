using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.Domain.GameObjects.Spell.Sector;
using System;

namespace Spells
{
    //Check for damage enemy reduce cd by 1 for a hit
    //Check for champion takedown and refresh cd
    public class EvelynnW : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true
        };

        private IObjAiBase _owner;

        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
            _owner = owner;
            ApiEventManager.OnSpellHit.AddListener(owner,spell,OnSpellHit,false);

        }

        private void OnSpellHit(ISpell spell, IAttackableUnit unit, ISpellMissile missle, ISpellSector sector)
        {
            AddBuff("EvelynnWPassive", 3f, 1, spell, spell.CastInfo.Targets[0].Unit, spell.CastInfo.Owner);
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnSpellPreCast(IObjAiBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
        }

        public void OnSpellCast(ISpell spell)
        {
            if (_owner.GetBuffWithName("Slow") != null) 
            {
                _owner.PlayAnimation("Spell2", .4f);
                _owner.RemoveBuffsWithName("Slow");
            }
        }

        public void OnSpellPostCast(ISpell spell)
        {
            //Apply Ghosted
            AddBuff("EvelynnWActive", 3f, 1, spell, spell.CastInfo.Targets[0].Unit, spell.CastInfo.Owner);

            var manaCost = new[] { 0, 0, 0, 0, 0 }[spell.CastInfo.SpellLevel - 1];
            var coolDown = new[] { 15f, 15f, 15f, 15f, 15f }[spell.CastInfo.SpellLevel - 1];
            spell.SetCooldown(coolDown);
            spell.CastInfo.Owner.Stats.CurrentMana -= manaCost;
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
