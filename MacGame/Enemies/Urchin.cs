using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class Urchin : Enemy
    {

        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        public Urchin(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var idle = new AnimationStrip(textures, Helpers.GetTileRect(14, 6), 2, "idle");
            idle.LoopAnimation = true;
            idle.FrameLength = 0.3f;
            animations.Add(idle);

            animations.Play("idle");

            isTileColliding = false;
            isEnemyTileColliding = false;
            Attack = 1;
            Health = 1;
            IsAffectedByGravity = false;

            SetCenteredCollisionRectangle(6, 5);

            // Shift it up a bit.
            this.CollisionRectangle = new Rectangle(this.collisionRectangle.X, this.collisionRectangle.Y - 8, this.collisionRectangle.Width, this.collisionRectangle.Height);
            
        }

        public override void Kill()
        {
            EffectsManager.EnemyPop(WorldCenter, 10, Color.White, 120f);

            Enabled = false;
            base.Kill();
        }
    }
}