public class Npc : Content
{
	const int NpcId = -1;
	const int attackAnimation = -1;

	public override void Load()
	{
		Npcs[NpcId].OnUpdate = (Npc npc) =>
		{
			if(npcs.Target == null)
			{
				var entities = World.FindEnity(
					(WorldEntity we) => !we.UnderAttack && we.GetType() == typeof(Player), 
					Region.FromRectangle(1000, 1000, 2000, 2000), 10); //limit to 10 entities
					
				if(entities != null)
				{
					var entity = entities.Random();
					
					npc.Target = entity;
					npc.Follow = entity;
				}
			}
			else
			{
				if(npc.Ticks % 3 == 0) //attack every 3rd tick
				{
					npc.Animate(attackAnimation);
					npc.Target.InflictDamage(100, Hitmark.Red);
				}
			}
			
		}
	}
}