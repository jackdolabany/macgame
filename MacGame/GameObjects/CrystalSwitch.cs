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
        /// <summary>
        /// last game object to trigger the switch can't trigger it again until it's not colliding anymore.
        /// </summary>
        GameObject? TriggerObject;

        StaticImageDisplay orangeImage;
        StaticImageDisplay blueImage;
        private Player _player;

        private bool isInitialized = false;

        private float coolDownTimer = 0f;
        private float coolDownTimerMax = 0.5f;


        /// <summary>
        /// If the blocks should change from one color to another we set this to eventually set the blocks the way they are supposed to be. 
        /// We must first wait until no blocks are interacting with the player because that could cause weird collision issues.
        /// </summary>
        bool isWaitingToSetBlocks = false;

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

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            orangeImage = new StaticImageDisplay(textures, Helpers.GetTileRect(12, 27));
            blueImage = new StaticImageDisplay(textures, Helpers.GetTileRect(13, 27));

            DisplayComponent = orangeImage;

            this.SetWorldLocationCollisionRectangle(8, 8);

        }

        public override void SetDrawDepth(float depth)
        {
            base.SetDrawDepth(depth);
            orangeImage.DrawDepth = depth;
            blueImage.DrawDepth = depth;
        }

        /// <summary>
        /// Open or close blocks based on the color set in LevelState.
        /// </summary>
        private void SetAllBlocks()
        {

            foreach (var obj in Game1.CurrentLevel.GameObjects)
            {
                if (obj is CrystalBlock)
                {
                    var cb = (CrystalBlock)obj;
                    if (cb is BlueCrystalBlock)
                    {
                        if (Game1.LevelState.CrystalSwitchIsBlue)
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
                        if (Game1.LevelState.CrystalSwitchIsOrange)
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

        private void SetBlocksWhenNotPlayerColliding()
        {
            isWaitingToSetBlocks = true;
        }

        public void ChangeColor(GameObject? triggerObject = null)
        {
            if (coolDownTimer <= 0)
            {
                SoundManager.PlaySound("Click");
                
                // Set all switch colors
                Game1.LevelState.CrystalSwitchIsOrange = !Game1.LevelState.CrystalSwitchIsOrange;
                
                foreach (var gameObject in Game1.CurrentLevel.GameObjects)
                {
                    if (gameObject is CrystalSwitch)
                    {
                        var cs = (CrystalSwitch)gameObject;
                        cs.coolDownTimer = coolDownTimerMax;
                        if (Game1.LevelState.CrystalSwitchIsOrange)
                        {
                            cs.DisplayComponent = orangeImage;
                        }
                        else
                        {
                            cs.DisplayComponent = blueImage;
                        }
                    }
                }

                SetBlocksWhenNotPlayerColliding();
                TriggerObject = triggerObject;
            }
        }

        private void Initialize()
        {
            SetAllBlocks();
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (!isInitialized)
            {
                Initialize();
                isInitialized = true;
            }

            // Clear the trigger object if it's no longer colliding or disabled.
            if (TriggerObject != null)
            {
                if (!TriggerObject.Enabled || !TriggerObject.CollisionRectangle.Intersects(this.CollisionRectangle))
                {
                    TriggerObject = null;
                }
            }

            // Cooldown timer only decrements if the player is no longer blocking the switch.
            if (coolDownTimer > 0 && _player != TriggerObject && !_player.CollisionRectangle.Intersects(this.CollisionRectangle))
            {
                coolDownTimer -= elapsed;
            }

            // Check if the player or an object is holding it down. 
            if (coolDownTimer <= 0 && _player.CollisionRectangle.Intersects(this.CollisionRectangle))
            {
                TriggerObject = _player;
                ChangeColor(_player);
            }

            // Check if enemies are colliding with it
            if (coolDownTimer <= 0)
            {
                foreach (var enemy in Game1.CurrentLevel.Enemies)
                {
                    if (enemy.Alive && enemy != TriggerObject && enemy.Enabled && enemy.CollisionRectangle.Intersects(this.CollisionRectangle))
                    {
                        ChangeColor(enemy);
                        break;
                    }
                }
            }

            if (isWaitingToSetBlocks)
            {
                // Wait until the player is no longer colliding with any blocks to set the blocks.
                bool isPlayerCollidingWithAnyBlocks = false;
                foreach (var obj in Game1.CurrentLevel.GameObjects)
                {
                    if (obj is CrystalBlock)
                    {
                        var cb = (CrystalBlock)obj;
                        if (cb.CollisionRectangle.Intersects(_player.CollisionRectangle))
                        {
                            isPlayerCollidingWithAnyBlocks = true;
                            break;
                        }
                    }
                }
                if (!isPlayerCollidingWithAnyBlocks)
                {
                    SetAllBlocks();
                    isWaitingToSetBlocks = false;
                }
            }

            base.Update(gameTime, elapsed);
        }
    }
}
