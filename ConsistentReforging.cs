using Terraria;
using Terraria.ModLoader;

namespace ConsistentReforging
{
	//Mod could be clientside if not for the send/receive code for syncing the reforge history
	public class ConsistentReforging : Mod
	{
		public override void Load()
		{
			On.Terraria.Item.Prefix += HandlePrefixToItemIfItHasNoHistory;
		}

		private bool HandlePrefixToItemIfItHasNoHistory(On.Terraria.Item.orig_Prefix orig, Item self, int pre)
		{
			bool ret = orig(self, pre);

			if (pre == -2) return ret; //Reforging, as this code should only run for non-reforge contexts

			if (self.IsAir) return ret;

			if (self.TryGetGlobalItem<CRGlobalItem>(out var result))
			{
				result.AddPrefixToItemIfItHasNoHistory(self);
			}

			return ret;
		}
	}
}
