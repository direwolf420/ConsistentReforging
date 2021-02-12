using ConsistentReforging.UI.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace ConsistentReforging.UI
{
	internal class ReforgeButton : UIImageButtonExtended
	{
		internal const float AlphaOver = 0.9f;
		internal const float AlphaOut = 0.6f;

		internal Texture2D buttonBGTexture;

		public ReforgeButton(Texture2D texture) : base(texture)
		{

		}

		public override void OnInitialize()
		{
			if (buttonBGTexture == null)
			{
				buttonBGTexture = ModContent.GetTexture("ConsistentReforging/UI/ButtonBG");
			}
			SetAlpha(1f, 1f);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			//Draw the outline when mouseovered. Relies on alpha being 1f when selected otherwise it will look bad
			if (IsMouseHovering)
			{
				DrawInternal(spriteBatch, buttonBGTexture, color: Color.White);
			}

			base.DrawSelf(spriteBatch);
		}
	}
}
