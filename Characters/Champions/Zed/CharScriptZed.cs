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
    public class CharScriptZed : ICharScript
    {

        IObjAiBase _owner;

        public void OnActivate(IObjAiBase owner, ISpell spell = null)
        {
            _owner = owner;
        }

        private void OnSpellCast(ISpell obj)
        {

        }

        private void OnTakeDamge(IDamageData damage)
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

