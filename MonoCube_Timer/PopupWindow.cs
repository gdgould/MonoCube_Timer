using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace MonoCube_Timer
{
    class PopupWindow : Control
    {
        protected GameContent gameContent;

        public System.Drawing.Size Size { get; set; }

        public event Action<object> Closing;

        protected bool xHover;
        protected Vector2 xButtonPosition;

        /// <summary>
        /// Creates, but does not show, a generic popup window.
        /// </summary>
        /// <param name="gameContent"></param>
        /// <param name="spriteBatch"></param>
        /// <param name="location"></param>
        /// <param name="size"></param>
        public PopupWindow(GameContent gameContent, SpriteBatch spriteBatch, Vector2 location, System.Drawing.Size size) : base()
        {
            this.gameContent = gameContent;
            this.spriteBatch = spriteBatch;
            this.ZDepth = 0.3f;
            this.Size = size;

            this.Location = location;

            this.Visible = true;
            this.Enabled = true;

            this.xHover = false;
        }

        protected void Close(object o)
        {
            Closing(o);
        }

        /// <summary>
        /// Updates the popup window.
        /// </summary>
        /// <param name="newMouseState">The current mouse state.</param>
        /// <param name="oldMouseState">The mouse state last tick.</param>
        /// <param name="newKeyboardState">The current keyboard state.</param>
        /// <param name="oldKeyboardState">The keyboard state last tick.</param>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        /// <param name="windowHasFocus">True if the main game window has focus.</param>
        public virtual void Update(MouseState newMouseState, MouseState oldMouseState, KeyboardState newKeyboardState, KeyboardState oldKeyboardState, GameTime gameTime, bool windowHasFocus)
        {
            if (!Visible)
            {
                return;
            }

            if (newMouseState.X >= xButtonPosition.X && newMouseState.X < xButtonPosition.X + gameContent.dataCloseX.Width &&
                newMouseState.Y >= xButtonPosition.Y && newMouseState.Y < xButtonPosition.Y + gameContent.dataCloseX.Height)
            {
                xHover = true;
            }
            else
            {
                xHover = false;
            }

            if (xHover && oldMouseState.LeftButton == ButtonState.Pressed && newMouseState.LeftButton == ButtonState.Released)
            {
                Closing(this);
            }

            if (oldKeyboardState.IsKeyDown(Keys.Escape) && newKeyboardState.IsKeyUp(Keys.Escape))
            {
                Closing(this);
            }
        }

        /// <summary>
        /// Draws the popup window.
        /// </summary>
        public override void Draw()
        {
            if (!Visible)
            {
                return;
            }

            this.xButtonPosition = new Vector2(Location.X + Size.Width - gameContent.dataCloseX.Width - 4, Location.Y + 4);

            spriteBatch.Draw(gameContent.buttonPixel, new Vector2(Location.X - 1, Location.Y - 1), null, Constants.GetColor("ChildBorderColor"), 0.0f, Vector2.Zero, new Vector2(Size.Width + 2, Size.Height + 2), SpriteEffects.None, ZDepth + 4 * Constants.SpriteLevelDepth);
            spriteBatch.Draw(gameContent.buttonPixel, Location, null, Constants.GetColor("ChildBackgroundColor"), 0.0f, Vector2.Zero, new Vector2(Size.Width, Size.Height), SpriteEffects.None, ZDepth + 3 * Constants.SpriteLevelDepth);

            Color xTint = Constants.GetColor("ChildXNormalColor");
            if (xHover)
            {
                xTint = Constants.GetColor("ChildXHoverColor");
            }

            spriteBatch.Draw(gameContent.buttonPixel, xButtonPosition, null, xTint, 0.0f, Vector2.Zero, new Vector2(gameContent.dataCloseX.Width, gameContent.dataCloseX.Height), SpriteEffects.None, ZDepth + 2 * Constants.SpriteLevelDepth);

            spriteBatch.Draw(gameContent.dataCloseX, xButtonPosition, null, Color.White, 0.0f, new Vector2(0, 0), 1.0f, SpriteEffects.None, ZDepth + 1 * Constants.SpriteLevelDepth);
        }
    }
}