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
 * Find particle used on champs damaged by W. Looked to be a shared particle with Ultimate on Damage particle.
 * Uses a placeholder particle for now.
 * 
 * Known Issues:
 * Spell cooldown needs to be definitaly stated to not ignore CDR or it just locks to 9s cd period
*/
//*========================================

namespace Spells
{
    public class OrianaDissonanceCommand : ISpellScript
    {
        //USED FOR DEBUGING
        bool _disableSpellCosts = false;

        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            IsDamagingSpell = true,
            NotSingleTargetSpell = true,
            TriggersSpellCasts  = true,
        };

        private IObjAiBase _orianna;
        private IMinion _oriannaBall;
        private ISpell _spell;

        private ISpellSector _allySectorHaste;
        private ISpellSector _enemySectorSlow;
        private ISpellSector _enemySectorDamage;

        private Buffs.OriannaBallHandler _ballHandler;
        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
            _orianna = owner;
            _spell = spell;

            ApiEventManager.OnSpellHit.AddListener(this, spell, TargetExecute, false);
        }

        private void TargetExecute(ISpell spell, IAttackableUnit target, ISpellMissile missile, ISpellSector sector)
        {
            if (sector == _allySectorHaste)
            {
                AddBuff("OrianaHaste", 2.0f, 1, spell, target, _orianna, false);

                return;
            }


            if (sector == _enemySectorSlow)
            {
                AddBuff("OrianaSlow", 2.0f, 1, spell, target, _orianna, false);

                return;
            }

            if(sector == _enemySectorDamage)
            {
                var spellLevel = spell.CastInfo.SpellLevel - 1;
                var baseDamage = new[] { 60, 105, 150, 195, 240 }[spellLevel];
                var magicDamage = _orianna.Stats.AbilityPower.Total * .7f;
                var finalDamage = baseDamage + magicDamage;

                //TODO: Find particle used on champs damaged by W. Looked to be a shared particle with Ultimate on Damage particle. this is a placeholder particle
                AddParticleTarget(_orianna, target, "Oriana_ts_tar.troy", target, 1f, teamOnly: _orianna.Team, bone: "pelvis", targetBone: "pelvis");
                target.TakeDamage(_orianna, finalDamage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
            }
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnSpellPreCast(IObjAiBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
            _ballHandler = (owner.GetBuffWithName("OriannaBallHandler").BuffScript as Buffs.OriannaBallHandler);
            _oriannaBall = _ballHandler.GetBall();
        }

        public void OnSpellCast(ISpell spell)
        {
            if (_ballHandler.GetStateAttached())
            {
                CreateSpellSectors(_ballHandler.GetAttachedChampion());
                CreateParticles(_ballHandler.GetAttachedChampion().Position);
            }
            else
            {
                if (_ballHandler.GetStateFlying())
                {
                    //TODO: Queue Spell to cast at final location. Grounded or Attached.
                }
                else
                {
                    CreateSpellSectors(_oriannaBall);
                    CreateParticles(_oriannaBall.Position);
                }
            }

            _orianna.PlayAnimation("Spell2", 1f, 0, 0);
        }

        private void CreateSpellSectors(IObjAiBase target)
        {
            var bindObject = AddMinion(_orianna, "TestCubeRender", "OriannaWMarker", target.Position, _orianna.Team, ignoreCollision: true, targetable: false);
            AddBuff("ExpirationTimer", 3.0f, 1, _spell, bindObject, _orianna);


            _allySectorHaste = _spell.CreateSpellSector(new SectorParameters
            {
                Length = 225,
                Lifetime = 3,
                Tickrate = 4,
                OverrideFlags = SpellDataFlags.InstantCast | SpellDataFlags.AffectFriends | SpellDataFlags.AlwaysSelf | SpellDataFlags.AffectMinions | SpellDataFlags.AffectHeroes,
                BindObject = bindObject,
                Type = SectorType.Area,
                CanHitSameTargetConsecutively = true,
            });

            _enemySectorSlow = _spell.CreateSpellSector(new SectorParameters
            {
                Length = 225,
                Lifetime = 3,
                Tickrate = 4,
                OverrideFlags = SpellDataFlags.InstantCast | SpellDataFlags.AffectEnemies | SpellDataFlags.AffectNeutral | SpellDataFlags.AffectMinions | SpellDataFlags.AffectHeroes,
                BindObject = bindObject,
                Type = SectorType.Area,
                CanHitSameTargetConsecutively = true,
            });

            _enemySectorDamage = _spell.CreateSpellSector(new SectorParameters
            {
                Length = 225,
                SingleTick = true,
                Tickrate = 6,
                OverrideFlags = SpellDataFlags.InstantCast | SpellDataFlags.AffectEnemies | SpellDataFlags.AffectNeutral | SpellDataFlags.AffectMinions | SpellDataFlags.AffectHeroes,
                BindObject = bindObject,
                Type = SectorType.Area,
                CanHitSameTarget = false,
                CanHitSameTargetConsecutively = false,
            });
        }

        private void CreateParticles(Vector2 position)
        {
            TeamId enemyTeamId;
            if (_orianna.Team == TeamId.TEAM_BLUE)
            {
                enemyTeamId = TeamId.TEAM_PURPLE;
            }
            else
            {
                enemyTeamId = TeamId.TEAM_BLUE;
            }

            AddParticlePos(_orianna, "OrianaDissonance_ally_green", position, position, lifetime: 3f, teamOnly: _orianna.Team);
            //AddParticlePos(_orianna, "OrianaDissonance_ball_green", position, position, lifetime: 3f, teamOnly: _orianna.Team);
            //AddParticlePos(_orianna, "OrianaDissonance_cas_green", position, position, lifetime: 3f, teamOnly: _orianna.Team);

            AddParticlePos(_orianna, "OrianaDissonance_ally_red", position, position, lifetime: 3f, teamOnly: enemyTeamId);
            //AddParticlePos(_orianna, "OrianaDissonance_ball_red", position, position, lifetime: 3f, teamOnly: enemyTeamId);
            //AddParticlePos(_orianna, "OrianaDissonance_cas_red", position, position, lifetime: 3f, teamOnly: enemyTeamId);
        }

        public void OnSpellPostCast(ISpell spell)
        {
            if (!_disableSpellCosts)
            {
                _spell.SetCooldown(9.0f,false);

                var manaCost = new[] { 70, 80, 90, 100, 110 }[_spell.CastInfo.SpellLevel - 1];
                _orianna.Stats.CurrentMana -= manaCost;
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
