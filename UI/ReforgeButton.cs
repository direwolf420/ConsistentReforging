using ConsistentReforging.UI.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace ConsistentReforging.UI
{
	internal class ReforgeButton : UIImageButtonExtended
	{
		internal Asset<Texture2D> buttonBGTexture;

		public ReforgeButton(Asset<Texture2D> asset) : base(asset)
		{

		}

		public override void OnInitialize()
		{
			if (buttonBGTexture == null)
			{
				//No immediate required, since UI dims don't depend on it
				buttonBGTexture = ModContent.Request<Texture2D>("ConsistentReforging/UI/ButtonBG");
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
