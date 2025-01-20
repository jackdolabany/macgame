using System;
using System.Collections.Generic;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;
using static System.Net.Mime.MediaTypeNames;

namespace MacGame.Enemies
{
    /// <summary>
    /// The pearl is an idle ball shot by the Clam.
    /// </summary>
    public class BlowfishSpike : Enemy
    {
        public EightWayRotation RotationDirection { get; set; }

        StaticImageDisplay straightImage;
        StaticImageDisplay diagonalImage;

        public BlowfishSpike(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            straightImage = new StaticImageDisplay(content.Load<Texture2D>(@"Textures\Textures"), Helpers.GetTileRect(8, 26));
            diagonalImage = new StaticImageDisplay(content.Load<Texture2D>(@"Textures\Textures"), Helpers.GetTileRect(9, 26));

            DisplayComponent = new AggregateDisplay(new List<DisplayComponent> { straightImage, diagonalImage });

            isTileColliding = false;
            isEnemyTileColliding = false;
            Attack = 1;
            Health = 1;
            IsAffectedByGravity = false;

            IsAbleToSurviveOutsideOfWorld = true;
            CanBeHitWithWeapons = false;
            CanBeJumpedOn = false;

            this.CollisionRectangle = new Rectangle(-5, -21, 10, 10);
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (!Enabled) return;

            base.Update(gameTime, elapsed);

            straightImage.TintColor = Color.Transparent;
            straightImage.Rotation = 0;
            straightImage.Effect = SpriteEffects.None;

            diagonalImage.TintColor = Color.Transparent;
            diagonalImage.Rotation = 0;
            diagonalImage.Effect = SpriteEffects.None;

            // Show the correct image and rotate properly.
            switch (this.RotationDirection.Direction)
            {
                case EightWayRotationDirection.Right:
                    straightImage.TintColor = Color.White;
                    straightImage.Rotation = MathHelper.PiOver2;
                    break;
                case EightWayRotationDirection.DownRight:
                    diagonalImage.TintColor = Color.White;
                    diagonalImage.Effect = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
                    break;
                case EightWayRotationDirection.Down:
                    straightImage.TintColor = Color.White;
                    straightImage.Effect = SpriteEffects.FlipVertically;
                    break;
                case EightWayRotationDirection.DownLeft:
                    diagonalImage.TintColor = Color.White;
                    diagonalImage.Effect = SpriteEffects.FlipVertically;
                    break;
                case EightWayRotationDirection.Left:
                    straightImage.TintColor = Color.White;
                    straightImage.Rotation = -MathHelper.PiOver2;
                    break;
                case EightWayRotationDirection.UpLeft:
                    diagonalImage.TintColor = Color.White;
                    break;
                case EightWayRotationDirection.Up:
                    straightImage.TintColor = Color.White;
                    break;
                case EightWayRotationDirection.UpRight:
                    diagonalImage.TintColor = Color.White;
                    diagonalImage.Effect = SpriteEffects.FlipHorizontally;
                    break;
            }
        }

        public override void Kill()
        {
            EffectsManager.SmallEnemyPop(WorldCenter);

            Enabled = false;
            base.Kill();
        }
    }
}