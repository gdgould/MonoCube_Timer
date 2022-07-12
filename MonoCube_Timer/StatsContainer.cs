using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoCube_Timer
{
    enum HAlignment { Left, Centre, Right }
    enum VAlignment { Top, Centre, Bottom }
    class StatsContainer : Control
    {
        private int textIndex;
        public string Title { get; set; }
        public System.Drawing.Size Size { get; set; }
        public Color TextColor { get; set; }

        protected GameContent gameContent;
        protected SpriteFont titleFont;
        protected SpriteFont textFont;

        private int verticalOffset;

        private List<RelativeText> textInstances;
        public bool DrawSeparatingStroke { get; set; }

        /// <summary>
        /// A container for displaying time and average statistics.
        /// </summary>
        /// <param name="gameContent"></param>
        /// <param name="spriteBatch"></param>
        /// <param name="titleFont"></param>
        /// <param name="textFont"></param>
        public StatsContainer(GameContent gameContent, SpriteBatch spriteBatch, SpriteFont titleFont, SpriteFont textFont) : base()
        {
            textIndex = 0;

            this.gameContent = gameContent;
            this.spriteBatch = spriteBatch;
            this.titleFont = titleFont;
            this.textFont = textFont;

            this.Location = new Vector2(0, 0);
            this.Size = new System.Drawing.Size(100, 100);

            this.BackColor = Color.White;
            this.TextColor = Color.Black;
            this.ZDepth = 0.9f;

            this.Visible = true;
            this.Enabled = true;

            this.Title = "Text";
            this.DrawSeparatingStroke = false;
            textInstances = new List<RelativeText>();
        }

        /// <summary>
        /// Add text to a specific location in the container.
        /// </summary>
        /// <param name="location">The location to place the text.</param>
        /// <param name="text">The text.</param>
        /// <param name="hPosition">The horizontal alignment of the text relative to the location.</param>
        /// <param name="vPosition">The vertical alignment of the text relative to the location.</param>
        /// <returns></returns>
        public int AddText(Vector2 location, string text, HAlignment hPosition = HAlignment.Left, VAlignment vPosition = VAlignment.Top)
        {
            return AddText(location, text, this.TextColor, hPosition, vPosition);
        }
        /// <summary>
        /// Add text to a specific location in the container.
        /// </summary>
        /// <param name="location">The location to place the text.</param>
        /// <param name="text">The text.</param>
        /// <param name="textColor">The color of the displayed text.</param>
        /// <param name="hPosition">The horizontal alignment of the text relative to the location.</param>
        /// <param name="vPosition">The vertical alignment of the text relative to the location.</param>
        /// <returns></returns>
        public int AddText(Vector2 location, string text, Color textColor, HAlignment hPosition = HAlignment.Left, VAlignment vPosition = VAlignment.Top)
        {
            textIndex++;
            textInstances.Add(new RelativeText(location, text, hPosition, vPosition, textColor, textIndex));
            return textIndex;
        }

        /// <summary>
        /// Edit already placed text in the container.
        /// </summary>
        /// <param name="index">The index of the object.</param>
        /// <param name="newText">The new text to write.</param>
        /// <returns></returns>
        public bool EditText(int index, string newText)
        {
            for (int i = 0; i < textInstances.Count(); i++)
            {
                if (textInstances[i].Index == index)
                {
                    textInstances[i] = new RelativeText(textInstances[i].Location, newText, textInstances[i].HPosition, textInstances[i].VPosition, textInstances[i].TextColor, textInstances[i].Index);
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Edit the color of already placed text in the container.
        /// </summary>
        /// <param name="index">The index of the object.</param>
        /// <param name="newTextColor">The new color for the object.</param>
        /// <returns></returns>
        public bool EditColor(int index, Color newTextColor)
        {
            for (int i = 0; i < textInstances.Count(); i++)
            {
                if (textInstances[i].Index == index)
                {
                    textInstances[i] = new RelativeText(textInstances[i].Location, textInstances[i].Text, textInstances[i].HPosition, textInstances[i].VPosition, newTextColor, textInstances[i].Index);
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Draw the container
        /// </summary>
        public override void Draw()
        {
            if (!Visible)
            {
                return;
            }

            float drawOrder = ZDepth + Constants.SpriteLevelDepth;

            spriteBatch.Draw(gameContent.buttonCorner, Location, null, BackColor, 0.0f, new Vector2(0, 0), 1.0f, SpriteEffects.None, drawOrder);
            spriteBatch.Draw(gameContent.buttonCorner, new Vector2(Location.X + Size.Width, Location.Y), null, BackColor, (float)Math.PI / 2.0f, new Vector2(0, 0), 1.0f, SpriteEffects.None, drawOrder);
            spriteBatch.Draw(gameContent.buttonCorner, new Vector2(Location.X + Size.Width, Location.Y + Size.Height), null, BackColor, (float)Math.PI, new Vector2(0, 0), 1.0f, SpriteEffects.None, drawOrder);
            spriteBatch.Draw(gameContent.buttonCorner, new Vector2(Location.X, Location.Y + Size.Height), null, BackColor, 3.0f * ((float)Math.PI / 2.0f), new Vector2(0, 0), 1.0f, SpriteEffects.None, drawOrder);

            spriteBatch.Draw(gameContent.buttonPixel, new Vector2(Location.X, Location.Y + Constants.CornerSize), null, BackColor, 0.0f, Vector2.Zero, new Vector2(Size.Width, Size.Height - 2 * Constants.CornerSize), SpriteEffects.None, drawOrder);
            spriteBatch.Draw(gameContent.buttonPixel, new Vector2(Location.X + Constants.CornerSize, Location.Y), null, BackColor, 0.0f, Vector2.Zero, new Vector2(Size.Width - 2 * Constants.CornerSize, Size.Height), SpriteEffects.None, drawOrder);

            Vector2 stringSpace = titleFont.MeasureString(Title);
            verticalOffset = (int)Math.Round(stringSpace.Y) + 10;
            spriteBatch.DrawString(titleFont, Title, new Vector2((float)Math.Round(Location.X + ((Size.Width - stringSpace.X) / 2)), (float)Math.Round(Location.Y + 5)), TextColor, 0.0f, new Vector2(0, 0), 1.0f, SpriteEffects.None, ZDepth); ;

            //Draw vertical separating stroke:
            if (DrawSeparatingStroke)
            {
                spriteBatch.Draw(gameContent.buttonPixel, new Vector2(Location.X + Size.Width / 2 - 5, Location.Y + verticalOffset), null, TextColor, 0.0f, Vector2.Zero, new Vector2(1, Size.Height - verticalOffset - 15), SpriteEffects.None, ZDepth);
            }

            DrawText();
        }

        /// <summary>
        /// Draw the text within the container
        /// </summary>
        private void DrawText()
        {
            for (int i = 0; i < textInstances.Count(); i++)
            {
                Vector2 stringSpace = textFont.MeasureString(textInstances[i].Text);

                Vector2 position = new Vector2();

                switch (textInstances[i].HPosition)
                {
                    case HAlignment.Left:
                        position = new Vector2((float)Math.Round(Location.X + textInstances[i].Location.X), 0);
                        break;
                    case HAlignment.Centre:
                        position = new Vector2((float)Math.Round(Location.X + textInstances[i].Location.X - 0.5f * stringSpace.X), 0);
                        break;
                    case HAlignment.Right:
                        position = new Vector2((float)Math.Round(Location.X + textInstances[i].Location.X - stringSpace.X), 0);
                        break;
                }

                switch (textInstances[i].VPosition)
                {
                    case VAlignment.Top:
                        position = new Vector2(position.X, verticalOffset + (float)Math.Round(Location.Y + textInstances[i].Location.Y));
                        break;
                    case VAlignment.Centre:
                        position = new Vector2(position.X, verticalOffset + (float)Math.Round(Location.Y + textInstances[i].Location.Y - 0.5f * stringSpace.Y));
                        break;
                    case VAlignment.Bottom:
                        position = new Vector2(position.X, verticalOffset + (float)Math.Round(Location.Y + textInstances[i].Location.Y - stringSpace.Y));
                        break;
                }

                spriteBatch.DrawString(textFont, textInstances[i].Text, position, textInstances[i].TextColor, 0.0f, new Vector2(0, 0), 1.0f, SpriteEffects.None, ZDepth);
            }
        }
    }

    internal struct RelativeText
    {
        internal Vector2 Location { get; }
        internal string Text { get; }
        internal HAlignment HPosition { get; }
        internal VAlignment VPosition { get; }
        internal Color TextColor { get; }

        internal long Index { get; }

        internal RelativeText(Vector2 location, string text, HAlignment hPosition, VAlignment vPosition, Color textColor, long index)
        {
            this.Location = location;
            this.Text = text;
            this.HPosition = hPosition;
            this.VPosition = vPosition;
            this.TextColor = textColor;
            this.Index = index;
        }
    }
}