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
 * Lastupdated: 3/30/2022
 * 
 * TODOS:
 * Wait for LeagueSandbox GamerServer to implement Stealth to hide E particle. Wil be implemented in OrianaGhost.
 * Implement Windwall Interactions. Might need to make DropBallOnBlock method in Ball handler.
 * 
 * ==OrianaRedactCommand==
 * 
 *= =OrianaRedact==
 * 
 * Known Issues:
 * Appears that trying to pull the current cooldown of a spell from within said spell is breaking. Or maybe there is a better way to do it. 
 * Disabling CD check on Q & E for now and allowing the normal CD to keep code logic in check
 * Spell cooldown needs to be definitaly stated to not ignore CDR or it just locks to 9s cd period
 * 
*/
//*========================================

namespace Spells
{
    public class OrianaRedactCommand : ISpellScript
    {
        //USED FOR DEBUGING
        bool _disableSpellCosts = false;

        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true,
        };

        IObjAiBase _orianna;
        Buffs.OriannaBallHandler _ballHandler;
        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
            _orianna = owner;
            ApiEventManager.OnLevelUpSpell.AddListener(owner, spell, SpellLevelUp, false);
        }

        private void SpellLevelUp(ISpell spell)
        {
            _ballHandler = (_orianna.GetBuffWithName("OriannaBallHandler").BuffScript as Buffs.OriannaBallHandler);

            if(_ballHandler.GetStateAttached())
            {
                if (_ballHandler.GetAttachedChampion() == (_orianna as IChampion)) 
                {
                    AddBuff("OrianaGhostSelf", 1.0f, 1, spell, _ballHandler.GetAttachedChampion(), _orianna, true);
                }
                else 
                {
                    AddBuff("OrianaGhost", 1.0f, 1, spell, _ballHandler.GetAttachedChampion(), _orianna, true);
                }
            }
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        { 
        }

        public void OnSpellPreCast(IObjAiBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
            _ballHandler = (_orianna.GetBuffWithName("OriannaBallHandler").BuffScript as Buffs.OriannaBallHandler);

            if (_orianna.Model == "Orianna")
            {
                _orianna.ChangeModel("OriannaNoBall");
            }

            _orianna.PlayAnimation("Spell2", 1f, 0, 0);

            if (_ballHandler.GetStateAttached())
            {
                if (_ballHandler.GetAttachedChampion() == (target as IChampion))
                {
                    AddBuff("OrianaRedactShield", 4f, 1, spell, target, _orianna);
                }
                else
                {
                    SpellCast(owner, 2, SpellSlotType.ExtraSlots, target.Position, target.Position, false, _ballHandler.GetAttachedChampion().Position, spell.CastInfo.Targets, overrideForceLevel: spell.CastInfo.SpellLevel);
                }
            }
            else
            {
                SpellCast(owner, 2, SpellSlotType.ExtraSlots, target.Position, target.Position, false, _ballHandler.GetBall().Position, spell.CastInfo.Targets, overrideForceLevel: spell.CastInfo.SpellLevel);
            }
        }

        public void OnSpellCast(ISpell spell)
        {
        }

        public void OnSpellPostCast(ISpell spell)
        {
            if(!_disableSpellCosts)
            {
                spell.SetCooldown(9.0f, false);
                _orianna.Stats.CurrentMana -= 60;
            }
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

    public class OrianaRedact : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true,
            NotSingleTargetSpell = true,
            IsDamagingSpell = true,
        };

        IObjAiBase _orianna;
        IChampion _target;
        ISpell _spell;
        Buffs.OriannaBallHandler _ballHandler;
        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
            _orianna = owner;
            _spell = spell;
            ApiEventManager.OnSpellHit.AddListener(this, spell, TargetExecute, false);
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnSpellPreCast(IObjAiBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
            _ballHandler = (owner.GetBuffWithName("OriannaBallHandler").BuffScript as Buffs.OriannaBallHandler);

            DisableAbilityCheck();

            _target = (spell.CastInfo.Targets[0].Unit as IChampion);

            _ballHandler.RemoveEBuff();

            _ballHandler.ChangeState(Buffs.OriannaBallHandler.BallState.FLYING);

            var missile = spell.CreateSpellMissile(new MissileParameters
            {
                Type = MissileType.Circle,
                OverrideEndPosition = spell.CastInfo.Targets[0].Unit.Position,
            });

            ApiEventManager.OnSpellMissileEnd.AddListener(this, missile, OnMissileFinish, true);
        }

        public void TargetExecute(ISpell spell, IAttackableUnit target, ISpellMissile missile, ISpellSector sector)
        {
            if (target.Team != _orianna.Team)
            {
                //if (missile is ISpellCircleMissile skillshot)
                var owner = spell.CastInfo.Owner;
                var spellLevel = spell.CastInfo.SpellLevel - 1;
                var baseDamage = new[] { 60, 90, 120, 150, 180 }[spellLevel];
                var magicDamage = owner.Stats.AbilityPower.Total * .3f;
                var damage = baseDamage + magicDamage;
                //Shares target hit partixcle with Orianna Q: OrianaIzuna
                AddParticleTarget(_orianna, target, "OrianaIzuna_tar", target, 1f, teamOnly: _orianna.Team, bone: "pelvis", targetBone: "pelvis");
                target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);    
            }
        }

        public void OnMissileFinish(ISpellMissile missile)
        {
            _ballHandler.ChangeState(Buffs.OriannaBallHandler.BallState.ATTACHED);

            if (_target.IsDead || !_target.GetIsTargetableToTeam(_orianna.Team))
            {
                //_ballHandler.DisableBall();
                //_ballHandler.AttachToChampion((IChampion)_orianna);
                //Need to PlayTest this to ensure it works as intended
                SpellCast(_orianna, 4, SpellSlotType.ExtraSlots, true, _orianna, Vector2.Zero);

                if (_orianna.Model == "OriannaNoBall")
                {
                    _orianna.ChangeModel("Orianna");
                }

                return;
            }

            _ballHandler.DisableBall();
            _ballHandler.AttachToChampion(_target);
            _ballHandler.ChangeState(Buffs.OriannaBallHandler.BallState.ATTACHED);

            AddBuff("OrianaRedactShield", 4f, 1, missile.SpellOrigin, _target, _orianna);

            if (_target == (_orianna as IChampion))
            {
                AddBuff("OrianaGhostSelf", 1f, 1, missile.SpellOrigin, _target, _orianna, true);

                if (_orianna.Model == "OriannaNoBall")
                {
                    _orianna.ChangeModel("Orianna");
                }
            }
            else
            {
                AddBuff("OrianaGhost", 1f, 1, missile.SpellOrigin, _target, _orianna, true);
            }

            EnableAbilityCheck();
        }


        bool acivateQ = false;
        bool acivateW = false;
        bool acivateE = false;
        bool acivateR = false;
        //Spell should disable Q, W, E, and R if not on CD.
        private void DisableAbilityCheck()
        {
            if (_orianna.GetSpell(0).CurrentCooldown <= 0)
            {
                _orianna.SetSpell("OrianaIzunaCommand", 0, false);
                acivateQ = true;
            }
            //Check W
            if (_orianna.GetSpell(1).CurrentCooldown <= 0)
            {
                _orianna.SetSpell("OrianaDissonanceCommand", 1, false);
                acivateW = true;
            }
            //Check E
            if (_orianna.GetSpell(2).CurrentCooldown <= 0)
            {
                //_orianna.SetSpell("OrianaRedactCommand", 2, false);
                //acivateE = true;
            }
            //Check R
            if (_orianna.GetSpell(3).CurrentCooldown <= 0)
            {
                _orianna.SetSpell("OrianaDetonateCommand", 3, false);
                acivateR = true;
            }
        }
        private void EnableAbilityCheck()
        {
            //Check Q
            if (acivateQ)
            {
                _orianna.SetSpell("OrianaIzunaCommand", 0, true);
                acivateQ = false;
            }
            //Check W
            if (acivateW)
            {
                _orianna.SetSpell("OrianaDissonanceCommand", 1, true);
                acivateW = false;
            }
            //Check E
            if (acivateE)
            {
                _orianna.SetSpell("OrianaRedactCommand", 2, true);
                acivateE = false;
            }
            //Check R
            if (acivateR)
            {
                _orianna.SetSpell("OrianaDetonateCommand", 3, true);
                acivateR = false;
            }
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
