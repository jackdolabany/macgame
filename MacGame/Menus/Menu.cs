using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame
{
    public abstract class Menu
    {
        public float TransitionAlpha;
        protected List<MenuOption> menuOptions = new List<MenuOption>();
        protected int selectedEntryIndex = 0;
        protected string menuTitle;
        protected int defaultSelectedEntryIndex = 0;

        public Vector2 Position { get; set; }
        
        public float Scale = 1f;
        protected Game1 Game;
        public float DrawDepth;
        
        /// <summary>
        /// Tells us weather or not to paint a nice gray overlay over the current screen before showing this menu.
        /// </summary>
        public bool IsOverlay = false;
        
        protected bool isPositioned = false;
        public bool IsVisible = true;

        /// <summary>
        /// true if they can press b to dismiss the menu
        /// </summary>
        public bool IsDismissable = false;

        public Menu(Game1 game)
        {
            this.Game = game;
        }

        public MenuOption SelectedEntry
        {
            get
            {
                return this.menuOptions[this.selectedEntryIndex];
            }
        }

        public virtual void HandleInputs(InputManager input, float elapsed)
        {
            int originalEntry = this.selectedEntryIndex;

            HandleNavigationInputs(ref input.CurrentAction, ref input.PreviousAction, elapsed);
            HandleSelectionInputs(ref input.CurrentAction, ref input.PreviousAction, elapsed);

            if (originalEntry != this.selectedEntryIndex)
            {
                PlayOptionChangedSound();
            }
        }

        public virtual void HandleNavigationInputs(ref Action ca, ref Action pa, float elapsed)
        {
            // Move to the previous menu entry?
            if (ca.up && !pa.up)
            {
                selectedEntryIndex--;

                if (selectedEntryIndex < 0)
                    selectedEntryIndex = menuOptions.Count - 1;
            }

            // Move to the next menu entry?
            if (ca.down && !pa.down)
            {
                selectedEntryIndex++;

                if (selectedEntryIndex >= menuOptions.Count)
                    selectedEntryIndex = 0;
            }
        }

        public void ResestMenuIndex()
        {
            selectedEntryIndex = defaultSelectedEntryIndex;
        }

        public virtual void HandleSelectionInputs(ref Action ca, ref Action pa, float elapsed)
        {
            // Accept or cancel the menu? We pass in our ControllingPlayer, which may
            // either be null (to accept input from any player) or a specific index.
            // If we pass a null controlling player, the InputState helper returns to
            // us which player actually provided the input. We pass that through to
            // OnSelectEntry and OnCancel, so they can tell which player triggered them.
            if (ca.acceptMenu && !pa.acceptMenu)
            {
                OnSelectEntry(selectedEntryIndex);

                // Hack: Cancel the previous action so button presses don't effect the next state.
                pa = ca;
            }
            else if (IsDismissable && ca.declineMenu && !pa.declineMenu)
            {
                OnCancel();
            }
        }

        /// <summary>
        /// Handler for when the user has chosen a menu entry.
        /// </summary>
        protected virtual void OnSelectEntry(int entryIndex)
        {
            menuOptions[entryIndex].OnChosenEntry();
        }

        /// <summary>
        /// Handler for when the user has cancelled the menu.
        /// </summary>
        protected virtual void OnCancel()
        {
            MenuManager.RemoveTopMenu();
        }

        public virtual void Update(float elapsed, bool isTopMenu)
        {
            if (!isPositioned)
            {
                SetMenuPositions();
                isPositioned = true;
            }

            if (!isTopMenu) return;
            HandleInputs(Game1.Player.InputManager, elapsed);
            for (int i = 0; i < menuOptions.Count; i++)
            {
                menuOptions[i].Update(elapsed, i == selectedEntryIndex);
            }
        }

        protected void Cancel(object sender, EventArgs args)
        {
            OnCancel();
            PlayOptionSelectedSound();
        }

        protected void PlayOptionChangedSound()
        {
            SoundManager.PlaySound("MenuChoice", 0.2f, 0.3f);
        }

        protected void PlayOptionSelectedSound()
        {
            SoundManager.PlaySound("MenuChoice", 0.35f, -0.3f);
        }

        protected void PlayConfirmMenuPoppedUpSound()
        {
            SoundManager.PlaySound("MenuChoice", 0.5f, 0f);
        }

        protected virtual void SetMenuPositions()
        {
            // each X value is generated per entry
            Vector2 position = Vector2.Zero;

            // Start the menu items pushed down by the height of the menu title.
            if (!string.IsNullOrEmpty(menuTitle))
            {
                position.Y += (int)Game1.Font.MeasureString(menuTitle).Y;
                position.Y += 40; // Plus some padding.
            }

            // update each menu entry's location in turn
            for (int i = 0; i < menuOptions.Count; i++)
            {
                MenuOption option = menuOptions[i];

                // each entry is to be centered horizontally
                position.X = 0;

                // set the entry's position
                option.Position += position;

                // move down for the next entry the size of this entry
                position.Y += option.GetHeight();
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible) return;
            if (!isPositioned) return;

            // A blocking square
            if (IsOverlay)
            {
                spriteBatch.Draw(Game1.TileTextures, new Rectangle(0, 0, Game1.GAME_X_RESOLUTION, Game1.GAME_Y_RESOLUTION), Game1.WhiteSourceRect, new Color(0f, 0f, 0f, 0.8f), 0f, Vector2.Zero, SpriteEffects.None, DrawDepth + 2 * Game1.MIN_DRAW_INCREMENT);
            }

            // Menu title
            if (!string.IsNullOrEmpty(menuTitle))
            {
                Vector2 size = Game1.Font.MeasureString(menuTitle);
                if (!string.IsNullOrEmpty(this.menuTitle))
                {
                    spriteBatch.DrawString(Game1.Font,
                        menuTitle,
                        Position,
                        Color.White,
                        0f,
                        new Vector2((int)(size.X / 2f), 0f),
                        Scale,
                        SpriteEffects.None,
                        DrawDepth);
                }
            }

            // update each menu entry's location in turn
            for (int i = 0; i < menuOptions.Count; i++)
            {
                MenuOption option = menuOptions[i];
                option.DrawDepth = DrawDepth;
                option.Draw(spriteBatch);
            }
        }

        public virtual void AddedToMenuManager()
        {

        }

        public MenuOption AddOption(string text, Action<object, MenuEventArgs> action)
        {
            var option = new MenuOption(text, this);
            option.Chosen += (sender, args) => action(sender, args);
            this.menuOptions.Add(option);
            return option;
        }
    }
}
