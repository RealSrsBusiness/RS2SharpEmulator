public class Trading : Content
{
	int tradeWindow = -1;
	int secondWindow = -1;
	int tradeStateId; //0 = window open 1-4 = player accepted
	int acceptButton = -1;
	
	public override void Load()
	{
		tradeStateId = AddState(typeof(TradeStatus));
		
		Players["Trade with"] = (Client c, Client trg) => 
		{
			c.SendMessage("Sending trade request...");
			trg.DisplayRequest($"{c.Player.Name} wants to trade with you.", () =>
			{
				trg.Player.WalkTo(c.Player, (bool sucess) => 
				{
					if(!sucess)
						c.SendMessage("Can't reach player.")
					else
					{
						foreach(var cl in new Client[]{c, trg}) //basic setup
						{
							cl.Interface = tradeWindow;
							cl.TradeState.Window = 0;
							cl.TradeState.valid = true;
							cl.OnInterrupt((Client c) => //invalidate trade state if player moves or exits
							{
								c.TradeState.valid = false;
							});
						}
						
						c.TradeState.player = trg;
						trg.TradeState.player = c;
						
						c.Inventory.OnAction[FIRST] = (Client c, int item) => //adding items to trade window
						{
							c.TradeState.items.Add(item);
							c.Interface.SendItem(item);
							c.TradeState.player.Interface.SendItem(item);
						}
					}
					
				});
			});
			
			Interface[tradeWindow].Subitem(acceptButton) = (Client c)
			{
				if(c.TradeState.valid)
				{
					c.TradeState.window++;
					if(c.TradeState.player.TradeState.window == 1) //other player has accepted
					{
						c.Interface = secondWindow;
						c.TradeState.player.Interface = secondWindow;
					}
				}
			}
			
			Interface[tradeWindow].Subitem(acceptButton) = (Client c)
			{
				if(c.TradeState.valid && c.TradeState == 1)
				{
					c.TradeState.window++;
					if(c.TradeState.player.TradeState.window == 2) //other player has accepted
					{
						//switch items
						c.Inventory.AddRange(c.TradeState.player.TradeState.items);
						c.TradeState.player.Inventory.AddRange(c.TradeState.items);
					}
				}
			}
		};
	}
	
	class TradeStatus
	{
		bool valid;
		int window;
		Client player;
		List<ItemStack> items = new List<ItemStack>();
	}
	
	
}