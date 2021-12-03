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

		public override bool CloneNewInstances => false; //IMPORTANT THAT IT'S FALSE, SO Clone(Item, Item) WORKS PROPERLY

		public override bool InstancePerEntity => true;

		//None is in by default. If item is applied a prefix outside of reforging (during loading, on drop, etc), it's replaced with the current prefix
		//Recently added prefix on reforge gets added at the end of the list
		//The 2nd-from-last element is the one to go back to on revert
		//On revert, move the last element to the start of the list
		public List<int> reforges = new List<int>()
		{
			0
		};

		//Orphaned mod prefixes will end up in this list until the mod is enabled again (or the prefix is re-added)
		public Dictionary<string, List<string>> orphanedModPrefixes = new Dictionary<string, List<string>>();

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
			myClone.orphanedModPrefixes = new Dictionary<string, List<string>>(orphanedModPrefixes);

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
			//Save if there are atleast two prefixes in history
			return Config.Instance.SaveReforges && reforges.Count > 1;
		}

		public override TagCompound Save(Item item)
		{
			List<int> vanillaReforges = reforges.Where(p => p < PrefixID.Count).ToList();

			Dictionary<string, List<string>> modReforges = new Dictionary<string, List<string>>();

			foreach (var p in reforges)
			{
				if (p < PrefixID.Count)
				{
					continue;
				}

				ModPrefix modPrefix = ModPrefix.GetPrefix((byte)p);

				if (modPrefix == null)
				{
					continue;
				}

				Mod mod = modPrefix.mod;

				if (mod == null)
				{
					continue;
				}

				string modName = mod.Name;
				string name = modPrefix.Name;

				if (!modReforges.ContainsKey(modName))
				{
					modReforges[modName] = new List<string>();
				}

				var reforgeNames = modReforges[modName];

				if (!reforgeNames.Contains(name))
				{
					reforgeNames.Add(name);
				}
			}

			//Dictionary to TagCompound

			TagCompound tag = new TagCompound();

			TagCompound moddedReforges = new TagCompound();

			foreach (var pair in modReforges)
			{
				moddedReforges.Add(pair.Key, pair.Value);
			}

			TagCompound orphanedModdedReforges = new TagCompound();

			foreach (var pair in orphanedModPrefixes)
			{
				orphanedModdedReforges.Add(pair.Key, pair.Value);
			}

			tag.Add("vanillaReforges", vanillaReforges);
			tag.Add("moddedReforges", moddedReforges);
			tag.Add("orphanedModdedReforges", orphanedModdedReforges);

			//Old
			//TagCompound tag = new TagCompound()
			//{
			//	{"reforges", reforges }
			//};

			return tag;
		}

		public override void Load(Item item, TagCompound tag)
		{
			if (!Config.Instance.SaveReforges) return;

			//Old compatibility
			if (tag.ContainsKey("reforges"))
			{
				reforges = (List<int>)tag.GetList<int>("reforges");
				return;
			}

			List<int> vanillaReforges = (List<int>)tag.GetList<int>("vanillaReforges");

			List<int> modReforges = new List<int>();

			TagCompound moddedReforges = tag.Get<TagCompound>("moddedReforges");

			TagCompound orphanedModdedReforges = tag.Get<TagCompound>("orphanedModdedReforges");

			//Merge previously loaded + orphaned together, and then filter through them at once

			foreach (var orphanedModTag in orphanedModdedReforges)
			{
				string modName = orphanedModTag.Key;
				var orphanNames = orphanedModTag.Value as List<string> ?? new List<string>();

				if (moddedReforges.ContainsKey(modName))
				{
					var reforgeNames = moddedReforges[modName] as List<string> ?? new List<string>();

					moddedReforges[modName] = reforgeNames.Union(orphanNames).ToList();
				}
				else
				{
					moddedReforges[modName] = orphanNames;
				}
			}

			orphanedModPrefixes = new Dictionary<string, List<string>>();

			foreach (var modTag in moddedReforges)
			{
				string modName = modTag.Key;
				var reforgeNames = modTag.Value as List<string> ?? new List<string>();

				Mod mod = ModLoader.GetMod(modName);

				if (mod == null)
				{
					if (!orphanedModPrefixes.ContainsKey(modName))
					{
						//If mod is null (not currently loaded), all its prefixes are also not loaded, so save the entire list, and continue
						orphanedModPrefixes[modName] = reforgeNames;
					}

					continue;
				}

				foreach (var name in reforgeNames)
				{
					ModPrefix modPrefix = mod.GetPrefix(name);
					if (modPrefix == null)
					{
						//If loaded prefix does not exist, add it as orphaned
						if (!orphanedModPrefixes.ContainsKey(modName))
						{
							orphanedModPrefixes[modName] = new List<string>();
						}

						List<string> lists = orphanedModPrefixes[modName];
						if (!lists.Contains(name))
						{
							lists.Add(name);
						}
						continue;
					}

					modReforges.Add(modPrefix.Type);
				}
			}

			reforges = vanillaReforges.Union(modReforges).ToList();
		}

		public override void NetSend(Item item, BinaryWriter writer)
		{
			int reforgeCount = reforges.Count;
			int orphanedCount = orphanedModPrefixes.Count;

			BitsByte flags = new BitsByte();
			bool hasReforges = flags[0] = reforgeCount > 1;
			bool hasOrphaned = flags[1] = orphanedCount > 0;

			writer.Write((byte)flags);

			if (hasReforges)
			{
				//RangeMax is < 255, and prefixes are bytes aswell
				writer.Write((byte)reforgeCount);
				for (int i = 0; i < reforgeCount; i++)
				{
					writer.Write((byte)reforges[i]);
				}
			}

			if (hasOrphaned)
			{
				//Send orphaned mod count first
				writer.Write((int)orphanedCount);

				foreach (var pair in orphanedModPrefixes)
				{
					//Mod name
					writer.Write((string)pair.Key);
					var list = pair.Value;

					//By extension, a mod can't have more than 255 prefixes
					writer.Write((byte)list.Count);
					for (int i = 0; i < list.Count; i++)
					{
						writer.Write((string)list[i]);
					}
				}
			}
		}

		public override void NetReceive(Item item, BinaryReader reader)
		{
			BitsByte flags = reader.ReadByte();
			bool hasReforges = flags[0]; //If atleast two prefixes in it
			bool hasOrphaned = flags[1]; //If atleast one prefix in it

			if (hasReforges)
			{
				byte count = reader.ReadByte();
				reforges = new List<int>();
				for (int i = 0; i < count; i++)
				{
					reforges.Add(reader.ReadByte());
				}
			}
			else
			{
				//Reforges already contains an entry due to field initializer
			}

			if (hasOrphaned)
			{
				int orphanCount = reader.ReadInt32();

				orphanedModPrefixes = new Dictionary<string, List<string>>();

				for (int i = 0; i < orphanCount; i++)
				{
					string modName = reader.ReadString();

					orphanedModPrefixes[modName] = new List<string>();

					byte prefixCount = reader.ReadByte();

					for (int j = 0; j < prefixCount; j++)
					{
						string name = reader.ReadString();

						List<string> lists = orphanedModPrefixes[modName];
						lists.Add(name);
					}
				}
			}
		}

		public override bool AllowPrefix(Item item, int pre)
		{
			return HandleReforging(pre);
		}

		public override void PostReforge(Item item)
		{
			//sortAfter = ReforgeArmor added in build.txt since it's code should run before ours (otherwise item has no prefix yet)
			//Its code: https://bitbucket.org/AJMJ2012/reforgearmor/src/master/ReforgeArmor.cs

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

			if (!Config.Instance.ShowOrphanedReforgeHistoryTooltip) return;

			foreach (var pair in orphanedModPrefixes)
			{
				string modName = pair.Key;

				List<string> names = pair.Value;

				List<TooltipLine> lines = new List<TooltipLine>();

				if (names.Count <= 0) continue;

				tooltips.Add(new TooltipLine(mod, $"Orphaned:{modName}", modName + ":"));

				for (int i = 0; i < names.Count; i++)
				{
					string name = names[i];
					tooltips.Add(new TooltipLine(mod, $"Orphaned:{modName}_{i}", "  " + name));
				}
			}
		}
	}
}
