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
 * Lastupdated: 3/25/2022
 * Notes:
 * Renamed Getter/Setters to an easier to parse scheme.
 * Organized everything into seperate regions
 * 
 * TODOS:
 * Clean and reorganize code more.
 * 
 * Known Issues:
 * Due to how intergated this script is on the overall functioning of Orianna it will crash League if you Reloadscripts or use Hotreload. 
 * Using an ability will crash the game as Buffs aren't reloaded properly and this will throw a bunch of null errors even if the script is technially applied
 * to Orianna still.
*/
//*=========================================

namespace Buffs
{
    class OriannaBallHandler : IBuffGameScript
    {
        public enum BallState : byte
        {
            ATTACHED,
            GROUNDED,
            FLYING,
            DISABLED
        }

        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.INTERNAL,
            BuffAddType = BuffAddType.RENEW_EXISTING,
            MaxStacks = 1
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier ();

        private IObjAiBase _orianna;
        private ISpell _spell;
        private OriannaBall _oriannaBallBuff;

        private IMinion _oriannaBall = null;
        private IChampion _attachedChampion;

        private BallState _currentState;
        private bool IsFlying = false;
        private bool IsGrounded = false;
        private bool IsAttached = true;
        private bool IsRendered = false;
        private bool CanBeReturned = false;

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            _orianna = (IObjAiBase) unit;
            _spell = ownerSpell;
            ApiEventManager.OnDeath.AddListener(this, unit, OnDeath, false);
            ApiEventManager.OnResurrect.AddListener(this, _orianna, OnRevive, false);

            SpawnBall(_orianna.Position);
            AttachToChampion(_orianna as IChampion);
        }

        private void OnRevive(IObjAiBase unit)
        {
            ChangeState(BallState.DISABLED);
            ChangeState(BallState.ATTACHED);
            AttachToChampion(_orianna as IChampion);
            AddEBuff(_orianna, _orianna.GetSpell(2));
        }

        private void OnDeath(IDeathData death)
        {
            ChangeState(BallState.DISABLED);
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            //DisableBall();
            //_oriannaBall.TakeDamage(_orianna, 10000f, DamageType.DAMAGE_TYPE_TRUE, DamageSource.DAMAGE_SOURCE_INTERNALRAW, DamageResultType.RESULT_NORMAL);
        }


        #region State-Getters

        /// <summary>
        /// Returns current pickable state
        /// </summary>
        /// <returns>Pickupable state.</returns>
        public bool GetStateReturnable()
        {
            return this.CanBeReturned;
        }

        /// <summary>
        /// Returns if the ball is in flight.
        /// </summary>
        /// <returns>Returns if the ball is in flight.</returns>
        public bool GetStateFlying()
        {
            return this.IsFlying;
        }

        /// <summary>
        /// Returns the current grounded state of the ball.
        /// </summary>
        /// <returns>If ball is grounded or not.</returns>
        public bool GetStateGrounded()
        {
            return this.IsGrounded;
        }

        /// <summary>
        /// Returns if the ball is currently attached to a chamption or not.
        /// </summary>
        /// <returns>If ball is attached to champ or not.</returns>
        public bool GetStateAttached()
        {
            return this.IsAttached;
        }

        /// <summary>
        /// Returns the current render state of the ball.
        /// </summary>
        /// <returns>If the ball is rendered or not.</returns>
        public bool GetStateRender()
        {
            return this.IsRendered;
        }

        #endregion

        #region State-Setters

        /// <summary>
        /// Sets if the ball can be picked up.
        /// </summary>
        /// <param name="pickupable">Sets if the ball can be picked up</param>
        /// <returns>Pickupable state.</returns>
        public bool SetStateReturnable(bool pickupable)
        {
            return CanBeReturned = pickupable;
        }

        /// <summary>
        /// Sets if the ball is in flight.
        /// </summary>
        /// <param name="flying">Sets ball state to in flight.</param>
        /// <returns>If ball is is in flight.</returns>
        public bool SetStateFlying(bool flying)
        {
            return IsFlying = flying;
        }

        /// <summary>
        /// Sets the ball grounded and attached states based on input.
        /// </summary>
        /// <param name="grounded">Should the ball be active or disabled</param>
        /// <returns>If ball is grounded or not.</returns>
        public bool SetStateGrounded(bool grounded)
        {
            return IsGrounded = grounded;
        }

        /// <summary>
        /// Sets a bool for if ball is attached to a champ or not
        /// </summary>
        /// <param name="attached">Bool if ball is attached or not. </param>
        /// <returns>If ball is attached to a champion or not.</returns>
        public bool SetStateAttached(bool attached)
        {
            return IsAttached = attached;
        }

        /// <summary>
        /// Set the ball render state.
        /// </summary>
        /// <param name="render">Bool for if the ball should be rendered or not.</param>
        /// <returns>If ball is rendered or not.</returns>
        public bool SetStateRendered(bool render)
        {
            if (render)
            {
                SetStatus(_oriannaBall, StatusFlags.NoRender, false);
            }
            else
            {
                SetStatus(_oriannaBall, StatusFlags.NoRender, true);
            }

            return IsRendered = render;
        }

        #endregion


        #region General Getter Methods

        /// <summary>
        /// Returns the current instance of a Orianna Ball.
        /// </summary>
        /// <returns>Current instance of Orianna Ball</returns>
        public IMinion GetBall()
        {
            return _oriannaBall;
        }

        /// <summary>
        /// Returns the current champion the ball is attached.
        /// </summary>
        /// <returns>Champion the Ball is Attached to.</returns>
        public IChampion GetAttachedChampion()
        {
            return this._attachedChampion;
        }

        #endregion

        #region General Setter Methods

        /// <summary>
        /// Sets the Chamption that the Ball is attached to.
        /// </summary>
        /// <param name="champion">Champion to attach the ball too./param>
        /// <returns>Champion the ball will be attached to.</returns>
        public IChampion SetAttachedChampion(IChampion champion)
        {
            SetStateAttached(true);
            this._attachedChampion = champion;
            return this._attachedChampion;
        }

        #endregion


        #region Helper-Methods

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
            _oriannaBall = AddMinion(_orianna, "OriannaBall", "OriannaBall" + _orianna.Team, position, _orianna.Team, _orianna.SkinID, true, false);
            _oriannaBall.FaceDirection(new Vector3(position.X, 0, position.Y));
            _oriannaBallBuff = AddBuff("OriannaBall", 2300.0f, 1, _spell, _oriannaBall, _orianna).BuffScript as OriannaBall;

            ChangeState(BallState.DISABLED);

            if(renderState)
            {
                SetStateRendered(true);
            }

            return _oriannaBall;
        }

        /// <summary>
        /// Destorys the current instance of Orianna Ball.
        /// </summary>
        public void DestoryBall()
        {
            ChangeState(BallState.DISABLED);

            _oriannaBall.TakeDamage(_orianna, 10000f, DamageType.DAMAGE_TYPE_TRUE, DamageSource.DAMAGE_SOURCE_INTERNALRAW, DamageResultType.RESULT_NORMAL);
            _oriannaBall = null;
        }

        /// <summary>
        /// Disables the all Ball States and clears Attached Champion & removes E Passive Buffs.
        /// </summary>
        public void DisableBall()
        {
            ChangeState(BallState.DISABLED);
            //theBall.TakeDamage(theBall.Owner, 10000f, DamageType.DAMAGE_TYPE_TRUE, DamageSource.DAMAGE_SOURCE_INTERNALRAW, DamageResultType.RESULT_NORMAL);
        }

        /// <summary>
        /// Teleports current instance of Orianna ball to the new position.
        /// </summary>
        /// <param name="newPosition">New position to move Orianna Ball to.</param>
        /// <param name="enableBall">Usually enabled for Q. Sets Ball to active configuration.</param>
        /// <returns>The new position of the ball.</returns>
        public Vector2 TeleportBall(Vector2 newPosition, bool enableBall = false)
        {
            ChangeState(BallState.DISABLED);

            _oriannaBall.TeleportTo(newPosition.X, newPosition.Y);

            if (enableBall)
            {
                ChangeState(BallState.GROUNDED);
            }

            return newPosition;
        }

        /// <summary>
        /// Places the ball at currently attached targets location. Used for on AttachedChampions death or if they become untargetable.
        /// </summary>
        /// <param name="newPosition">New position to place Orianna Ball at.</param>
        /// <param name="activateBall">Usually enabled for Q. Sets Ball to active configuration.</param>
        /// <returns>The new position of the ball.</returns>
        public Vector2 DropBall()
        {
            _oriannaBall.TeleportTo(GetAttachedChampion().Position.X, GetAttachedChampion().Position.Y);
            var droppedPosition = new Vector2(GetAttachedChampion().Position.X, GetAttachedChampion().Position.Y);

            ChangeState(BallState.GROUNDED);

            return droppedPosition;
        }

        /// <summary>
        /// Attaches Ball to Champion and sets relevant states and removes Buffs.
        /// </summary>
        /// <param name="champion">Champion to attach the ball too./param>
        /// <returns>Champion the ball will be attached to.</returns>
        public IChampion AttachToChampion(IChampion champion)
        {
            RemoveEBuff();

            ChangeState(BallState.ATTACHED);
            SetAttachedChampion(champion);

            return champion;
        }

        /// <summary>
        /// Handles bulk stat changing actions.
        /// </summary>
        public void ChangeState(BallState newState)
        {
            if (_currentState == newState)
            {
                return;
            }

            if (newState == BallState.DISABLED)
            {
                RemoveEBuff();

                SetAttachedChampion(null);

                SetStateFlying(false);
                SetStateGrounded(false);
                SetStateAttached(false);
                SetStateReturnable(false);
                SetStateRendered(false);
            }

            if (newState == BallState.ATTACHED)
            {
                SetStateFlying(false);
                SetStateGrounded(false);
                SetStateAttached(true);
                SetStateReturnable(true);
                SetStateRendered(false);
            }

            if (newState == BallState.GROUNDED)
            {
                RemoveEBuff();

                SetAttachedChampion(null);

                SetStateFlying(false);
                SetStateGrounded(true);
                SetStateAttached(false);
                SetStateReturnable(true);
                SetStateRendered(true);
            }

            if (newState == BallState.FLYING)
            {
                RemoveEBuff();

                SetStateFlying(true);
                SetStateGrounded(false);
                SetStateAttached(false);
                SetStateReturnable(false);
                SetStateRendered(false);
            }

            _currentState = newState;
        }

        /// <summary>
        /// Adds Orianna E Passive Buff to target
        /// </summary>
        public void AddEBuff(IAttackableUnit target, ISpell spell)
        {
            if (target == _orianna)
            {
                AddBuff("OrianaGhost", 1.0f, 1, spell, target, _orianna, true);
            }
            else
            {
                AddBuff("OrianaGhostSelf", 1.0f, 1, spell, target, _orianna, true);
            }
        }

        /// <summary>
        /// Removes Orianna E Passive Buff from currently attached champion.
        /// </summary>
        public void RemoveEBuff()
        {
            if (_attachedChampion != null)
            {
                GetAttachedChampion().RemoveBuffsWithName("OrianaGhost");
                GetAttachedChampion().RemoveBuffsWithName("OrianaGhostSelf");
            }
        }

        #endregion


        #region SpellCastMethods

        /// <summary>
        /// Casts the spell OrianaReturn.
        /// </summary>
        public void ReturnBall(bool fireWithoutCasting = false)
        {
            if (CanBeReturned)
            {
                SpellCast(_orianna, 4, SpellSlotType.ExtraSlots, fireWithoutCasting, _oriannaBall, Vector2.Zero);
            }
        }

        /// <summary>
        /// Perform a cast of the given spell using OriannaBall, (Applies to Q, W, E, R)
        /// </summary>
        /// <param name="spell">Spell which triggered this ball will cast.</param>
        /// TODO: Test this Q,W,E,R
        public void CastSpellFromBall(ISpell spell)
        {
            var slot = spell.CastInfo.SpellSlot;
            if (slot != 1 || slot != 3 && _oriannaBall != null)
            {
                return;
            }
            //FaceDirection(new Vector2(spell.CastInfo.TargetPositionEnd.X, spell.CastInfo.TargetPositionEnd.Z), _ball, true);
            SpellCast(spell.CastInfo.Owner, slot, SpellSlotType.SpellSlots, true, spell.CastInfo.Targets[0].Unit, _oriannaBall.Position);
        }

        #endregion


        public void OnUpdate(float diff)
        {
        }
    }
}
