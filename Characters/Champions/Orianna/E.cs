using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Domain.GameObjects.Spell.Sector;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using System;

namespace Spells
{
    public class OrianaRedactCommand : ISpellScript
    {

        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true,
            IsDamagingSpell = true,
            NotSingleTargetSpell = true,
        };

        IObjAiBase _owner;
        Buffs.OriannaBallHandler BallHandler;
        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
            _owner = owner;
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        { 
        }

        public void OnSpellPreCast(IObjAiBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
            BallHandler = (owner.GetBuffWithName("OriannaBallHandler").BuffScript as Buffs.OriannaBallHandler);
            IChampion attachedChamp = BallHandler.GetAttachedChampion();

            if (BallHandler.GetIsAttachedtoChampion())
            {
                if (attachedChamp == (IChampion) owner && (IChampion) target == (IChampion) owner)
                {
                    AddBuff("OrianaRedactShield", 2.5f, 1, spell, owner, owner);
                }
                else 
                { 
                    SpellCast(owner, 2, SpellSlotType.ExtraSlots, target.Position, target.Position, false, attachedChamp.Position, spell.CastInfo.Targets);

                    if (_owner.Model == "Orianna")
                    {
                        _owner.ChangeModel("OriannaNoBall");
                    }
                }
            }
            else
            {
                SpellCast(owner, 2, SpellSlotType.ExtraSlots, target.Position, target.Position, false, BallHandler.GetBall().Position, spell.CastInfo.Targets);

                if (_owner.Model == "Orianna")
                {
                    _owner.ChangeModel("OriannaNoBall");
                }
            }

            

            _owner.PlayAnimation("Spell2", 1f, 0, 0);
        }

        public void OnSpellCast(ISpell spell)
        {
        }

        public void OnSpellPostCast(ISpell spell)
        {
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, ISpellMissile missile)
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

    public class OrianaRedact : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true,
            NotSingleTargetSpell = true,
            IsDamagingSpell = true,
        };

        IObjAiBase _owner;
        IChampion _target;
        ISpell _spell;
        Buffs.OriannaBallHandler BallHandler;
        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
            _owner = owner;
            _spell = spell;
            BallHandler = (owner.GetBuffWithName("OriannaBallHandler").BuffScript as Buffs.OriannaBallHandler);
            ApiEventManager.OnSpellHit.AddListener(this, spell, TargetExecute, false);
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnSpellPreCast(IObjAiBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
            _target = (IChampion)spell.CastInfo.Targets[0].Unit;

            BallHandler.RemoveBall();

            if (BallHandler.GetAttachedChampion() != null)
            {
                BallHandler.GetAttachedChampion().RemoveBuffsWithName("TheBall");
            }

            var missile = spell.CreateSpellMissile(new MissileParameters
            {
                Type = MissileType.Circle,
                OverrideEndPosition = spell.CastInfo.Targets[0].Unit.Position,
            });

            ApiEventManager.OnSpellMissileEnd.AddListener(this, missile, OnMissileFinish, true);

            AbilityStateDisableCheck();
        }

        bool acivateQ;
        bool acivateW;
        bool acivateE;
        bool acivateR;
        private void AbilityStateDisableCheck()
        {
            //Check Q
            if (_owner.GetSpell(0).CurrentCooldown <= 0)
            {
                _owner.SetSpell("OrianaIzunaCommand", 0, false);
                acivateQ = true;
            }
            //Check @
            if (_owner.GetSpell(1).CurrentCooldown <= 0)
            {
                _owner.SetSpell("OrianaDissonanceCommand", 1, false);
                acivateW = true;
            }
            //Check E
            if (_owner.GetSpell(2).CurrentCooldown <= 0)
            {
                _owner.SetSpell("OrianaRedactCommand", 2, false);
                acivateE = true;
            }
            //Check R
            if (_owner.GetSpell(3).CurrentCooldown <= 0)
            {
                _owner.SetSpell("OrianaDetonateCommand", 3, false);
                acivateR = true;
            }
        }

        public void OnMissileFinish(ISpellMissile missile)
        {
            AddBuff("OrianaRedactShield", 2.5f, 1, _spell, _target, _owner);
            AddBuff("TheBall", 1f, 1, _spell, _target, _owner, true);

            if(_target.NetId == _owner.NetId)
            {
                if (_owner.Model == "OriannaNoBall")
                {
                    _owner.ChangeModel("Orianna");
                }
            }

            BallHandler.SetAttachedChampion(_target);
            BallHandler.SetIsAttachedtoChampion(true);

            

            EnableAbilities();
        }

        private void EnableAbilities()
        {
            //Check Q
            if (acivateQ)
            {
                _owner.SetSpell("OrianaIzunaCommand", 0, true);
                acivateQ = false;
            }
            //Check W
            if (acivateW)
            {
                _owner.SetSpell("OrianaDissonanceCommand", 1, true);
                acivateW = false;
            }
            //Check E
            if (acivateE)
            {
                _owner.SetSpell("OrianaRedactCommand", 2, true);
                acivateE = false;
            }
            //Check R
            if (acivateR)
            {
                _owner.SetSpell("OrianaDetonateCommand", 3, true);
                acivateR = false;
            }
        }

        public void OnSpellCast(ISpell spell)
        {
        }

        public void OnSpellPostCast(ISpell spell)
        {
        }

        public void TargetExecute(ISpell spell, IAttackableUnit target, ISpellMissile missile, ISpellSector sector)
        {
            if(target.Team != _owner.Team)
            {
                if (missile is ISpellCircleMissile skillshot)
                {
                    var owner = spell.CastInfo.Owner;
                    var spellLevel = spell.CastInfo.SpellLevel - 1;
                    var baseDamage = new[] { 60, 90, 120, 150, 180 }[spellLevel];
                    var magicDamage = owner.Stats.AbilityPower.Total * .3f;
                    var damage = baseDamage + magicDamage;
                    target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
                }
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
