using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoCube_Timer
{
    class Tab : Button
    {
        private const int setBorderSize = 2;

        /// <summary>
        /// Creates a Tab.
        /// </summary>
        /// <param name="gameContent"></param>
        /// <param name="spriteBatch"></param>
        /// <param name="spriteFont"></param>
        public Tab(GameContent gameContent, SpriteBatch spriteBatch, SpriteFont spriteFont) : base(gameContent, spriteBatch, spriteFont)
        {
            this.ToggleOnClick = true;
        }

        /// <summary>
        /// Gets the size of the border, which is larger than the size of the tab itself
        /// </summary>
        /// <returns></returns>
        protected override System.Drawing.Size BorderSize()
        {
            return new System.Drawing.Size(internalSize.Width + 2 * internalBorderSize, internalSize.Height + internalBorderSize);
        }

        /// <summary>
        /// Draws a generic tab shape of the given coordinates with the given size.
        /// </summary>
        /// <param name="location">The position to draw at.</param>
        /// <param name="size">The size of the rectangle.</param>
        /// <param name="tint">The colour of rectangle.</param>
        /// <param name="zOffset">The difference between the Tab's zDepth and this rectangle's zDepth.</param>
        protected override void DrawControlShape(Vector2 location, System.Drawing.Size size, Color tint, float zOffset)
        {
            float drawOrder = ZDepth + zOffset;
            if (drawOrder > 1) { drawOrder = 1; }
            if (drawOrder < 0) { drawOrder = 0; }

            spriteBatch.Draw(gameContent.buttonCorner, location, null, tint, 0.0f, new Vector2(0, 0), 1.0f, SpriteEffects.None, drawOrder);
            spriteBatch.Draw(gameContent.buttonCorner, new Vector2(location.X + size.Width, location.Y), null, tint, (float)Math.PI / 2.0f, new Vector2(0, 0), 1.0f, SpriteEffects.None, drawOrder);
            spriteBatch.Draw(gameContent.buttonInvertedCorner, new Vector2(location.X + size.Width, location.Y + size.Height), null, tint, 3.0f * ((float)Math.PI / 2.0f), new Vector2(0, 0), 1.0f, SpriteEffects.None, drawOrder);
            spriteBatch.Draw(gameContent.buttonInvertedCorner, new Vector2(location.X, location.Y + size.Height), null, tint, (float)Math.PI, new Vector2(0, 0), 1.0f, SpriteEffects.None, drawOrder);

            spriteBatch.Draw(gameContent.buttonPixel, new Vector2(location.X + Constants.CornerSize, location.Y), null, tint, 0.0f, Vector2.Zero, new Vector2(size.Width - 2 * Constants.CornerSize, Constants.CornerSize), SpriteEffects.None, drawOrder);
            spriteBatch.Draw(gameContent.buttonPixel, new Vector2(location.X, location.Y + Constants.CornerSize), null, tint, 0.0f, Vector2.Zero, new Vector2(size.Width, size.Height - Constants.CornerSize), SpriteEffects.None, drawOrder);
        }
    }
}