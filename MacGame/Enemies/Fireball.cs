using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    /// <summary>
    /// A fireball that just sits there and does nothing. Some other class should control it.
    /// </summary>
    public class Fireball : Enemy
    {


        public Fireball(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var ad = new AnimationDisplay();
            this.DisplayComponent = ad;
            
            var fire = new AnimationStrip(textures, Helpers.GetTileRect(4, 22), 4, "fire");
            fire.LoopAnimation = true;
            fire.FrameLength = 0.2f;
            ad.Add(fire);

            ad.Play("fire");

            isTileColliding = false;
            isEnemyTileColliding = false;
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;
            CanBeHitWithWeapons = false;
            CanBeJumpedOn = false;
            Attack = 1;
            Health = 100;
            IsAffectedByGravity = false;
            Enabled = true;

            SetWorldLocationCollisionRectangle(8, 8);

            ad.Offset += new Vector2(0, 16);
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            this.CollisionRectangle = new Rectangle(-8, -8, 16, 16);

            base.Update(gameTime, elapsed);
        }


    }

  
}