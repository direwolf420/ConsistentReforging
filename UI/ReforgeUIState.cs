﻿using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace ConsistentReforging.UI
{
	internal class ReforgeUIState : UIState
	{
		internal Texture2D buttonTexture;
		private ReforgeButton button;

		public override void OnInitialize()
		{
			if (buttonTexture == null)
			{
				buttonTexture = ModContent.GetTexture("ConsistentReforging/UI/Button");
			}

			button = new ReforgeButton(buttonTexture);
			button.OnClick += Button_OnClick;
			button.OnMouseOver += Button_OnMouseOver;
			Append(button);

			Recalculate();
		}

		public override void Recalculate()
		{
			if (button == null) return;

			int x = 50;
			int y = 270;

			int reforgeX = x + 70;
			int reforgeY = y + 40;

			int ourReforgeX = reforgeX;
			int ourReforgeY = reforgeY;

			if (Config.Instance.Bottom)
			{
				ourReforgeY += Main.reforgeTexture[0]?.Height ?? 28;
			}
			else
			{
				ourReforgeX += Main.reforgeTexture[0]?.Width ?? 28;
			}
			//spriteBatch.Draw(texture2D3, new Vector2(num66, num67), null, Microsoft.Xna.Framework.Color.White, 0f, texture2D3.Size() / 2f, reforgeScale, SpriteEffects.None, 0f);

			button.Left.Pixels = ourReforgeX - button.Width.Pixels / 2;
			button.Top.Pixels = ourReforgeY - button.Height.Pixels / 2;
			base.Recalculate();
		}

		private void Button_OnMouseOver(UIMouseEvent evt, UIElement listeningElement)
		{
			SetHoverTextItem();
		}

		public void SetHoverTextItem()
		{
			int pre = Main.reforgeItem.GetGlobalItem<CRGlobalItem>().RevertPrefix;
			string name = pre > 0 ? Lang.prefix[pre].Value : "None";
			button.SetHoverText($"Undo last reforge (-> {name})");
		}

		private void Button_OnClick(UIMouseEvent evt, UIElement listeningElement)
		{
			if (!ConsistentReforging.UIFunctional())
			{
				return;
			}

			CRGlobalItem.RevertReforge(ref Main.reforgeItem);

			SetHoverTextItem();
		}
	}
}