using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    /// <summary>
    /// The head of the Canada goose boss. The boss stretches his head out and it moves across the screen.
    /// The neck will fill in behind it.
    /// </summary>
    public class CanadaGooseHead : Enemy
    {

        private Player _player;

        public CanadaGooseHead(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            var textures = content.Load<Texture2D>(@"Textures\BigTextures");
            
            DisplayComponent = new StaticImageDisplay(textures, Helpers.GetBigTileRect(11, 0));
            IsAffectedByGravity = false;
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;
            isTileColliding = false;
            isEnemyTileColliding = false;

            SetCenteredCollisionRectangle(16, 8);
            
            this.Enabled = false;
            this.Dead = true;
          
            _player = player;
        }
    }
}