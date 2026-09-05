using System;
using MacGame.Behaviors;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    /// <summary>
    /// Faster and weaker than 7
    /// </summary>
    public class EnemyShip6 : EnemyShipBase
    {

        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        public EnemyShip6(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\SpaceTextures");
            var fly = new AnimationStrip(textures, Helpers.GetTileRect(1, 1), 1, "fly");
            fly.LoopAnimation = true;
            fly.FrameLength = 0.14f;
            animations.Add(fly);

            animations.Play("fly");

            Attack = 1;
            SetInitialHealth(3);

            SetCenteredCollisionRectangle(8, 8, 8, 8);

            var shipBehavior = new EnemyShipBehavior(180, camera, player);
            shipBehavior.SetupShootAtPlayer(1.0f, 250f, ShotSize.Medium);
            Behavior = shipBehavior;

        }
    }
}
