using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame
{
    /// <summary>
    /// This class represents the top of water where it animates like a wave. Since we only need one of these to update
    /// we'll use the flyweight pattern.
    /// </summary>
    public class WaterWaveFlyweight : GameObject
    {
        public WaterWaveFlyweight(bool altColor)
        {
            var ad = new AnimationDisplay();
            this.DisplayComponent = ad;

            AnimationStrip wave;
            if (altColor)
            {
                // For darker backgrounds
                wave = new AnimationStrip(Game1.TileTextures, Helpers.GetTileRect(8, 16), 4, "wave");
            }
            else
            {
                // For lighter backgrounds
                wave = new AnimationStrip(Game1.TileTextures, Helpers.GetTileRect(7, 6), 4, "wave");
            }
            
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
            DisplayComponent.Update(gameTime, elapsed);
        }
    }

    public class WaterWave : GameObject
    {
        protected float _drawDepth;

        public int CellX { get; private set; }
        public int CellY { get; private set; }

        public WaterWave(int cellX, int cellY, float drawDepth)
        {
            WorldLocation = new Vector2(cellX * TileMap.TileSize + TileMap.TileSize / 2, (cellY + 1) * TileMap.TileSize);
            CellX = cellX;
            CellY = cellY;
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

        public override void SetDrawDepth(float depth)
        {
            _drawDepth = depth;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Enabled) return;

            if (!Game1.Camera.IsObjectVisible(new Rectangle(WorldLocation.X.ToInt(), WorldLocation.Y.ToInt(), Game1.TileSize, Game1.TileSize))) return;

            // hijack the flyweight's display component, that's what it's there for.
            var ad = (AnimationDisplay)Game1.WaterWaveFlyweight.DisplayComponent;
            ad.DrawDepth = _drawDepth;
            ad.Draw(spriteBatch, this.WorldLocation, false);
        }
    }

    /// <summary>
    /// Save as Waterwave but an alt color for maps with a darker background.
    /// </summary>
    public class WaterWaveAlt : WaterWave
    {
        public WaterWaveAlt(int cellX, int cellY, float drawDepth) : base(cellX, cellY, drawDepth)
        {
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // hijack the flyweight's display component, that's what it's there for.
            var ad = (AnimationDisplay)Game1.WaterWaveFlyweightAlt.DisplayComponent;
            ad.DrawDepth = _drawDepth;
            ad.Draw(spriteBatch, this.WorldLocation, false);
        }
    }
}