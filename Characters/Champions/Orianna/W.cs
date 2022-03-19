using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.Domain.GameObjects.Spell.Sector;
using System;

namespace Spells
{
    public class OrianaDissonanceCommand : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true,
            IsDamagingSpell = true,
        };

        IObjAiBase _owner;
        IBuff BallHandlerBuff;
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
            BallHandlerBuff = owner.GetBuffWithName("OriannaBallHandler");
        }

        public void OnSpellCast(ISpell spell)
        {
            //AddParticleTarget(_owner, _owner, "OrianaDissonance_cas", _owner, 1.0f);
            //AddParticleTarget(_owner, _owner, "OrianaDissonance_tar", _owner, 1.0f); 
            oriannaBall = (BallHandlerBuff.BuffScript as Buffs.OriannaBallHandler).GetBall();


            if (oriannaBall == null)
            {
                AddParticlePos(_owner, "OrianaDissonance_cas_green", _owner.Position, _owner.Position, 3f);
            }
            else
            {
                
                AddParticlePos(_owner, "OrianaDissonance_cas_green", oriannaBall.Position, oriannaBall.Position, 3f);
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
