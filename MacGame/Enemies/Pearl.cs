using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    /// <summary>
    /// The pearl is an idle ball shot by the Clam.
    /// </summary>
    public class Pearl : Enemy
    {

        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        public Pearl(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new StaticImageDisplay(content.Load<Texture2D>(@"Textures\Textures"), Helpers.GetTileRect(15, 8));
            
            isTileColliding = false;
            isEnemyTileColliding = false;
            Attack = 1;
            Health = 1;
            IsAffectedByGravity = false;

            IsAbleToSurviveOutsideOfWorld = false;

            SetCenteredCollisionRectangle(4, 4);
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (!Enabled) return;

            if (!Game1.Camera.IsObjectVisible(this.CollisionRectangle))
            {
                this.Enabled = false;
            }

            var mapSquare = Game1.CurrentMap.GetMapSquareAtPixel(WorldCenter);

            if (mapSquare == null)
            {
                this.Enabled = false;
            }
            else if (!mapSquare.Passable)
            {
                Kill();
            }

            base.Update(gameTime, elapsed);
        }

        public override void Kill()
        {
            EffectsManager.SmallEnemyPop(WorldCenter);

            Enabled = false;
            base.Kill();
        }
    }
}