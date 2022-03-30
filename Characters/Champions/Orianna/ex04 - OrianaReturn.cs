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
    public class OrianaReturn : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true,
            IsDamagingSpell = false
        };

        IObjAiBase _orianna;
        IMinion _oriannaBall;
        ISpell _spell;

        Buffs.OriannaBallHandler _ballHandler;
        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
            _orianna = owner;
            _spell = spell;
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnSpellPreCast(IObjAiBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
            _ballHandler = _orianna.GetBuffWithName("OriannaBallHandler").BuffScript as Buffs.OriannaBallHandler;
            _oriannaBall = _ballHandler.GetBall();

            if (_ballHandler.GetStateAttached())
            {
                _ballHandler.RemoveEBuff();
            }

            _ballHandler.DisableBall();
            _ballHandler.AttachToChampion(_orianna as IChampion);

            if (_orianna.GetSpell(2).CastInfo.SpellLevel > 0)
            {
                AddBuff("OrianaGhostSelf", 1f, 1, _orianna.GetSpell(2), _orianna, _orianna, true);
            }

            if (_orianna.Model == "OriannaNoBall")
            {
                _orianna.ChangeModel("Orianna");
            }
        }

        public void OnSpellCast(ISpell spell)
        { 
        }

        public void OnSpellPostCast(ISpell spell)
        {
            AddParticlePos(_orianna, "Orianna_Ball_Flash", _oriannaBall.Position, _oriannaBall.Position);

            //Should only trigger if leash range is broken.
            _orianna.GetSpell(0).SetCooldown(.5f);
            _orianna.GetSpell(1).SetCooldown(.5f);
            _orianna.GetSpell(2).SetCooldown(.5f);
            _orianna.GetSpell(3).SetCooldown(.5f);
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
