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
 * TODOS:
 * See if I can find definitive numbers on the leash ranges used by Orianna
 * 
 * Known Issues:
 * Leash Range becomes way to large if Ball is passed from Orianna to a Allied Champion.
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
        private ISpellSector _pickupSector;

        Buffs.OriannaBallHandler _ballHandler;
        
        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier ();

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            _orianna = ownerSpell.CastInfo.Owner;
            _spell = ownerSpell;
            _buff = buff;
            _ballHandler = _orianna.GetBuffWithName("OriannaBallHandler").BuffScript as Buffs.OriannaBallHandler;
            _oriannaBall = unit as IMinion;

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

        public int CurrentGroundedLeashRange()
        {
            var dist = Vector2.Distance(_oriannaBall.Owner.Position, _oriannaBall.Position);
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

            return state;
        }

        public int CurrentAttachedLeashRange()
        {
            var dist = Vector2.Distance(_orianna.Position, _oriannaBall.Position);
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

            return state;
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

        private void CheckIndicator()
        {
            if (_currentIndicator != null)
            {
                _currentIndicator.SetToRemove();
            }
        }

        private void LeashRangeCheck()
        {
            if (_ballHandler.GetAttachedChampion() != _orianna as IChampion)
            {
                if (!_ballHandler.GetStateFlying())
                {
                    CheckIndicator();

                    if (_ballHandler.GetStateAttached() && _ballHandler.GetAttachedChampion() != _orianna as IChampion)
                    {
                        if (CurrentAttachedLeashRange() == 0)
                        {
                            SpellCast(_orianna, 4, SpellSlotType.ExtraSlots, true, _orianna, Vector2.Zero);
                        }
                        else
                        {
                            _currentIndicator = AddParticleTarget(_orianna, _orianna, GetIndicatorName(CurrentAttachedLeashRange()), _ballHandler.GetAttachedChampion(), _buff.Duration - _buff.TimeElapsed, flags: FXFlags.TargetDirection);
                        }
                    }
                    else
                    {
                        if (CurrentGroundedLeashRange() == 0)
                        {
                            SpellCast(_orianna, 4, SpellSlotType.ExtraSlots, false, _orianna, Vector2.Zero);
                        }
                        else
                        {
                            _currentIndicator = AddParticleTarget(_orianna, _orianna, GetIndicatorName(CurrentGroundedLeashRange()), _oriannaBall, _buff.Duration - _buff.TimeElapsed, flags: FXFlags.TargetDirection);
                        }
                    }
                }
                else
                {
                    CheckIndicator();
                }
            }
        }

        private void PickupBallCheck()
        {
            if(!_ballHandler.GetStateAttached() && GetUnitsInRange(_oriannaBall.Position, 135f, true).Contains(_orianna))
            {
                _currentIndicator.SetToRemove();
                SpellCast(_orianna, 4, SpellSlotType.ExtraSlots, true, _orianna, Vector2.Zero);
            }
        }

        public void OnUpdate(float diff)
        {
            PickupBallCheck();
            LeashRangeCheck();
        }
    }
}
