using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoCube_Timer
{
    class Button : Control
    {
        protected const float textPadding = 6.0f;

        // Adjusts for the fact that string measurement will account for descenders even when the aren't present in the string
        const int baselineAdjustment = -4;
        const int baselineAdjustmentNoHanging = -8;

        protected int internalBorderSize;

        public bool IsToggled { get; set; }
        public bool ToggleOnClick { get; set; }
        public int borderSize { get; set; }

        public Color ToggleColor { get; set; }
        public Color TextColor { get; set; }
        public Color BorderColor { get; set; }

        public bool AutoSize { get; set; }

        protected Color currentBackColor; //Set to BackColor except when the mouse is over and pressed, at which time it is set to DepressColor.

        // To avoid Size being set when AutoSize is being used
        public System.Drawing.Size internalSize;
        public System.Drawing.Size Size
        {
            get
            {
                return internalSize;
            }
            set
            {
                if (!AutoSize)
                {
                    internalSize = Size;
                }
            }
        }

        public int Width
        {
            get
            {
                return internalSize.Width;
            }
            set
            {
                if (!AutoSize)
                {
                    internalSize.Width = Width;
                }
            }
        }
        public int Height
        {
            get
            {
                return internalSize.Height;
            }
            set
            {
                if (!AutoSize)
                {
                    internalSize.Height = Height;
                }
            }
        }


        protected SpriteFont spriteFont;
        protected GameContent gameContent;

        public event Action<object, long> Click;
        public event Action<object, long> MouseEnter;
        public event Action<object, long> MouseLeave;

        protected string Text;
        /// <summary>
        /// Creates a button.
        /// </summary>
        /// <param name="gameContent"></param>
        /// <param name="spriteBatch"></param>
        /// <param name="spriteFont"></param>
        public Button(GameContent gameContent, SpriteBatch spriteBatch, SpriteFont spriteFont) : base()
        {
            this.gameContent = gameContent;
            this.spriteBatch = spriteBatch;
            this.spriteFont = spriteFont;

            this.Location = new Vector2(0, 0);

            this.BackColor = Color.White;
            this.ZDepth = 0.5f;

            currentBackColor = this.BackColor;

            this.Visible = true;
            this.Enabled = true;

            this.IsToggled = false;
            this.ToggleOnClick = false;
            this.borderSize = 2;
            this.ToggleColor = Color.LightGray;
            this.TextColor = Color.Black;
            this.BorderColor = Color.Black;
            this.AutoSize = true;
            this.Text = "Text";
            DetermineSize();

            this.oldMouseIn = false;

            this.MouseEnter += Button_MouseEnter;
            this.MouseLeave += Button_MouseLeave;
            this.Click += Button_Click;
        }

        // These methods prevent their corresponding actions from being null and throwing an exception when called.
        // I have no idea why they need to be here.
        private void Button_Click(object arg1, long arg2)
        {
        }
        private void Button_MouseLeave(object arg1, long arg2)
        {
        }
        private void Button_MouseEnter(object arg1, long arg2)
        {
        }

        /// <summary>
        /// Determine the size automatically from the button's text
        /// </summary>
        protected void DetermineSize()
        {
            Vector2 textSize = SpecialMeasureText(this.Text);

            this.internalSize = new System.Drawing.Size((int)Math.Ceiling(textSize.X + 2 * textPadding), (int)Math.Ceiling(textSize.Y + 2 * textPadding));

            // Corners are fixed-size sprites, so the button must be at least this big
            if (internalSize.Width < 2 * Constants.CornerSize + 1)
            {
                internalSize.Width = 2 * Constants.CornerSize + 1;
            }
            if (internalSize.Height < 2 * Constants.CornerSize + 1)
            {
                internalSize.Height = 2 * Constants.CornerSize + 1;
            }
        }

        /// <summary>
        /// Avoids the problem of font.MeasureString including descenders even when the string doesn't have any
        /// </summary>
        /// <param name="s">The string to measure</param>
        /// <returns></returns>
        private Vector2 SpecialMeasureText(string s)
        {
            Vector2 textSize = spriteFont.MeasureString(s);

            // The MeasureString method does weird things by including the height of decenders even when there aren't any in the string.  This fixes that problem
            if (s.Contains("g") || s.Contains("j") || s.Contains("p") || s.Contains("q") || s.Contains("y"))
            {
                textSize.Y += baselineAdjustment;
            }
            else
            {
                textSize.Y += baselineAdjustmentNoHanging;
            }

            return textSize;
        }


        /// <summary>
        /// Sets the button's text, and updates Size if necessary
        /// </summary>
        /// <param name="text">The text to display on the button</param>
        public void SetText(string text)
        {
            this.Text = text;
            if (AutoSize)
            {
                DetermineSize();
            }
        }

        /// <summary>
        /// Gets the button's text
        /// </summary>
        /// <returns>The text displayed by the button</returns>
        public string GetText()
        {
            return this.Text;
        }

        /// <summary>
        /// Gets the top-leftmost corner of the border (which is larger than the button itself)
        /// </summary>
        /// <returns></returns>
        protected virtual Vector2 BorderLocation()
        {
            return new Vector2(Location.X - internalBorderSize, Location.Y - internalBorderSize);
        }

        /// <summary>
        /// Gets the size of the border, which is larger than the size of the button itself
        /// </summary>
        /// <returns></returns>
        protected virtual System.Drawing.Size BorderSize()
        {
            return new System.Drawing.Size(internalSize.Width + 2 * internalBorderSize, internalSize.Height + 2 * internalBorderSize);
        }


        protected bool oldMouseIn; // Monogame IO requires comparing the state last from to the state this frame, so we need to store state in global varaibles

        /// <summary>
        /// Updates the button's information.
        /// </summary>
        /// <param name="newMouseState">The current mouseState.</param>
        /// <param name="oldMouseState">Last frame's mouseState.</param>
        public virtual void Update(MouseState newMouseState, MouseState oldMouseState)
        {
            Vector2 collisionDetectionLocation = BorderLocation();
            bool newMouseIn = newMouseState.X >= collisionDetectionLocation.X && newMouseState.X <= collisionDetectionLocation.X + BorderSize().Width &&
                              newMouseState.Y >= collisionDetectionLocation.Y && newMouseState.Y <= collisionDetectionLocation.Y + BorderSize().Height;

            internalBorderSize = borderSize;
            if (Enabled)
            {
                if (newMouseIn && !oldMouseIn)
                {
                    MouseEnter(this, this.Index);
                }
                if (!newMouseIn && oldMouseIn)
                {
                    MouseLeave(this, this.Index);
                }

                if (newMouseIn)
                {
                    internalBorderSize = borderSize + 1; // The button expands when hovered
                    if (newMouseState.LeftButton == ButtonState.Released && oldMouseState.LeftButton == ButtonState.Pressed)
                    {
                        Click(this, this.Index);
                        if (ToggleOnClick)
                        {
                            IsToggled = !IsToggled;
                        }
                    }
                }
            }

            if (IsToggled)
            {
                currentBackColor = ToggleColor;
            }
            else
            {
                currentBackColor = BackColor;
            }

            oldMouseIn = newMouseIn;
        }

        /// <summary>
        /// Draws the button.
        /// </summary>
        public override void Draw()
        {
            if (gameContent == null || !Visible)
            {
                return;
            }

            float drawOrderOffset = 0;
            if (IsToggled)
            {
                drawOrderOffset = Constants.ToggleLevelDepth;
            }

            DrawBorder(BorderLocation(), BorderSize(), BorderColor, drawOrderOffset + 2 * Constants.SpriteLevelDepth);
            DrawControlShape(Location, internalSize, currentBackColor, drawOrderOffset + 1 * Constants.SpriteLevelDepth);


            spriteFont.LineSpacing = 0;
            Vector2 textSize = SpecialMeasureText(this.Text);

            spriteBatch.DrawString(spriteFont, Text, new Vector2(Location.X + (internalSize.Width - textSize.X) / 2, Location.Y + (internalSize.Height - textSize.Y) / 2), TextColor, 0.0f, new Vector2(0, 0), 1.0f, SpriteEffects.None, Math.Max(0, ZDepth + drawOrderOffset));
        }

        /// <summary>
        /// Draw's the button's border.
        /// </summary>
        /// <param name="location">The location to draw the border.</param>
        /// <param name="size">The size of the border.</param>
        /// <param name="tint">The color of the border.</param>
        /// <param name="zOffset">The additional zDepth of the border, on top of the zDepth of the button.</param>
        protected virtual void DrawBorder(Vector2 location, System.Drawing.Size size, Color tint, float zOffset)
        {
            DrawControlShape(location, size, tint, zOffset); //The zOffset ensures the border is drawn behind the button
        }

        /// <summary>
        /// Draws a generic button shape of the given coordinates with the given size.
        /// </summary>
        /// <param name="location">The position to draw at.</param>
        /// <param name="size">The size of the rectangle.</param>
        /// <param name="tint">The colour of rectangle.</param>
        /// <param name="zOffset">The difference between the Button's zDepth and this rectangle's zDepth.</param>
        protected virtual void DrawControlShape(Vector2 location, System.Drawing.Size size, Color tint, float zOffset)
        {
            float drawOrder = ZDepth + zOffset;
            if (drawOrder > 1) { drawOrder = 1; }
            if (drawOrder < 0) { drawOrder = 0; }

            spriteBatch.Draw(gameContent.buttonCorner, location, null, tint, 0.0f, new Vector2(0, 0), 1.0f, SpriteEffects.None, drawOrder);
            spriteBatch.Draw(gameContent.buttonCorner, new Vector2(location.X + size.Width, location.Y), null, tint, (float)Math.PI / 2.0f, new Vector2(0, 0), 1.0f, SpriteEffects.None, drawOrder);
            spriteBatch.Draw(gameContent.buttonCorner, new Vector2(location.X + size.Width, location.Y + size.Height), null, tint, (float)Math.PI, new Vector2(0, 0), 1.0f, SpriteEffects.None, drawOrder);
            spriteBatch.Draw(gameContent.buttonCorner, new Vector2(location.X, location.Y + size.Height), null, tint, 3.0f * ((float)Math.PI / 2.0f), new Vector2(0, 0), 1.0f, SpriteEffects.None, drawOrder);

            spriteBatch.Draw(gameContent.buttonPixel, new Vector2(location.X, location.Y + Constants.CornerSize), null, tint, 0.0f, Vector2.Zero, new Vector2(size.Width, size.Height - 2 * Constants.CornerSize), SpriteEffects.None, drawOrder);
            spriteBatch.Draw(gameContent.buttonPixel, new Vector2(location.X + Constants.CornerSize, location.Y), null, tint, 0.0f, Vector2.Zero, new Vector2(size.Width - 2 * Constants.CornerSize, size.Height), SpriteEffects.None, drawOrder);
        }
    }
}