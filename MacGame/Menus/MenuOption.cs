using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame
{
    public class MenuOption
    {
        public string Text;

        protected float selectionFade;
        public Vector2 Position;
        
        float pulsateScale = 1f;
        float pulsateTime;

        protected bool IsSelected;
        public float DrawDepth;
        public float Scale = 1f;

        public Color Color = Color.White;
        public Color SelectedColor = Color.Yellow;

        private bool wasSelected = false;

        public float DrawScale
        {
            get
            {
                return pulsateScale * this.Menu.Scale * Scale;
            }
        }

        /// <summary>
        /// Event raised when the menu entry is selected.
        /// </summary>
        public event EventHandler<MenuEventArgs> Chosen;
        public event EventHandler<MenuEventArgs> Highlighted;

        protected Menu Menu;

        /// <summary>
        /// Method for raising the Chosen event.
        /// </summary>
        protected internal virtual void OnChosenEntry()
        {
            if (Chosen != null)
            {
                Chosen(this, new MenuEventArgs());
            }
        }

        /// <summary>
        /// Method for raising the Highlighted event.
        /// </summary>
        protected internal virtual void OnHighlightedEntry()
        {
            if (Highlighted != null)
            {
                Highlighted(this, new MenuEventArgs());
            }
        }

        /// <summary>
        /// Constructs a new menu entry with the specified text.
        /// </summary>
        public MenuOption(string text, Menu menu)
        {
            this.Text = text;
            this.Menu = menu;
        }

        /// <summary>
        /// Updates the menu entry.
        /// </summary>
        public virtual void Update(float elapsed, bool isSelected)
        {
            // When the menu selection changes, entries gradually fade between
            // their selected and deselected appearance, rather than instantly
            // popping to the new state.
            float fadeSpeed = elapsed * 4;
            
            this.IsSelected = isSelected;

            if (!wasSelected && isSelected)
            {
                OnHighlightedEntry();
            }

            if (isSelected)
            {
                selectionFade = Math.Min(selectionFade + fadeSpeed, 1);
            }
            else
            {
                selectionFade = Math.Max(selectionFade - fadeSpeed, 0);
            }

            pulsateTime += elapsed;
            float pulsate = (float)Math.Sin(pulsateTime * 6f) + 1;
            this.pulsateScale = 1 + pulsate * 0.02f * selectionFade;

            wasSelected = isSelected;
        }

        /// <summary>
        /// Draws the menu entry. This can be overridden to customize the appearance.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw the selected entry in yellow, otherwise white.
            Color color = IsSelected ? SelectedColor : Color;

            var unscaledWidth = Game1.Font.MeasureString(Text).X;
            var unscaledHeight = (float)Game1.Font.LineSpacing;
            var origin = new Vector2(unscaledWidth / 2, unscaledHeight / 2);

            spriteBatch.DrawString(Game1.Font, Text, Menu.Position + Position, color, 0,
                                   origin, DrawScale, SpriteEffects.None, DrawDepth);
        }

        /// <summary>
        /// Queries how much space this menu entry requires.
        /// </summary>
        public int GetHeight()
        {
            return (int)((float)Game1.Font.LineSpacing * this.DrawScale);
        }

        /// <summary>
        /// Queries how wide the entry is, used for centering on the screen.
        /// </summary>
        public int GetWidth()
        {
            return (int)(Game1.Font.MeasureString(Text).X * this.DrawScale);
        }

    }
}
