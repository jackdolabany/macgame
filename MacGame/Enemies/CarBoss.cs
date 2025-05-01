using System;
using System.ComponentModel.Design;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class CarBoss : Enemy
    {
        const int MaxHealth = 6;

        StaticImageDisplay normalCar;
        StaticImageDisplay beatUpCar;
        StaticImageDisplay deadCar;


        public CarBoss(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var reallyBigTexture = content.Load<Texture2D>(@"Textures\ReallyBigTextures");

            normalCar = new StaticImageDisplay(reallyBigTexture, Helpers.GetReallyBigTileRect(4, 2));
            beatUpCar = new StaticImageDisplay(reallyBigTexture, Helpers.GetReallyBigTileRect(5, 2));
            deadCar = new StaticImageDisplay(reallyBigTexture, Helpers.GetReallyBigTileRect(6, 2));

            DisplayComponent = normalCar;

            isEnemyTileColliding = false;
            Attack = 0;

            Health = MaxHealth;

            IsAffectedByGravity = true;
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;

            SetWorldLocationCollisionRectangle(14, 14);

            // TODO Set up smoke puffs
        }

        public override void Kill()
        {
            base.Kill();
            Enabled = true;
            DisplayComponent = deadCar;
        }

        public override void PlayDeathSound()
        {
            SoundManager.PlaySound("HarshHit");
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            base.Update(gameTime, elapsed);

            Game1.DrawBossHealth = true;
            Game1.MaxBossHealth = MaxHealth;
            Game1.BossHealth = Health;
            Game1.BossName = "Car";

            if (Health < 4 && Health > 0)
            {
                DisplayComponent = beatUpCar;
            }
        }
    }
}