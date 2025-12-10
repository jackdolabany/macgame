using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using TileEngine;

namespace MacGame
{
    /// <summary>
    /// BreakBricks block the map until you break them by triggering something with a button.
    /// 
    /// Place BreakBrick tiles on the map. Put an object rectangle over them and give it the GroupName property.
    /// GroupName: Bricks1
    /// OverrideSave: 1 - Optional, use this property to make it so the brick doesn't save its state when it breaks. For example, for when you beat a boss.
    /// Save: True or False. Same as override save but that's annoying because of the double negative. Saves by default.
    /// 
    /// From code call: Game1.CurrentLevel.BreakBricks("Bricks1");
    /// 
    /// or from a button add the properties
    /// DownAction: BreakBricks
    /// Args: Bricks1
    /// </summary>
    public class BreakBrick : GameObject
    {
        int _cellX;
        int _cellY;

        public bool IsBroken { get; set; } = true;

        private bool _isInitialized = false;

        NoDisplay noDisplay;

        public string GroupName { get; set; }

        /// <summary>
        /// Normal behavior is to save the state of these when they break. For bosses or other things you
        /// might want it to reset every time. Set the OverrideSave property to "1" in the map to set this.
        /// </summary>
        public bool OverrideSave
        { 
            get => !Save; 
            set 
            {
                Save = !value;
            }
        }

        public bool Save { get; set; } = true;

        public BreakBrick(ContentManager content, int cellX, int cellY, Player player, bool isBroken) : base()
        {
            _cellX = cellX;
            _cellY = cellY;

            this.WorldLocation = new Vector2(cellX * TileMap.TileSize + TileMap.TileSize / 2, (cellY + 1) * TileMap.TileSize);
            Enabled = true;

            SetWorldLocationCollisionRectangle(8, 8);

            IsBroken = isBroken;

            var textures = content.Load<Texture2D>(@"Textures\Textures");

            noDisplay = new NoDisplay();

            if (IsBroken)
            {
                DisplayComponent = noDisplay;
            }
            else
            {
                DisplayComponent = new StaticImageDisplay(textures, Helpers.GetTileRect(9, 0));
            }
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                Game1.CurrentMap.GetMapSquareAtCell(_cellX, _cellY)!.Passable = IsBroken;
            }
        }

        public void Break(float explosionDelay = 0f)
        {
            if (IsBroken) return;

            IsBroken = true;
            Game1.CurrentMap.GetMapSquareAtCell(_cellX, _cellY)!.Passable = true;

            if (Save)
            {
                Game1.StorageState.AddUnblockedMapSquare(Game1.CurrentLevel.LevelNumber, Game1.CurrentLevel.Name, _cellX, _cellY);
            }

            // Instantly become passable for timing reasons, but we may delay the explosion for dramatic effect
            // break but do it randomly in a small time so that it doesn't all happen at once
            var randomFloat = Game1.Randy.NextFloat();
            var randomTime = randomFloat * explosionDelay;
            TimerManager.AddNewTimer(randomTime, () =>
            {
                this.DisplayComponent = noDisplay;
                EffectsManager.AddExplosion(this.CollisionCenter);
            });
        }

    }
}
