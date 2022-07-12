using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoCube_Timer
{
    class CubeSelectWindow : PopupWindow
    {
        private List<CubeCategoryBox> categories;
        private List<string> categoryText;

        private TextBox textBox;

        const int column1 = 40;
        const int textboxBottomOffset = 70;

        public event Action<object, long, string> CategorySelected;
        public event Action<object, long, string> CategoryAdded;

        /// <summary>
        /// Creates, but does not display, a Cube selection window, which allows the user to select types of cubes they have defined themselves, and to define new cube types.
        /// </summary>
        public CubeSelectWindow(GameContent gameContent, SpriteBatch spriteBatch, List<string> categories, Vector2 location, System.Drawing.Size size) : base(gameContent, spriteBatch, location, size)
        {
            this.categoryText = categories;
            SetTextLocations(categoryText);

            textBox = new TextBox(gameContent, spriteBatch, gameContent.menuTitleFont)
            {
                Text = new StringBuilder(""),

                BackColor = Constants.GetColor("ContainerColor"),
                FocusColor = Constants.GetColor("TimeBoxDefaultColor"),
                TextColor = Constants.GetColor("TextBoxTextColor"),
                BorderColor = Constants.GetColor("ButtonBorderColor"),

                maxChars = 80, // Must be limited so we don't exceed the max windows path length (264 for file names), or run out of space in the text box
                SanatizeForFilenames = true,

                Location = new Vector2(Location.X + column1, Location.Y + Size.Height - textboxBottomOffset),
                Size = new System.Drawing.Size(Size.Width - 2 * column1, textboxBottomOffset - column1),
            };
            textBox.LineReturned += AddNewCategory;
        }

        /// <summary>
        /// Adds a new category of cube to the window's list.  Triggers the CategoryAdded action.
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        private void AddNewCategory(object arg1, long arg2, string arg3)
        {
            categoryText.Add(arg3);
            SetTextLocations(categoryText);

            textBox.Text = new StringBuilder("");
            CategoryAdded(this, this.Index, arg3);
        }

        /// <summary>
        /// Triggered whenever an existing cube category is clicked.  Triggers the CategorySelected action and closes the dialog.
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private void CategoryClick(object arg1, long arg2)
        {
            CategorySelected(this, this.Index, ((CubeCategoryBox)arg1).OriginalText);
            Close(this);
        }

        /// <summary>
        /// Builds the columns of cube categories to be dislpayed, chopping off text at a certain length if necessary.
        /// There is no protection for adding columns that display outside the window!!!
        /// </summary>
        /// <param name="text">The list of cube categories to display.</param>
        private void SetTextLocations(List<string> text)
        {
            this.categories = new List<CubeCategoryBox>();

            int columnWidth = 0;
            int totalColumnOffset = column1;
            int columnPadding = 10;

            int maxColumnWidth = Math.Max(80, (this.Size.Width - 2 * column1 - 3 * columnPadding) / 4); //Will give us the maximum column width for 4 columns


            CubeCategoryBox measure = new CubeCategoryBox(gameContent, spriteBatch, gameContent.menuTitleFont, this, "Ay", "Ay", new Vector2(0, 0)); // "Ay" sets the button to the max height
            int buttonHeight = measure.Height + 4; // The +4 is since borders expand when a button is hovered over, so we need to make sure the buttons don't overlap
            int maxHeight = this.Size.Height - column1 / 2 - textboxBottomOffset;

            int currentHeight = column1;

            for (int i = 0; i < text.Count(); i++)
            {
                Vector2 textwidth = gameContent.menuTitleFont.MeasureString(text[i]);

                string modifiedText = text[i];

                // Add ellipsis where necessary to prevent overly long names
                if (textwidth.X > maxColumnWidth)
                {
                    modifiedText += "...";
                    while (textwidth.X > maxColumnWidth)
                    {
                        modifiedText = modifiedText.Remove(modifiedText.Length - 4, 1);
                        textwidth = gameContent.menuTitleFont.MeasureString(modifiedText);
                    }
                }

                // Tries out adding an additional category.  If runs out of the bottom of the box, a new column is started.
                CubeCategoryBox temp = new CubeCategoryBox(gameContent, spriteBatch, gameContent.menuTitleFont, this, text[i], modifiedText, new Vector2(0, 0));

                if (!(currentHeight + buttonHeight <= maxHeight))
                {
                    totalColumnOffset += columnWidth + columnPadding;
                    currentHeight = column1;
                    columnWidth = 0;
                }

                temp.Location = new Vector2(totalColumnOffset + this.Location.X, currentHeight + this.Location.Y);
                currentHeight += buttonHeight;
                temp.Click += CategoryClick;

                categories.Add(temp);

                columnWidth = Math.Max(temp.Size.Width, columnWidth);
            }
        }


        Vector2 previousLocation;
        /// <summary>
        /// Updates the window.
        /// </summary>
        /// <param name="newMouseState">The current mouseState.</param>
        /// <param name="oldMouseState">The previous mouseState.</param>
        /// <param name="newKeyboardState">The current keyboardState.</param>
        /// <param name="oldKeyboardState">Thep previous keyboardState.</param>
        /// <param name="gameTime">The Game's gameTime.</param>
        /// <param name="windowHasFocus">A bool indicating whether the game window has focus.</param>
        public override void Update(MouseState newMouseState, MouseState oldMouseState, KeyboardState newKeyboardState, KeyboardState oldKeyboardState, GameTime gameTime, bool windowHasFocus)
        {
            if (previousLocation != this.Location)
            {
                SetTextLocations(this.categoryText);
            }
            previousLocation = new Vector2(this.Location.X, this.Location.Y);

            if (!Enabled)
            {
                return;
            }

            base.Update(newMouseState, oldMouseState, newKeyboardState, oldKeyboardState, gameTime, windowHasFocus);

            foreach (CubeCategoryBox c in categories)
            {
                c.Update(newMouseState, oldMouseState);
            }

            textBox.Update(newKeyboardState, oldKeyboardState, newMouseState, oldMouseState, gameTime, windowHasFocus);
        }

        /// <summary>
        /// Draws the window
        /// </summary>
        public override void Draw()
        {
            if (!Visible)
            {
                return;
            }

            base.Draw();

            foreach (CubeCategoryBox c in categories)
            {
                c.Draw();
            }

            textBox.Location = new Vector2(Location.X + column1, Location.Y + Size.Height - textboxBottomOffset);
            textBox.Size = new System.Drawing.Size(Size.Width - 2 * column1, textboxBottomOffset - column1);
            textBox.Draw();
        }
    }

    /// <summary>
    /// Simple buttons that display the cube category text, but can also be clicked
    /// </summary>
    sealed class CubeCategoryBox : Button
    {
        public CubeSelectWindow parent { get; }
        public string OriginalText { get; }

        public CubeCategoryBox(GameContent gameContent, SpriteBatch spriteBatch, SpriteFont spriteFont, CubeSelectWindow parent, string originalText, string text, Vector2 location) : base(gameContent, spriteBatch, spriteFont)
        {
            SetText(text); //So that autosize activates
            this.parent = parent;
            this.Location = location;

            this.ToggleColor = Constants.GetColor("ChildBackgroundColor");
            this.BackColor = Constants.GetColor("ChildBackgroundColor");
            this.BorderColor = Constants.GetColor("ChildBackgroundColor");
            this.TextColor = Constants.GetColor("TabTextColor");

            this.ZDepth = parent.ZDepth - Constants.UserLevelDepth;
            this.Visible = true;
            this.Enabled = true;

            this.MouseEnter += Button_MouseEnter;
            this.MouseLeave += Button_MouseLeave;

            this.OriginalText = originalText;
        }

        private void Button_Click(object arg1, long arg2)
        {
        }
        private void Button_MouseLeave(object arg1, long arg2)
        {
            this.BackColor = Constants.GetColor("ChildBackgroundColor");
            //this.BorderColor = Constants.GetColor("ChildBackgroundColor");
        }
        private void Button_MouseEnter(object arg1, long arg2)
        {
            this.BackColor = Constants.GetColor("TimeBoxDefaultColor");
            //this.BorderColor = Constants.GetColor("TimeBoxDefaultColor");
        }

    }
}