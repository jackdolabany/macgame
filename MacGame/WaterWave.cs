using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection.Metadata.Ecma335;
using TileEngine;

namespace MacGame
{
    /// <summary>
    /// This class represents the top of water where it animates like a wave. Since we only need one of these to update
    /// we'll use the flyweight pattern.
    /// </summary>
    public class WaterWaveFlyweight : GameObject
    {
        public WaterWaveFlyweight()
        {
            var ad = new AnimationDisplay();
            this.DisplayComponent = ad;

            var wave = new AnimationStrip(Game1.TileTextures, Helpers.GetTileRect(7, 6), 4, "wave");
            wave.LoopAnimation = true;
            wave.Oscillate = true;
            wave.FrameLength = 0.2f;
            ad.Add(wave);

            ad.Play("wave");

            Enabled = true;

            IsAffectedByForces = false;
            IsAffectedByGravity = false;
            IsAffectedByPlatforms = false;
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;
            isEnemyTileColliding = false;
            isTileColliding = false;
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            // There's no real logic here. Just update the display component.
            DisplayComponent.Update(gameTime, elapsed, Vector2.Zero, false);
        }
    }

    public class WaterWave : GameObject
    {
        private float _drawDepth;

        public WaterWave(int cellX, int cellY, float drawDepth)
        {
            WorldLocation = new Vector2(cellX * TileMap.TileSize, cellY * TileMap.TileSize);
            Enabled = true;
            IsAffectedByForces = false;
            IsAffectedByGravity = false;
            IsAffectedByPlatforms = false;
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;
            isEnemyTileColliding = false;
            isTileColliding = false;
            _drawDepth = drawDepth;
        }

        /// <summary>
        /// Normally drawDepth is grabbed from the underlying DrawObject, but this
        /// class is special and hijacks a shared drawObject.
        /// </summary>
        public override float DrawDepth => _drawDepth;

        public override void Update(GameTime gameTime, float elapsed)
        {
            // Do nothing
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // hijack the flyweight's display component, that's what it's there for.
            var ad = (AnimationDisplay)Game1.WaterWaveFlyweight.DisplayComponent;
            ad.WorldLocation = this.WorldLocation;
            ad.DrawDepth = _drawDepth;
            ad.Draw(spriteBatch);
        }
    }
}