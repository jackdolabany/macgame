using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    /// <summary>
    /// An animated flame shot by dracula
    /// </summary>
    public class DraculaFireball : Enemy
    {

        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        public DraculaFireball(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var fire = new AnimationStrip(textures, Helpers.GetTileRect(1, 22), 3, "fire");
            fire.LoopAnimation = true;
            fire.Oscillate = true;
            fire.FrameLength = 0.1f;
            animations.Add(fire);
            animations.Play("fire");

            isTileColliding = false;
            isEnemyTileColliding = false;
            Attack = 1;
            Health = 1;
            IsAffectedByGravity = false;
            CanBeHitWithWeapons = false;
            CanBeJumpedOn = false;

            IsAbleToSurviveOutsideOfWorld = false;

            SetCenteredCollisionRectangle(4, 4);
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (!Enabled) return;

            var mapSquare = Game1.CurrentMap.GetMapSquareAtPixel(WorldCenter);

            if (mapSquare == null)
            {
                this.Enabled = false;
            }
            else if (!mapSquare.Passable)
            {
                Kill();
            }

            base.Update(gameTime, elapsed);
        }

        public override void PlayDeathSound()
        {
            SoundManager.PlaySound("Fire", 0.6f, 0.9f);
        }

        public override void AfterHittingPlayer()
        {
            base.AfterHittingPlayer();
            this.Kill();
        }

        public override void Kill()
        {
            EffectsManager.SmallEnemyPop(WorldCenter);
            Enabled = false;
            base.Kill();
        }
    }
}