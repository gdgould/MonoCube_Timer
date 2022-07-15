using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoCube_Timer
{
    // Invert reverses the order of display from the data list, and also reverses numbering.
    public enum ContainerDisplayState { Nothing = 0, Averages, Buttons, Dates, InvertButtons, InvertDates }
    public struct Filter
    {
        public bool DisplayPBOnly { get; }
        public bool DisplayCommentOnly { get; }
        public DateTime MinDate { get; }
        public DateTime MaxDate { get; }

        public Filter(bool displayPBOnly, bool displayCommentOnly)
        {
            this.DisplayPBOnly = displayPBOnly;
            this.DisplayCommentOnly = displayCommentOnly;
            this.MinDate = DateTime.MinValue;
            this.MaxDate = DateTime.MaxValue;
        }
        public Filter(bool displayPBOnly, bool displayCommentOnly, DateTime minDate, DateTime maxDate)
        {
            this.DisplayPBOnly = displayPBOnly;
            this.DisplayCommentOnly = displayCommentOnly;
            this.MinDate = minDate;
            this.MaxDate = maxDate;
        }
    }
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


        public int NumberOffset { get; set; }
        public int TimeOffset { get; set; }
        public int Plus2Offset { get; set; }
        public int DNFOffset { get; set; }
        public int xOffset { get; set; }

        protected List<Time> allTimes; // Stores the full data of the class (reference data)
        protected List<int> filterTimes; // Stores the indices of times to be displayed, and can be configured

        public int VerticalOffset { get; set; } //Changes on scroll

        protected Rectangle Plus2Rectangle;
        protected Rectangle DNFRectangle;
        protected Rectangle XRectangle;

        bool Plus2Hover;
        bool DNFHover;
        bool XHover;

        public ContainerDisplayState DisplayState { get; set; }
        protected Filter filter;
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
            filterTimes = new List<int>();
            VerticalOffset = 0;

            Plus2Hover = false;
            DNFHover = false;
            XHover = false;
            ButtonsEnabled = true;
            DisplayState = ContainerDisplayState.InvertButtons;
            this.filter = new Filter(false, false);
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
            if (DisplayState == ContainerDisplayState.Buttons || DisplayState == ContainerDisplayState.InvertButtons)
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
            this.allTimes = times;
            UpdateFilter();
        }


        /// <summary>
        /// Clears all display filters in place.
        /// </summary>
        public virtual void ClearFilter()
        {
            UpdateFilter(new Filter(false, false));
        }


        /// <summary>
        /// Filters displayed times by the given parameters.
        /// </summary>
        /// <param name="displayPBOnly">Specifies whether to display only times that are PB's.</param>
        /// <param name="displayCommentOnly">Specifies whether to display only times with comments.</param>
        /// <param name="minDate">Specifies the earliest date displayed times may have, inclusive.</param>
        /// <param name="maxDate">Specifies the latest date displayed times may have, inclusive.</param>
        public virtual void UpdateFilter(Filter f)
        {
            this.filter = f;
            filterTimes = new List<int>();

            for (int i = 0; i < allTimes.Count(); i++)
            {
                if ((!f.DisplayPBOnly || allTimes[i].BackColor == Constants.GetColor("TimeBoxPBColor")) &&
                    (!f.DisplayCommentOnly || allTimes[i].Comments != "") &&
                    f.MinDate <= allTimes[i].DateRecorded && f.MaxDate >= allTimes[i].DateRecorded)
                {
                    filterTimes.Add(i);
                }
            }

            if (DisplayState == ContainerDisplayState.InvertButtons || DisplayState == ContainerDisplayState.InvertDates)
            {
                filterTimes.Reverse();
            }
        }
        /// <summary>
        /// Filters displayed times by the current stored parameters.
        /// </summary>
        public virtual void UpdateFilter()
        {
            UpdateFilter(this.filter);
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
            Update(newMouseState, oldMouseState, gameTime, filterTimes.Count());
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

            if (Enabled)
            {
                VerticalOffset += (int)Math.Round((oldMouseState.ScrollWheelValue - newMouseState.ScrollWheelValue) * gameTime.ElapsedGameTime.TotalSeconds * 60d * Constants.ScrollSpeed);
                VerticalOffset = Math.Max(0, VerticalOffset);
                VerticalOffset = Math.Min(VerticalOffset, Math.Max(0, numberOfEntries * timeHeight - (Size.Height - 2 * padding)));

                if (newMouseIn)
                {
                    Plus2Hover = Plus2Rectangle.Contains(newMouseState.Position);
                    DNFHover = DNFRectangle.Contains(newMouseState.Position);
                    XHover = XRectangle.Contains(newMouseState.Position);

                    if (newMouseState.LeftButton == ButtonState.Released && oldMouseState.LeftButton == ButtonState.Pressed)
                    {
                        if (ButtonsEnabled && (Plus2Hover || DNFHover || XHover)
                             && (DisplayState == ContainerDisplayState.Buttons || DisplayState == ContainerDisplayState.InvertButtons))
                        {
                            UpdateAdjustableMetadata(newMouseState.Position);
                        }
                        else if (DisplayState != ContainerDisplayState.Averages)
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

            if (filterTimes.Count() <= mouseHeight)
            {
                return;
            }

            Time timeToEdit = allTimes[filterTimes[mouseHeight]];
            Time oldTime = timeToEdit.Copy();
            if (Plus2Hover)
            {
                timeToEdit.Plus2 = !timeToEdit.Plus2;
                TimeChanged(timeToEdit, oldTime);
            }
            else if (DNFHover)
            {
                timeToEdit.DNF = !timeToEdit.DNF;
                TimeChanged(timeToEdit, oldTime);
            }
            else if (XHover)
            {
                TimeDeleted(timeToEdit);
            }

            UpdateFilter();
        }

        /// <summary>
        /// Displays the time statistics window for the time under the cursor.
        /// </summary>
        /// <param name="mousePosition">The mouse position at the time of the click event.</param>
        private void DisplayTimeData(Point mousePosition)
        {
            int mouseHeight = GetMouseHeight(mousePosition);

            if (filterTimes.Count() > mouseHeight)
            {
                DisplayTime(this, allTimes[filterTimes[mouseHeight]]);
            }
        }

        /// <summary>
        /// Gets which item in the data list the mouse is currently over.
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
            // Inversion is done by the filter list, so no inversion calculations need be done here

            // Sets the number offset so that where there are numbers of different lengths on screen (ie 99 and 100), the longer one is still fully within the time box
            int filterTopIndex = Math.Min(filterTimes.Count() - 1, (VerticalOffset + Size.Height - 2 * padding) / timeHeight + 1);
            int filterBottomIndex = VerticalOffset / timeHeight;
            int maxDisplayedLength = 0;
            if (filterTopIndex < filterTimes.Count() && filterTopIndex >= 0)
            {
                maxDisplayedLength = filterTimes[filterTopIndex].ToString().Length;
            }
            if (filterBottomIndex < filterTimes.Count())
            {
                maxDisplayedLength = Math.Max(maxDisplayedLength, filterTimes[filterBottomIndex].ToString().Length);
            }

            NumberOffset = 10 * Math.Max(3, maxDisplayedLength + 1);
            bool hoverIntersect;

            for (int i = Math.Max(0, VerticalOffset / timeHeight); i < filterTimes.Count(); i++)
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

                DrawTimeBox(allTimes[filterTimes[i]], new Vector2(Location.X + padding, Location.Y + padding + i * timeHeight - VerticalOffset), this.Size.Width - 2 * padding, allTimes[filterTimes[i]].BackColor == null ? Constants.GetColor("TimeBoxDefaultColor") : (Color)allTimes[filterTimes[i]].BackColor, filterTimes[i] + 1, hoverIntersect & Plus2Hover, hoverIntersect & DNFHover, hoverIntersect & XHover);
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

            if (DisplayState == ContainerDisplayState.Buttons || DisplayState == ContainerDisplayState.InvertButtons)
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
            else if (DisplayState == ContainerDisplayState.Dates || DisplayState == ContainerDisplayState.InvertDates)
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