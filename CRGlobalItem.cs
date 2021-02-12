using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ConsistentReforging
{
	public class CRGlobalItem : GlobalItem
	{
		public static bool revertingReforge = false;

		public override bool CloneNewInstances => false;

		public override bool InstancePerEntity => true;

		//None is in by default
		//Recently added prefix on reforge gets added at the end of the list
		//The 2nd-from last element is the one to go back to on revert
		//On revert, move the last element to the start of the list
		public List<int> reforges = new List<int>()
		{
			0
		};

		private const int InvalidIndex = -1;
		private const int InvalidPrefix = 0;

		public int RevertPrefix
		{
			get
			{
				if (PrefixIndex == InvalidIndex)
				{
					return InvalidPrefix;
				}
				else
				{
					return reforges[PrefixIndex];
				}
			}
		}

		public int PrefixIndex => reforges.Count > 1 ? reforges.Count - 2 : InvalidIndex;

		private int rollCount = 0;

		public override GlobalItem Clone(Item item, Item itemClone)
		{
			CRGlobalItem myClone = (CRGlobalItem)base.Clone(item, itemClone);

			myClone.reforges = new List<int>(reforges);

			return myClone;
		}

		internal void AddPrefixToItemIfItHasNoHistory(Item item)
		{
			if (item.prefix > 0 && reforges.Count == 1 && reforges[0] == 0)
			{
				reforges[0] = item.prefix;
			}
		}

		public override bool NeedsSaving(Item item)
		{
			return Config.Instance.SaveReforges && reforges.Count > 2;
		}

		public override TagCompound Save(Item item)
		{
			TagCompound tag = new TagCompound()
			{
				{"reforges", reforges }
			};
			return tag;
		}

		public override void Load(Item item, TagCompound tag)
		{
			if (!Config.Instance.SaveReforges) return;

			reforges = (List<int>)tag.GetList<int>("reforges");
		}

		public override void NetSend(Item item, BinaryWriter writer)
		{
			//RangeMax is < 255, and prefixes are bytes aswell
			writer.Write((byte)reforges.Count);
			for (int i = 0; i < reforges.Count; i++)
			{
				writer.Write((byte)reforges[i]);
			}
		}

		public override void NetReceive(Item item, BinaryReader reader)
		{
			byte count = reader.ReadByte();
			reforges = new List<int>();
			for (int i = 0; i < count; i++)
			{
				reforges.Add(reader.ReadByte());
			}
		}

		public override bool AllowPrefix(Item item, int pre)
		{
			return HandleReforging(pre);
		}

		public override void PostReforge(Item item)
		{
			AddRecentReforge(item);
		}

		private bool HandleReforging(int pre)
		{
			if (revertingReforge && pre == RevertPrefix)
			{
				revertingReforge = false;
				return true;
			}

			if (Config.Instance.PreventDuplicatesFromHistory && rollCount < Config.RangeMax && reforges.Contains(pre))
			{
				//Softlock protection
				rollCount++;
				return false;
			}

			rollCount = 0;

			return true;
		}

		private void AddRecentReforge(Item item)
		{
			var prefix = item.prefix;
			if (prefix > 0)
			{
				if (reforges.Count >= Config.Instance.ReforgeHistoryLength)
				{
					reforges.RemoveAt(0);
				}

				if (!reforges.Contains(prefix))
				{
					reforges.Add(prefix);
				}
			}
		}

		public static void RevertReforge(ref Item item)
		{
			revertingReforge = true;

			CRGlobalItem global = item.GetGlobalItem<CRGlobalItem>();
			int revertPrefix = global.RevertPrefix;

			int oldPrefix = global.reforges.Last();

			//Below code mostly copied from how tml handles reforging

			bool favorited = item.favorited;
			int stack = item.stack;
			Item r = new Item();
			r.netDefaults(item.netID);
			//TODO: method only used here, probably a poor implementation - noted by CB
			r = r.CloneWithModdedDataFrom(item);

			if (revertPrefix > 0)
			{
				r.Prefix(revertPrefix);
			}

			item = r.Clone();

			//Get new global from item
			global = item.GetGlobalItem<CRGlobalItem>();

			//Required for the ItemText to display properly
			item.Center = Main.LocalPlayer.Center;
			item.favorited = favorited;
			item.stack = stack;
			ItemText.NewText(item, item.stack, noStack: true);

			Main.PlaySound(SoundID.Tink);

			if (global.reforges.Count > 1)
			{
				//Move last element to the start
				global.reforges.RemoveAt(global.reforges.Count - 1);
				global.reforges.Insert(0, oldPrefix);
			}
		}

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			if (!Main.InReforgeMenu) return;

			if (!Config.Instance.ShowReforgeHistoryTooltip) return;

			bool inVanity = tooltips.Any(t => t.mod == "Terraria" && t.Name == "Social");
			if (inVanity) return;

			if (reforges.Count > 0)
			{
				var all = reforges.Select(pre => pre > 0 ? Lang.prefix[pre].Value : "None");

				tooltips.Add(new TooltipLine(mod, "PrefixHistory", string.Join(", ", all)));
			}

			if (RevertPrefix > 0)
			{
				tooltips.Add(new TooltipLine(mod, "PreviousPrefix", "Previous prefix: " + Lang.prefix[RevertPrefix].Value));
			}
			else
			{
				tooltips.Add(new TooltipLine(mod, "PreviousPrefix", "Previous prefix: None"));
			}
		}
	}
}
