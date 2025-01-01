using System;
using System.Collections.Generic;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    /// <summary>
    /// Not really an enemy, they just fly away from Dracula after he dies.
    /// </summary>
    public class DraculaDeathBat : Enemy
    {

        private Player _player;

        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        public DraculaDeathBat(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var fly = new AnimationStrip(textures, Helpers.GetTileRect(0, 8), 2, "fly");
            fly.LoopAnimation = true;
            fly.Oscillate = true;
            fly.FrameLength = 0.08f;
            animations.Add(fly);
            animations.Play("fly");

            IsAffectedByGravity = false;
            IsAbleToSurviveOutsideOfWorld = false;
            isTileColliding = false;
            isEnemyTileColliding = false;
            IsAbleToMoveOutsideOfWorld = true;
            CanBeHitWithWeapons = false;
            CanBeJumpedOn = false;

            this.CollisionRectangle = Rectangle.Empty;
            Attack = 0;
            this.Enabled = false;
            this.Dead = true;
          
            _player = player;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            base.Update(gameTime, elapsed);
            Flipped = Velocity.X <= 0;
        }
    }
}