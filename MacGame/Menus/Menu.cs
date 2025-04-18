﻿using System;
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

        /// <summary>
        /// Set this timer to a value to ignroe inputs for that amount of time. Useful for
        /// when a menu first pops up and you don't want the player's gameplay button presses to select something.
        /// </summary>
        public float ignoreInputsTimer = 0f;

        public Vector2 Position { get; set; }
        
        public float Scale = Game1.FontScale;
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

        public virtual void HandleNavigationInputs(ref InputAction ca, ref InputAction pa, float elapsed)
        {
            // Move to the previous menu entry that is not hidden.
            if (ca.up && !pa.up)
            {
                do {
                    selectedEntryIndex--;

                    if (selectedEntryIndex < 0)
                        selectedEntryIndex = menuOptions.Count - 1;
                }
                while (menuOptions[selectedEntryIndex].Hidden);
            }

            // Move to the next menu entry that is not hidden.
            if (ca.down && !pa.down)
            {
                do
                {
                    selectedEntryIndex++;

                    if (selectedEntryIndex >= menuOptions.Count)
                        selectedEntryIndex = 0;
                }
                while (menuOptions[selectedEntryIndex].Hidden);
            }
        }

        public void ResestMenuIndex()
        {
            selectedEntryIndex = defaultSelectedEntryIndex;
        }

        public virtual void HandleSelectionInputs(ref InputAction ca, ref InputAction pa, float elapsed)
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
            
            if (ignoreInputsTimer > 0)
            {
                ignoreInputsTimer -= elapsed;
            }

            if (ignoreInputsTimer <= 0)
            {
                HandleInputs(Game1.Player.InputManager, elapsed);
            }

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
                var titleSize = Game1.Font.MeasureString(menuTitle);
                position.Y += (titleSize.Y * Scale * 2f).ToInt();
            }

            // update each menu entry's location in turn
            for (int i = 0; i < menuOptions.Count; i++)
            {
                MenuOption option = menuOptions[i];

                if (!option.Hidden)
                {
                    // each entry is to be centered horizontally
                    position.X = 0;

                    // set the entry's position
                    option.Position = position;

                    // move down for the next entry the size of this entry
                    position.Y += option.GetHeight() + 8;
                }
            }
        }

        public void CenterMenuAndChoices()
        {
            
            var totalMenuHeight = 0f;

            if (!string.IsNullOrEmpty(menuTitle))
            {
                // Start with a fudge factor to favor the top of the screen and give room for the title.
                totalMenuHeight += 100 + Game1.Font.MeasureString(menuTitle).Y * Scale;
            }

            foreach (var choice in this.menuOptions)
            {
                if (!choice.Hidden)
                {
                    totalMenuHeight += Game1.Font.MeasureString(choice.Text).Y * Scale;
                }
            }

            this.Position = new Vector2(Game1.GAME_X_RESOLUTION / 2, (Game1.GAME_Y_RESOLUTION - totalMenuHeight) / 2);
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

                // Don't draw hidden menu items.
                if (!option.Hidden)
                {
                    option.DrawDepth = DrawDepth;
                    option.Draw(spriteBatch);
                }
            }
        }

        public virtual void AddedToMenuManager()
        {
            isPositioned = false;
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
