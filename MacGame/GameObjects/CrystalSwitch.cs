using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TileEngine;

namespace MacGame.GameObjects
{
    public class CrystalSwitch : GameObject
    {
        StaticImageDisplay orangeImage;
        StaticImageDisplay blueImage;
        private Player _player;

        bool isOrange = true;
        bool isBlue
        {
            get
            {
                return !isOrange;
            }
            set
            {
                isOrange = !value;
            }
        }

        private bool isInitialized = false;

        private float coolDownTimer = 0f;
        private float coolDownTimerMax = 0.5f;

        public CrystalSwitch (ContentManager content, int cellX, int cellY, Player player)
        {
            WorldLocation = new Vector2(cellX * TileMap.TileSize + TileMap.TileSize / 2, (cellY + 1) * TileMap.TileSize);

            _player = player;

            Enabled = true;

            IsAffectedByForces = false;
            IsAffectedByGravity = false;
            IsAffectedByPlatforms = false;
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;
            isEnemyTileColliding = false;
            isTileColliding = false;

            isOrange = true;

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            orangeImage = new StaticImageDisplay(textures, Helpers.GetTileRect(12, 27));
            blueImage = new StaticImageDisplay(textures, Helpers.GetTileRect(13, 27));


            DisplayComponent = orangeImage;


            this.SetWorldLocationCollisionRectangle(8, 8);

        }

        private void SetBlocks()
        {

            foreach (var obj in Game1.CurrentLevel.GameObjects)
            {
                if (obj is CrystalBlock)
                {
                    var cb = (CrystalBlock)obj;
                    if (cb is BlueCrystalBlock)
                    {
                        if (isBlue)
                        {
                            cb.Open();
                        }
                        else
                        {
                            cb.Close();
                        }
                    }

                    if (cb is OrangeCrystalBlock)
                    {
                        if (isOrange)
                        {
                            cb.Open();
                        }
                        else
                        {
                            cb.Close();
                        }
                    }

                }
            }
        }

        public void Trigger()
        {
            if (coolDownTimer <= 0)
            {
                SoundManager.PlaySound("Click");
                var newColor = !isOrange;
                SetAllSwitchColors(newColor);
                SetBlocks();
            }
        }

        private void SetAllSwitchColors(bool isOrange)
        {
            foreach (var gameObject in Game1.CurrentLevel.GameObjects)
            {
                if (gameObject is CrystalSwitch)
                {
                    var cs = (CrystalSwitch)gameObject;
                    cs.SetSwitchColor(isOrange);
                }
            }
        }

        private void SetSwitchColor(bool isOrange)
        {
            this.isOrange = isOrange;
            coolDownTimer = coolDownTimerMax;
            if (isOrange)
            {
                 DisplayComponent = orangeImage;
            }
            else
            {
                DisplayComponent = blueImage;
            }
        }

        private void Initialize()
        {
            SetBlocks();
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (!isInitialized)
            {
                Initialize();
                isInitialized = true;
            }

            // Cooldown timer only decrements if the player is no longer blocking the switch.
            if (coolDownTimer > 0 && !_player.CollisionRectangle.Intersects(this.CollisionRectangle))
            {
                coolDownTimer -= elapsed;
            }

            // Check if the player or an object is holding it down. 
            if (coolDownTimer <= 0 && _player.CollisionRectangle.Intersects(this.CollisionRectangle))
            {
                Trigger();
            }

            // Check if enemies are colliding with it
            if (coolDownTimer <= 0)
            {
                foreach (var enemy in Game1.CurrentLevel.Enemies)
                {
                    if (enemy.Alive && enemy.Enabled && enemy.CollisionRectangle.Intersects(this.CollisionRectangle))
                    {
                        Trigger();
                        break;
                    }
                }
            }

            base.Update(gameTime, elapsed);
        }
    }
}
