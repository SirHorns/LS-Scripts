using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Scripting.CSharp;

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
            if (_owner.GetBuffWithName("Slow") != null) 
            { 
                RemoveBuff(_owner.GetBuffWithName("Slow"));
            }
            //Apply Ghosted
            AddBuff("EvelynnDarkFrenzy", 3f, 1, spell, spell.CastInfo.Targets[0].Unit, spell.CastInfo.Owner);
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
