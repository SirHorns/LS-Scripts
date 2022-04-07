using LeagueSandbox.GameServer.API;
using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Scripting.CSharp;
using GameServerCore.Domain;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using System;

namespace CharScripts
{
    public class CharScriptEvelynn : ICharScript
    {
        IObjAiBase _owner;
        IBuff _evelynnPassive;
        ISpell _spell = null;
        public void OnActivate(IObjAiBase owner, ISpell spell = null)
        {
            _spell = spell;
            ApiEventManager.OnTakeDamage.AddListener(this, owner, OnTakeDamge, false);
            ApiEventManager.OnResurrect.AddListener(this, owner, OnResurrect, false);
            //_evelynnPassive = AddBuff("EvelynnPassive", 0f, 1, _spell, owner, owner, true);
        }

        private void OnTakeDamge(IDamageData damage)
        {
            //AddBuff("ShadowWalkRevealedDebuff", 6f, 1, _spell, _owner, _owner, false);
        }

        private void OnResurrect(IObjAiBase owner)
        {
            LogDebug("GAME STARTED=================");
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell = null)
        {
        }
        public void OnUpdate(float diff)
        {
        }
    }
}

