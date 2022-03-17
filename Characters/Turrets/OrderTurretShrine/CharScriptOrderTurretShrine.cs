using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using System;
using System.Collections.Generic;
using System.Text;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;


namespace CharScripts
{
    internal class CharScriptOrderTurretShrine : ICharScript
    {
        IObjAiBase _owner;
        public void OnActivate(IObjAiBase owner, ISpell spell = null)
        {
            _owner = owner;
        }
        public void OnDeactivate(IObjAiBase owner, ISpell spell = null)
        {
        }
        public void OnUpdate(float diff)
        {
            var target = GetClosestUnitInRange(_owner, 1250f, true);
            if(target.NetId != _owner.NetId)
                target.TakeDamage(_owner, 1f,DamageType.DAMAGE_TYPE_TRUE,DamageSource.DAMAGE_SOURCE_SPELL, true);
        }
    }
}