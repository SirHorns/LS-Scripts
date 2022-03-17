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
            BuffType = BuffType.COMBAT_ENCHANCER,
            BuffAddType = BuffAddType.REPLACE_EXISTING,
            MaxStacks = 1
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier ();

        private IObjAiBase _owner;
        private ISpell ThisSpell;
        private IBuff ThisBuff;
        private IMinion theBall;
        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            _owner = (IObjAiBase) unit;
            ThisSpell = ownerSpell;
            ThisBuff = buff;
            theBall = null;
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
        }

        public void ToggleBallRender(bool toggle)
        {
            if(!toggle)
            {
                SetStatus(theBall, StatusFlags.NoRender, true);
            }
            else
            {
                SetStatus(theBall, StatusFlags.NoRender, false);
            }
        }

        public IMinion SpawnBall(Vector2 position)
        {
            var spellPos = new Vector2(position.X, position.Y);

            theBall = AddMinion(_owner, "OriannaBall", "Ball", spellPos, _owner.Team, _owner.SkinID, true, false);

            //AddBuff("OriannaBall", 2300.0f, 1, ThisSpell, theBall, _owner);

            //var goingTo = new Vector2(ThisSpell.CastInfo.TargetPositionEnd.X, ThisSpell.CastInfo.TargetPositionEnd.Z) - _owner.Position;
            //var dirTemp = Vector2.Normalize(goingTo);
            //theBall.FaceDirection(new Vector3(dirTemp.X, 0, dirTemp.Y));

            return theBall;
        }

        public void MoveBall(Vector2 newPosition)
        {
            theBall.TeleportTo(newPosition.X, newPosition.Y);
        }

        public IMinion GetBall()
        {
            return theBall;
        }

        public void RemoveBall()
        {
            ToggleBallRender(false);
            theBall.TakeDamage(theBall.Owner, 10000f, DamageType.DAMAGE_TYPE_TRUE, DamageSource.DAMAGE_SOURCE_INTERNALRAW, DamageResultType.RESULT_NORMAL);
            theBall = null;
        }

        /// <summary>
        /// Perform a cast of the given spell using OriannaBall, (Applies to Q, W, E, R)
        /// </summary>
        /// <param name="spell">Spell which triggered this ball will cast.</param>
        /// TODO: Test this Q,W,E,R
        public void BallCast(ISpell spell)
        {
            var slot = spell.CastInfo.SpellSlot;
            if (slot != 0 || slot != 2 && theBall != null)
            {
                return;
            }
            FaceDirection(new Vector2(spell.CastInfo.TargetPositionEnd.X, spell.CastInfo.TargetPositionEnd.Z), theBall, true);
            SpellCast(spell.CastInfo.Owner, slot, SpellSlotType.SpellSlots, true, spell.CastInfo.Targets[0].Unit, theBall.Position);
        }

        public void OnUpdate(float diff)
        {
        }
    }
}
