 using GameServerCore.Domain.GameObjects; 
 using GameServerCore.Domain.GameObjects.Spell; 
 using GameServerCore.Domain.GameObjects.Spell.Missile; 
 using static LeagueSandbox.GameServer.API.ApiFunctionManager; 
 using LeagueSandbox.GameServer.Scripting.CSharp; 
 using System.Numerics; 
 using GameServerCore.Scripting.CSharp; 
 using LeagueSandbox.GameServer.GameObjects.Stats; 
 using LeagueSandbox.GameServer.API; 
 using GameServerCore.Domain;  
 namespace ItemPassives {     
    public class ItemID_1502 : IItemScript {         
      public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();          
      public void OnActivate(IObjAiBase owner){}          
      public void OnDeactivate(IObjAiBase owner){}          
      public void OnUpdate(float diff){}       
} 
 }