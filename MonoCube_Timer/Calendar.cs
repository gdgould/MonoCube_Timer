using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoCube_Timer
{
    class Calendar : Control
    {
        /// <summary>
        /// Calendar displays S M T W Th F S
        /// </summary>

        private DateTime previousDate;
        public DateTime Date { get; set; }

        private GameContent gameContent;
        public Color TextColor { get; set; }
        public Color HighlightColor { get; set; }
        public System.Drawing.Size MaxSize
        {
            get
            {
                // 8 rows by 7 columns
                return new System.Drawing.Size(7 * blockWidth + 6 * buffer, 8 * blockHeight + 7 * buffer);
            }
        }
        public System.Drawing.Size Size
        {
            get
            {
                return new System.Drawing.Size(7 * blockWidth + 6 * buffer, (rows + 2) * blockHeight + (rows + 1) * buffer);
            }
        }

        private int rows;
        private Rectangle leftArrow;
        private Rectangle rightArrow;
        private Rectangle yearBox;
        private Rectangle[] dayBoxes;
        private bool skipByYear; // Makes the left and right arrows move by year instead of month.  Activates when control is pressed.

        public Action<object, long> DateChanged;

        /// <summary>
        /// A Calendar for selecting dates.
        /// </summary>
        /// <param name="gameContent"></param>
        /// <param name="spriteBatch"></param>
        /// <param name="location"></param>
        /// <param name="date">The date to initially display on the calendar.</param>
        public Calendar(GameContent gameContent, SpriteBatch spriteBatch, Vector2 location, DateTime date) : base()
        {
            this.gameContent = gameContent;
            this.spriteBatch = spriteBatch;
            this.Location = location;

            this.BackColor = Constants.GetColor("CalendarSquareColor");
            this.HighlightColor = Constants.GetColor("CalendarHighlightColor");
            this.TextColor = Constants.GetColor("CalendarTextColor");

            this.ZDepth = 0.2f;
            this.Visible = true;
            this.Enabled = true;
            this.Date = date;
            if (date > new DateTime(5000,1,1))
            {
                this.previousDate = date.AddYears(-1);
            }
            else
            {
                this.previousDate = date.AddYears(1);
            }

            this.rows = 6;
            this.leftArrow = new Rectangle();
            this.rightArrow = new Rectangle();
            this.yearBox = new Rectangle();
            this.dayBoxes = new Rectangle[32];
        }


        /// <summary>
        /// Updates the calendar.
        /// </summary>
        /// <param name="newMouseState">The current mouse state.</param>
        /// <param name="oldMouseState">The mouse state from last tick.</param>
        /// <param name="newKeyboardState">The current keyboard state.</param>
        public void Update(MouseState newMouseState, MouseState oldMouseState, KeyboardState newKeyboardState)
        {
            int daysInMonth = DateTime.DaysInMonth(this.Date.Year, this.Date.Month);

            this.rows = (int)Math.Ceiling((daysInMonth + (int)this.Date.DayOfWeek) / 7d);

            this.leftArrow = new Rectangle((int)Location.X, (int)Location.Y, blockWidth, blockHeight);
            this.rightArrow = new Rectangle((int)Location.X + 6 * (blockWidth + buffer), (int)Location.Y, blockWidth, blockHeight);
            this.yearBox = new Rectangle((int)Location.X + (blockWidth + buffer), (int)Location.Y, 5 * blockWidth + 4 * buffer, blockHeight);


            if (!Enabled)
            {
                return;
            }
            this.skipByYear = newKeyboardState.IsKeyDown(Keys.LeftControl) || newKeyboardState.IsKeyDown(Keys.RightControl);

            if (newMouseState.LeftButton == ButtonState.Released && oldMouseState.LeftButton == ButtonState.Pressed)
            {
                for (int i = 1; i < dayBoxes.Length; i++)
                {
                    if (this.dayBoxes[i].Contains(newMouseState.Position))
                    {
                        this.Date = new DateTime(this.Date.Year, this.Date.Month, i);
                        break;
                    }
                }

                if (this.leftArrow.Contains(newMouseState.Position))
                {
                    this.Date = this.skipByYear ? this.Date.AddYears(-1) : this.Date.AddMonths(-1);
                }
                else if (this.rightArrow.Contains(newMouseState.Position))
                {
                    this.Date = this.skipByYear ? this.Date.AddYears(1) : this.Date.AddMonths(1);
                }
                else if (this.yearBox.Contains(newMouseState.Position))
                {
                    this.Date = DateTime.Today;
                }
            }
            if (this.Date != this.previousDate)
            {
                DateChanged(this, this.Index);
            }

            if (this.Date.Month != this.previousDate.Month || this.Date.Year != this.previousDate.Year)
            {
                daysInMonth = DateTime.DaysInMonth(this.Date.Year, this.Date.Month);
                int day = -(int)(new DateTime(this.Date.Year, this.Date.Month, 1).DayOfWeek) + 1;

                for (int i = 2; i <= rows + 2; i++)
                {
                    for (int j = 0; j < 7; j++, day++)
                    {
                        if (day >= 1 && day <= daysInMonth)
                        {
                            dayBoxes[day] = new Rectangle((int)Math.Round(Location.X + j * (blockWidth + buffer)), (int)Math.Round(Location.Y + i * (blockHeight + buffer)), blockWidth, blockHeight);
                        }
                    }
                }
            }

            this.previousDate = this.Date;

        }

        private const int blockWidth = 32;
        private const int blockHeight = 26;
        private const int buffer = 6;

        /// <summary>
        /// Draws the calendar
        /// </summary>
        public override void Draw()
        {
            if (!Visible)
            {
                return;
            }

            // Arrows
            DrawSquareWithText(this.skipByYear ? "<<" : "<", this.leftArrow, gameContent.menuTitleFont, this.BackColor);
            DrawSquareWithText(this.skipByYear ? ">>" : ">", this.rightArrow, gameContent.menuTitleFont, this.BackColor);

            // Year
            DrawSquareWithText(this.Date.ToString("MMMMMMMMMMM yyyy"), this.yearBox, gameContent.menuTitleFont, this.BackColor);

            // Days of the week
            string[] text = new string[7] { "Su", "Mo", "Tu", "We", "Th", "Fr", "Sa"};
            for (int i = 0; i < 7; i++)
            {
                DrawSquareWithText(text[i], new Rectangle((int)Math.Round(Location.X + i * (blockWidth + buffer)), (int)Math.Round(Location.Y + (blockHeight + buffer)), blockWidth, blockHeight), gameContent.menuFont, this.BackColor);
            }


            for (int i = 1; i <= DateTime.DaysInMonth(this.Date.Year, this.Date.Month); i++)
            {
                DrawSquareWithText(i.ToString(), dayBoxes[i], gameContent.menuTitleFont, i == this.Date.Day ? this.HighlightColor : this.BackColor);
            }
        }

        /// <summary>
        /// Draws a single calendar cell with the specified text.
        /// </summary>
        /// <param name="text">The text to display on the cell.</param>
        /// <param name="location">The location and dimensions of the cell.</param>
        /// <param name="sf">The font to display the text in.</param>
        /// <param name="color">The color of the cell's background.</param>
        private void DrawSquareWithText(string text, Rectangle location, SpriteFont sf, Color color)
        {
            spriteBatch.Draw(gameContent.buttonPixel, new Vector2(location.X, location.Y), null, color, 0.0f, Vector2.Zero, new Vector2(location.Width, location.Height), SpriteEffects.None, ZDepth + 2 * Constants.SpriteLevelDepth);

            int heightNoDescender = (int)Math.Round(DataProcessing.SpecialMeasureText("A", sf).Y);
            Vector2 size = DataProcessing.SpecialMeasureText(text, sf);
            spriteBatch.DrawString(sf, text, new Vector2((int)Math.Round(location.X + (location.Width - size.X) / 2f), (int)Math.Round(location.Y + (location.Height - heightNoDescender) / 2f)), TextColor, 0.0f, new Vector2(0, 0), 1.0f, SpriteEffects.None, ZDepth);
        }

    }
}
