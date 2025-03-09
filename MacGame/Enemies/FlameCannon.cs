using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class FlameCannon : Enemy
    {

        private AnimationDisplay _animations;

        float idleTimer = 0f;
        float flameTimer = 0f;

        // True when the flame is coming out.
        bool isOn = false;

        Rectangle _collisionRectangle;
        Rectangle _smallCollisionRectangle;

        public FlameCannon(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            var textures2 = content.Load<Texture2D>(@"Textures\Textures2");
            var bigTextures = content.Load<Texture2D>(@"Textures\BigTextures");

            var potImage = new StaticImageDisplay(textures2, Helpers.GetTileRect(1, 0));
            
            _animations = new AnimationDisplay();
            var start = new AnimationStrip(bigTextures, Helpers.GetBigTileRect(11, 0), 3, "start");
            start.LoopAnimation = false;
            start.FrameLength = 0.3f;
            _animations.Add(start);

            var end = (AnimationStrip)start.Clone();
            end.Reverse = true;
            end.Name = "end";
            _animations.Add(end);

            var flame = new AnimationStrip(bigTextures, Helpers.GetBigTileRect(13, 0), 2, "flame");
            flame.LoopAnimation = true;
            flame.FrameLength = 0.3f;
            _animations.Add(flame);

            // This enemy is composed of a static pot along with a flame animation.
            var displayComponents = new DisplayComponent[] { potImage, _animations };

            this.DisplayComponent = new AggregateDisplay(displayComponents);

            _animations.Play("flame");

            isTileColliding = false;
            isEnemyTileColliding = false;
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;
            CanBeHitWithWeapons = false;
            CanBeJumpedOn = false;
            Attack = 1;
            Health = 100;
            IsAffectedByGravity = false;
            Enabled = true;

            _animations.Offset += new Vector2(0, -Game1.TileSize);

            _collisionRectangle = new Rectangle(-12, -82, 24, 56);
            _smallCollisionRectangle = new Rectangle(-12, -42, 24, 16);

            CollisionRectangle = Rectangle.Empty;
        }


        public override void Update(GameTime gameTime, float elapsed)
        {
            base.Update(gameTime, elapsed);

            if (!isOn)
            {
                _animations.TintColor = Color.Transparent;
                idleTimer += elapsed;
                if (idleTimer > 3f)
                {
                    idleTimer = 0f;
                    flameTimer = 0f;
                    isOn = true;
                    _animations.Play("start");
                    
                }
            }
            else
            {
                _animations.TintColor = Color.White;
                flameTimer += elapsed;

                if (_animations.CurrentAnimationName == "start")
                {
                    if (_animations.CurrentAnimation.currentFrameIndex == 0)
                    {
                        CollisionRectangle = _smallCollisionRectangle;
                    }
                    else
                    {
                        CollisionRectangle = _collisionRectangle;
                    }

                    if (_animations.CurrentAnimation.FinishedPlaying)
                    {
                        _animations.Play("flame");
                    }
                }
                else if (_animations.CurrentAnimationName == "flame")
                {
                    if (flameTimer > 2.5f)
                    {
                        _animations.Play("end");
                    }
                }
                else if (_animations.CurrentAnimationName == "end")
                {
                    if (_animations.CurrentAnimation.currentFrameIndex == 0)
                    {
                        CollisionRectangle = _collisionRectangle;
                    }
                    else
                    {
                        CollisionRectangle = _smallCollisionRectangle;
                    }

                    if (_animations.CurrentAnimation.FinishedPlaying)
                    {
                        idleTimer = 0f;
                        flameTimer = 0f;
                        isOn = false;
                        CollisionRectangle = Rectangle.Empty;
                    }
                }

            }
            
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }
    }
}