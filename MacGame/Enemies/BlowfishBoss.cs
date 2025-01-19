using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class Blowfish : Enemy
    {

        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        private float speed = 40;
        private float maxTravelDistance = 5 * Game1.TileSize;

        private float minXLocation;
        private float maxXLocation;
        const int MaxHealth = 20;

        public Blowfish(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\MegaTextures");
            var swim = new AnimationStrip(textures, Helpers.GetMegaTileRect(0, 2), 2, "swim");
            swim.LoopAnimation = true;
            swim.FrameLength = 0.3f;
            animations.Add(swim);

            animations.Play("swim");

            isEnemyTileColliding = false;
            Attack = 1;
            Health = MaxHealth;
            IsAffectedByGravity = false;

            this.CollisionRectangle = new Rectangle(-50, -170, 100, 80);

            var startLocationX = WorldLocation.X;
            minXLocation = startLocationX - maxTravelDistance / 2;
            maxXLocation = startLocationX + maxTravelDistance / 2;
        }

        public override void Kill()
        {
            EffectsManager.SmallEnemyPop(WorldCenter);

            Enabled = false;
            base.Kill();
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            Game1.DrawBossHealth = true;
            Game1.MaxBossHealth = MaxHealth;
            Game1.BossHealth = Health;

            

            if (Alive)
            {
                velocity.X = speed;
                if (Flipped)
                {
                    velocity.X *= -1;
                }
            }

            if (velocity.X > 0 && (WorldLocation.X >= maxXLocation || OnRightWall))
            {
                Flipped = !Flipped;
                minXLocation = WorldLocation.X - maxTravelDistance;
            }
            else if (velocity.X < 0 && (WorldLocation.X <= minXLocation || OnLeftWall))
            {
                Flipped = !Flipped;
                maxXLocation = WorldLocation.X + maxTravelDistance;
            }

            base.Update(gameTime, elapsed);

        }
    }
}