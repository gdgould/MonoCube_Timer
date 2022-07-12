using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace MonoCube_Timer
{
    class GameContent
    {
        public SpriteFont scrambleFont { get; set; }
        public SpriteFont cubeCategoryFont { get; set; }
        public SpriteFont timerFont { get; set; }
        public SpriteFont buttonFont { get; set; }
        public SpriteFont menuTitleFont { get; set; }
        public SpriteFont menuFont { get; set; }
        public SpriteFont menuFontBold { get; set; }


        public Texture2D buttonCorner { get; set; }
        public Texture2D buttonInvertedCorner { get; set; }
        public Texture2D buttonHorizBar { get; set; }
        public Texture2D buttonPixel { get; set; }
        public Texture2D dataCloseX { get; set; }
        public Texture2D commentIcon { get; set; }

        public GameContent(ContentManager Content)
        {
            //load images
            buttonCorner = Content.Load<Texture2D>("Corner");
            buttonInvertedCorner = Content.Load<Texture2D>("InvertedCorner");
            buttonHorizBar = Content.Load<Texture2D>("HorizontalBar");
            buttonPixel = Content.Load<Texture2D>("Pixel");
            dataCloseX = Content.Load<Texture2D>("DataClose");
            commentIcon = Content.Load<Texture2D>("Comment");

            //load sounds

            //load fonts
            scrambleFont = Content.Load<SpriteFont>("ScrambleText");
            cubeCategoryFont = Content.Load<SpriteFont>("CubeCategoryText");
            buttonFont = Content.Load<SpriteFont>("ScrambleText");
            menuTitleFont = Content.Load<SpriteFont>("MenuTitleText");
            menuFont = Content.Load<SpriteFont>("MenuText");
            menuFontBold = Content.Load<SpriteFont>("MenuTextB");
            timerFont = Content.Load<SpriteFont>("TimerText");

        }
    }
}