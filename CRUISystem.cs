using ConsistentReforging.UI;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace ConsistentReforging
{
	//Mod could be clientside if not for the send/receive code for syncing the reforge history
	public class CRUISystem : ModSystem
	{
		internal static UserInterface reforgeInterface;
		internal static ReforgeUIState uiState;

		private static GameTime _lastUpdateUiGameTime;

		internal static void ShowUI()
		{
			reforgeInterface?.SetState(uiState);
		}

		internal static void HideUI()
		{
			reforgeInterface?.SetState(null);
		}

		public static bool UIFunctional()
		{
			Item item = Main.reforgeItem;
			return Main.InReforgeMenu && item != null && !item.IsAir && item.TryGetGlobalItem<CRGlobalItem>(out var global) && global.reforges.Count > 1;
		}

		public override void OnModLoad()
		{
			if (!Main.dedServ)
			{
				reforgeInterface = new UserInterface();

				uiState = new ReforgeUIState();
				uiState.Activate();
			}
		}

		public override void Unload()
		{
			reforgeInterface = null;
			uiState = null;
			_lastUpdateUiGameTime = null;
		}

		public override void UpdateUI(GameTime gameTime)
		{
			_lastUpdateUiGameTime = gameTime;

			if (!UIFunctional())
			{
				HideUI();
			}
			else
			{
				if (reforgeInterface?.CurrentState != null)
				{
					reforgeInterface.Update(gameTime);
				}
				else
				{
					ShowUI();
				}
			}
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
			if (index != -1)
			{
				layers.Insert(index, new LegacyGameInterfaceLayer(
					"ConsistentReforging: Revert last reforge",
					delegate
					{
						if (_lastUpdateUiGameTime != null && reforgeInterface?.CurrentState != null)
						{
							reforgeInterface.Draw(Main.spriteBatch, _lastUpdateUiGameTime);
						}
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
		}
	}
}
