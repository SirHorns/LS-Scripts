using System;
using System.Numerics;
using System.Collections.Generic;

using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Sector;

using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Domain.GameObjects.Spell.Missile;

//*=========================================
/*
 * ValkyrieHorns
 * Lastupdated: 3/25/2022
 * 
 * TODOS:
 * 
 * Known Issues:
 * 
*/
//*========================================

namespace Spells
{
    public class OrianaDetonateCommand : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true,
            NotSingleTargetSpell = true,
            IsDamagingSpell = true,
            ChannelDuration = 0.5f,
            AutoFaceDirection = true,
        };

        private IObjAiBase _orianna;
        private ISpell _spell;

        private Buffs.OriannaBallHandler _ballHandler;
        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
            _orianna = owner;
            _spell = spell;
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnSpellPreCast(IObjAiBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
            _ballHandler = (_orianna.GetBuffWithName("OriannaBallHandler").BuffScript as Buffs.OriannaBallHandler);
        }

        public void OnSpellCast(ISpell spell)
        {
        }

        public void OnSpellPostCast(ISpell spell)
        {
        }

        public void OnSpellChannel(ISpell spell)
        {
            Console.WriteLine("OnChannl");

            foreach (var unit in GetUnitsInRange(_ballHandler.GetBall().Position, 1000, true))
            {
                if (unit.Team != _orianna.Team)
                {
                    AddBuff("OrianaStun", .75f, 1, spell, unit, _orianna);
                }
            }

        }

        public void OnSpellChannelCancel(ISpell spell, ChannelingStopSource reason)
        {
        }

        public void OnSpellPostChannel(ISpell spell)
        {
            Console.WriteLine("OnPostChannl");
        }

        public void OnUpdate(float diff)
        {
        }
    }
}
