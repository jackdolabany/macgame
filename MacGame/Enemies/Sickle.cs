using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    /// <summary>
    /// A sickle throwing by the murderer.
    /// </summary>
    public class Sickle : Enemy
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        public bool IsBouncing { get; set; } = false;
        public Sickle(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures2");

            var toss = new AnimationStrip(textures, Helpers.GetTileRect(7, 1), 4, "toss");
            toss.LoopAnimation = true;
            toss.FrameLength = 0.14f;
            animations.Add(toss);

            isTileColliding = false;
            isEnemyTileColliding = false;
            Attack = 1;
            Health = 1;

            IsAffectedByGravity = false;
            IsAbleToSurviveOutsideOfWorld = true;
            IsAbleToMoveOutsideOfWorld = true;
            CanBeHitWithWeapons = false;
            CanBeJumpedOn = false;

            SetWorldLocationCollisionRectangle(6, 6);
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            base.Update(gameTime, elapsed);
        }

        public override void PlayDeathSound()
        {
            SoundManager.PlaySound("Break");
        }

        public override void Kill()
        {
            if (Enabled && Alive)
            {
                EffectsManager.EnemyPop(WorldCenter, 10, Color.Pink, 120f);
                Enabled = false;
            }
            base.Kill();
        }
    }
}