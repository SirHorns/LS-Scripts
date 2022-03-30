using System;
using System.Numerics;
using System.Collections.Generic;

using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.Domain.GameObjects.Spell.Sector;

using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

//*=========================================
/*
 * ValkyrieHorns
 * Lastupdated: 3/30/2022
 * 
 * TODOS:
 * Implement Windwall Interactions. Might need to make DropBallOnBlock method in Ball handler.
 * Implement minimum travel distance away from orianna if ball is attached to her.
 * 
 * ==OrianaIzunaCommand==
 * Create better model Change method.
 * Determine if there is better animation handling.
 * 
 *= =OrianaIzuna==
 * Figure out which bones in the ball that oriana_ball_glow_[green/red].troy are attached to. Look at bones in Blender?
 * Create better particle management
 * Spell Queueing for W & R. Might needs to handle this within the BallHandlerBuff
 * Sector is not ceated at Ball landing location and does not apply damage.
 * 
 * Known Issues:
 * If ball is launched to close to Orianna and pickupball it not triggered the ball slowl slides from her model to its position.
 * 
 * Notes:
 * Appears that trying to pull the current cooldown of a spell from within said spell is breaking. Or maybe there is a better way to do it. 
 * Disabling CD check on Q & E for now and allowing the normal CD to keep code logic in check
*/
//*=========================================

namespace Spells
{
    public class OrianaIzunaCommand : ISpellScript
    {
        //USED FOR DEBUGING
        bool _disableSpellCosts = false;

        public ISpellScriptMetadata ScriptMetadata => new SpellScriptMetadata()
        {
            TriggersSpellCasts = true,
        };

        IObjAiBase _orianna;
        Buffs.OriannaBallHandler _ballHandler;
        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
            _orianna = owner;
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnSpellPreCast(IObjAiBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
            _ballHandler = (owner.GetBuffWithName("OriannaBallHandler").BuffScript as Buffs.OriannaBallHandler);

            //Possible that this needs to replaced with a better system in the GameSever downt he line
            if (_orianna.Model== "Orianna")
            {
                _orianna.ChangeModel("OriannaNoBall");
            }
        }
        
        public void OnSpellCast(ISpell spell)
        {
            _orianna.PlayAnimation("Spell1", 1f, 0, 0);

            //TODO: Determine if Orianna should walk in range and cast spell or leave it so that she just walks to the cast location if it casted out of her spell range.
            var spellPos = new Vector2(spell.CastInfo.TargetPosition.X, spell.CastInfo.TargetPosition.Z);


            if (_ballHandler.GetStateAttached())
            {
                SpellCast(_orianna, 0, SpellSlotType.ExtraSlots, spellPos, spellPos, false, _ballHandler.GetAttachedChampion().Position,overrideForceLevel: spell.CastInfo.SpellLevel);
            }
            else
            {
                SpellCast(_orianna, 0, SpellSlotType.ExtraSlots, spellPos, spellPos, false, _ballHandler.GetBall().Position, overrideForceLevel: spell.CastInfo.SpellLevel);
            }
        }

        public void OnSpellPostCast(ISpell spell)
        {
            if (!_disableSpellCosts)
            {
                var coolDown = new[] { 6f, 5.25f, 4.5f, 3.75f, 3f }[spell.CastInfo.SpellLevel - 1];
                spell.SetCooldown(coolDown);

                var manaCost = new[] { 30, 35, 40, 45, 50 }[spell.CastInfo.SpellLevel - 1];
                _orianna.Stats.CurrentMana -= 50;
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
    public class OrianaIzuna : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true,
            NotSingleTargetSpell = true,
            IsDamagingSpell = true,
        };

        private IObjAiBase _orianna;
        private ISpell _spell;
        private Vector2 _spellPos;
        private ISpellMissile _missile;
        private Buffs.OriannaBallHandler _ballHandler;
        private ISpellSector _damageSector;
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
            _spellPos = new Vector2(spell.CastInfo.TargetPosition.X, spell.CastInfo.TargetPosition.Z);
            _ballHandler = (owner.GetBuffWithName("OriannaBallHandler").BuffScript as Buffs.OriannaBallHandler);
            DisableAbilityCheck();

            _ballHandler.RemoveEBuff();
        }

        public void OnSpellCast(ISpell spell)
        {
        }

        public void OnSpellPostCast(ISpell spell)
        {
            var ballPos = _ballHandler.GetBall().Position;

            if (ballPos == _spellPos)
            {
                CreateDamageSector(spell);
                _damageSector.SetToRemove();
                AddParticlePos(_orianna, "Oriana_Izuna_nova", ballPos, ballPos, 1f, bone: "BUFFBONE_CSTM_WEAPONA");
                _ballHandler.ChangeState(Buffs.OriannaBallHandler.BallState.GROUNDED);
            }
            else
            {
                _ballHandler.ChangeState(Buffs.OriannaBallHandler.BallState.FLYING);

                _missile = spell.CreateSpellMissile(new MissileParameters
                {
                    Type = MissileType.Circle,
                    OverrideEndPosition = _spellPos,
                });

                ApiEventManager.OnSpellMissileEnd.AddListener(this, _missile, OnMissileFinish, true);
            }
        }

        private int _targetHitCount = 0;
        private List<IAttackableUnit> _targetsHit = new List<IAttackableUnit>();
        public void TargetExecute(ISpell spell, IAttackableUnit target, ISpellMissile missile, ISpellSector sector)
        {
            if(_targetsHit.Contains(target))
            {
                return;
            }

            var owner = spell.CastInfo.Owner;
            var spellLevel = spell.CastInfo.SpellLevel - 1;
            var baseDamage = new[] { 60, 90, 120, 150, 180 }[spellLevel];
            var magicDamage = owner.Stats.AbilityPower.Total * .5f;
            var damage = baseDamage + magicDamage;

            int reductionAmount;
            if (_targetHitCount >= 7)
            {
                reductionAmount = 7;
            }
            else
            {
                reductionAmount = _targetHitCount;
            }

            _targetHitCount++;
            _targetsHit.Add(target);

            if (missile == _missile)
            {
                var finalDamage = damage * (1.10f - (.1f * reductionAmount));
                AddParticleTarget(_orianna, target, "OrianaIzuna_tar", target, 1f, teamOnly: _orianna.Team, bone: "pelvis", targetBone: "pelvis");
                target.TakeDamage(owner, finalDamage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
                return;
            }

            if(sector == _damageSector)
            {
                var finalDamage = damage * (1.10f - (.1f * reductionAmount));
                AddParticleTarget(_orianna, target, "OrianaIzuna_tar", target, 1f, teamOnly: _orianna.Team, bone: "pelvis", targetBone: "pelvis");
                target.TakeDamage(owner, finalDamage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
            }
        }

        IParticle allyMarker;
        IParticle enemyMarker;
        public void OnMissileFinish(ISpellMissile missile)
        {
            var paritclePos = _ballHandler.TeleportBall(_spellPos, true);

            _ballHandler.ChangeState(Buffs.OriannaBallHandler.BallState.GROUNDED);

            AddParticlePos(_orianna, "Oriana_Izuna_nova", paritclePos, paritclePos, 1f, bone: "BUFFBONE_CSTM_WEAPONA");

            CreateDamageSector(missile.SpellOrigin);

            _targetHitCount = 0;

            TeamId enemyTeamId;
            if(_orianna.Team == TeamId.TEAM_BLUE)
            {
                enemyTeamId = TeamId.TEAM_PURPLE;
            }
            else
            {
                enemyTeamId = TeamId.TEAM_BLUE;
            }

            //TODO figure out which bonesets the markers to surround the ball and not be on the ground
            //Possible it shouldn't be around the ball.On Live servers they use updated paticle indicators so this might just be a ground particle for this patch.
            //TODO: Better particle management.
            if (enemyMarker == null || allyMarker == null)
            {
                allyMarker = AddParticleTarget(_orianna, _ballHandler.GetBall(), "oriana_ball_glow_green", _ballHandler.GetBall(), 2300f, teamOnly: _orianna.Team, bone: "BUFFBONE_CSTM_WEAPONA");
                enemyMarker = AddParticlePos(_orianna, "oriana_ball_glow_red", _ballHandler.GetBall().Position, _ballHandler.GetBall().Position, 2300f, teamOnly: enemyTeamId, bone: "BUFFBONE_CSTM_WEAPONA");
            }
            else
            {
                //allyMarker.SetToRemove();
                enemyMarker.SetToRemove();
                //allyMarker = AddParticlePos(_owner, "oriana_ball_glow_green.troy", BallHandler.GetBall().Position, BallHandler.GetBall().Position, 2300f, teamOnly: _owner.Team, bone: "BUFFBONE_CSTM_WEAPONA");
                enemyMarker = AddParticlePos(_orianna, "oriana_ball_glow_red", _ballHandler.GetBall().Position, _ballHandler.GetBall().Position, 2300f, teamOnly: enemyTeamId, bone: "BUFFBONE_CSTM_WEAPONA");
            }
            _damageSector.SetToRemove();
            _targetsHit.Clear();

            EnableAbilityCheck();
        }

        private void CreateDamageSector(ISpell spell)
        {
            _damageSector = spell.CreateSpellSector(new SectorParameters
            {
                Length = 175,
                SingleTick = true,
                Tickrate = 6,
                OverrideFlags = SpellDataFlags.InstantCast | SpellDataFlags.AffectEnemies | SpellDataFlags.AffectNeutral | SpellDataFlags.AffectMinions | SpellDataFlags.AffectHeroes,
                BindObject = _ballHandler.GetBall(),
                Type = SectorType.Area,
                CanHitSameTarget = false,
                CanHitSameTargetConsecutively = false,
            });
        }

        bool acivateQ = false;
        bool acivateW = false;
        bool acivateE = false;
        bool acivateR = false;
        //Spell should only disable Q and E if not on CD asince W and R can be queued cast.
        //Not sure if queue casting was present during this patch so will just asume it was.
        private void DisableAbilityCheck()
        {
            //Check Q
            if (_orianna.GetSpell(0).CurrentCooldown <= 0)
            {
                //_orianna.SetSpell("OrianaIzunaCommand", 0, false);
                //acivateQ = true;
            }
            //Check W
            if (_orianna.GetSpell(1).CurrentCooldown <= 0)
            {
                //_orianna.SetSpell("OrianaDissonanceCommand", 1, false);
                //acivateW = true;
            }
            //Check E
            if (_orianna.GetSpell(2).CurrentCooldown <= 0)
            {
                _orianna.SetSpell("OrianaRedactCommand", 2, false);
                acivateE = true;
            }
            //Check R
            if (_orianna.GetSpell(3).CurrentCooldown <= 0)
            {
                //_orianna.SetSpell("OrianaDetonateCommand", 3, false);
                //acivateR = true;
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

