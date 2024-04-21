using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class YarnBall : Enemy
    {

        StaticImageDisplay image => (StaticImageDisplay)DisplayComponent;

        public YarnBall(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            var textures = content.Load<Texture2D>(@"Textures\Textures");
            DisplayComponent = new StaticImageDisplay(textures);
            image.Source = Helpers.GetTileRect(5, 2);

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
            EffectsManager.EnemyPop(WorldCenter, 10, Color.Pink, 30f);

            Enabled = false;
            base.Kill();
        }
    }
}