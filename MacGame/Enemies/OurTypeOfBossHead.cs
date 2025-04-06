using System;
using System.Collections.Generic;
using System.Linq;
using MacGame.DisplayComponents;
using MacGame.Items;
using MacGame.Platforms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame.Enemies
{
    public class OurTypeOfBossHead : Enemy
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        // Every few seconds he pops out of the creatures stomach
        float popOutTimer = 0;
        float popOutTimerGoal = 3f;

        float stayOutTimer = 0;
        float stayoutTimerGoal = 2;
        bool isFullyOut = false;

        float poppedOutXPosition;
        float originalXPosition;

        float speed = 30f;

        OurTypeOfBoss _boss;

        public OurTypeOfBossHead(ContentManager content, int cellX, int cellY, Player player, Camera camera, OurTypeOfBoss boss)
            : base(content, cellX, cellY, player, camera)
        {
            _boss = boss;

            isEnemyTileColliding = false;
            IsAbleToMoveOutsideOfWorld = true;
            isTileColliding = false;
            IsAbleToSurviveOutsideOfWorld = true;
            IsAffectedByForces = false;
            IsAffectedByGravity = false;
            IsAffectedByPlatforms = false;
            CanBeHitWithWeapons = true;
            CanBeJumpedOn = false;

            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\BigTextures");
            
            var openMouth = new AnimationStrip(textures, Helpers.GetBigTileRect(11, 2), 3, "openMouth");
            openMouth.LoopAnimation = false;
            openMouth.FrameLength = 0.15f;
            animations.Add(openMouth);

            var closeMouth = (AnimationStrip)openMouth.Clone();
            closeMouth.Reverse = true;
            closeMouth.Name = "closeMouth";
            animations.Add(closeMouth);

            animations.Play("openMouth").FollowedBy("closeMouth");

            isEnemyTileColliding = false;
            Attack = 1;
            Health = 1;

            IsAffectedByGravity = false;

            InvincibleTimeAfterBeingHit = 0.1f;

            SetCenteredCollisionRectangle(16, 16, 14, 11);
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            // Pop out every so often.
            
            if (!isFullyOut && this.Velocity == Vector2.Zero)
            {
                popOutTimer += elapsed;
                if (popOutTimer >= popOutTimerGoal)
                {
                    this.Velocity = new Vector2(-speed, 0);
                    originalXPosition = this.WorldLocation.X;
                    poppedOutXPosition = originalXPosition - 32;
                    stayOutTimer = 0;
                    popOutTimer = 0;
                }
            }

            // Stop them if they move too far out
            if (!isFullyOut && velocity.X < 0 && this.WorldLocation.X <= poppedOutXPosition)
            {
                this.Velocity = Vector2.Zero;
                stayOutTimer += elapsed;
                isFullyOut = true;
            }

            if (isFullyOut)
            {
                stayOutTimer += elapsed;
            }

            // Go back after a while.
            if (stayOutTimer >= stayoutTimerGoal)
            {
                isFullyOut = false;
                stayOutTimer = 0;
                this.Velocity = new Vector2(speed, 0);
            }

            if (velocity.X > 0 && this.worldLocation.X >= originalXPosition)
            {
                this.Velocity = Vector2.Zero;
            }

            if (animations.CurrentAnimationName == "closeMouth" && animations.CurrentAnimation.FinishedPlaying)
            {
                 animations.Play("openMouth").FollowedBy("closeMouth");
            }

            base.Update(gameTime, elapsed);
        }

        public override void TakeHit(GameObject attacker, int damage, Vector2 force)
        {
            // hitting the head really hits the boss.
            _boss.TakeHit(attacker, damage, force);
            InvincibleTimer += InvincibleTimeAfterBeingHit;
        }
    }
}