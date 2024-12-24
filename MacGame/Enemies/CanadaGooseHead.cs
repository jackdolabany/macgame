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

        public CanadaGooseHead(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            var textures = content.Load<Texture2D>(@"Textures\ReallyBigTextures");
            
            DisplayComponent = new StaticImageDisplay(textures, Helpers.GetReallyBigTileRect(2, 1));
            IsAffectedByGravity = false;
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;
            isTileColliding = false;
            isEnemyTileColliding = false;
            CanBeHitWithWeapons = false;
            CanBeJumpedOn = false;
            Health = 100000;

            CollisionRectangle = new Rectangle(-48, -48, 64, 48);

            this.Enabled = false;
            this.Dead = true;
        }
    }
}