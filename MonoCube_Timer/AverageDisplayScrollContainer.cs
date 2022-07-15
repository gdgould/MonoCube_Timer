using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace MonoCube_Timer
{
    class AverageDisplayScrollContainer : ScrollContainer
    {
        private List<Average> displayAverages;  // Can be set to contain just PBs
        private List<Average> allAverages;      // Always contains all the averages

        /// <summary>
        /// A scroll container specially designed to display Averages.  No +2, DNF, or delete buttons, but it can display date ranges.
        /// </summary>
        /// <param name="gameContent"></param>
        /// <param name="spriteBatch"></param>
        /// <param name="textFont"></param>
        /// <param name="textFontBold"></param>
        public AverageDisplayScrollContainer(GameContent gameContent, SpriteBatch spriteBatch, SpriteFont textFont, SpriteFont textFontBold) : base(gameContent, spriteBatch, textFont, textFontBold)
        {
            displayAverages = new List<Average>();
            this.ButtonsEnabled = false; //So that we cannot view the stats of averages, which have no stats currently.
        }


        /// <summary>
        /// Change the Scroll Container's internal list of Averages.
        /// </summary>
        /// <param name="time">The new list of Averages.</param>
        public void ChangeData(List<Average> time)
        {
            this.allAverages = time;
            UpdateDisplayTimes();
        }

        /// <summary>
        /// Updates the internal field that controls what averages are displayed, independent of what averages are being stored.
        /// </summary>
        public override void UpdateDisplayTimes()
        {
            // RenderPBs allows us to only display averages that are pbs
            if (!RenderPBs)
            {
                this.displayAverages = new List<Average>();
                this.displayAverages = this.allAverages;
            }
            else
            {
                this.displayAverages = new List<Average>();
                for (int i = 0; i < allAverages.Count(); i++)
                {
                    if (allAverages[i].BackColor == Constants.GetColor("TimeBoxPBColor"))
                    {
                        displayAverages.Add(allAverages[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the number of averages being displayed.
        /// </summary>
        /// <returns></returns>
        public int GetLengthofAverages()
        {
            return displayAverages.Count();
        }


        /// <summary>
        /// Draws all of the averages currently visible on screen.
        /// </summary>
        protected override void DrawTimes()
        {
            // Sets the number offset so that where there are numbers of different lengths on screen (ie 99 and 100), the longer one is still fully within the time box
            NumberOffset = 10 * Math.Max(3, ((VerticalOffset + Size.Height - 2 * padding) / timeHeight + 1).ToString().Length + 1);

            for (int i = Math.Max(0, VerticalOffset / timeHeight); i < displayAverages.Count(); i++)
            {
                if (padding + i * timeHeight - VerticalOffset > this.Size.Height - padding)
                {
                    break;
                }

                DrawTimeBox(displayAverages[i], new Vector2(Location.X + padding, Location.Y + padding + i * timeHeight - VerticalOffset), this.Size.Width - 2 * padding, displayAverages[i].BackColor == null ? Constants.GetColor("TimeBoxDefaultColor") : (Color)displayAverages[i].BackColor, i + 1);
            }

            // Re-draws the top and bottom bars of the scrollbox so that the time boxes appear to be moving under the edges as they scroll
            spriteBatch.Draw(gameContent.buttonPixel, new Vector2(Location.X + padding, Location.Y), null, BackColor, 0.0f, Vector2.Zero, new Vector2(this.Size.Width - 2 * padding, padding), SpriteEffects.None, ZDepth + 1 * Constants.SpriteLevelDepth);
            spriteBatch.Draw(gameContent.buttonPixel, new Vector2(Location.X + padding, Location.Y + this.Size.Height - padding), null, BackColor, 0.0f, Vector2.Zero, new Vector2(this.Size.Width - 2 * padding, padding), SpriteEffects.None, ZDepth + 1 * Constants.SpriteLevelDepth);
        }

        /// <summary>
        /// Draws the Average, its number, and its date range.
        /// </summary>
        /// <param name="average">The Average to be displayed.</param>
        /// <param name="location">The screen position to draw at.</param>
        /// <param name="width">The width of the time box.</param>
        /// <param name="timeBoxColor">The background colour of the time box.</param>
        /// <param name="number">The rank number of the Average.</param>
        private void DrawTimeBox(Average average, Vector2 location, int width, Color timeBoxColor, int number)
        {
            // Draw the box
            spriteBatch.Draw(gameContent.buttonPixel, location, null, timeBoxColor, 0.0f, Vector2.Zero, new Vector2(width, timeHeight - timeGap), SpriteEffects.None, ZDepth + 3 * Constants.SpriteLevelDepth);

            // Draw the index number
            Vector2 stringSpace = textFont.MeasureString(number.ToString() + ":");
            spriteBatch.DrawString(textFont, number.ToString() + ":", new Vector2((float)Math.Round(location.X + NumberOffset - stringSpace.X), (float)Math.Round(location.Y + textFromTop)), Constants.GetColor("NumberTextColor"), 0.0f, new Vector2(0, 0), 1.0f, SpriteEffects.None, ZDepth + 2 * Constants.SpriteLevelDepth);

            // Draw the string
            string text = DataProcessing.ConvertMillisecondsToString(average.Milliseconds);
            if (average.DNF)
            {
                text = "DNF";
            }
            stringSpace = textFontBold.MeasureString(text);
            stringSpace = new Vector2(stringSpace.X / 4.0f, stringSpace.Y / 4.0f); //The bold font is downscaled to increase anti-aliasing quality
            spriteBatch.DrawString(textFontBold, text, new Vector2((float)Math.Round(location.X + Size.Width - 2 * padding - TimeOffset - stringSpace.X), (float)Math.Round(location.Y + textFromTop)), average.TextColor, 0.0f, new Vector2(0, 0), 0.25f, SpriteEffects.None, ZDepth + 2 * Constants.SpriteLevelDepth);

            // Draw the date or date range.  Width is limited, so we display less specific information if the dates are further apart
            string dateDisplay = "";
            if (average.StartDate.Date == average.EndDate.Date)
            {
                dateDisplay = average.StartDate.Date.ToString("MMM. dd, yyyy");
            }
            else if (average.StartDate.Month == average.EndDate.Month && average.StartDate.Year == average.EndDate.Year)
            {
                dateDisplay = average.StartDate.Date.ToString("MMM. dd") + "-" + average.EndDate.Date.ToString("dd, yyyy");
            }
            else if (average.StartDate.Year == average.EndDate.Year)
            {
                dateDisplay = average.StartDate.Date.ToString("MMM. dd") + " - " + average.EndDate.Date.ToString("MMM. dd, yyyy");
            }
            else
            {
                dateDisplay = average.StartDate.Date.ToString("MMM. yyyy") + " - " + average.EndDate.Date.ToString("MMM. yyyy");
            }

            stringSpace = textFont.MeasureString(dateDisplay);
            spriteBatch.DrawString(textFont, dateDisplay, new Vector2((float)Math.Round(location.X + Size.Width - 2 * padding - stringSpace.X - 2), (float)Math.Round(location.Y + textFromTop)), Constants.GetColor("DateTextColor"), 0.0f, new Vector2(0, 0), 1.0f, SpriteEffects.None, ZDepth + 2 * Constants.SpriteLevelDepth);
        }
    }
}