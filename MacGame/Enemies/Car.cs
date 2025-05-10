using System;
using System.ComponentModel.Design;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{

    /// <summary>
    /// Just a random car you beat up.
    /// </summary>
    public class Car : Enemy
    {
        const int MaxHealth = 6;

        StaticImageDisplay normalCar;
        StaticImageDisplay beatUpCar;
        StaticImageDisplay deadCar;
        StaticImageDisplay bike;

        private bool _isInitialized = false;
        private BlueSmoke _blueSmoke;
        private OrangeSmoke _orangeSmoke;

        public Car(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var reallyBigTexture = content.Load<Texture2D>(@"Textures\ReallyBigTextures");

            normalCar = new StaticImageDisplay(reallyBigTexture, Helpers.GetReallyBigTileRect(4, 2));
            beatUpCar = new StaticImageDisplay(reallyBigTexture, Helpers.GetReallyBigTileRect(5, 2));
            deadCar = new StaticImageDisplay(reallyBigTexture, Helpers.GetReallyBigTileRect(6, 2));
            bike = new StaticImageDisplay(reallyBigTexture, Helpers.GetReallyBigTileRect(4, 3));

            DisplayComponent = normalCar;

            isEnemyTileColliding = false;
            Attack = 0;

            Health = MaxHealth;

            IsAffectedByGravity = true;
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;

            SetWorldLocationCollisionRectangle(18, 12);

            // Set up smoke puffs
            _blueSmoke = new BlueSmoke(content);
            _blueSmoke.Enabled = false;

            _orangeSmoke = new OrangeSmoke(content);
            _orangeSmoke.Enabled = false;
        }

        public override void Kill()
        {
            base.Kill();
            Enabled = true;
            DisplayComponent = deadCar;
            _blueSmoke.Enabled = true;
            _orangeSmoke.Enabled = true;
            Game1.LevelState.JobState = JobState.CarDestroyed;
        }

        public override void PlayDeathSound()
        {
            SoundManager.PlaySound("HarshHit");
        }

        private void Initialize()
        {
            _blueSmoke.SetDrawDepth(this.DrawDepth + Game1.MIN_DRAW_INCREMENT);
            _blueSmoke.WorldLocation = this.WorldLocation + new Vector2(20, -20);
            _orangeSmoke.SetDrawDepth(this.DrawDepth + Game1.MIN_DRAW_INCREMENT);
            _orangeSmoke.WorldLocation = this.WorldLocation + new Vector2(-20, -25);

            if (Game1.LevelState.JobState == JobState.CarDamaged)
            {
                DisplayComponent = beatUpCar;
                Health = 4;
            }
            else if (Game1.LevelState.JobState == JobState.CarDestroyed)
            {
                DisplayComponent = deadCar;
                _blueSmoke.Enabled = true;
                _orangeSmoke.Enabled = true;
                Health = 0;
                Alive = false;
            }
        }

        public void SetToBike()
        {
            DisplayComponent = bike;
            _blueSmoke.Enabled = false;
            _orangeSmoke.Enabled = false;
            CollisionRectangle = Rectangle.Empty;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (!_isInitialized)
            {
                Initialize();
                _isInitialized = true;
            }

            base.Update(gameTime, elapsed);

            if (Health < 4 && Health > 0)
            {
                DisplayComponent = beatUpCar;
                if (Game1.LevelState.JobState == JobState.Accepted)
                {
                    Game1.LevelState.JobState = JobState.CarDamaged;
                }
            }

            _blueSmoke.Update(gameTime, elapsed);
            _orangeSmoke.Update(gameTime, elapsed);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            _blueSmoke.Draw(spriteBatch);
            _orangeSmoke.Draw(spriteBatch);
        }
    }
}