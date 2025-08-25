using MacGame.DisplayComponents;
using MacGame.Npcs;
using MacGame.Platforms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata;
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

        private bool isInitialized = false;

        //LadderPlatform ladderPlatform;

        public SpaceShipStairs(ContentManager content, Vector2 spaceShipLocation, Player player)
        {

            // Adjust relative to the ship
            WorldLocation = spaceShipLocation + new Vector2(-4, 0);

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

            // Add a hidden platform at the top of ladders so you can climb to the top and stand on them.
            //ladderPlatform = new LadderPlatform(content, -100000, -100000);
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
