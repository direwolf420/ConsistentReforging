using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.UI;

namespace ConsistentReforging.UI.Common
{
	/// <summary>
	/// Same as UIImageButton from vanilla
	/// </summary>
	public class UIImageButtonExtended : UIElement
	{
		protected Asset<Texture2D> asset;
		private float alphaOver = 1f;
		private float alphaOut = 0.4f;

		private string hoverText = "";

		public UIImageButtonExtended(Asset<Texture2D> asset)
		{
			SetImage(asset);
			Recalculate();
		}

		public override void MouseOver(UIMouseEvent evt)
		{
			SoundEngine.PlaySound(SoundID.MenuTick);

			base.MouseOver(evt);
		}

		protected void DrawInternal(SpriteBatch spriteBatch, Asset<Texture2D> asset, Vector2 off = default, Color color = default)
		{
			if (color == default) color = Color.White;

			spriteBatch.Draw(position: GetDimensions().Position() + off, texture: asset.Value, color: color * (IsMouseHovering ? alphaOver : alphaOut));
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			DrawInternal(spriteBatch, asset);

			if (IsMouseHovering)
			{
				Main.LocalPlayer.mouseInterface = true;
				Main.LocalPlayer.cursorItemIconEnabled = false;
				Main.ItemIconCacheUpdate(0);
				Main.hoverItemName = hoverText;
			}
		}

		public void SetImage(Asset<Texture2D> asset)
		{
			this.asset = asset;
			Width.Pixels = this.asset.Width();
			Height.Pixels = this.asset.Height();
		}

		public void SetHoverText(string hoverText)
		{
			this.hoverText = hoverText;
		}

		public void SetAlpha(float whenOver, float whenOut)
		{
			alphaOver = MathHelper.Clamp(whenOver, 0f, 1f);
			alphaOut = MathHelper.Clamp(whenOut, 0f, 1f);
		}
	}
}
