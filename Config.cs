using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace ConsistentReforging
{
	public class Config : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		public static Config Instance => ModContent.GetInstance<Config>();

		public const int RangeMin = 2;
		public const int RangeMax = PrefixID.Count / 2;
		[Tooltip("Amount of prefixes that should be stored per item")]
		[Label("Reforge History Length")]
		[DefaultValue(5)]
		[Range(RangeMin, RangeMax)]
		[Slider]
		public int ReforgeHistoryLength;

		[Tooltip("If reforge history should be displayed on the item tooltip (only in reforge UI)")]
		[Label("Reforge History Tooltip")]
		[DefaultValue(false)]
		public bool ShowReforgeHistoryTooltip;

		[Tooltip("If reforge history should be saved on world exit (and loaded on world join)")]
		[Label("Save Reforge History")]
		[DefaultValue(false)]
		public bool SaveReforges;

		[Tooltip("If reforging should not give prefixes already in the reforge history")]
		[Label("Prevent Duplicate Prefixes")]
		[DefaultValue(true)]
		public bool PreventDuplicatesFromHistory;

		public const string AnchorBottom = "Bottom";
		public const string AnchorRight = "Right";
		public const string AnchorDefault = AnchorRight;
		public static readonly string[] AnchorOptions = new string[] { AnchorBottom, AnchorRight };

		[Label("'Undo Button' Position")]
		[Tooltip("Choose between positioning the undo button below or right of the reforge button")]
		[DrawTicks]
		[OptionStrings(new string[] { AnchorBottom, AnchorRight })]
		[DefaultValue(AnchorDefault)]
		public string UndoButtonAnchorPos;

		[JsonIgnore]
		public bool Bottom => UndoButtonAnchorPos == AnchorBottom;

		[OnDeserialized]
		internal void OnDeserializedMethod(StreamingContext context)
		{
			ReforgeHistoryLength = Utils.Clamp(ReforgeHistoryLength, RangeMin, RangeMax);

			if (Array.IndexOf(AnchorOptions, UndoButtonAnchorPos) <= -1)
			{
				UndoButtonAnchorPos = AnchorDefault;
			}
		}
	}
}
