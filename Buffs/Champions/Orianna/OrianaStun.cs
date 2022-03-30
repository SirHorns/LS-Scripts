using System.Numerics;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects.Stats;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System;
using GameServerCore.Domain;

//*=========================================
/*
 * ValkyrieHorns
 * Lastupdated: 3/30/2022
 * 
 * TODOS:
 * 
 * Known Issues:
 * I have no idea how to get force moment to work properly.
*/
//*=========================================

namespace Buffs
{
    class OrianaStun : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_DEHANCER,
            BuffAddType = BuffAddType.REPLACE_EXISTING,
            MaxStacks = 1,
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier ()
        {
        };

        IObjAiBase _orianna;
        IMinion _oriannaBall;
        OriannaBallHandler _ballHandler;
        IParticle stun;
        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            _orianna = ownerSpell.CastInfo.Owner;
            _ballHandler = (_orianna.GetBuffWithName("OriannaBallHandler").BuffScript as Buffs.OriannaBallHandler);
            _oriannaBall = _ballHandler.GetBall();

            var tossPos = GetPointFromUnit(unit, 500f);
            ForceMovement(unit, "STUNNED", _ballHandler.GetBall().Position, 1000f, 500f, .3f, 100f, movementOrdersFacing: ForceMovementOrdersFacing.FACE_MOVEMENT_DIRECTION);

            SetStatusFlags(buff, true);
            stun = AddParticleTarget(ownerSpell.CastInfo.Owner, unit, "LOC_Stun", unit, buff.Duration, bone: "head");
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            SetStatusFlags(buff, false);
            RemoveParticle(stun);
        }

        private void SetStatusFlags(IBuff buff, bool toggle)
        {
            buff.SetStatusEffect(StatusFlags.Stunned, toggle);
        }

        public void OnPreAttack(ISpell spell)
        {
        }

        public void OnUpdate(float diff)
        {
        }
    }
}
