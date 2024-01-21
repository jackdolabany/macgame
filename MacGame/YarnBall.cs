using System;
using MacGame;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame
{
    public class YarnBall : Enemy
    {

        StaticImageDisplay image => (StaticImageDisplay)DisplayComponent;

        private float speed = 10;

        public YarnBall(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            var textures = content.Load<Texture2D>(@"Textures\Textures");
            this.DisplayComponent = new StaticImageDisplay(textures);
            image.Source = new Rectangle(5 * 8, 2 * 8, 8, 8);
           
            isTileColliding = false;
            isEnemyTileColliding = false;
            Attack = 1;
            Health = 1;
            IsAffectedByGravity = false;
            IsAbleToSurviveOutsideOfWorld = false;
            IsAbleToMoveOutsideOfWorld = true;

            SetCenteredCollisionRectangle(7, 7);
        }

        public override void Kill()
        {
            EffectsManager.EnemyPop(this.WorldCenter, 10, Color.Pink, 30f);

            this.Enabled = false;
            base.Kill();
        }
    }
}