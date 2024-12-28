using System;
using MacGame.Behaviors;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class BeeMinecart : Enemy
    {

        Vector2 startLocation;

        Behavior behavior;

        public BeeMinecart(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            var textures = content.Load<Texture2D>(@"Textures\Textures");
            DisplayComponent = new StaticImageDisplay(textures, Helpers.GetTileRect(3, 9));
            
            isTileColliding = true;
            isEnemyTileColliding = false;
            Attack = 1;
            Health = 1;
            IsAffectedByGravity = true;

            SetCenteredCollisionRectangle(6, 7);
            startLocation = this.WorldLocation;

            Enabled = false;

            behavior = new MinecartEnemyBehavior(player, false);
        }

        public override void Kill()
        {
            EffectsManager.SmallEnemyPop(WorldCenter);

            Enabled = false;
            this.WorldLocation = startLocation;

            base.Kill();
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            behavior.Update(this, gameTime, elapsed);

            base.Update(gameTime, elapsed);
        }
    }
}