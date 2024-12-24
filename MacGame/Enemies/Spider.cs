using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class Spider : Enemy
    {

        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        public Spider(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var idle = new AnimationStrip(textures, Helpers.GetTileRect(3, 20), 2, "idle");
            idle.LoopAnimation = true;
            idle.FrameLength = 0.3f;
            animations.Add(idle);

            animations.Play("idle");

            isTileColliding = false;
            isEnemyTileColliding = false;
            Attack = 1;
            Health = 1;
            IsAffectedByGravity = false;

            SetCenteredCollisionRectangle(6, 6);
            
        }

        public override void Kill()
        {
            EffectsManager.SmallEnemyPop(WorldCenter);

            Enabled = false;
            base.Kill();
        }
    }
}