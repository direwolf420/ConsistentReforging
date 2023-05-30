using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ConsistentReforging
{
	//Mod could be clientside if not for the send/receive code for syncing the reforge history
	public class ConsistentReforging : Mod
	{
		public static LocalizedText UndoLastReforgeText { get; private set; }
		public static LocalizedText NoPrefixText { get; private set; }
		public static LocalizedText PreviousPrefixText { get; private set; }

		public static bool CurrentlyReforging { get; private set; }

		public override void Load()
		{
			On_Item.Prefix += Prefix;

			string category = $"Common.";
			UndoLastReforgeText ??= Language.GetOrRegister(this.GetLocalizationKey($"{category}UndoLastReforge"));
			NoPrefixText ??= Language.GetOrRegister(this.GetLocalizationKey($"{category}NoPrefix"));
			PreviousPrefixText ??= Language.GetOrRegister(this.GetLocalizationKey($"{category}PreviousPrefix"));
		}

		private static bool Prefix(On_Item.orig_Prefix orig, Item self, int pre)
		{
			CurrentlyReforging = pre == -2; //Flag the current reforge as being processed in the goblin tinkerer reforge menu
			bool ret = orig(self, pre);
			CurrentlyReforging = false;

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
