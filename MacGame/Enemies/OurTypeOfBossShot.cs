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
    public class OurTypeOfBossShot : Enemy
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        public OurTypeOfBossShot(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {

            isEnemyTileColliding = false;
            IsAbleToMoveOutsideOfWorld = false;
            isTileColliding = false;
            IsAbleToSurviveOutsideOfWorld = false;
            IsAffectedByForces = false;
            IsAffectedByGravity = false;
            IsAffectedByPlatforms = false;
            CanBeHitWithWeapons = false;
            CanBeJumpedOn = false;

            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\SpaceTextures");
            
            //DisplayComponent = new StaticImageDisplay(textures, Helpers.GetTileRect(4, 4));



            DisplayComponent = new AnimationDisplay();

            //var textures = content.Load<Texture2D>(@"Textures\BigTextures");

            var flash = new AnimationStrip(textures, Helpers.GetTileRect(6, 2), 2, "flash");
            flash.LoopAnimation = true;
            flash.FrameLength = 0.15f;
            animations.Add(flash);
            animations.Play("flash");

            isEnemyTileColliding = false;
            Attack = 1;
            Health = 1;

            IsAffectedByGravity = false;

            SetCenteredCollisionRectangle(8, 8, 6, 6);

            InvincibleTimeAfterBeingHit = 0f;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            base.Update(gameTime, elapsed);
        }
    }
}