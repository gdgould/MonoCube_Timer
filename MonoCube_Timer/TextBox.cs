using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoCube_Timer
{
    class TextBox : Control
    {
        protected GameContent gameContent;
        protected SpriteFont spriteFont;

        public bool HasFocus
        {
            get
            {
                return hasFocus;
            }
        }

        private bool hasFocus;
        public StringBuilder Text { get; set; }

        public System.Drawing.Size Size { get; set; }

        private Keys[] oldPressedKeys;
        private Keys currentlyPressed;
        private double keyPressTime;
        private double keyRepeatPressTime;

        public Color TextColor;
        public Color FocusColor;
        public Color BorderColor;

        public bool ShowCursor;
        private double CursorFlashCounter;
        private bool CursorState;

        public event Action<object, long, string> LineReturned;
        public int maxChars;
        public bool SanatizeForFilenames; //Blocks characters not allowed in file names.
        public bool BlockCommas; //Blocks commas for csv export (needs to be fixed so this is not necessary)

        public TextBox(GameContent gameContent, SpriteBatch spriteBatch, SpriteFont spriteFont) : base()
        {
            this.gameContent = gameContent;
            this.spriteBatch = spriteBatch;
            this.spriteFont = spriteFont;

            this.Location = new Vector2(0, 0);
            this.Size = new System.Drawing.Size(100, 100);

            this.BackColor = Color.White;
            this.TextColor = Color.Black;
            this.FocusColor = Color.DarkGray;
            this.BorderColor = Color.White;
            this.ZDepth = 0.29f;

            this.Visible = true;
            this.Enabled = true;

            this.hasFocus = false;

            this.ShowCursor = true;
            this.CursorFlashCounter = 0;
            this.CursorState = true;

            Text = new StringBuilder();

            currentlyPressed = Keys.None;
            keyPressTime = 0;
            keyRepeatPressTime = 0;

            this.LineReturned += TextBox_LineReturned;
            this.maxChars = -1;
            this.SanatizeForFilenames = false;
            this.BlockCommas = false;

            this.oldPressedKeys = new Keys[0];
        }

        // Prevents weird null errors
        private void TextBox_LineReturned(object arg1, long arg2, string arg3)
        {

        }

        /// <summary>
        /// Updates the text box.
        /// </summary>
        /// <param name="newKeyboardState">The current keyboard state.</param>
        /// <param name="oldKeyboardState">The keyboard state from the previous tick.</param>
        /// <param name="newMouseState">The current mouse state.</param>
        /// <param name="oldMouseState">The mouse state from the previous tick.</param>
        /// <param name="gameTime">A snapshot of timing values.</param>
        /// <param name="windowHasFocus">Represents whether the main game window has focus.</param>
        public void Update(KeyboardState newKeyboardState, KeyboardState oldKeyboardState, MouseState newMouseState, MouseState oldMouseState, GameTime gameTime, bool windowHasFocus)
        {
            bool mouseOver = newMouseState.X >= this.Location.X && newMouseState.X <= this.Location.X + this.Size.Width && newMouseState.Y >= this.Location.Y && newMouseState.Y <= this.Location.Y + this.Size.Height;

            //If a click is processed, the textBox has focus iff the mouse is over it.
            if (oldMouseState.LeftButton == ButtonState.Pressed && newMouseState.LeftButton == ButtonState.Released)
            {
                hasFocus = mouseOver;
            }

            if (!windowHasFocus)
            {
                currentlyPressed = Keys.None;
            }


            if (!hasFocus || !Enabled)
            {
                CursorState = false;
                return;
            }

            Keys[] pressedKeys = newKeyboardState.GetPressedKeys();

            if (!pressedKeys.Contains(currentlyPressed))
            {
                currentlyPressed = Keys.None;
                keyPressTime = 0;
            }

            for (int i = 0; i < pressedKeys.Length; i++)
            {
                if (!oldPressedKeys.Contains(pressedKeys[i]))
                {
                    currentlyPressed = pressedKeys[i];
                    keyPressTime = 0;
                    break;
                }
            }

            if (keyPressTime == 0 || (keyPressTime >= Constants.InitialKeyDelay && keyRepeatPressTime >= Constants.KeyRepeatDelay))
            {
                keyRepeatPressTime = 0;
                Print(currentlyPressed, newKeyboardState.IsKeyDown(Keys.LeftShift) || newKeyboardState.IsKeyDown(Keys.RightShift));
            }

            if (oldKeyboardState.IsKeyDown(Keys.Enter) && newKeyboardState.IsKeyUp(Keys.Enter) && this.Text.ToString() != "")
            {
                LineReturned(this, this.Index, this.Text.ToString());
            }

            CursorFlashCounter += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (CursorFlashCounter >= Constants.CursorFlashTimespan)
            {
                CursorFlashCounter = 0;
                CursorState = !CursorState;
            }


            keyPressTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            keyRepeatPressTime += gameTime.ElapsedGameTime.TotalMilliseconds;

            oldPressedKeys = pressedKeys;
        }

        /// <summary>
        /// Prints a key into the textbox's text.
        /// </summary>
        /// <param name="key">The key that has been pressed.</param>
        /// <param name="shiftPressed">Whether or not Shift is pressed.</param>
        private void Print(Keys key, bool shiftPressed)
        {
            if (key == Keys.Back && Text.Length > 0)
            {
                Text.Remove(Text.Length - 1, 1);
            }
            else if (Text.Length >= maxChars && maxChars != -1)
            {
                return;
            }
            else if ((int)key >= (int)Keys.A && (int)key <= (int)Keys.Z)
            {
                if (shiftPressed)
                {
                    Text.Append(key.ToString().ToUpper());
                }
                else
                {
                    Text.Append(key.ToString().ToLower());
                }

            }
            else if (((int)key >= (int)Keys.D0 && (int)key <= (int)Keys.D9) ||
                     ((int)key >= (int)Keys.OemSemicolon && (int)key <= (int)Keys.OemTilde) ||
                     ((int)key >= (int)Keys.OemOpenBrackets && (int)key <= (int)Keys.OemQuotes))
            {
                Dictionary<Keys, string[]> characters = new Dictionary<Keys, string[]>()
                {
                    {Keys.D0, new string[2] {"0", ")" } },
                    {Keys.D1, new string[2] {"1", "!" } },
                    {Keys.D2, new string[2] {"2", "@" } },
                    {Keys.D3, new string[2] {"3", "#" } },
                    {Keys.D4, new string[2] {"4", "$" } },
                    {Keys.D5, new string[2] {"5", "%" } },
                    {Keys.D6, new string[2] {"6", "^" } },
                    {Keys.D7, new string[2] {"7", "&" } },
                    {Keys.D8, new string[2] {"8", "*" } },
                    {Keys.D9, new string[2] {"9", "(" } },

                    {Keys.OemSemicolon, new string[2] {";", ":" } },
                    {Keys.OemPlus, new string[2] {"=", "+" } },
                    {Keys.OemComma, new string[2] {",", "<" } },
                    {Keys.OemMinus, new string[2] {"-", "_" } },
                    {Keys.OemPeriod, new string[2] {".", ">" } },
                    {Keys.OemQuestion, new string[2] {"/", "?" } },
                    {Keys.OemTilde, new string[2] {"`", "~" } },
                    {Keys.OemOpenBrackets, new string[2] {"[", "{" } },
                    {Keys.OemPipe, new string[2] {"\\", "|" } },
                    {Keys.OemCloseBrackets, new string[2] {"]", "}" } },
                    {Keys.OemQuotes, new string[2] {"\'", "\"" } },
                };

                if ((!SanatizeForFilenames || Constants.AllowedFilenameChars.Contains(characters[key][shiftPressed ? 1 : 0])) &&
                    (!BlockCommas || characters[key][shiftPressed ? 1 : 0] != ","))
                {
                    Text.Append(characters[key][shiftPressed ? 1 : 0]);
                }

            }
            else if (key == Keys.Space)
            {
                Text.Append(" ");
            }
        }


        protected Vector2 BorderLocation()
        {
            return new Vector2(Location.X - 2, Location.Y - 2);
        }
        protected System.Drawing.Size BorderSize()
        {
            return new System.Drawing.Size(Size.Width + 2 * 2, Size.Height + 2 * 2);
        }

        /// <summary>
        /// Draws the text box.
        /// </summary>
        public override void Draw()
        {
            if (!Visible)
            {
                return;
            }

            float drawOrder = ZDepth + Constants.SpriteLevelDepth;

            Color drawColor = BackColor;
            if (hasFocus && Enabled)
            {
                drawColor = FocusColor;
            }

            DrawControlShape(BorderLocation(), BorderSize(), BorderColor, 2 * Constants.SpriteLevelDepth);
            DrawControlShape(Location, Size, drawColor, 1 * Constants.SpriteLevelDepth);

            string[] lines = DataProcessing.DisplayString(Text.ToString(), Size.Width, spriteFont);
            spriteFont.LineSpacing = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                Vector2 lineLength = gameContent.scrambleFont.MeasureString(lines[i]);

                if (i + 1 == lines.Length && ShowCursor && CursorState)
                {
                    lines[i] = lines[i] + "|";
                }

                spriteBatch.DrawString(spriteFont, lines[i], new Vector2(Location.X + 5, Location.Y + i * 20 + 5), TextColor, 0.0f, new Vector2(0, 0), 1.0f, SpriteEffects.None, ZDepth);
            }

        }


        /// <summary>
        /// Draws the generic shape of the text box.
        /// </summary>
        /// <param name="draw_location">The location (on the game window) to draw at.</param>
        /// <param name="size">The size of the window.</param>
        /// <param name="tint">The color of the window.</param>
        /// <param name="zOffset">The additional zOffset for the shape.</param>
        protected void DrawControlShape(Vector2 draw_location, System.Drawing.Size size, Color tint, float zOffset)
        {
            float drawOrder = ZDepth + zOffset;
            if (drawOrder > 1) { drawOrder = 1; }
            if (drawOrder < 0) { drawOrder = 0; }

            spriteBatch.Draw(gameContent.buttonCorner, draw_location, null, tint, 0.0f, new Vector2(0, 0), 1.0f, SpriteEffects.None, drawOrder);
            spriteBatch.Draw(gameContent.buttonCorner, new Vector2(draw_location.X + size.Width, draw_location.Y), null, tint, (float)Math.PI / 2.0f, new Vector2(0, 0), 1.0f, SpriteEffects.None, drawOrder);
            spriteBatch.Draw(gameContent.buttonCorner, new Vector2(draw_location.X + size.Width, draw_location.Y + size.Height), null, tint, (float)Math.PI, new Vector2(0, 0), 1.0f, SpriteEffects.None, drawOrder);
            spriteBatch.Draw(gameContent.buttonCorner, new Vector2(draw_location.X, draw_location.Y + size.Height), null, tint, 3.0f * ((float)Math.PI / 2.0f), new Vector2(0, 0), 1.0f, SpriteEffects.None, drawOrder);

            spriteBatch.Draw(gameContent.buttonPixel, new Vector2(draw_location.X, draw_location.Y + Constants.CornerSize), null, tint, 0.0f, Vector2.Zero, new Vector2(size.Width, size.Height - 2 * Constants.CornerSize), SpriteEffects.None, drawOrder);
            spriteBatch.Draw(gameContent.buttonPixel, new Vector2(draw_location.X + Constants.CornerSize, draw_location.Y), null, tint, 0.0f, Vector2.Zero, new Vector2(size.Width - 2 * Constants.CornerSize, size.Height), SpriteEffects.None, drawOrder);
        }
    }
}