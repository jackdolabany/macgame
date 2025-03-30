using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    /// <summary>
    /// Placeholder for the final boss. Just the cat who stands there. Kill it to trigger the credits.
    /// </summary>
    public class FinalBoss : Enemy
    {

        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        const int MaxHealth = 1;
       

        public FinalBoss(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\BigTextures");
            var idle = new AnimationStrip(textures, Helpers.GetBigTileRect(0, 0), 3, "idle");
            idle.LoopAnimation = true;
            idle.Oscillate = true;
            idle.FrameLength = 0.14f;
            animations.Add(idle);

            animations.Play("idle");

            isEnemyTileColliding = false;
            Attack = 1;

            Health = MaxHealth;

            IsAffectedByGravity = true;
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;

            SetWorldLocationCollisionRectangle(14, 14);
        }

        public override void TakeHit(GameObject attacker, int damage, Vector2 force)
        {
         
            Health -= damage;

            SoundManager.PlaySound("CatBossHit");

            if (!IsTempInvincibleFromBeingHit)
            {
                InvincibleTimer += 2f;
            }

            if (Health <= 0)
            {
                // DEATH!!!
                Dead = true;
                this.velocity = Vector2.Zero;
                Kill();
            }
        }

        public override void Kill()
        {
            Enabled = false;
            base.Kill();

            TimerManager.AddNewTimer(4f, () =>
            {
                GlobalEvents.FireFinalBossComplete();
            });
            
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            base.Update(gameTime, elapsed);

            Game1.DrawBossHealth = true;
            Game1.MaxBossHealth = MaxHealth;
            Game1.BossHealth = Health;
            Game1.BossName = "Final Boss";
        }
    }
}