﻿using System.Numerics;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects.Stats;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;

using System;

namespace Buffs
{
    class OriannaBallHandler : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.INTERNAL,
            BuffAddType = BuffAddType.RENEW_EXISTING,
            MaxStacks = 1
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier ();

        private IObjAiBase _owner;
        private ISpell _spell;
        private IBuff _buff;
        private OriannaBall _ballBuff;

        private IMinion _ball = null;
        private IChampion AttachedChampion;

        private bool IsGrounded = false;
        private bool IsAttached = true;
        private bool IsRendered = false;
        private bool IsInFlight = false;


        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            _owner = (IObjAiBase) unit;
            _spell = ownerSpell;
            _buff = buff;
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
        }
        /// <summary>
        /// Returns if the ball is in flight.
        /// </summary>
        /// <returns>Returns if the ball is in flight.</returns>
        public bool GetFlightState()
        {
            return this.IsInFlight;
        }

        /// <summary>
        /// Sets if the ball is in flight.
        /// </summary>
        /// <param name="IsInFlight">Sets ball state to in flight.</param>
        /// <returns>If ball is is in flight.</returns>
        public bool SetFlightState(bool IsInFlight)
        {
            return this.IsInFlight;
        }

        /// <summary>
        /// Returns the current champion the ball is attached.
        /// </summary>
        /// <returns>Champion the Ball is Attached to.</returns>
        public IChampion GetAttachedChampion()
       {
            return AttachedChampion;
       }

        /// <summary>
        /// Sets the Chamption that the Ball is attached to.
        /// </summary>
        /// <param name="chamption">Champion to attach the ball too./param>
        /// <returns>Champion the ball will be attached to.</returns>
        public IChampion SetAttachedChampion(IChampion chamption)
        {
            AttachedChampion = chamption;
            return AttachedChampion;
        }

        /// <summary>
        /// Returns if the ball is currently attached to a chamption or not.
        /// </summary>
        /// <returns>If ball is attached to champ or not.</returns>
        public bool GetIsAttached()
        {
            return IsAttached;
        }

        /// <summary>
        /// Sets a bool for if ball is attached to a champ or not
        /// </summary>
        /// <param name="attached">Bool if ball is attached or not. </param>
        /// <returns>If ball is attached to a champion or not.</returns>
        public bool SetAttachedState(bool attached)
        {
            return this.IsAttached = attached;
        }

        /// <summary>
        /// Returns the current grounded state of the ball.
        /// </summary>
        /// <returns>If ball is grounded or not.</returns>
        public bool GetGroundedState()
        {
            return IsGrounded;
        }

        /// <summary>
        /// Sets the ball grounded and attached states based on input.
        /// </summary>
        /// <param name="IsGrounded">Should the ball be active or disabled</param>
        /// <returns>If ball is grounded or not.</returns>
        public bool SetGroundedState(bool IsGrounded)
        {
            this.IsGrounded = IsGrounded;

            if (IsGrounded)
            {
                SetRenderState(true);
            }
            else
            {
                SetRenderState(false);
            }

            return this.IsGrounded;
        }

        /// <summary>
        /// Set the ball render state.
        /// </summary>
        /// <param name="IsRendered">Bool for if the ball should be rendered or not.</param>
        /// <returns>If ball is rendered or not.</returns>
        public bool SetRenderState(bool IsRendered)
        {
            if (IsRendered)
            {
                SetStatus(_ball, StatusFlags.NoRender, false);
            }
            else 
            {
                SetStatus(_ball, StatusFlags.NoRender, true);
            }
            
            this.IsRendered = IsRendered;
            return this.IsRendered;
        }

        /// <summary>
        /// Returns the current render state of the ball.
        /// </summary>
        /// <returns>If the ball is rendered or not.</returns>
        public bool GetRenderState()
        {
            return this.IsRendered;
        }

        /// <summary>
        /// Spawns a new instance of Orianna's Ball if OriannaBall is not null
        /// </summary>
        /// <param name="position">Position to spawn ball at.</param>
        /// <param name="renderState">Sets if the ball should be rendered on spawn.</param>
        /// <returns>New instance of a Orianna Ball.</returns>
        public IMinion SpawnBall(Vector2 position, bool renderState = false)
        {
            //var spellPos = new Vector2(position.X, position.Y);
            //OriannaBall = AddMinion(_owner, "OriannaBall", "OriannaBall" + _owner.Team, position, _owner.Team, _owner.SkinID, true, false,SpellDataFlags.NonTargetableAll,visibilityOwner: null,true);
            _ball = AddMinion(_owner, "OriannaBall", "OriannaBall" + _owner.Team, position, _owner.Team, _owner.SkinID, true, false);
            _ball.FaceDirection(new Vector3(position.X, 0, position.Y));
            _ballBuff = AddBuff("OriannaBall", 2300.0f, 1, _spell, _ball, _owner).BuffScript as OriannaBall;
            SetRenderState(renderState);

            return _ball;
        }

        /// <summary>
        /// Teleports current instance of Orianna ball to the new position.
        /// </summary>
        /// <param name="newPosition">New position to move Orianna Ball to.</param>
        /// <param name="activateBall">Usually enabled for Q. Sets Ball to active configuration.</param>
        /// <returns>The new position of the ball.</returns>
        public Vector2 MoveBall(Vector2 newPosition,bool activateBall = false)
        {
            _ball.TeleportTo(newPosition.X, newPosition.Y);

            if(activateBall)
            {
                SetRenderState(true);
                SetGroundedState(true);
                SetAttachedChampion(null);
                SetAttachedState(false);
            }

            return newPosition;
        }

        /// <summary>
        /// Returns the current instance of a Orianna Ball.
        /// </summary>
        /// <returns>Current instance of Orianna Ball</returns>
        public IMinion GetBall()
        {
            return _ball;
        }

        /// <summary>
        /// Disables the Ball state and attaches it back to Orianna unless
        /// </summary>
        /// <param name="attachToChamp">If the ball should be attached to a new champ, defaults to Orianna if false</param>
        /// <param name="champion">Champion to attach ball to once it is disabled.</param>
        /// TODO: Possible better way to handle this rather than Active/Deactive locking other methods. Would prefer to not constantly destory and remake a ball instance if possible.
        public void DisableBall(bool attachToChamp = false, IChampion champion = null)
        {
            SetRenderState(false);
            SetAttachedState(true);
            SetGroundedState(false);


            if (attachToChamp)
            {
                SetAttachedChampion(champion);
                SetAttachedState(true);
            }
            else
            {
                SetAttachedChampion((IChampion)_owner);
                SetAttachedState(true);
            }

            //theBall.TakeDamage(theBall.Owner, 10000f, DamageType.DAMAGE_TYPE_TRUE, DamageSource.DAMAGE_SOURCE_INTERNALRAW, DamageResultType.RESULT_NORMAL);
        }

        /// <summary>
        /// Casts the spell OrianaReturn.
        /// </summary>
        public void ReturnBall() 
        {
            SpellCast(_owner, 4, SpellSlotType.ExtraSlots, false, _ball, Vector2.Zero);
        }

        /// <summary>
        /// Perform a cast of the given spell using OriannaBall, (Applies to Q, W, E, R)
        /// </summary>
        /// <param name="spell">Spell which triggered this ball will cast.</param>
        /// TODO: Test this Q,W,E,R
        public void BallCast(ISpell spell)
        {
            var slot = spell.CastInfo.SpellSlot;
            if (slot != 0 || slot != 2 && _ball != null)
            {
                return;
            }
            FaceDirection(new Vector2(spell.CastInfo.TargetPositionEnd.X, spell.CastInfo.TargetPositionEnd.Z), _ball, true);
            SpellCast(spell.CastInfo.Owner, slot, SpellSlotType.SpellSlots, true, spell.CastInfo.Targets[0].Unit, _ball.Position);
        }

        public void OnUpdate(float diff)
        {
        }
    }
}
