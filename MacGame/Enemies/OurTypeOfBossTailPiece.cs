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
    public class OurTypeOfBossTailPiece : Enemy
    {

        public OurTypeOfBossTailPiece(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {

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

            var textures = content.Load<Texture2D>(@"Textures\SpaceTextures");
            
            DisplayComponent = new StaticImageDisplay(textures, Helpers.GetTileRect(4, 3));

            isEnemyTileColliding = false;
            Attack = 1;
            Health = 1000;


            IsAffectedByGravity = false;

            SetCenteredCollisionRectangle(8, 8, 6, 6);

            InvincibleTimeAfterBeingHit = 0f;
        }

        public override void Kill()
        {
            EffectsManager.AddExplosion(this.CollisionCenter);
            base.Kill();
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            base.Update(gameTime, elapsed);
        }
    }
}