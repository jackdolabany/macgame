using MacGame.DisplayComponents;
using MacGame.Platforms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using TileEngine;

namespace MacGame
{


    /// <summary>
    /// Stand in front of this tile for your directional pad to control a mapped GhostPlatformControllable.
    /// 
    /// Put an object modifier over this tile and set the PlatformName property to the name of the GhostPlatformControllable you want to control.
    /// </summary>
    public class GhostPlatformController : GameObject
    {
        private bool IsInitialized = false;
        GhostPlatformControllable platform;

        public string PlatformName { get; set; } = "";

        private Player _player;

        public GhostPlatformController(ContentManager content, int cellX, int cellY, Player player)
        {
            this.WorldLocation = new Vector2(cellX * TileMap.TileSize + TileMap.TileSize / 2, (cellY + 1) * TileMap.TileSize);
            Enabled = true;

            var texture = content.Load<Texture2D>(@"Textures/Textures2");
            DisplayComponent = new StaticImageDisplay(texture, Helpers.GetTileRect(7, 4));
            SetWorldLocationCollisionRectangle(4, 4);
            _player = player;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (!IsInitialized)
            {
                foreach (var platform in Game1.CurrentLevel.Platforms)
                {
                    if (platform is GhostPlatformControllable controllable && controllable.Name == this.PlatformName)
                    {
                        this.platform = controllable;
                        IsInitialized = true;
                        break;
                    }
                }

                if (this.platform == null)
                {
                   if (Game1.IS_DEBUG)
                    {
                        throw new Exception("Ghost platform missing sub platform with name: " + this.PlatformName);
                    }
                }
            }

            platform.StopMoving();

            // Control the platform
            if (_player.OnGround && _player.CollisionRectangle.Intersects(this.CollisionRectangle))
            {

                if (_player.InputManager.CurrentAction.left && _player.OnLeftWall)
                {
                    platform.MoveLeft();
                }
                else if (_player.InputManager.CurrentAction.right && _player.OnRightWall)
                {
                    platform.MoveRight();
                }

                if (_player.InputManager.CurrentAction.up)
                {
                    platform.MoveUp();
                }
                else if (_player.InputManager.CurrentAction.down)
                {
                    platform.MoveDown();
                }
            }

            base.Update(gameTime, elapsed);
        }

    }

}
