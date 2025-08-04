using MacGame.DisplayComponents;
using MacGame.Npcs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using TileEngine;

namespace MacGame
{
    /// <summary>
    /// Stairs that go up when you enter the space ship.
    /// </summary>
    public class SpaceShipStairs : GameObject
    {

        private Player _player;
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        public SpaceShipStairs(ContentManager content, int cellX, int cellY, Player player)
        {

            WorldLocation = new Vector2(cellX * TileMap.TileSize + (Game1.TileSize / 2), (cellY + 1) * TileMap.TileSize);

            // Adjust relative to the ship
            WorldLocation = new Vector2(WorldLocation.X - 4, WorldLocation.Y + 4);

            this.CollisionRectangle = new Rectangle(-1, -65, 34, 66);

            _player = player;

            Enabled = true;

            IsAffectedByForces = false;
            IsAffectedByGravity = false;
            IsAffectedByPlatforms = false;
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;
            isEnemyTileColliding = false;
            isTileColliding = false;

            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\SpaceTextures");

            var idle = new AnimationStrip(textures, Helpers.GetTileRect(0, 8), 1, "idle");
            idle.LoopAnimation = false;
            idle.FrameLength = 0.1f;
            animations.Add(idle);

            var raise = new AnimationStrip(textures, Helpers.GetTileRect(0, 8), 5, "raise");
            raise.LoopAnimation = false;
            raise.FrameLength = 0.1f;
            animations.Add(raise);

            animations.Play("idle");
        }

        public void RaiseStairs()
        {
            animations.Play("raise");
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            base.Update(gameTime, elapsed);

            if (Enabled && animations.CurrentAnimationName == "raise" && animations.CurrentAnimation.FinishedPlaying)
            {
                this.Enabled = false;
            }
        }
    }
}
