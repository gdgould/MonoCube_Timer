using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoCube_Timer
{
    public enum ContainerDisplayState { Nothing = 0, Buttons = 1, Dates = 2 }
    class ScrollContainer : Control
    {
        private System.Drawing.Size size;
        public System.Drawing.Size Size
        {
            get { return size; }
            set { size = value; UpdateCollisionDetection(); }
        }

        private Vector2 location;
        new public Vector2 Location
        {
            get { return location; }
            set { location = value; UpdateCollisionDetection(); }
        }

        protected GameContent gameContent;
        protected SpriteFont textFont;
        protected SpriteFont textFontBold;

        protected bool RenderPBs;

        public int NumberOffset { get; set; }
        public int TimeOffset { get; set; }
        public int Plus2Offset { get; set; }
        public int DNFOffset { get; set; }
        public int xOffset { get; set; }


        protected List<Time> displayTimes; // Stores only the times that will be displayed
        protected List<Time> allTimes; // Stores the full data of the class

        public int VerticalOffset { get; set; } //Changes on scroll

        protected Rectangle Plus2Rectangle;
        protected Rectangle DNFRectangle;
        protected Rectangle XRectangle;

        bool Plus2Hover;
        bool DNFHover;
        bool XHover;

        public ContainerDisplayState DisplayState { get; set; }
        public bool DisplaySeparaterLines { get; set; }

        protected bool ButtonsEnabled;

        public event Action<Time, Time> TimeChanged;
        public event Action<Time> TimeDeleted;
        public event Action<object, Time> DisplayTime;

        /// <summary>
        /// A scroll container for displaying times, with +2, DNF, and delete buttons, or with dates.
        /// </summary>
        /// <param name="gameContent"></param>
        /// <param name="spriteBatch"></param>
        /// <param name="textFont"></param>
        /// <param name="textFontBold"></param>
        public ScrollContainer(GameContent gameContent, SpriteBatch spriteBatch, SpriteFont textFont, SpriteFont textFontBold) : base()
        {
            allTimes = new List<Time>();
            displayTimes = new List<Time>();
            VerticalOffset = 0;

            RenderPBs = false;
            Plus2Hover = false;
            DNFHover = false;
            XHover = false;
            ButtonsEnabled = true;
            DisplayState = ContainerDisplayState.Buttons;
            DisplaySeparaterLines = true;

            this.gameContent = gameContent;
            this.spriteBatch = spriteBatch;
            this.textFont = textFont;
            this.textFontBold = textFontBold;

            this.Location = new Vector2(0, 0);
            this.Size = new System.Drawing.Size(100, 100);

            this.BackColor = Color.White;
            this.ZDepth = 0.9f;

            this.NumberOffset = 40;
            this.TimeOffset = 120;
            this.Plus2Offset = 90;
            this.DNFOffset = 60;
            this.xOffset = 15;

            this.Visible = true;
            this.Enabled = true;

            this.TimeChanged += ScrollContainer_TimeChanged;
            this.TimeDeleted += ScrollContainer_TimeDeleted;
            this.DisplayTime += ScrollContainer_DisplayTime;
        }

        // These avoid weird null exceptions:
        private void ScrollContainer_DisplayTime(object arg1, Time arg2)
        {
        }
        private void ScrollContainer_TimeDeleted(Time obj)
        {
        }
        private void ScrollContainer_TimeChanged(Time arg1, Time arg2)
        {
        }


        /// <summary>
        /// Updates the collision detection of the +2, DNF, and delete buttons.
        /// </summary>
        public virtual void UpdateCollisionDetection()
        {
            if (DisplayState ==ContainerDisplayState.Buttons)
            {
                Plus2Rectangle = new Rectangle((int)Math.Round((location.X + padding) + (Size.Width - 2 * padding - Plus2Offset)), (int)Math.Round(location.Y + padding), (int)Math.Round(textFont.MeasureString("+2").X), Size.Height - 2 * padding);
                DNFRectangle = new Rectangle((int)Math.Round((location.X + padding) + (Size.Width - 2 * padding - DNFOffset)), (int)Math.Round(location.Y + padding), (int)Math.Round(textFont.MeasureString("DNF").X), Size.Height - 2 * padding);
                XRectangle = new Rectangle((int)Math.Round((location.X + padding) + (Size.Width - 2 * padding - xOffset)), (int)Math.Round(location.Y + padding), (int)Math.Round(textFont.MeasureString("X").X), Size.Height - 2 * padding);
            }
            else
            {
                Plus2Rectangle = new Rectangle(0, 0, 0, 0);
                DNFRectangle = new Rectangle(0, 0, 0, 0);
                XRectangle = new Rectangle(0, 0, 0, 0);
            }
        }

        /// <summary>
        /// Changes the scroll container's internal list of Times.
        /// </summary>
        /// <param name="times">The new list of Times.</param>
        public void ChangeData(List<Time> times)
        {
            Time[] temp = new Time[times.Count()];
            times.CopyTo(temp);

            this.allTimes = temp.ToList();
            UpdateDisplayTimes();
        }

        /// <summary>
        /// Toggles whether to render all times or only PB's
        /// </summary>
        /// <param name="value"></param>
        public void SetPBRender(bool value)
        {
            RenderPBs = value;
            UpdateDisplayTimes();
        }

        /// <summary>
        /// Updates the list of times to be displayed based on whether all times or only PB's are displaying.
        /// </summary>
        public virtual void UpdateDisplayTimes()
        {
            if (!RenderPBs)
            {
                this.displayTimes = new List<Time>();
                this.displayTimes = this.allTimes;
            }
            else
            {
                this.displayTimes = new List<Time>();
                for (int i = 0; i < allTimes.Count(); i++)
                {
                    if (allTimes[i].BackColor == Constants.GetColor("TimeBoxPBColor"))
                    {
                        displayTimes.Add(allTimes[i]);
                    }
                }
            }
        }

        protected Point oldMousePosition;
        /// <summary>
        /// Updates the Scroll Container.
        /// </summary>
        /// <param name="newMouseState">The current mouse state.</param>
        /// <param name="oldMouseState">The mouse state from last tick.</param>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public virtual void Update(MouseState newMouseState, MouseState oldMouseState, GameTime gameTime)
        {
            Update(newMouseState, oldMouseState, gameTime, displayTimes.Count());
        }
        /// <summary>
        /// Updates the Scroll Container.
        /// </summary>
        /// <param name="newMouseState">The current mouse state.</param>
        /// <param name="oldMouseState">The mouse state from last tick.</param>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        /// <param name="numberOfEntries">The number of times to be displayed.</param>
        public virtual void Update(MouseState newMouseState, MouseState oldMouseState, GameTime gameTime, int numberOfEntries)
        {
            if (!Visible)
            {
                return;
            }

            bool newMouseIn = newMouseState.X >= Location.X + padding && newMouseState.X <= Location.X + Size.Width - padding && newMouseState.Y >= Location.Y + padding && newMouseState.Y <= Location.Y + Size.Height - padding;

            if (newMouseIn)
            {
                VerticalOffset += (int)Math.Round((oldMouseState.ScrollWheelValue - newMouseState.ScrollWheelValue) * gameTime.ElapsedGameTime.TotalSeconds * 60d * Constants.ScrollSpeed);
                VerticalOffset = Math.Max(0, VerticalOffset);
                VerticalOffset = Math.Min(VerticalOffset, Math.Max(0, numberOfEntries * timeHeight - (Size.Height - 2 * padding)));

                if (Enabled)
                {
                    Plus2Hover = Plus2Rectangle.Contains(newMouseState.Position);
                    DNFHover = DNFRectangle.Contains(newMouseState.Position);
                    XHover = XRectangle.Contains(newMouseState.Position);

                    if (newMouseState.LeftButton == ButtonState.Released && oldMouseState.LeftButton == ButtonState.Pressed)
                    {
                        if (ButtonsEnabled && DisplayState == ContainerDisplayState.Buttons && (Plus2Hover || DNFHover || XHover))
                        {
                            UpdateAdjustableMetadata(newMouseState.Position);
                        }
                        else
                        {
                            DisplayTimeData(newMouseState.Position);
                        }
                    }
                }
            }


            oldMousePosition = newMouseState.Position;
        }
        /// <summary>
        /// Updates or deletes times based on which button (+2, DNF, delete) was clicked.
        /// </summary>
        /// <param name="mousePosition">The position of the mouse at the time of the click event.</param>
        private void UpdateAdjustableMetadata(Point mousePosition)
        {
            int mouseHeight = GetMouseHeight(mousePosition);

            if (displayTimes.Count() <= mouseHeight)
            {
                return;
            }

            Time oldTime = displayTimes[mouseHeight];
            if (Plus2Hover)
            {
                displayTimes[mouseHeight] = new Time(displayTimes[mouseHeight].TrueMilliseconds, !displayTimes[mouseHeight].Plus2, displayTimes[mouseHeight].DNF, displayTimes[mouseHeight].DateRecorded, displayTimes[mouseHeight].Puzzle, displayTimes[mouseHeight].TextColor, Constants.GetColor("TimeBoxDefaultColor"), displayTimes[mouseHeight].Scramble, displayTimes[mouseHeight].Comments);
                TimeChanged(displayTimes[mouseHeight], oldTime);
            }
            else if (DNFHover)
            {
                displayTimes[mouseHeight] = new Time(displayTimes[mouseHeight].TrueMilliseconds, displayTimes[mouseHeight].Plus2, !displayTimes[mouseHeight].DNF, displayTimes[mouseHeight].DateRecorded, displayTimes[mouseHeight].Puzzle, displayTimes[mouseHeight].TextColor, Constants.GetColor("TimeBoxDefaultColor"), displayTimes[mouseHeight].Scramble, displayTimes[mouseHeight].Comments);
                TimeChanged(displayTimes[mouseHeight], oldTime);
            }
            else if (XHover)
            {
                displayTimes.RemoveAt(mouseHeight);
                TimeDeleted(oldTime);
            }
        }

        /// <summary>
        /// Displays the time statistics window for the time under the cursor.
        /// </summary>
        /// <param name="mousePosition">The mouse position at the time of the click event.</param>
        private void DisplayTimeData(Point mousePosition)
        {
            int mouseHeight = GetMouseHeight(mousePosition);

            if (displayTimes.Count() > mouseHeight)
            {
                DisplayTime(this, displayTimes[mouseHeight]);
            }
        }

        /// <summary>
        /// Gets the mouse height within the scroll container.
        /// </summary>
        /// <param name="mousePosition">The current mouse position (relative to the entire screen).</param>
        /// <returns></returns>
        private int GetMouseHeight(Point mousePosition)
        {
            return (int)Math.Floor((mousePosition.Y - Location.Y - padding + VerticalOffset) / timeHeight);
        }

        /// <summary>
        /// Draws the scroll container.
        /// </summary>
        public override void Draw()
        {
            if (!Visible)
            {
                return;
            }

            float drawOrder = ZDepth + 4 * Constants.SpriteLevelDepth;

            spriteBatch.Draw(gameContent.buttonCorner, Location, null, BackColor, 0.0f, new Vector2(0, 0), 1.0f, SpriteEffects.None, drawOrder);
            spriteBatch.Draw(gameContent.buttonCorner, new Vector2(Location.X + Size.Width, Location.Y), null, BackColor, (float)Math.PI / 2.0f, new Vector2(0, 0), 1.0f, SpriteEffects.None, drawOrder);
            spriteBatch.Draw(gameContent.buttonCorner, new Vector2(Location.X + Size.Width, Location.Y + Size.Height), null, BackColor, (float)Math.PI, new Vector2(0, 0), 1.0f, SpriteEffects.None, drawOrder);
            spriteBatch.Draw(gameContent.buttonCorner, new Vector2(Location.X, Location.Y + Size.Height), null, BackColor, 3.0f * ((float)Math.PI / 2.0f), new Vector2(0, 0), 1.0f, SpriteEffects.None, drawOrder);

            spriteBatch.Draw(gameContent.buttonPixel, new Vector2(Location.X, Location.Y + Constants.CornerSize), null, BackColor, 0.0f, Vector2.Zero, new Vector2(Size.Width, Size.Height - 2 * Constants.CornerSize), SpriteEffects.None, drawOrder);
            spriteBatch.Draw(gameContent.buttonPixel, new Vector2(Location.X + Constants.CornerSize, Location.Y), null, BackColor, 0.0f, Vector2.Zero, new Vector2(Size.Width - 2 * Constants.CornerSize, Size.Height), SpriteEffects.None, drawOrder);

            DrawTimes();
        }

        protected const int padding = 20;
        protected const int timeHeight = 20;
        protected const int timeGap = 2;
        protected const int textFromTop = 2;

        /// <summary>
        /// Draws all of the times currently visible on screen.
        /// </summary>
        protected virtual void DrawTimes()
        {
            // Sets the number offset so that where there are numbers of different lengths on screen (ie 99 and 100), the longer one is still fully within the time box
            NumberOffset = 10 * Math.Max(3, (displayTimes.Count() - Math.Max(0, VerticalOffset / timeHeight)).ToString().Length + 1);
            bool hoverIntersect;
            for (int i = Math.Max(0, VerticalOffset / timeHeight); i < displayTimes.Count(); i++)
            {
                if (padding + i * timeHeight - VerticalOffset > this.Size.Height - padding)
                {
                    break;
                }

                hoverIntersect = new Rectangle((int)Math.Round(Location.X + padding), (int)Math.Round(Location.Y + padding + i * timeHeight - VerticalOffset), this.Size.Width - 2 * padding, timeHeight).Contains(oldMousePosition);

                if (DisplaySeparaterLines && (i == 4 || i == 11 || i == 19 || i == 49 || i == 99 || i == 999)) // TODO: link these to the constant in Game1
                {
                    spriteBatch.Draw(gameContent.buttonPixel, new Vector2(Location.X + padding, Location.Y + padding + i * timeHeight - VerticalOffset), null, Constants.GetColor("TimeBoxDividerColor"), 0.0f, Vector2.Zero, new Vector2(this.Size.Width - 2 * padding, timeHeight), SpriteEffects.None, ZDepth + 3.5f * Constants.SpriteLevelDepth);
                }

                DrawTimeBox(displayTimes[i], new Vector2(Location.X + padding, Location.Y + padding + i * timeHeight - VerticalOffset), this.Size.Width - 2 * padding, displayTimes[i].BackColor == null ? Constants.GetColor("TimeBoxDefaultColor") : (Color)displayTimes[i].BackColor, DisplayState == ContainerDisplayState.Buttons ? displayTimes.Count() - i : i + 1, hoverIntersect & Plus2Hover, hoverIntersect & DNFHover, hoverIntersect & XHover);
            }

            // Re-draws the top and bottom bars of the scrollbox so that the time boxes appear to be moving under the edges as they scroll
            spriteBatch.Draw(gameContent.buttonPixel, new Vector2(Location.X + padding, Location.Y), null, BackColor, 0.0f, Vector2.Zero, new Vector2(this.Size.Width - 2 * padding, padding), SpriteEffects.None, ZDepth + 1 * Constants.SpriteLevelDepth);
            spriteBatch.Draw(gameContent.buttonPixel, new Vector2(Location.X + padding, Location.Y + this.Size.Height - padding), null, BackColor, 0.0f, Vector2.Zero, new Vector2(this.Size.Width - 2 * padding, padding), SpriteEffects.None, ZDepth + 1 * Constants.SpriteLevelDepth);
        }

        /// <summary>
        /// Draws the Time, its number, the +2, DNF, and delete buttons, or the date.
        /// </summary>
        /// <param name="time">The Time to be displayed.</param>
        /// <param name="location">The screen position to draw at.</param>
        /// <param name="width">The width of the time box.</param>
        /// <param name="timeBoxColor">The background colour of the time box.</param>
        /// <param name="number">The rank number of the Time.</param>
        /// <param name="plus2Hover">Whether the mouse is over the +2 button.</param>
        /// <param name="dnfHover">Whether the mouse is over the DNF button.</param>
        /// <param name="xHover">Whether the mouse is over the delete button.</param>
        private void DrawTimeBox(Time time, Vector2 location, int width, Color timeBoxColor, int number, bool plus2Hover, bool dnfHover, bool xHover)
        {
            spriteBatch.Draw(gameContent.buttonPixel, location, null, timeBoxColor, 0.0f, Vector2.Zero, new Vector2(width, timeHeight - timeGap), SpriteEffects.None, ZDepth + 3 * Constants.SpriteLevelDepth);

            Vector2 stringSpace = textFont.MeasureString(number.ToString() + ":");
            spriteBatch.DrawString(textFont, number.ToString() + ":", new Vector2((float)Math.Round(location.X + NumberOffset - stringSpace.X), (float)Math.Round(location.Y + textFromTop)), Constants.GetColor("NumberTextColor"), 0.0f, new Vector2(0, 0), 1.0f, SpriteEffects.None, ZDepth + 2 * Constants.SpriteLevelDepth);

            if (time.Comments != "")
            {
                // Display the "has comment" icon
                spriteBatch.Draw(gameContent.commentIcon, new Vector2(location.X + NumberOffset + 8, location.Y + textFromTop), null, Constants.GetColor("CommentIconColor"), 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, ZDepth + 2 * Constants.SpriteLevelDepth);
            }

            string text = DataProcessing.ConvertMillisecondsToString(time.Milliseconds);

            if (DisplayState == ContainerDisplayState.Buttons)
            {
                stringSpace = textFontBold.MeasureString(text);
                stringSpace = new Vector2(stringSpace.X / 4.0f, stringSpace.Y / 4.0f); //The bold font is downscaled to increase anti-aliasing quality
                spriteBatch.DrawString(textFontBold, text, new Vector2((float)Math.Round(location.X + Size.Width - 2 * padding - TimeOffset - stringSpace.X), (float)Math.Round(location.Y + textFromTop)), Constants.GetColor("TimeTextDefaultColor"), 0.0f, new Vector2(0, 0), 0.25f, SpriteEffects.None, ZDepth + 2 * Constants.SpriteLevelDepth);


                Color plus2Color = Constants.GetColor("Plus2TextColor");
                if (time.Plus2) { plus2Color = Constants.GetColor("Plus2ToggleColor"); }
                if (plus2Hover) { plus2Color = Constants.GetColor("Plus2HoverColor"); }
                spriteBatch.DrawString(textFont, "+2", new Vector2((float)Math.Round(location.X + Size.Width - 2 * padding - Plus2Offset), (float)Math.Round(location.Y + textFromTop)), plus2Color, 0.0f, new Vector2(0, 0), 1.0f, SpriteEffects.None, ZDepth + 2 * Constants.SpriteLevelDepth);


                Color dnfColor = Constants.GetColor("Plus2TextColor");
                if (time.DNF) { dnfColor = Constants.GetColor("Plus2ToggleColor"); }
                if (dnfHover) { dnfColor = Constants.GetColor("Plus2HoverColor"); }
                spriteBatch.DrawString(textFont, "DNF", new Vector2((float)Math.Round(location.X + Size.Width - 2 * padding - DNFOffset), (float)Math.Round(location.Y + textFromTop)), dnfColor, 0.0f, new Vector2(0, 0), 1.0f, SpriteEffects.None, ZDepth + 2 * Constants.SpriteLevelDepth);


                Color xColor = Constants.GetColor("XTextColor");
                if (xHover) { xColor = Constants.GetColor("XHoverColor"); }
                spriteBatch.DrawString(textFontBold, "X", new Vector2((float)Math.Round(location.X + Size.Width - 2 * padding - xOffset), (float)Math.Round(location.Y + textFromTop)), xColor, 0.0f, new Vector2(0, 0), 0.25f, SpriteEffects.None, ZDepth + 2 * Constants.SpriteLevelDepth);
            }
            else if (DisplayState == ContainerDisplayState.Dates)
            {
                if (time.Plus2) { text += " (+2)"; }
                if (time.DNF) { text = "DNF"; }

                stringSpace = textFontBold.MeasureString(text);
                stringSpace = new Vector2(stringSpace.X / 4.0f, stringSpace.Y / 4.0f); //The bold font is downscaled to increase anti-aliasing quality
                spriteBatch.DrawString(textFontBold, text, new Vector2((float)Math.Round(location.X + Size.Width - 2 * padding - TimeOffset - stringSpace.X), (float)Math.Round(location.Y + textFromTop)), time.TextColor, 0.0f, new Vector2(0, 0), 0.25f, SpriteEffects.None, ZDepth + 2 * Constants.SpriteLevelDepth);

                stringSpace = textFont.MeasureString(time.DateRecorded.Date.ToString("MMM. dd, yyyy"));
                spriteBatch.DrawString(textFont, time.DateRecorded.Date.ToString("MMM. dd, yyyy"), new Vector2((float)Math.Round(location.X + Size.Width - 2 * padding - stringSpace.X) - 2, (float)Math.Round(location.Y + textFromTop)), Constants.GetColor("DateTextColor"), 0.0f, new Vector2(0, 0), 1.0f, SpriteEffects.None, ZDepth + 2 * Constants.SpriteLevelDepth);
            }
        }
    }
}