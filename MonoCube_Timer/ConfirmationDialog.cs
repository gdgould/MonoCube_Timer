using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoCube_Timer
{
    public enum DialogResult { OK = 0, Cancel = 1 }
    class ConfirmationDialog : PopupWindow
    {
        public string Message { get; set; }

        public event Action<object, long, DialogResult> CloseResult;

        private Button OkButton;
        private Button CancelButton;

        /// <summary>
        /// Creates, but does not display, a new confirmation dialog box.
        /// </summary>
        /// <param name="gameContent"></param>
        /// <param name="spriteBatch"></param>
        /// <param name="location"></param>
        public ConfirmationDialog(GameContent gameContent, SpriteBatch spriteBatch, Vector2 location) : base(gameContent, spriteBatch, location, new System.Drawing.Size(500, 300))
        {
            this.Visible = false;
            this.Enabled = false;
            this.Location = location;

            this.Closing += ConfirmationDialog_Closing;
            InitializeButtons();
        }

        /// <summary>
        /// Triggered when the dialog is closed with the X button.  Acts as though cancel had been pressed instead.
        /// </summary>
        /// <param name="obj"></param>
        private void ConfirmationDialog_Closing(object obj)
        {
            Button_Click(CancelButton, CancelButton.Index);
        }

        /// <summary>
        /// Sets up the Ok and Cancel buttons, along with click events
        /// </summary>
        private void InitializeButtons()
        {
            SetupButton(ref OkButton);
            OkButton.SetText("Ok");
            OkButton.Click += Button_Click;


            SetupButton(ref CancelButton);
            CancelButton.SetText("Cancel");
            CancelButton.Click += Button_Click;

            UpdateButtonLocation();
        }
        /// <summary>
        /// Initializes everything that is the same for both buttons
        /// </summary>
        /// <param name="b">The button to be initialized</param>
        private void SetupButton(ref Button b)
        {
            b = new Button(gameContent, spriteBatch, gameContent.menuTitleFont)
            {
                ZDepth = this.ZDepth - 10 * Constants.SpriteLevelDepth,

                BackColor = Constants.GetColor("ButtonBackColor"),
                BorderColor = Constants.GetColor("ButtonBorderColor"),
                TextColor = Constants.GetColor("TabTextColor"),

                ToggleOnClick = false,
                IsToggled = false,
                Visible = true,
                Enabled = true,
            };
        }

        /// <summary>
        /// Updates the button's locations based on that of the dialog window
        /// </summary>
        private void UpdateButtonLocation()
        {
            OkButton.Location = new Vector2(this.Location.X + padding, this.Location.Y + this.Size.Height - padding - OkButton.Height);
            CancelButton.Location = new Vector2(this.Location.X + this.Size.Width - padding - CancelButton.Width, this.Location.Y + this.Size.Height - padding - CancelButton.Height);
        }

        /// <summary>
        /// Triggered when one of the buttons is pressed
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private void Button_Click(object arg1, long arg2)
        {
            if (arg2 == OkButton.Index)
            {
                CloseResult(this, this.Index, DialogResult.OK);
            }
            if (arg2 == CancelButton.Index)
            {
                CloseResult(this, this.Index, DialogResult.Cancel);
            }

            this.Visible = false;
            this.Enabled = false;
        }

        /// <summary>
        /// Shows the dialog box, with the given message displayed
        /// </summary>
        /// <param name="message">The message to be displayed</param>
        public void Show(string message)
        {
            UpdateButtonLocation();
            if (!Visible)
            {
                this.Message = message;
            }
            this.Visible = true;
            this.Enabled = true;
        }

        /// <summary>
        /// Updates the dialog
        /// </summary>
        /// <param name="newMouseState">The current mouseState</param>
        /// <param name="oldMouseState">The previous mouseState</param>
        /// <param name="newKeyboardState">The current keyboardState</param>
        /// <param name="oldKeyboardState">Thep previous keyboardState</param>
        /// <param name="gameTime">The Game's gameTime</param>
        /// <param name="windowHasFocus">A bool indicating whether the game window has focus</param>
        public override void Update(MouseState newMouseState, MouseState oldMouseState, KeyboardState newKeyboardState, KeyboardState oldKeyboardState, GameTime gameTime, bool windowHasFocus)
        {
            if (!Enabled)
            {
                return;
            }

            base.Update(newMouseState, oldMouseState, newKeyboardState, oldKeyboardState, gameTime, windowHasFocus);

            OkButton.Update(newMouseState, oldMouseState);
            CancelButton.Update(newMouseState, oldMouseState);
        }

        const int padding = 40;

        /// <summary>
        /// Draws the dialog
        /// </summary>
        public override void Draw()
        {
            if (!Visible)
            {
                return;
            }

            base.Draw();


            //Draw the message, one line at a time

            if (Message != null && Message != "")
            {
                string[] messageLines = DataProcessing.DisplayString(Message, this.Size.Width - 2 * padding, gameContent.menuTitleFont);
                gameContent.menuTitleFont.LineSpacing = 0;

                for (int i = 0; i < messageLines.Length; i++)
                {
                    Vector2 lineLength = gameContent.menuTitleFont.MeasureString(messageLines[i]);

                    if (i * (lineLength.Y) + 20 >= this.Size.Height - 20 - CancelButton.Height)
                    {
                        break;
                    }

                    spriteBatch.DrawString(gameContent.menuTitleFont, messageLines[i], new Vector2(this.Location.X + padding, this.Location.Y + i * (lineLength.Y) + padding), Constants.GetColor("TimerColor"));
                }
            }

            OkButton.Draw();
            CancelButton.Draw();
        }
    }
}