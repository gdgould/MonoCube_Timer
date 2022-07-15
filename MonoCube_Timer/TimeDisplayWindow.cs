using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoCube_Timer
{
    class TimeDisplayWindow : PopupWindow
    {
        private Time displayTime;
        public bool CanEdit;

        private Button deleteButton;
        private ConfirmationDialog confirmDelete;

        private TextBox textBox;

        public bool ShouldDeleteTime; // Avoids hiding the base Closing action

        /// <summary>
        /// A popup window for displaying the information about a Time.
        /// </summary>
        /// <param name="gameContent"></param>
        /// <param name="spriteBatch"></param>
        /// <param name="displayTime"></param>
        /// <param name="location"></param>
        /// <param name="size"></param>
        /// <param name="canEdit"></param>
        public TimeDisplayWindow(GameContent gameContent, SpriteBatch spriteBatch, Time displayTime, Vector2 location, System.Drawing.Size size, bool canEdit) : base(gameContent, spriteBatch, location, size)
        {
            this.displayTime = displayTime;
            this.ShouldDeleteTime = false;
            this.Location = location;
            this.Size = size;

            textBox = new TextBox(gameContent, spriteBatch, gameContent.menuTitleFont);
            textBox.Text = new StringBuilder(displayTime.Comments);
            textBox.ZDepth = this.ZDepth - Constants.SpriteLevelDepth;
            textBox.BlockCommas = true;

            textBox.BackColor = Constants.GetColor("ContainerColor");
            textBox.FocusColor = Constants.GetColor("TimeBoxDefaultColor");
            textBox.TextColor = Constants.GetColor("TextBoxTextColor");
            textBox.BorderColor = Constants.GetColor("ButtonBorderColor");
            textBox.Enabled = canEdit;

            InitializeButton();
            InitializeDialog();
        }

        /// <summary>
        /// Initialize the Delete Time button
        /// </summary>
        private void InitializeButton()
        {
            deleteButton = new Button(gameContent, spriteBatch, gameContent.menuTitleFont)
            {
                ZDepth = this.ZDepth - Constants.SpriteLevelDepth,

                BackColor = Constants.GetColor("ButtonBackColor"),
                BorderColor = Constants.GetColor("ButtonBorderColor"),
                TextColor = Constants.GetColor("TabTextColor"),

                ToggleOnClick = false,
                IsToggled = false,
                Visible = true,
                Enabled = true,
            };

            deleteButton.SetText("Delete Time");
            deleteButton.Location = new Vector2(this.Location.X + this.Size.Width - column1 - deleteButton.Width, this.Location.Y + column1 + 10);

            deleteButton.Click += DeleteButton_Click;
        }
        /// <summary>
        /// Initialize the confirm deletion dialog.
        /// </summary>
        private void InitializeDialog()
        {
            confirmDelete = new ConfirmationDialog(gameContent, spriteBatch, new Vector2(0, 0));
            confirmDelete.Location = new Vector2(this.Location.X + (this.Size.Width - confirmDelete.Size.Width) / 2, this.Location.Y + (this.Size.Height - confirmDelete.Size.Height) / 2);
            confirmDelete.ZDepth = this.ZDepth - 10 * Constants.SpriteLevelDepth;


            confirmDelete.CloseResult += ConfirmDelete_CloseResult;
        }
        private void DeleteButton_Click(object arg1, long arg2)
        {
            confirmDelete.Show("Are you sure you want to irreversibly delete this time?  Warning, averages will not update until the program is restarted.");
        }
        private void ConfirmDelete_CloseResult(object arg1, long arg2, DialogResult arg3)
        {
            if (arg3 == DialogResult.OK)
            {
                this.ShouldDeleteTime = true;
                Close(this);
            }
        }


        /// <summary>
        /// Refresh the comments field textbox.
        /// </summary>
        public void RefreshText()
        {
            textBox.Text = new StringBuilder(displayTime.Comments);
            textBox.Enabled = this.CanEdit;
        }

        /// <summary>
        /// Sets the Time to display.
        /// </summary>
        /// <param name="time">The Time to display.</param>
        public void SetTime(Time time)
        {
            this.displayTime = time;
            this.ShouldDeleteTime = false;
        }


        /// <summary>
        /// Gets the comment field of the Time.
        /// </summary>
        /// <returns></returns>
        public string GetComment()
        {
            return textBox.Text.ToString();
        }

        /// <summary>
        /// Gets the Time being displayed.
        /// </summary>
        /// <returns></returns>
        public Time GetTime()
        {
            displayTime.Comments = textBox.Text.ToString();
            return displayTime;
        }

        /// <summary>
        /// Update the display window.
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

            deleteButton.Update(newMouseState, oldMouseState);
            confirmDelete.Update(newMouseState, oldMouseState, newKeyboardState, oldKeyboardState, gameTime, windowHasFocus);

            textBox.Update(newKeyboardState, oldKeyboardState, newMouseState, oldMouseState, gameTime, windowHasFocus);

            base.Update(newMouseState, oldMouseState, newKeyboardState, oldKeyboardState, gameTime, windowHasFocus);
        }

        static int column1 = 40;
        static int column2 = 160;

        /// <summary>
        /// Draws the display window.
        /// </summary>
        public override void Draw()
        {
            if (!Visible)
            {
                return;
            }

            float zDepth = this.ZDepth - Constants.SpriteLevelDepth;

            base.Draw();

            deleteButton.Location = new Vector2(this.Location.X + this.Size.Width - column1 - deleteButton.Width, this.Location.Y + column1 + 10);
            deleteButton.Draw();

            confirmDelete.Location = new Vector2(this.Location.X + (this.Size.Width - confirmDelete.Size.Width) / 2, this.Location.Y + (this.Size.Height - confirmDelete.Size.Height) / 2);
            confirmDelete.Draw();

            SpriteFont spriteFont = gameContent.menuTitleFont;


            string[] labels = { "Time:", "Penalties:", "Date:", "Scramble:" };

            for (int i = 0; i < labels.Length; i++)
            {
                spriteBatch.DrawString(spriteFont, labels[i], new Vector2(Location.X + column1, Location.Y + column1 + 50 * i), Constants.GetColor("TimeTextDefaultColor"), 0.0f, new Vector2(0, 0), 1.0f, SpriteEffects.None, ZDepth);
            }

            spriteBatch.DrawString(spriteFont, DataProcessing.ConvertMillisecondsToString(displayTime.Milliseconds), new Vector2(Location.X + column2, Location.Y + 40), Constants.GetColor("TimeTextDefaultColor"), 0.0f, new Vector2(0, 0), 1.0f, SpriteEffects.None, ZDepth);

            string penalties = "";
            if (displayTime.DNF && displayTime.Plus2)
            {
                penalties = "+2, DNF";
            }
            else if (displayTime.Plus2)
            {
                penalties = "+2";
            }
            else if (displayTime.DNF)
            {
                penalties = "DNF";
            }
            else
            {
                penalties = "No Penalties";
            }

            spriteBatch.DrawString(spriteFont, penalties, new Vector2(Location.X + column2, Location.Y + column1 + 50), Constants.GetColor("TimeTextDefaultColor"), 0.0f, new Vector2(0, 0), 1.0f, SpriteEffects.None, ZDepth);
            spriteBatch.DrawString(spriteFont, displayTime.DateRecorded.ToString("MMM. dd, yyyy   HH:mm:ss"), new Vector2(Location.X + column2, Location.Y + column1 + 100), Constants.GetColor("TimeTextDefaultColor"), 0.0f, new Vector2(0, 0), 1.0f, SpriteEffects.None, ZDepth);

            string[] lines;

            if (displayTime.Scramble == "")
            {
                lines = new string[1] { "No Scramble Data" };
            }
            else
            {
                lines = DataProcessing.DisplayString(displayTime.Scramble, Size.Width - column2 - column1, spriteFont);
            }

            spriteFont.LineSpacing = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                spriteBatch.DrawString(spriteFont, lines[i], new Vector2(Location.X + column2, Location.Y + column1 + 150 + i * 26), Constants.GetColor("TimeTextDefaultColor"), 0.0f, new Vector2(0, 0), 1.0f, SpriteEffects.None, ZDepth);
            }


            spriteBatch.DrawString(spriteFont, "Comments:", new Vector2(Location.X + column1, Location.Y + column1 + 175 + lines.Length * 26), Constants.GetColor("TimeTextDefaultColor"), 0.0f, new Vector2(0, 0), 1.0f, SpriteEffects.None, ZDepth);

            textBox.Location = new Vector2(Location.X + column2, Location.Y + column1 + 50 * 3.5f + lines.Length * 26);
            textBox.Size = new System.Drawing.Size(Size.Width - column2 - column1, Size.Height - (column1 + 175 + lines.Length * 26) - 50);

            textBox.Draw();
        }
    }
}