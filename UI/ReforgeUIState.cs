using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;

namespace ConsistentReforging.UI
{
	internal class ReforgeUIState : UIState
	{
		internal Asset<Texture2D> buttonAsset;
		private ReforgeButton button;

		public override void OnInitialize()
		{
			if (buttonAsset == null)
			{
				//UI textures should load immediate if dimensions depend on it
				buttonAsset = ModContent.Request<Texture2D>("ConsistentReforging/UI/Button", AssetRequestMode.ImmediateLoad);
			}

			button = new ReforgeButton(buttonAsset);
			button.OnLeftClick += Button_OnLeftClick;
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

			Asset<Texture2D> reforgeIcon = TextureAssets.Reforge[0];
			if (Config.Instance.UndoButtonAnchor == Config.UndoButtonAnchorPosType.Bottom)
			{
				ourReforgeY += reforgeIcon?.Height() ?? 28;
			}
			else
			{
				ourReforgeX += reforgeIcon?.Width() ?? 28;
			}
			//spriteBatch.Draw(value5, new Vector2(num64, num65), null, Microsoft.Xna.Framework.Color.White, 0f, value5.Size() / 2f, reforgeScale, SpriteEffects.None, 0f);

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
			if (!Main.reforgeItem.TryGetGlobalItem<CRGlobalItem>(out var global))
			{
				return;
			}

			int pre = global.RevertPrefix;
			string name = pre > 0 ? Lang.prefix[pre].Value : ConsistentReforging.NoPrefixText.ToString();
			button.SetHoverText(ConsistentReforging.UndoLastReforgeText.Format(name));
		}

		private void Button_OnLeftClick(UIMouseEvent evt, UIElement listeningElement)
		{
			if (!CRUISystem.UIFunctional())
			{
				return;
			}

			CRGlobalItem.RevertReforge(ref Main.reforgeItem);

			SetHoverTextItem();
		}
	}
}
