using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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

		//Old data and names for reference
		[JsonExtensionData]
		private IDictionary<string, JToken> _additionalData = new Dictionary<string, JToken>();

		public const string AnchorBottom = "Bottom";
		public const string AnchorRight = "Right";

		public enum UndoButtonAnchorPosType : byte
		{
			Bottom = 0,
			Right = 1
		}

		[DefaultValue(UndoButtonAnchorPosType.Right)]
		public UndoButtonAnchorPosType UndoButtonAnchor;

		[OnDeserialized]
		internal void OnDeserializedMethod(StreamingContext context)
		{
			ReforgeHistoryLength = Utils.Clamp(ReforgeHistoryLength, RangeMin, RangeMax);

			//port "UndoButtonAnchorPos": "Bottom" from string to enum, which requires (!) a member rename aswell
			JToken token;
			if (_additionalData.TryGetValue("UndoButtonAnchorPos", out token))
			{
				var undoButtonAnchorPos = token.ToObject<string>();
				if (undoButtonAnchorPos == AnchorBottom)
				{
					UndoButtonAnchor = UndoButtonAnchorPosType.Bottom;
				}
				else
				{
					UndoButtonAnchor = UndoButtonAnchorPosType.Right;
				}
			}
			_additionalData.Clear(); //Clear this or it'll crash.

			//Correct invalid values to default fallback
			EnumFallback(ref UndoButtonAnchor, UndoButtonAnchorPosType.Right);
		}

		private static void EnumFallback<T>(ref T value, T defaultValue) where T : Enum
		{
			if (!Enum.IsDefined(typeof(T), value))
			{
				value = defaultValue;
			}
		}

		public override void OnChanged()
		{
			CRUISystem.uiState?.Recalculate();
		}
	}
}
