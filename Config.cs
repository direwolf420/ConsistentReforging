using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace ConsistentReforging
{
	public class Config : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		public static Config Instance => ModContent.GetInstance<Config>();

		public const int RangeMin = 2;
		public const int RangeMax = 90 / 2; //90 is the amount of prefixes in vanilla
		[DefaultValue(8)]
		[Range(RangeMin, RangeMax)]
		[Slider]
		public int ReforgeHistoryLength;

		[DefaultValue(false)]
		public bool ShowReforgeHistoryTooltip;

		[DefaultValue(false)]
		public bool ShowOrphanedReforgeHistoryTooltip;

		[DefaultValue(false)]
		public bool SaveReforges;

		[DefaultValue(true)]
		public bool PreventDuplicatesFromHistory;

		//TODO figure out localization for this
		public const string AnchorBottom = "Bottom";
		public const string AnchorRight = "Right";
		public const string AnchorDefault = AnchorRight;
		public static readonly string[] AnchorOptions = new string[] { AnchorBottom, AnchorRight };

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

		public override void OnChanged()
		{
			CRUISystem.uiState?.Recalculate();
		}
	}
}
