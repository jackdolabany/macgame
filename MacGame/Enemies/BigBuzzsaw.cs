using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class BigBuzzsaw : Enemy
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        public BigBuzzsaw(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = Game1.ReallyBigTileTextures;
            var spin = new AnimationStrip(textures, Helpers.GetReallyBigTileRect(0, 6), 4, "spin");
            spin.LoopAnimation = true;
            spin.FrameLength = 0.03f; // Fast spinning animation
            animations.Add(spin);

            animations.Play("spin");

            isTileColliding = false;
            isEnemyTileColliding = false;
            Attack = 1;
            Health = 1;
            IsAffectedByGravity = false;
            CanBeHitWithWeapons = false;
            CanBeJumpedOn = false;
            IsAbleToMoveOutsideOfWorld = false;
            IsAbleToSurviveOutsideOfWorld = false;

            WorldLocation = new Vector2(((cellX + 1) * TileMap.TileSize) + (TileMap.TileSize / 2), (cellY + 1) * TileMap.TileSize);

            // Collision rectangle slightly smaller than 3 tiles (24x24)
            SetCenteredCollisionRectangle(24, 24, 20, 20);
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            base.Update(gameTime, elapsed);
            
            if (camera.IsObjectVisible(this.CollisionRectangle))
            {
                SoundManager.IsBuzzsawOnScreen = true;
            }
        }
    }
}
