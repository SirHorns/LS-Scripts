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
        private ISpell ThisSpell;
        private IBuff ThisBuff;

        private IMinion OriannaBall = null;
        private bool IsBallActive = false;
        private bool IsBallRendered = false;


        private IChampion AttachedChampion;
        private bool IsAttachedtoChampion = true;

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            _owner = (IObjAiBase) unit;
            ThisSpell = ownerSpell;
            ThisBuff = buff;
            AttachedChampion = (IChampion) unit;
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
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
        /// <returns>Champion the ball will be attached to</returns>
        public IChampion SetAttachedChampion(IChampion chamption)
        {
            AttachedChampion = chamption;
            return AttachedChampion;
        }

        /// <summary>
        /// Returns if ball is attached to champ or not
        /// </summary>
        /// <returns>If the ball is attached to a champ or not. Bool</returns>
        public bool GetIsAttachedtoChampion()
        {
            return IsAttachedtoChampion;
        }

        /// <summary>
        /// Sets a bool for if ball is attached to a champ or not
        /// </summary>
        /// <param name="attched">Bool if ball is attached or not. </param>
        public bool SetIsAttachedtoChampion(bool attched)
        {
            return IsAttachedtoChampion = attched;
        }

        /// <summary>
        /// Returns the current Ball state
        /// </summary>
        /// <returns>Orianna Ball state as a bool</returns>
        public bool GetIsBallActive()
        {
            return IsBallActive;
        }


        /// <summary>
        /// Toggles if the Ball is in a Active State or not depending on its current state.
        /// </summary>
        public void ToggleBallState()
        {
            if(IsBallActive)
            {
                IsBallActive = false;
            }
            else
            {
                IsBallActive = true;
            }
        }

        /// <summary>
        /// Sets the Ball's state based on parem input.
        /// </summary>
        /// <param name="IsBallActive">Should the ball be active or disabled</param>
        public void SetBallState(bool IsBallActive)
        {
            this.IsBallActive = IsBallActive;
        }

        /// <summary>
        /// Set if the ball should be rendered or not.
        /// </summary>
        /// <param name="IsBallRendered">Should the ball be rendered or not.</param>
        public void SetBallRender(bool IsBallRendered)
        {
            if(IsBallRendered)
            {
                IsBallActive = true;
                this.IsBallRendered = true;
                SetStatus(OriannaBall, StatusFlags.NoRender, false);
            }
            else
            {
                IsBallActive = false;
                this.IsBallRendered = false;
                SetStatus(OriannaBall, StatusFlags.NoRender, true);
            }
        }

        /// <summary>
        /// Spawns a new instance of Orianna's Ball if OriannaBall is not null
        /// </summary>
        /// <param name="position">The spot to spawn a new Orianna Ball instance</param>
        /// <returns>New Orianna Ball Instance, will return the current Instance of one already exists.</returns>
        public IMinion SpawnBall(Vector2 position)
        {
            if(OriannaBall == null)
            {
                //var spellPos = new Vector2(position.X, position.Y);

                //OriannaBall = AddMinion(_owner, "OriannaBall", "OriannaBall" + _owner.Team, position, _owner.Team, _owner.SkinID, true, false,SpellDataFlags.NonTargetableAll,visibilityOwner: null,true);
                OriannaBall = AddMinion(_owner, "OriannaBall", "OriannaBall" + _owner.Team, position, _owner.Team, _owner.SkinID, true, false);

                SetBallRender(true);
                SetBallState(true);

                //AddBuff("OriannaBall", 2300.0f, 1, ThisSpell, OriannaBall, _owner);

                //var goingTo = new Vector2(ThisSpell.CastInfo.TargetPositionEnd.X, ThisSpell.CastInfo.TargetPositionEnd.Z) - _owner.Position;
                //var dirTemp = Vector2.Normalize(goingTo);
                //theBall.FaceDirection(new Vector3(dirTemp.X, 0, dirTemp.Y));
            }

            return OriannaBall;
        }

        /// <summary>
        /// Teleports current Orianna Ball instance to the new position
        /// </summary>
        /// <param name="newPosition">New position to move Orianna Ball to.</param>
        public void MoveBall(Vector2 newPosition)
        {
            if (OriannaBall == null) 
            {
                SpawnBall(newPosition);
            }
            else 
            {
                OriannaBall.TeleportTo(newPosition.X, newPosition.Y);
                SetBallRender(true);
                SetBallState(true);
            }
        }

        /// <summary>
        /// Returns the current Orianna Ball instance or null if the Ball state is not active.
        /// </summary>
        /// <returns>Current Orianna Ball Instance or Null</returns>
        public IMinion GetBall()
        {
            return OriannaBall;
        }

        /// <summary>
        /// Disables the current Orianna Ball instances render and sets Ball state to false.
        /// </summary>
        /// TODO: Possible better way to handle this rather than Active/Deactive locking other methods. Would prefer to not constantly destory and remake a ball instance if possible.
        public void RemoveBall()
        {
            SetBallRender(false);
            SetBallState(false);
            
            //theBall.TakeDamage(theBall.Owner, 10000f, DamageType.DAMAGE_TYPE_TRUE, DamageSource.DAMAGE_SOURCE_INTERNALRAW, DamageResultType.RESULT_NORMAL);
        }

        public void ReturnBall() 
        {
            SpellCast(_owner, 4, SpellSlotType.ExtraSlots, false, OriannaBall, Vector2.Zero);
        }

        /// <summary>
        /// Perform a cast of the given spell using OriannaBall, (Applies to Q, W, E, R)
        /// </summary>
        /// <param name="spell">Spell which triggered this ball will cast.</param>
        /// TODO: Test this Q,W,E,R
        public void BallCast(ISpell spell)
        {
            var slot = spell.CastInfo.SpellSlot;
            if (slot != 0 || slot != 2 && OriannaBall != null)
            {
                return;
            }
            FaceDirection(new Vector2(spell.CastInfo.TargetPositionEnd.X, spell.CastInfo.TargetPositionEnd.Z), OriannaBall, true);
            SpellCast(spell.CastInfo.Owner, slot, SpellSlotType.SpellSlots, true, spell.CastInfo.Targets[0].Unit, OriannaBall.Position);
        }

        public void OnUpdate(float diff)
        {
            if (GetUnitsInRange(_owner.Position, 135f, true).Contains(OriannaBall))
            {
                if(IsBallActive)
                {
                    ReturnBall();
                }
            }
        }
    }
}
