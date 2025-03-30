using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    /// <summary>
    /// A piece of the stretchy neck of the Canada Goose Boss. This literally does nothing, just stretches out
    /// and the goose will add or remove these.
    /// </summary>
    public class CanadaGooseNeck : Enemy
    {
        public CanadaGooseNeck(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            var textures = content.Load<Texture2D>(@"Textures\Textures");
            
            DisplayComponent = new StaticImageDisplay(textures, Helpers.GetTileRect(8, 27));
            IsAffectedByGravity = false;
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;
            isTileColliding = false;
            isEnemyTileColliding = false;
            CanBeHitWithWeapons = false;
            CanBeJumpedOn = false;
            Health = 100000;

            SetWorldLocationCollisionRectangle(8, 6);
           
            this.Enabled = false;
            this.Dead = true;
        }
    }
}