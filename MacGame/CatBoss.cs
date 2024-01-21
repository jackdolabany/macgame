using System;
using MacGame;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame
{
    public class CatBoss : Enemy
    {

        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        bool hasBeenSeen = false;

        YarnBall[] yarnBalls = new YarnBall[5];

        int nextYarnBallToThrowIndex = 0;
        const float maxThrowTimer = 2f;
        float throwTimer = maxThrowTimer;

        public CatBoss(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            this.DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var idle = new AnimationStrip(textures, new Rectangle(40, 0, 16, 16), 3, "idle");
            idle.LoopAnimation = true;
            idle.Oscillate = true;
            idle.FrameLength = 0.14f;
            animations.Add(idle);

            animations.Play("idle");

            isEnemyTileColliding = false;
            Attack = 1;
            Health = 5;
            IsAffectedByGravity = false;
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;

            SetCenteredCollisionRectangle(14, 14);

            // Cat has 5 yarn balls.
            for (int i = 0; i < 5; i++)
            {
                yarnBalls[i] = new YarnBall(content, 0, 0, player, camera);
                yarnBalls[i].Enabled = false;
                Level.AddEnemy(yarnBalls[i]);
            }

        }

        public override void TakeHit(int damage, Vector2 force)
        {
            base.TakeHit(damage, force);
            if (!IsInvincibleAfterHit)
            {
                InvincibleTimer += 1f;
            }
        }

        public override void Kill()
        {
            EffectsManager.EnemyPop(this.WorldCenter, 40, Color.White, 30f);

            this.Enabled = false;
            base.Kill();
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            base.Update(gameTime, elapsed);

            if (!hasBeenSeen)
            {
                if (Game1.Camera.IsObjectVisible(this.CollisionRectangle))
                {
                    hasBeenSeen = true;
                    SoundManager.PlaySong("BossFight", true, 0.2f);
                    Game1.Camera.CanScrollLeft = false;
                }
            } 

            if (hasBeenSeen && Alive)
            {
                throwTimer -= elapsed;
                if (throwTimer < 0f)
                {
                    // Throw a yarn ball at the player.
                    var yarnBall = yarnBalls[nextYarnBallToThrowIndex];
                    yarnBall.Enabled = true;
                    yarnBall.Alive = true;
                    yarnBall.WorldLocation = this.WorldCenter;
                    var direction = Player.WorldCenter - yarnBall.WorldCenter;
                    direction.Normalize();
                    yarnBall.Velocity = direction * 40;

                    nextYarnBallToThrowIndex++;
                    if (nextYarnBallToThrowIndex >= yarnBalls.Length)
                    {
                        nextYarnBallToThrowIndex = 0;
                    }
                    throwTimer = maxThrowTimer;
                }
            }
        }
    }
}