using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoCube_Timer
{
    /// <summary>
    /// A basic class from which all controls are derived.  Mainly serves the purpose of giving controls unique index numbers
    /// </summary>
    abstract class Control
    {
        internal SpriteBatch spriteBatch;

        public Vector2 Location { get; set; }

        public Color BackColor { get; set; }
        public float ZDepth { get; set; }

        public bool Visible { get; set; }
        public bool Enabled { get; set; }

        public long Index { get; }

        public Control()
        {
            this.Index = GenIndex.getNewIndex();
        }

        public abstract void Draw();

    }
}