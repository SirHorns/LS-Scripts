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
    class OriannaBall : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER,
            BuffAddType = BuffAddType.REPLACE_EXISTING,
            MaxStacks = 1
        };

        IObjAiBase _owner;
        IBuff ThisBuff;
        IMinion Ball;
        IParticle currentIndicator;
        int previousIndicatorState;
        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier ();

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            
            ThisBuff = buff;
            Ball = unit as IMinion;
            _owner = Ball.Owner;

            buff.SetStatusEffect(StatusFlags.Targetable, false);
            buff.SetStatusEffect(StatusFlags.Ghosted, true);

            AddParticleTarget(Ball.Owner, Ball, "zed_base_w_tar", Ball);

            currentIndicator = AddParticleTarget(Ball.Owner, Ball.Owner, "OrianaBallIndicatorFar", Ball, 5f, flags: FXFlags.TargetDirection);
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            if (Ball != null && !Ball.IsDead)
            {
                if (currentIndicator != null)
                {
                    currentIndicator.SetToRemove();
                }

                SetStatus(Ball, StatusFlags.NoRender, true);
                AddParticle(Ball.Owner, null, "zed_base_clonedeath", Ball.Position);
                //Ball.TakeDamage(Ball.Owner, 10000f, DamageType.DAMAGE_TYPE_TRUE, DamageSource.DAMAGE_SOURCE_INTERNALRAW, DamageResultType.RESULT_NORMAL);
            }
        }

        public int GetIndicatorState()
        {
            var dist = Vector2.Distance(Ball.Owner.Position, Ball.Position);
            var state = 0;

            if (!Ball.Owner.HasBuff("TheBall"))
            {
                return state;
            }

            if (dist >= 1290.0f)
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

        public void OnUpdate(float diff)
        {
            if (Ball != null && !Ball.IsDead)
            {
                int state = GetIndicatorState();

                if (state == 0) 
                {
                    SpellCast(_owner,3,SpellSlotType.ExtraSlots,true,Ball,Ball.Position);
                }
                else 
                {
                    if (state != previousIndicatorState)
                    {
                        previousIndicatorState = state;
                        if (currentIndicator != null)
                        {
                            currentIndicator.SetToRemove();
                        }

                        currentIndicator = AddParticleTarget(Ball.Owner, Ball.Owner, GetIndicatorName(state), Ball, ThisBuff.Duration - ThisBuff.TimeElapsed, flags: FXFlags.TargetDirection);
                    }
                }
                
            }
        }
    }
}
