using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MacGame
{
    public class SoundEffectsMenu : Menu
    {
        private List<MenuOption> soundOptions = new List<MenuOption>();
        private Dictionary<string, MenuOption> soundNameToOption = new Dictionary<string, MenuOption>();
        private float menuScrollOffset = 0f;
        private float navigationTimer = 0f;
        private const float NavigationDelay = 0.15f; // Time between auto-advances when holding button

        public SoundEffectsMenu(Game1 game)
            : base(game)
        {
            IsDismissable = true;
            this.IsOverlay = false;

            this.menuTitle = "";
            // Center the menu
            this.Position = new Vector2(Game1.GAME_X_RESOLUTION / 2, Game1.GAME_Y_RESOLUTION / 2);

            // Get all sound names sorted alphabetically
            var sortedSoundNames = SoundManager.Sounds.Keys.OrderBy(s => s).ToList();

            foreach (var soundName in sortedSoundNames)
            {
                Func<string> GetVolumeText = () =>
                {
                    var volume = SoundManager.GetSoundVolume(soundName);
                    return $"{soundName}: {volume}%";
                };

                var option = AddOption(GetVolumeText(), (a, b) =>
                {
                    // Play the sound when selected
                    SoundManager.PlaySound(soundName, 1f);
                });

                soundOptions.Add(option);
                soundNameToOption[soundName] = option;
            }

            AddOption("Back", (a, b) =>
            {
                PlayOptionSelectedSound();
                SoundManager.SaveSoundVolumeSettings();
                MenuManager.RemoveTopMenu();
            });
        }

        public override void HandleNavigationInputs(ref InputAction ca, ref InputAction pa, float elapsed)
        {
            int previousIndex = selectedEntryIndex;

            // Handle volume adjustments with left/right
            if (selectedEntryIndex < soundOptions.Count)
            {
                var soundName = SoundManager.Sounds.Keys.OrderBy(s => s).ElementAt(selectedEntryIndex);
                var currentVolume = SoundManager.GetSoundVolume(soundName);

                bool volumeChanged = false;

                if (ca.right && !pa.right)
                {
                    currentVolume += 5;
                    if (currentVolume > 100) currentVolume = 100;
                    volumeChanged = true;
                }
                else if (ca.left && !pa.left)
                {
                    currentVolume -= 5;
                    if (currentVolume < 0) currentVolume = 0;
                    volumeChanged = true;
                }

                if (volumeChanged)
                {
                    SoundManager.SetSoundVolume(soundName, currentVolume);

                    // Update the menu text
                    var option = soundNameToOption[soundName];
                    option.Text = $"{soundName}: {currentVolume}%";

                    // Play the sound so you can hear the new volume
                    SoundManager.PlaySound(soundName, 1f);

                    // Reposition menu to update text width
                    isPositioned = false;
                }
            }

            // Handle up/down navigation with hold support
            bool movedUp = false;
            bool movedDown = false;

            // Initial press
            if (ca.up && !pa.up)
            {
                movedUp = true;
                navigationTimer = NavigationDelay; // Start timer for hold
            }
            else if (ca.down && !pa.down)
            {
                movedDown = true;
                navigationTimer = NavigationDelay; // Start timer for hold
            }
            // Holding the button
            else if (ca.up && pa.up)
            {
                navigationTimer -= elapsed;
                if (navigationTimer <= 0)
                {
                    movedUp = true;
                    navigationTimer = NavigationDelay;
                }
            }
            else if (ca.down && pa.down)
            {
                navigationTimer -= elapsed;
                if (navigationTimer <= 0)
                {
                    movedDown = true;
                    navigationTimer = NavigationDelay;
                }
            }

            if (movedUp)
            {
                do
                {
                    selectedEntryIndex--;
                    if (selectedEntryIndex < 0)
                        selectedEntryIndex = menuOptions.Count - 1;
                }
                while (menuOptions[selectedEntryIndex].Hidden);

                PlayOptionChangedSound();
            }
            else if (movedDown)
            {
                do
                {
                    selectedEntryIndex++;
                    if (selectedEntryIndex >= menuOptions.Count)
                        selectedEntryIndex = 0;
                }
                while (menuOptions[selectedEntryIndex].Hidden);

                PlayOptionChangedSound();
            }

            // Update scroll offset if selection changed
            if (previousIndex != selectedEntryIndex)
            {
                UpdateScrollOffset();
            }
        }

        private void UpdateScrollOffset()
        {
            // Calculate the offset needed to center the selected item
            // Each menu item has a height + spacing
            var itemHeight = Game1.Font.MeasureString("A").Y * Scale + 8;

            // Calculate offset to center selected item
            // We want the selected item at screen center (Game1.GAME_Y_RESOLUTION / 2)
            // Title takes up some space at the top
            var titleHeight = 0f;
            if (!string.IsNullOrEmpty(menuTitle))
            {
                var titleSize = Game1.Font.MeasureString(menuTitle);
                titleHeight = titleSize.Y * Scale * 2f;
            }

            // Target: selected item should be at center of screen
            // selectedItemY = Position.Y + titleHeight + (selectedEntryIndex * itemHeight) + menuScrollOffset
            // We want: selectedItemY = Game1.GAME_Y_RESOLUTION / 2
            menuScrollOffset = (Game1.GAME_Y_RESOLUTION / 2) - Position.Y - titleHeight - (selectedEntryIndex * itemHeight);

            isPositioned = false;
        }

        protected override void SetMenuPositions()
        {
            base.SetMenuPositions();

            // Apply scroll offset to all menu items
            foreach (var option in menuOptions)
            {
                option.Position = new Vector2(option.Position.X, option.Position.Y + menuScrollOffset);
            }
        }

        public override void AddedToMenuManager()
        {
            UpdateScrollOffset();
            base.AddedToMenuManager();
        }

        protected override void OnCancel()
        {
            PlayOptionSelectedSound();
            SoundManager.SaveSoundVolumeSettings();
            MenuManager.RemoveTopMenu();
        }
    }
}
