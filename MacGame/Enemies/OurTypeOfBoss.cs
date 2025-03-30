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
    public class OurTypeOfBoss : Enemy
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        /// <summary>
        /// Don't do anything until you've been seen.
        /// </summary>
        private bool _hasBeenSeen = false;

        float openAndCloseMouthTimer = 0;
        float openAndCloseMouthTimerGoal = 2.5f;

        private Player _player;

        private int MaxHealth = 6;

        private bool isInitialized = false;

        public OurTypeOfBoss(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            _player = player;

            isEnemyTileColliding = false;
            IsAbleToMoveOutsideOfWorld = true;
            isTileColliding = false;
            IsAbleToSurviveOutsideOfWorld = true;
            IsAffectedByForces = false;
            IsAffectedByGravity = false;
            IsAffectedByPlatforms = false;
            CanBeHitWithWeapons = false;
            CanBeJumpedOn = true;

            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\MegaTextures");
            
            var idle = new AnimationStrip(textures, Helpers.GetMegaTileRect(2, 3), 1, "idle");
            idle.LoopAnimation = false;
            idle.FrameLength = 0.15f;
            animations.Add(idle);

            var closeMouth = new AnimationStrip(textures, Helpers.GetMegaTileRect(0, 3), 3, "closeMouth");
            closeMouth.LoopAnimation = false;
            closeMouth.FrameLength = 0.15f;
            closeMouth.Oscillate = true;
            animations.Add(closeMouth);

            var openMouth = (AnimationStrip)closeMouth.Clone();
            openMouth.Reverse = true;
            openMouth.Name = "openMouth";
            animations.Add(openMouth);

            animations.Play("idle");

            isEnemyTileColliding = false;
            Attack = 1;
            Health = MaxHealth;

            IsAffectedByGravity = false;
        }

    

        private void Initialize()
        {
           
            isInitialized = true;
        }

      

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (!isInitialized)
            {
                Initialize();
            }

            if (!_hasBeenSeen)
            {
                if (Game1.Camera.IsPointVisible(new Vector2(this.CollisionRectangle.Right, this.CollisionRectangle.Center.Y)))
                {
                    _hasBeenSeen = true;
                }
                else
                {
                    return;
                }
            }

            openAndCloseMouthTimer += elapsed;
            if (openAndCloseMouthTimer > openAndCloseMouthTimerGoal)
            {
                openAndCloseMouthTimer = 0;
                animations.Play("openMouth").FollowedBy("closeMouth");
            }

            Game1.DrawBossHealth = true;
            Game1.MaxBossHealth = MaxHealth;
            Game1.BossHealth = Health;
            Game1.BossName = "Grok";

            base.Update(gameTime, elapsed);

        }

     

      
    }
}