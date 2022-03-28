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
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.INTERNAL,
            BuffAddType = BuffAddType.RENEW_EXISTING,
            MaxStacks = 1
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier ();

        private IObjAiBase _orianna;
        private ISpell _spell;
        private OriannaBall _ballBuff;

        private IMinion _oriannaBall = null;
        private IChampion AttachedChampion;

        private bool IsFlying = false;
        private bool IsGrounded = false;
        private bool IsAttached = true;
        private bool IsRendered = false;
        

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            _orianna = (IObjAiBase) unit;
            _spell = ownerSpell;
            ApiEventManager.OnDeath.AddListener(this, unit, OnDeath, false);
            SpawnOriannaBall(_orianna.Position);
        }

        private void OnDeath(IDeathData obj)
        {
            DisableOriannaBall();
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            DisableOriannaBall();
            _oriannaBall.TakeDamage(_orianna, 10000f, DamageType.DAMAGE_TYPE_TRUE, DamageSource.DAMAGE_SOURCE_INTERNALRAW, DamageResultType.RESULT_NORMAL);
        }



        #region State-Getters

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
        /// Sets if the ball is in flight.
        /// </summary>
        /// <param name="IsFlying">Sets ball state to in flight.</param>
        /// <returns>If ball is is in flight.</returns>
        public bool SetStateFlying(bool IsFlying)
        {
            if (IsFlying)
            {
                SetStateAttached(false);
                SetAttachedChampion(null);
                SetStateRendered(false);
            }

            this.IsFlying = IsFlying;
            return this.IsFlying;
        }

        /// <summary>
        /// Sets the ball grounded and attached states based on input.
        /// </summary>
        /// <param name="IsGrounded">Should the ball be active or disabled</param>
        /// <returns>If ball is grounded or not.</returns>
        public bool SetStateGrounded(bool IsGrounded)
        {
            this.IsGrounded = IsGrounded;

            if (IsGrounded)
            {
                SetStateRendered(true);
            }
            else
            {
                SetStateRendered(false);
            }

            return this.IsGrounded;
        }

        /// <summary>
        /// Sets a bool for if ball is attached to a champ or not
        /// </summary>
        /// <param name="attached">Bool if ball is attached or not. </param>
        /// <returns>If ball is attached to a champion or not.</returns>
        public bool SetStateAttached(bool attached)
        {
            return this.IsAttached = attached;
        }

        /// <summary>
        /// Set the ball render state.
        /// </summary>
        /// <param name="IsRendered">Bool for if the ball should be rendered or not.</param>
        /// <returns>If ball is rendered or not.</returns>
        public bool SetStateRendered(bool IsRendered)
        {
            if (IsRendered)
            {
                SetStatus(_oriannaBall, StatusFlags.NoRender, false);
            }
            else
            {
                SetStatus(_oriannaBall, StatusFlags.NoRender, true);
            }

            this.IsRendered = IsRendered;
            return this.IsRendered;
        }

        #endregion



        #region General Getter Methods

        /// <summary>
        /// Returns the current champion the ball is attached.
        /// </summary>
        /// <returns>Champion the Ball is Attached to.</returns>
        public IChampion GetAttachedChampion()
        {
            return this.AttachedChampion;
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
            this.AttachedChampion = champion;
            return this.AttachedChampion;
        }

        #endregion



        #region Helper-Methods

        /// <summary>
        /// Spawns a new instance of Orianna's Ball if OriannaBall is not null
        /// </summary>
        /// <param name="position">Position to spawn ball at.</param>
        /// <param name="renderState">Sets if the ball should be rendered on spawn.</param>
        /// <returns>New instance of a Orianna Ball.</returns>
        public IMinion SpawnOriannaBall(Vector2 position, bool renderState = false)
        {
            //var spellPos = new Vector2(position.X, position.Y);
            //OriannaBall = AddMinion(_owner, "OriannaBall", "OriannaBall" + _owner.Team, position, _owner.Team, _owner.SkinID, true, false,SpellDataFlags.NonTargetableAll,visibilityOwner: null,true);
            _oriannaBall = AddMinion(_orianna, "OriannaBall", "OriannaBall" + _orianna.Team, position, _orianna.Team, _orianna.SkinID, true, false);
            _oriannaBall.FaceDirection(new Vector3(position.X, 0, position.Y));
            _ballBuff = AddBuff("OriannaBall", 2300.0f, 1, _spell, _oriannaBall, _orianna).BuffScript as OriannaBall;
            SetStateRendered(renderState);
            SetStateFlying(false);

            return _oriannaBall;
        }

        /// <summary>
        /// Disables the Ball state and attaches it back to Orianna unless
        /// </summary>
        /// <param name="attachToChamp">If the ball should be attached to a new champ, defaults to Orianna if false</param>
        /// <param name="champion">Champion to attach ball to once it is disabled.</param>
        /// TODO: Possible better way to handle this rather than Active/Deactive locking other methods. Would prefer to not constantly destory and remake a ball instance if possible.
        /// TODO: THink of better name for this method.
        public void DisableOriannaBall(bool attachToChamp = false, IChampion champion = null)
        {
            SetStateRendered(false);
            SetStateAttached(true);
            SetStateGrounded(false);
            SetStateFlying(false);

            if (attachToChamp)
            {
                SetAttachedChampion(champion);
            }
            else
            {
                SetAttachedChampion(_orianna as IChampion);
            }

            //theBall.TakeDamage(theBall.Owner, 10000f, DamageType.DAMAGE_TYPE_TRUE, DamageSource.DAMAGE_SOURCE_INTERNALRAW, DamageResultType.RESULT_NORMAL);
        }

        /// <summary>
        /// Destorys the current instance of Orianna Ball.
        /// </summary>
        public void DestoryOriannaBall()
        {
            SetStatus(_oriannaBall, StatusFlags.NoRender, true);
            SetStateRendered(false);
            SetStateAttached(false);
            SetStateGrounded(false);
            SetStateFlying(false);
            _oriannaBall.TakeDamage(_orianna, 10000f, DamageType.DAMAGE_TYPE_TRUE, DamageSource.DAMAGE_SOURCE_INTERNALRAW, DamageResultType.RESULT_NORMAL);
            _oriannaBall = null;
        }

        /// <summary>
        /// Teleports current instance of Orianna ball to the new position.
        /// </summary>
        /// <param name="newPosition">New position to move Orianna Ball to.</param>
        /// <param name="activateBall">Usually enabled for Q. Sets Ball to active configuration.</param>
        /// <returns>The new position of the ball.</returns>
        public Vector2 TeleportOriannaBall(Vector2 newPosition, bool activateBall = false)
        {
            _oriannaBall.TeleportTo(newPosition.X, newPosition.Y);

            if (activateBall)
            {
                SetStateRendered(true);
                SetStateGrounded(true);
                SetAttachedChampion(null);
                SetStateAttached(false);
            }

            return newPosition;
        }

        /// <summary>
        /// Places the ball at currently attached targets location. Used for on AttachedChampions death or if they become untargetable.
        /// </summary>
        /// <param name="newPosition">New position to place Orianna Ball at.</param>
        /// <param name="activateBall">Usually enabled for Q. Sets Ball to active configuration.</param>
        /// <returns>The new position of the ball.</returns>
        public Vector2 DropOriannaBall()
        {
            _oriannaBall.TeleportTo(GetAttachedChampion().Position.X, GetAttachedChampion().Position.Y);
            var droppedPosition = new Vector2(GetAttachedChampion().Position.X, GetAttachedChampion().Position.Y);

            SetStateRendered(true);
            SetStateGrounded(true);
            SetAttachedChampion(null);
            SetStateAttached(false);

            return droppedPosition;
        }

        /// <summary>
        /// Casts the spell OrianaReturn.
        /// </summary>
        public void ReturnOriannaBall()
        {
            SpellCast(_orianna, 4, SpellSlotType.ExtraSlots, false, _oriannaBall, Vector2.Zero);
        }

        /// <summary>
        /// Returns the current instance of a Orianna Ball.
        /// </summary>
        /// <returns>Current instance of Orianna Ball</returns>
        public IMinion GetOriannaBall()
        {
            return _oriannaBall;
        }

        /// <summary>
        /// Perform a cast of the given spell using OriannaBall, (Applies to Q, W, E, R)
        /// </summary>
        /// <param name="spell">Spell which triggered this ball will cast.</param>
        /// TODO: Test this Q,W,E,R
        public void BallCast(ISpell spell)
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
