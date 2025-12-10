using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class SmallBuzzsaw : Enemy
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        float clickTimer = 0f;
        float clickTimerGoal = 0.2f;

        public SmallBuzzsaw(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = Game1.BigTileTextures;
            var spin = new AnimationStrip(textures, Helpers.GetBigTileRect(8, 5), 4, "spin");
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

            WorldLocation = new Vector2((cellX + 1) * TileMap.TileSize, (cellY + 1) * TileMap.TileSize);

            // Collision rectangle slightly smaller than 64x64
            SetCenteredCollisionRectangle(16, 16, 14, 14);

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
