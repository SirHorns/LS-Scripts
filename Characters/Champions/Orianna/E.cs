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

            oriannaBall = (BallHandlerBuff.BuffScript as Buffs.OriannaBallHandler).GetBall();

            if (oriannaBall == null)
            {
                SpellCast(spell.CastInfo.Owner, 2, SpellSlotType.ExtraSlots, target.Position, target.Position, false, _owner.Position);
            }
            else
            {
                SpellCast(spell.CastInfo.Owner, 2, SpellSlotType.ExtraSlots, target.Position, target.Position, false, oriannaBall.Position);
            }
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
        IBuff BallHandler;
        IMinion oriannaBall;
        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
            _owner = owner;
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnSpellPreCast(IObjAiBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
            BallHandler = owner.GetBuffWithName("OriannaBallHandler");

            oriannaBall = (BallHandler.BuffScript as Buffs.OriannaBallHandler).GetBall();

            var missile = spell.CreateSpellMissile(new MissileParameters
            {
                Type = MissileType.Circle,
                OverrideEndPosition = end,
            });

            ApiEventManager.OnSpellMissileEnd.AddListener(this, missile, OnMissileFinish, true);
        }

        public void OnMissileFinish(ISpellMissile missile)
        {
            (BallHandler.BuffScript as Buffs.OriannaBallHandler).RemoveBall();
        }

        public void OnSpellCast(ISpell spell)
        {
            Console.WriteLine("SpellCast");
        }

        public void OnSpellPostCast(ISpell spell)
        {
            Console.WriteLine("SpelPostlCast");
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

    public class OrianaRedactShield : ISpellScript
    {

        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
        };

        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void TargetExecute(ISpell spell, IAttackableUnit target, ISpellMissile missile, ISpellSector sector)
        {
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
}
