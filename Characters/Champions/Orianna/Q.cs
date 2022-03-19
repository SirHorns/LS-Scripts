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
 * Lastupdated: 3/19/2022
 * 
 * TODOS:
 * ==OrianaIzunaCommand==
 * Determine Q out-of-range casting logic/functionality.
 * Create better model Change method.
 * Determine if there is better animation handling.
 * 
 *= =OrianaIzuna==
 * Figure out how oriana_ball_glow_[green/red].troy was displayed in patch 4.20
 * Create better particle management
*/
//*=========================================

namespace Spells
{
    public class OrianaIzunaCommand : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata => new SpellScriptMetadata()
        {
            TriggersSpellCasts = true,
        };

        IObjAiBase _orianna;
        Buffs.OriannaBallHandler BallHandler;
        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
            _orianna = owner;
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnSpellPreCast(IObjAiBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
            BallHandler = (owner.GetBuffWithName("OriannaBallHandler").BuffScript as Buffs.OriannaBallHandler);
        }
        
        public void OnSpellCast(ISpell spell)
        {
            //TODO: Determine if Orianna should walk in range and cast spell or leave it so that she just walks to the cast location if it casted out of her spell range.
            var spellPos = new Vector2(spell.CastInfo.TargetPosition.X, spell.CastInfo.TargetPosition.Z);


            if (BallHandler.GetIsAttached())
            {
                SpellCast(_orianna, 0, SpellSlotType.ExtraSlots, spellPos, spellPos, false, BallHandler.GetAttachedChampion().Position);
            }
            else
            {
                SpellCast(_orianna, 0, SpellSlotType.ExtraSlots, spellPos, spellPos, false, BallHandler.GetBall().Position);
            }

            //TODO: Think of better way of handling Orianna Model Changing between spells.
            //Maybe use a internal buff to keep track of overall model state instead of setting it within individual spells.
            if (_orianna.Model == "Orianna")
            {
                _orianna.ChangeModel("OriannaNoBall");
            }

            //TODO: Clean up animation. Slight sliding as animtion fully plays while moving. Spell does not stop Orianna's Movement commands.
            _orianna.PlayAnimation("Spell1", 1f, 0, 0);
        }

        public void OnSpellPostCast(ISpell spell)
        {
            var coolDown = new[] { 6f, 5.25f, 4.5f, 3.75f, 3f }[spell.CastInfo.SpellLevel - 1];
            spell.SetCooldown(coolDown);

            var manaCost = new[] { 30, 35, 40, 45, 50 }[spell.CastInfo.SpellLevel - 1];
            _orianna.Stats.CurrentMana -= manaCost;
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
        }

        public void OnSpellCast(ISpell spell)
        {
            var ballPos = _ballHandler.GetBall().Position;
            if (ballPos == _spellPos)
            {
                CreateDamageSector();
                AddParticlePos(_orianna, "Oriana_Izuna_nova", ballPos, ballPos, 1f, bone: "BUFFBONE_CSTM_WEAPONA");
            }
            else 
            {
                var missile = spell.CreateSpellMissile(new MissileParameters
                {
                    Type = MissileType.Circle,
                    OverrideEndPosition = _spellPos,
                });

                _ballHandler.SetFlightState(true);

                ApiEventManager.OnSpellMissileEnd.AddListener(this, missile, OnMissileFinish, true);

                _ballHandler.SetRenderState(false);

                if (_ballHandler.GetAttachedChampion() != null)
                {
                    _ballHandler.GetAttachedChampion().RemoveBuffsWithName("OrianaGhost");
                    _ballHandler.GetAttachedChampion().RemoveBuffsWithName("OrianaGhostSef");
                }
            }
        }

        public void OnSpellPostCast(ISpell spell)
        {
        }

        private int _targetHitCount = 0;
        private List<IAttackableUnit> _targetsHit = new List<IAttackableUnit>();
        public void TargetExecute(ISpell spell, IAttackableUnit target, ISpellMissile missile, ISpellSector sector)
        {
            if (!_targetsHit.Contains(target)) 
            {
                _targetHitCount++;

                var owner = spell.CastInfo.Owner;
                var spellLevel = spell.CastInfo.SpellLevel - 1;
                var baseDamage = new[] { 60, 90, 120, 150, 180 }[spellLevel];
                var magicDamage = owner.Stats.AbilityPower.Total * .5f;
                var damage = baseDamage + magicDamage;

                var reductionAmount = 0;
                if (_targetHitCount >= 7)
                {
                    reductionAmount = 7;
                }
                else
                {
                    reductionAmount = _targetHitCount;
                }
                var finalDamage = damage * (1.10f - (.1f * reductionAmount));
                AddParticleTarget(_orianna, target, "OrianaIzuna_tar", target, 1f, teamOnly: _orianna.Team, bone: "pelvis", targetBone: "pelvis");
                target.TakeDamage(owner, finalDamage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
                _targetsHit.Add(target);
            }
        }

        IParticle allyMarker;
        IParticle enemyMarker;
        public void OnMissileFinish(ISpellMissile missile)
        {
            var paritclePos = _ballHandler.MoveBall(_spellPos, true);

            _ballHandler.SetFlightState(false);

            AddParticlePos(_orianna, "Oriana_Izuna_nova", paritclePos, paritclePos, 1f, bone: "BUFFBONE_CSTM_WEAPONA");

            CreateDamageSector();

            _targetHitCount = 0;
            _targetsHit.Clear();

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
        }

        private void CreateDamageSector()
        {
            _damageSector = _spell.CreateSpellSector(new SectorParameters
            {
                Length = 180,
                SingleTick = true,
                OverrideFlags = SpellDataFlags.AffectEnemies | SpellDataFlags.AffectNeutral | SpellDataFlags.AffectMinions | SpellDataFlags.AffectHeroes,
                BindObject = _orianna,
                Type = SectorType.Area,
            });
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

