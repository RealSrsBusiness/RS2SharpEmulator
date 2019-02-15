

public class Woodcutting : Content
{
	const int CUT_ANIM = -1;
	const int AXE = -1, AXE_LEVEL = 1;
	
	public override void Load()
	{
		foreach(var tree in Tree.Get())
		{
			foreach(int id in tree.objectIds)
			{
				Objects[id].OnAction[FIRST] = (Client c, WorldEntity we) =>
				{
					if(c.Skills.Level[Skills.Woodcutting] < tree.id)
					{
						c.SendMessage($"You need a woodcutting level of {tree.level} to cut down this Tree.");
						return;
					}
					
					if(!c.Inventory.Contains(AXE) || !c.Equipment.Contains(AXE) || c.Skills.Level[Skills.Woodcutting] < AXE_LEVEL)
					{
						c.SendMessage($"You don't have a suitable axe on you.");
						return;
					}
					
					c.ClearOn(Interrupt.ALL);
					c.Player.Animate(CUT_ANIM);
					c.Schedule((Client c) =>
					{
						if(c.Inventory.Add(tree.logId))
							c.Skills.AddXP(Skills.Woodcutting, tree.xp);
						else
							c.SendMessage("Your inventory is too full to hold any more logs..");
					}, 
					Random().Next(2, 5)).Until(
						() => Random().Next(10, 25),
						(Client c) => {
							if(World.OvershadowObject(we, tree.treeStump))
								c.Schedule((Client c) => World.RemoveOvershadow(we));
						}
					);					
				}
			}
		}
	}
	
	class Tree
    {
        public int[] objectIds;
        public int logId;
        public int level;
        public int xp;
        public int treeStump;

        public Tree(int[] objectIds, int logId, int level = 1, int xp = 25, int treeStump = 2891)
        {
            this.objectIds = objectIds;
            this.logId = logId;
            this.level = level;
            this.xp = xp;
            this.treeStump = treeStump;
        }

        public static Tree[] Get()
        {
            return new[]
            {
                new Tree(new[]{ 1291, 1282, 1289, 1286, 2409, 1276, 1278, 1315, 1316 }, //regular
                    logId: 1511, level: 1),
                new Tree(new[]{ 2023 }, logId: 2862, level: 1), //achey
                new Tree(new[]{ 1281 }, logId: 1521, level: 15), //oak
                new Tree(new[]{ 1308 }, logId: 1519, level: 30), //willow
                new Tree(new[]{ 9036 }, logId: 6333, level: 35), //teak
                new Tree(new[]{ 1307 }, logId: 1517, level: 45), //maple
                new Tree(new[]{ 9034 }, logId: 6332, level: 50), //mahogony
                //new Tree(new[]{ -1 }, logId: 10810, level: 10, xp: 10), //artic pine
                new Tree(new[]{ 1309 }, logId: 1515, level: 60), //yew
                new Tree(new[]{ 1306 }, logId: 1513, level: 75), //magic
            };
        }
    }
	
}