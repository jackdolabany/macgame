using MacGame.DisplayComponents;
using MacGame.Npcs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using TileEngine;
using MacGame.Doors;

namespace MacGame
{
    /// <summary>
    /// The door of the space ship that closes behind mac
    /// </summary>
    public class SpaceShipDoor : Door
    {

        private Player _player;
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        public SpaceShipDoor(ContentManager content, Vector2 spaceShipLocation, Player player) : base(content, 0, 0, player)
        {

            // Adjust the position to the space ship
            WorldLocation = spaceShipLocation + new Vector2(-4, -Game1.TileSize);

            this.CollisionRectangle = new Rectangle(-16, -Game1.TileSize, Game1.TileSize, Game1.TileSize);

            _player = player;

            Enabled = true;

            IsAffectedByForces = false;
            IsAffectedByGravity = false;
            IsAffectedByPlatforms = false;
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;
            isEnemyTileColliding = false;
            isTileColliding = false;

            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\SpaceTextures");
            var close = new AnimationStrip(textures, Helpers.GetTileRect(0, 7), 5, "close");
            close.LoopAnimation = false;
            close.FrameLength = 0.1f;
            animations.Add(close);

            var open = (AnimationStrip)close.Clone();
            open.Name = "open";
            open.Reverse = true;
            animations.Add(open);

            this.Enabled = false;
        }

        public void CloseDoor()
        {
            animations.Play("close");
            this.Enabled = true;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            base.Update(gameTime, elapsed);
        }

        public override void PlayerSlidingOut()
        {
            // Do nothing.
        }
    }
}
