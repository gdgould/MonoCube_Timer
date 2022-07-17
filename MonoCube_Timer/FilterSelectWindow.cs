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
    class FilterSelectWindow : PopupWindow
    {
        private Button pbButton;
        private Button commentButton;
        private Button resetFilter;
        private Calendar startDate;
        private Calendar endDate;
        private Button resetDate;
        private bool startUnlimited; // Allows any time from the beginning of history
        private bool endUnlimited; // Allows any time to the end of history

        /// <summary>
        /// A popup window for selecting criteria to filter by.
        /// </summary>
        /// <param name="gameContent"></param>
        /// <param name="spriteBatch"></param>
        /// <param name="location"></param>
        /// <param name="size"></param>
        public FilterSelectWindow(GameContent gameContent, SpriteBatch spriteBatch, Vector2 location, System.Drawing.Size size) : base(gameContent, spriteBatch, location, size)
        {
            this.Location = location;
            this.Size = size;
            this.startUnlimited = false;
            this.endUnlimited = false;

            InitializeButtons(new Filter(false,false, DateTime.Today, DateTime.Today));
            InitializeCalendars(new Filter(false, false, DateTime.Today, DateTime.Today));
        }
        /// <summary>
        /// A popup window for selecting criteria to filter by.
        /// </summary>
        /// <param name="gameContent"></param>
        /// <param name="spriteBatch"></param>
        /// <param name="location"></param>
        /// <param name="size"></param>
        /// <param name="filter"></param>
        public FilterSelectWindow(GameContent gameContent, SpriteBatch spriteBatch, Vector2 location, System.Drawing.Size size, Filter filter) : base(gameContent, spriteBatch, location, size)
        {
            this.Location = location;
            this.Size = size;
            this.startUnlimited = false;
            this.endUnlimited = false;

            InitializeButtons(filter);
            InitializeCalendars(filter);
            SetFilter(filter);
        }

        private const int column1 = 70;
        private const int column2 = column1 + 210;
        private const int row1 = 70;
        private const int buffer = 30;
        /// <summary>
        /// Initializes the PB, Comment, Reset Filter, and Reset Date buttons.
        /// </summary>
        /// <param name="f">The filter to initially dispaly.</param>
        private void InitializeButtons(Filter f)
        {
            pbButton = new Button(gameContent, spriteBatch, gameContent.menuTitleFont)
            {
                ZDepth = this.ZDepth - Constants.SpriteLevelDepth,

                BackColor = Constants.GetColor("ButtonBackColor"),
                BorderColor = Constants.GetColor("ButtonBorderColor"),
                TextColor = Constants.GetColor("ButtonTextColor"),

                ToggleOnClick = true,
                IsToggled = f.DisplayPBOnly,
                Visible = true,
                Enabled = true,
                AutoSize = true,
            };

            pbButton.SetText("PB's: Off");
            pbButton.AutoSize = false;
            pbButton.SetText(f.DisplayPBOnly ? "PB's: On" : "PB's: Off");
            pbButton.Location = new Vector2(this.Location.X + column1, this.Location.Y + 50);
            pbButton.Click += PBButton_Click;


            commentButton = new Button(gameContent, spriteBatch, gameContent.menuTitleFont)
            {
                ZDepth = this.ZDepth - Constants.SpriteLevelDepth,

                BackColor = Constants.GetColor("ButtonBackColor"),
                BorderColor = Constants.GetColor("ButtonBorderColor"),
                TextColor = Constants.GetColor("ButtonTextColor"),

                ToggleOnClick = true,
                IsToggled = f.DisplayCommentOnly,
                Visible = true,
                Enabled = true,
                AutoSize = true,
            };

            commentButton.SetText("Comments: Off");
            commentButton.AutoSize = false;
            commentButton.SetText(f.DisplayCommentOnly ? "Comments: On" : "Comments: Off");
            commentButton.Location = new Vector2(this.Location.X + column1, this.Location.Y + 50 + pbButton.Height + buffer);
            commentButton.Click += CommentButton_Click;


            resetFilter = new Button(gameContent, spriteBatch, gameContent.menuTitleFont)
            {
                ZDepth = this.ZDepth - Constants.SpriteLevelDepth,

                BackColor = Constants.GetColor("ButtonBackColor"),
                BorderColor = Constants.GetColor("ButtonBorderColor"),
                TextColor = Constants.GetColor("ButtonTextColor"),

                ToggleOnClick = false,
                Visible = true,
                Enabled = true,
                AutoSize = true,
            };

            resetFilter.SetText("Reset Filter");
            resetFilter.Location = new Vector2(this.Location.X + column1, this.Location.Y + row1 + pbButton.Height + commentButton.Height + 2 * buffer);
            resetFilter.Click += ResetFilter_Click;

            resetDate = new Button(gameContent, spriteBatch, gameContent.menuTitleFont)
            {
                ZDepth = this.ZDepth - Constants.SpriteLevelDepth,

                BackColor = Constants.GetColor("ButtonBackColor"),
                BorderColor = Constants.GetColor("ButtonBorderColor"),
                TextColor = Constants.GetColor("ButtonTextColor"),

                ToggleOnClick = false,
                Visible = true,
                Enabled = true,
                AutoSize = true,
            };

            resetDate.SetText("All Dates");
            resetDate.Location = new Vector2(0, 0);
            resetDate.Click += ResetDate_Click;
        }
        /// <summary>
        /// Initializes the Start Date and End Date calendars.
        /// </summary>
        /// <param name="f">The filter to initially display.</param>
        private void InitializeCalendars(Filter f)
        {
            startDate = new Calendar(gameContent, spriteBatch, new Vector2(Location.X + column2, Location.Y + row1), f.MinDate)
            {
                Enabled = true,
                Visible = true,
            };

            startDate.DateChanged += StartDateChanged;

            endDate = new Calendar(gameContent, spriteBatch, new Vector2(Location.X + column2 + startDate.Size.Width + buffer, Location.Y + row1), f.MaxDate)
            {
                Enabled = true,
                Visible = true,
            };

            endDate.DateChanged += EndDateChanged;
        }

        /// <summary>
        /// Triggered when the PB Only selection button is clicked, and toggles its displayed text.
        /// </summary>
        /// <param name="arg1">The sender of the event.</param>
        /// <param name="arg2">The index number of the sender.</param>
        private void PBButton_Click(object arg1, long arg2)
        {
            pbButton.SetText(pbButton.IsToggled ? "PB's: Off" : "PB's: On");
        }
        /// <summary>
        /// Triggered when the Comments Only selection button is clicked, and toggles its displayed text.
        /// </summary>
        /// <param name="arg1">The sender of the event.</param>
        /// <param name="arg2">The index number of the sender.</param>
        private void CommentButton_Click(object arg1, long arg2)
        {
            commentButton.SetText(commentButton.IsToggled ? "Comments: Off" : "Comments: On");
        }
        /// <summary>
        /// Triggered when the Reset Date button is clicked.
        /// </summary>
        /// <param name="arg1">The sender of the event.</param>
        /// <param name="arg2">The index number of the sender.</param>
        private void ResetDate_Click(object arg1, long arg2)
        {
            startUnlimited = true;
            endUnlimited = true;
        }
        /// <summary>
        /// Triggered when the Reset Filter button is clicked, and resets all options to their defaults.
        /// </summary>
        /// <param name="arg1">The sender of the event.</param>
        /// <param name="arg2">The index number of the sender.</param>
        private void ResetFilter_Click(object arg1, long arg2)
        {
            if (pbButton.IsToggled)
            {
                PBButton_Click(null, 0);
                pbButton.IsToggled = false;
            }
            if (commentButton.IsToggled)
            {
                CommentButton_Click(null, 0);
                commentButton.IsToggled = false;
            }
            ResetDate_Click(null, 0);
        }


        /// <summary>
        /// Triggers when the start date is changed.  Moves the end date if necessary to prevent invalid date intervals.
        /// </summary>
        /// <param name="arg1">The sender of the event.</param>
        /// <param name="arg2">The index number of the sender.</param>
        private void StartDateChanged(object arg1, long arg2)
        {
            if (startDate.Date > endDate.Date)
            {
                endDate.Date = startDate.Date;
            }
            if (startDate.Date != endDate.Date)
            {
                startUnlimited = false;
            }
        }
        /// <summary>
        /// Triggers when the end date is changed.  Moves the start date if necessary to prevent invalid date intervals.
        /// </summary>
        /// <param name="arg1">The sender of the event.</param>
        /// <param name="arg2">The index number of the sender.</param>
        private void EndDateChanged(object arg1, long arg2)
        {
            if (startDate.Date > endDate.Date)
            {
                startDate.Date = endDate.Date;
            }
            if (startDate.Date != endDate.Date)
            {
                endUnlimited = false;
            }
        }

        /// <summary>
        /// Sets the window's Filter.
        /// </summary>
        /// <param name="f">The new Filter.</param>
        public void SetFilter(Filter f)
        {
            if (f.MinDate == DateTime.MinValue)
            {
                this.startUnlimited = true;
                f = new Filter(f.DisplayPBOnly, f.DisplayCommentOnly, DateTime.Today, f.MaxDate);
            }
            if (f.MaxDate == DateTime.MaxValue)
            {
                this.endUnlimited = true;
                f = new Filter(f.DisplayPBOnly, f.DisplayCommentOnly, f.MinDate, DateTime.Today);
            }

            f = new Filter(f.DisplayPBOnly, f.DisplayCommentOnly,
                    new DateTime(f.MinDate.Year, f.MinDate.Month, f.MinDate.Day),
                    new DateTime(f.MaxDate.Year, f.MaxDate.Month, f.MaxDate.Day));

            startDate.Date = f.MinDate;
            endDate.Date = f.MaxDate;

            pbButton.SetText(f.DisplayPBOnly ? "PB's: On" : "PB's: Off");
            pbButton.IsToggled = f.DisplayPBOnly;

            commentButton.SetText(f.DisplayCommentOnly ? "Comments: On" : "Comments: Off");
            commentButton.IsToggled = f.DisplayCommentOnly;
        }
        /// <summary>
        /// Gets the Filter currently being displayed.
        /// </summary>
        /// <returns></returns>
        public Filter GetFilter()
        {
            return new Filter(pbButton.IsToggled, commentButton.IsToggled,
                              startUnlimited ? DateTime.MinValue : startDate.Date,
                              endUnlimited ? DateTime.MaxValue : endDate.Date.AddDays(1).AddSeconds(-1));
        }

        /// <summary>
        /// Updates the selection window.
        /// </summary>
        /// <param name="newMouseState">The current mouse state.</param>
        /// <param name="oldMouseState">The mouse state from the previous tick.</param>
        /// <param name="newKeyboardState">The new keyboard state.</param>
        /// <param name="oldKeyboardState">The keyboard state from the previous tick.</param>
        /// <param name="gameTime">A snapshot of timing values.</param>
        /// <param name="windowHasFocus">Determines whether the main game window has focus.</param>
        public override void Update(MouseState newMouseState, MouseState oldMouseState, KeyboardState newKeyboardState, KeyboardState oldKeyboardState, GameTime gameTime, bool windowHasFocus)
        {
            if (!Enabled)
            {
                return;
            }

            pbButton.Update(newMouseState, oldMouseState);
            commentButton.Update(newMouseState, oldMouseState);
            resetDate.Update(newMouseState, oldMouseState);
            resetFilter.Update(newMouseState, oldMouseState);

            startDate.Update(newMouseState, oldMouseState, newKeyboardState);
            endDate.Update(newMouseState, oldMouseState, newKeyboardState);

            base.Update(newMouseState, oldMouseState, newKeyboardState, oldKeyboardState, gameTime, windowHasFocus);
        }

        /// <summary>
        /// Draws the selection window.
        /// </summary>
        public override void Draw()
        {
            if (!Visible)
            {
                return;
            }
            pbButton.Location = new Vector2(this.Location.X + column1, this.Location.Y + row1);
            pbButton.Draw();

            commentButton.Location = new Vector2(this.Location.X + column1, this.Location.Y + row1 + pbButton.Height + buffer);
            commentButton.Draw();

            resetFilter.Location = new Vector2(this.Location.X + column1, this.Location.Y + row1 + pbButton.Height + commentButton.Height + 2 * buffer);
            resetFilter.Draw();

            startDate.Location = new Vector2(Location.X + column2, Location.Y + row1);
            startDate.Draw();

            endDate.Location = new Vector2(Location.X + column2 + startDate.Size.Width + 2 * buffer, Location.Y + row1);
            endDate.Draw();

            resetDate.Location = new Vector2(this.Location.X + column1, this.Location.Y + row1 + startDate.MaxSize.Height + 2 * buffer);
            resetDate.Draw();

            string dates;
            if (startUnlimited && endUnlimited)
            {
                dates = "Any Time Period";
            }
            else if (startUnlimited)
            {
                dates = "On or before " + endDate.Date.ToString("MMM. dd, yyyy");
            }
            else if (endUnlimited)
            {
                dates = startDate.Date.ToString("MMM. dd, yyyy") + " or later";
            }
            else
            {
                dates = startDate.Date.ToString("MMM. dd, yyyy") + " - " + endDate.Date.ToString("MMM. dd, yyyy");
            }

            Vector2 size = DataProcessing.SpecialMeasureText(dates, gameContent.menuTitleFont);
            size = new Vector2(size.X, DataProcessing.SpecialMeasureText("A", gameContent.menuTitleFont).Y);
            spriteBatch.DrawString(gameContent.menuTitleFont, dates, new Vector2((int)Math.Round(endDate.Location.X - buffer - (size.X / 2f)), (int)Math.Round(resetDate.Location.Y + (resetDate.Size.Height - size.Y) / 2f)), Constants.GetColor("DateTextColor"), 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, this.ZDepth);

            base.Draw();
        }
    }
}
