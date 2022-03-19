using LeagueSandbox.GameServer.API;
using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Scripting.CSharp;
using GameServerCore.Domain;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using System;
using GameServerCore.Enums;


namespace CharScripts
{
    public class CharScriptOrianna : ICharScript
    {
        IObjAiBase _owner;
        IMinion oriannaBall;
        private IAttackableUnit passiveTarget = null;
        private IAttackableUnit currentTarget = null;
        ISpell _spell;
        Buffs.OriannaBallHandler BallHandler;
        public void OnActivate(IObjAiBase owner, ISpell spell = null)
        {
            _owner = owner;
            _spell = spell;
            
            AddBuff("ClockworkWinding", 1f, 1, spell, owner, owner, true);

            BallHandler = (AddBuff("OriannaBallHandler", 1.0f, 1, spell, owner, owner, true).BuffScript as Buffs.OriannaBallHandler);
            BallHandler.SetAttachedChampion((IChampion)owner);

            ApiEventManager.OnDeath.AddListener(owner, owner, OnDeath, false);
            ApiEventManager.OnHitUnit.AddListener(this, _owner, TargetExecute, false);
        }

        private void TargetExecute(IDamageData data)
        {
            currentTarget = data.Target;

            if (passiveTarget == currentTarget)
            {
                AddBuff("OrianaPowerDagger", 4f, 1, _spell, _owner, _owner);
            }
            else
            {
                _owner.RemoveBuffsWithName("OrianaPowerDagger");
                passiveTarget = currentTarget;
            }
        }

        private void OnDeath(IDeathData death)
        {
        }
        public void OnDeactivate(IObjAiBase owner, ISpell spell = null)
        {
        }

        public void OnUpdate(float diff)
        {
            if(BallHandler.GetBall() == null)
            {
               BallHandler.SpawnBall(_owner.Position);
            }
        }
    }

    public class CharScriptOriannaNoBall : ICharScript
    {
        IObjAiBase _owner;
        IMinion oriannaBall;
        public void OnActivate(IObjAiBase owner, ISpell spell = null)
        {
            _owner = owner;
            ApiEventManager.OnDeath.AddListener(owner,owner, OnDeath, false);
            AddBuff("TheBall", 1f, 1, spell, owner, owner);
            AddBuff("ClockworkWinding", 1f, 1, spell, owner, owner, true);
        }

        private void OnDeath(IDeathData death)
        {
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell = null)
        {
        }
        public void OnUpdate(float diff)
        {

            foreach (var unit in GetUnitsInRange(_owner.Position, 1290f, true))
            {
                if (unit.HasBuff("OriannaBall") && unit.Team == _owner.Team)
                {
                    oriannaBall = (IMinion)unit;
                    oriannaBall.Owner.GetBuffWithName("TheBall").Update(diff);
                    //SetStatus(oriannaBall, StatusFlags.NoRender, true);
                    //oriannaBall.TakeDamage(oriannaBall.Owner, 10000f, DamageType.DAMAGE_TYPE_TRUE, DamageSource.DAMAGE_SOURCE_INTERNALRAW, DamageResultType.RESULT_NORMAL);
                }
            }

        }
    }
}

