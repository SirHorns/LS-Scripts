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
using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.Domain.GameObjects.Spell.Sector;

//*=========================================
/*
 * ValkyrieHorns
 * Lastupdated: 3/28/2022
 * 
 * Notes:
 * Might rename this script or possibly nest this functionality with BallHandler or package in with Orianna's Clockwork Winding buff file.
 * 
 * TODOS:
 * Find definitive numbers on the leash and pickup ranges used by Orianna, for now they are just wiki and aproximation.
 * 
 * Known Issues:
 * 
*/
//*========================================

namespace Buffs
{
    class OriannaBall : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.INTERNAL,
            BuffAddType = BuffAddType.REPLACE_EXISTING,
            MaxStacks = 1,
        };

        private IObjAiBase _orianna;
        private IMinion _oriannaBall;
        private ISpell _spell;
        private IBuff _buff;
        private IParticle _currentIndicator;

        Buffs.OriannaBallHandler _ballHandler;
        
        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier ();

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            _orianna = ownerSpell.CastInfo.Owner;
            _spell = ownerSpell;
            _buff = buff;
            _ballHandler = _orianna.GetBuffWithName("OriannaBallHandler").BuffScript as Buffs.OriannaBallHandler;
            _oriannaBall = _ballHandler.GetBall();

            buff.SetStatusEffect(StatusFlags.Targetable, false);
            buff.SetStatusEffect(StatusFlags.Ghosted, true);
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            if (_currentIndicator != null)
            {
                _currentIndicator.SetToRemove();
            }
        }


        public string GetIndicatorName(int state)
        {
            switch (state)
            {
                case 1:
                    {
                        return "OrianaBallIndicatorFar";
                    }
                case 2:
                    {
                        return "OrianaBallIndicatorMedium";
                    }
                case 3:
                    {
                        return "OrianaBallIndicatorNear";
                    }
                default:
                    {
                        return "OrianaBallIndicatorFar";
                    }
            }
        }

        private void RemoveIndicator()
        {
            if (_currentIndicator != null)
            {
                _currentIndicator.SetToRemove();
            }
        }

        private void PickupBallCheck()
        {
            if(!_ballHandler.GetStateAttached() && GetUnitsInRange(_oriannaBall.Position, 135f, true).Contains(_orianna))
            {
                RemoveIndicator();
                _ballHandler.ReturnBall(true);
                //SpellCast(_orianna, 4, SpellSlotType.ExtraSlots, true, _orianna, Vector2.Zero);
            }
        }

        private void LeashAttached()
        {
            if (_ballHandler.GetAttachedChampion() == _orianna as IChampion)
            {
                return;
            }

            if (_ballHandler.GetStateFlying())
            {
                RemoveIndicator();
                return;
            }

            var dist = Vector2.Distance(_orianna.Position, _ballHandler.GetAttachedChampion().Position);
            var state = 0;

            if (dist >= 1300.0f)
            {
                state = 0;
            }
            else if (dist >= 1200.0f)
            {
                state = 1;
            }
            else if (dist >= 1000.0f)
            {
                state = 2;
            }
            else if (dist >= 0f)
            {
                state = 3;
            }

            var indicator = GetIndicatorName(state);

            RemoveIndicator();

            if (state == 0)
            {
                _ballHandler.ReturnBall(true);
                return;
            }
            
            _currentIndicator = AddParticleTarget(_orianna, _orianna, indicator, _ballHandler.GetAttachedChampion(), _buff.Duration - _buff.TimeElapsed, flags: FXFlags.TargetDirection); 
        }

        private void LeashGrounded()
        {
            if (_ballHandler.GetStateFlying())
            {
                RemoveIndicator();
                return;
            }

            var dist = Vector2.Distance(_orianna.Position, _oriannaBall.Position);
            var state = 0;

            if (dist >= 1290.0f)
            {
                state = 0;
            }
            else if (dist >= 1190.0f)
            {
                state = 1;
            }
            else if (dist >= 1000.0f)
            {
                state = 2;
            }
            else if (dist >= 0f)
            {
                state = 3;
            }

            var indicator = GetIndicatorName(state);

            RemoveIndicator();

            if (state == 0)
            {
                _ballHandler.ReturnBall(false);
                return;
            }
            
            _currentIndicator = AddParticleTarget(_orianna, _orianna, indicator, _oriannaBall, _buff.Duration - _buff.TimeElapsed, flags: FXFlags.TargetDirection);
        }

        public void OnUpdate(float diff)
        {
            PickupBallCheck();

            if (_ballHandler.GetStateAttached())
            {
                LeashAttached();
            }
            else
            {
                LeashGrounded();
            }
        }
    }
}
