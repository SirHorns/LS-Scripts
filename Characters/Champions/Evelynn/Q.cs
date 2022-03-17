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

using log4net;
using LeagueSandbox.GameServer.Logging;
using System.Collections.Generic;

namespace Spells
{
    public class EvelynnQ : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            AutoFaceDirection = false,
            TriggersSpellCasts = true,
            IsDamagingSpell = true
        };

        private IObjAiBase _owner;
        private ISpell _spell;

        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
            _owner = owner;
            _spell = spell;

        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnSpellPreCast(IObjAiBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {

        }

        public void OnSpellCast(ISpell spell)
        {
            var owner = spell.CastInfo.Owner;
            var startPos = owner.Position;
            var target = GetClosestUnitInRange(owner, 500f, true);

            //var endPos = GetPointFromUnit(target, -500f);

            if (target != null)
            {
                if (!(target is IBaseTurret) && target.GetIsTargetableToTeam(owner.Team))
                {
                    SpellCast(owner, 3, SpellSlotType.ExtraSlots, owner.Position, target.Position, true, Vector2.Zero);
                }
            }

        }


        public void OnSpellPostCast(ISpell spell)
        {
            var manaCost = new[] { 12, 18, 24, 30, 36 }[spell.CastInfo.SpellLevel - 1];
            var coolDown = new[] { 1.5f, 1.5f, 1.5f, 1.5f, 1.5f }[spell.CastInfo.SpellLevel - 1];
            spell.SetCooldown(coolDown);
            _owner.Stats.CurrentMana -= manaCost;
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

        bool CanCastQ = false;
        public void OnUpdate(float diff)
        {
            var targets = GetUnitsInRange(_owner.Position, (_owner.GetSpell(0).SpellData.CastRangeDisplayOverride - 10), true);

            foreach (var target in targets)
            {
                if (_owner.Team != target.Team)
                {
                    if (!(target is IBaseTurret) && !(target is ILaneTurret) && !(target is IAzirTurret))
                    {
                        if (target.GetIsTargetableToTeam(_owner.Team))
                        {
                            if(!CanCastQ)
                            {
                                _owner.SetSpell("EvelynnQ", 0, true);
                                CanCastQ = true;
                            }
                            
                            break;
                        }
                    }
                }
                _owner.SetSpell("EvelynnQ", 0, false);
                CanCastQ = false;
            }
        }
    }


    public class HateSpikeLineMissile : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            MissileParameters = new MissileParameters{Type = MissileType.Circle},
            IsDamagingSpell = true,
            
        };

        //Vector2 direction;
        IObjAiBase _owner;
        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
            _owner = owner;
            ApiEventManager.OnSpellHit.AddListener(this, spell, TargetExecute, false);
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnSpellPreCast(IObjAiBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
        }

        public void OnSpellCast(ISpell spell)
        {
        }

        public void OnSpellPostCast(ISpell spell)
        {
            Console.WriteLine("DEBUG: EvelynnHateSpikeMissle - SpellPostCast");
            var owner = spell.CastInfo.Owner;
        }

        public void TargetExecute(ISpell spell, IAttackableUnit target, ISpellMissile missile, ISpellSector sector)
        {
            var owner = spell.CastInfo.Owner;

            //DAMAGE CALC
            var spellLevel = spell.CastInfo.SpellLevel - 1;

            var flat = new[] { 40, 50, 60, 70, 80 }[spellLevel];
            var ad = new[] { 
                    owner.Stats.AttackDamage.FlatBonus * .5f,
                    owner.Stats.AttackDamage.FlatBonus * .55f,
                    owner.Stats.AttackDamage.FlatBonus * .6f,
                    owner.Stats.AttackDamage.FlatBonus * .65f,
                    owner.Stats.AttackDamage.FlatBonus * .7f
                }[spellLevel];
            var ap = new[] {
                    owner.Stats.AbilityPower.Total * .35f,
                    owner.Stats.AbilityPower.Total * .4f,
                    owner.Stats.AbilityPower.Total * .45f,
                    owner.Stats.AbilityPower.Total * .5f,
                    owner.Stats.AbilityPower.Total * .55f
                }[spellLevel];
            var damage = flat + ad + ap;

            AddParticle(owner, target, "Evelynn_Q_tar", target.Position, bone: "BUFFBONE_GLB_GROUND_LOC", lifetime: 2.0f);
            target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
            //AddParticle(owner, target, "Evelynn_Q_mis", target.Position, bone: "BUFFBONE_GLB_GROUND_LOC", lifetime: 2.0f);
            
            //AddParticleTarget(owner, target, "Evelynn_HateSpike", target, 1.0f);
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

