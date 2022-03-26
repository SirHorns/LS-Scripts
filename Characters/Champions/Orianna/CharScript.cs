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
 * 
 * TODOS:
 * Figure out how to package her passive(s)
 * 
 * Known Issues:
 * 
*/
//*=========================================

namespace CharScripts
{
    public class CharScriptOrianna : ICharScript
    {
        IObjAiBase _orianna;
        Buffs.OriannaBallHandler _ballHandler;

        bool IsBallMoving = false;
        public void OnActivate(IObjAiBase owner, ISpell spell = null)
        {
            _orianna = owner;

            _ballHandler = AddBuff("OriannaBallHandler", 1.0f, 1, spell, owner, owner, true).BuffScript as Buffs.OriannaBallHandler;
            _ballHandler.SetAttachedChampion((IChampion)owner);

            AddBuff("ClockworkWinding", 1f, 1, spell, owner, owner, true);
            ApiEventManager.OnDeath.AddListener(owner, owner, OnDeath, false);
        }

        private void OnDeath(IDeathData death)
        {
            _ballHandler.DisableBall();
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell = null)
        {
        }

        public void OnUpdate(float diff)
        {
            if (_ballHandler.GetBall() == null)
            {
                _ballHandler.SpawnBall(_orianna.Position);
            }
        }
    }

    public class CharScriptOriannaNoBall : ICharScript
    {
        public void OnActivate(IObjAiBase owner, ISpell spell = null)
        {
        }

        private void OnDeath(IDeathData death)
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

