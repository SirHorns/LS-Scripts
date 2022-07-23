using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using System.Numerics;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using GameServerCore.Domain;
using GameServerLib.GameObjects.AttackableUnits;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Enums;

namespace Spells
{
    public class EvelynnPassive : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata => new SpellScriptMetadata()
        {
           
        };

        IBuff _revealedDebuff;
        IBuff _shadowWalk;
        IBuff _hateSpikeMarker;
        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
            //_shadowWalk = AddBuff("ShadowWalk", 23f, 1, spell, owner, owner, false);
            //_hateSpikeMarker = AddBuff("EvelynnHateSpikeMarker", 0f, 1, spell, owner, owner, true);
        }
        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
            //_revealedDebuff = AddBuff("ShadowWalkRevealedDebuff", 5f, 1, spell, owner, owner, false);
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
        }
    }
}

