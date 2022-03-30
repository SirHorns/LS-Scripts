using LeagueSandbox.GameServer.API;
using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Scripting.CSharp;
using GameServerCore.Domain;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using System;
using GameServerCore.Enums;

//*=========================================
/*
 * ValkyrieHorns
 * Lastupdated: 3/25/2022
 * Notes:
 * Nested Two parts of Orianna AA passive within eachother. Need to work on Internal Buff to handle ball leash ranges and related particles
 * 
 * TODOS:
 * 
 * Known Issues:
 * Hotreload/reload scripts will crash league due to BallHandlerScript trying to creat things in update before the script is added.
*/
//*=========================================

namespace CharScripts
{
    public class CharScriptOrianna : ICharScript
    {
        IObjAiBase _orianna;
        ISpell _spell;
        Buffs.OriannaBallHandler _ballHandler;
        public void OnActivate(IObjAiBase owner, ISpell spell = null)
        {
            _orianna = owner;
            _spell = spell;

            _ballHandler = AddBuff("OriannaBallHandler", 1.0f, 1, spell, owner, owner, true).BuffScript as Buffs.OriannaBallHandler;

            AddBuff("ClockworkWinding", 1f, 1, spell, owner, owner, true);
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell = null)
        {
        }

        public void OnUpdate(float diff)
        {
        }
    }

    public class CharScriptOriannaNoBall : ICharScript
    {
        public void OnActivate(IObjAiBase owner, ISpell spell = null)
        {
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell = null)
        {
        }

        public void OnUpdate(float diff)
        {
        }
    }
}

