using System;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Scripting.CSharp;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.API;
using GameServerCore.Domain.GameObjects.Spell.Sector;

using System.Collections.Generic;
using GameServerCore.Domain;

namespace Spells
{
    public class OrianaIzunaCommand : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata => new SpellScriptMetadata()
        {
            TriggersSpellCasts = true,
            
        };

        IObjAiBase _owner;
        IBuff BallHandlerBuff;
        IMinion oriannaBall = null;
        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
            _owner = owner;
            
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnSpellPreCast(IObjAiBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
            BallHandlerBuff = owner.GetBuffWithName("OriannaBallHandler");
        }
        
        public void OnSpellCast(ISpell spell)
        {
            var targetPosition = new Vector2(spell.CastInfo.TargetPosition.X, spell.CastInfo.TargetPosition.Z);

            oriannaBall = (BallHandlerBuff.BuffScript as Buffs.OriannaBallHandler).GetBall();

            if (oriannaBall != null)
            {
                SpellCast(spell.CastInfo.Owner, 0, SpellSlotType.ExtraSlots, targetPosition, targetPosition, false, oriannaBall.Position);
            }
            else
            {
                SpellCast(spell.CastInfo.Owner, 0, SpellSlotType.ExtraSlots, targetPosition, targetPosition, false, _owner.Position); 
            }

            /*
            if (HasBall || _owner.Model == "Orianna")
            {
                _owner.ChangeModel("OriannaNoBall");
                HasBall = false;
            }
            */

            _owner.PlayAnimation("Spell1", 1f, 0, 0);
        }

        public void OnSpellPostCast(ISpell spell)
        {
            //var manaCost = new[] { 30, 35, 40, 45, 50 }[spell.CastInfo.SpellLevel - 1];
            //_owner.Stats.CurrentMana -= manaCost;
            //var coolDown = new[] { 6f, 5.25f, 4.5f, 3.75f, 3f }[spell.CastInfo.SpellLevel - 1];
            //spell.SetCooldown(coolDown);
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

        IObjAiBase _owner;
        IBuff BallHandlerBuff = null;
        IMinion oriannaBall = null;
        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
            _owner = owner;
            ApiEventManager.OnSpellHit.AddListener(this, spell, TargetExecute, false);
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }


        Vector2 targetPosition;
        public void OnSpellPreCast(IObjAiBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
            BallHandlerBuff = owner.GetBuffWithName("OriannaBallHandler");

            oriannaBall = (BallHandlerBuff.BuffScript as Buffs.OriannaBallHandler).GetBall();

            targetPosition = end;

            if(oriannaBall != null)
            {
                (BallHandlerBuff.BuffScript as Buffs.OriannaBallHandler).ToggleBallRender(false); 
            }

            var missile = spell.CreateSpellMissile(new MissileParameters
            {
                Type = MissileType.Circle,
                OverrideEndPosition = end,
            });

            ApiEventManager.OnSpellMissileEnd.AddListener(this, missile, OnMissileFinish, true);


            AbilityStateDisableCheck();
        }

        bool acivateQ;
        bool acivateE;
        private void AbilityStateDisableCheck()
        {
            //Check Q
            if (_owner.GetSpell(0).CurrentCooldown <= 0)
            {
                _owner.SetSpell("OrianaIzunaCommand", 0, false);
                acivateQ = true;
            }
            //Check E
            if (_owner.GetSpell(2).CurrentCooldown <= 0)
            {
                _owner.SetSpell("OrianaRedactCommand", 2, false);
                acivateE = true;
            }
        }

        public void OnMissileFinish(ISpellMissile missile) 
        {
            if (oriannaBall == null)
            {
                oriannaBall = (BallHandlerBuff.BuffScript as Buffs.OriannaBallHandler).SpawnBall(targetPosition);
            }
            else
            {

                (BallHandlerBuff.BuffScript as Buffs.OriannaBallHandler).MoveBall(targetPosition);
                (BallHandlerBuff.BuffScript as Buffs.OriannaBallHandler).ToggleBallRender(true);
            }

            EnableAbilities();
        }

        private void EnableAbilities() 
        {
            //Check Q
            if (acivateQ)
            {
                _owner.SetSpell("OrianaIzunaCommand", 0, true);
            }
            //Check E
            if (acivateE)
            {
                _owner.SetSpell("OrianaRedactCommand", 2, true);
            }
        }

        public void OnSpellCast(ISpell spell)
        {
        }

        public void TargetExecute(ISpell spell, IAttackableUnit target, ISpellMissile missile, ISpellSector sector)
        {
            if (missile is ISpellCircleMissile skillshot)
            {
                var owner = spell.CastInfo.Owner;
                var spellLevel = spell.CastInfo.SpellLevel - 1;
                var baseDamage = new[] { 60, 90, 120, 150, 180 }[spellLevel];
                var magicDamage = owner.Stats.AbilityPower.Total * .5f;
                var damage = baseDamage + magicDamage;
                target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
            }
        }


        public void OnSpellPostCast(ISpell spell)
        {
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

