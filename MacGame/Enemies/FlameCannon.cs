using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public abstract class FlameCannonBase : Enemy
    {

        protected AnimationDisplay _animations;

        float idleTimer = 0f;
        float flameTimer = 0f;

        // True when the flame is coming out.
        bool isOn = false;

        bool soundPlayed = false;

        protected abstract Rectangle GetPotImageSourceRectangle();
        protected abstract Rectangle _smallCollisionRectangle { get; }
        protected abstract Rectangle _collisionRectangle { get; }

        protected float Rotation { get; set; }

        

        public FlameCannonBase(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            var textures2 = content.Load<Texture2D>(@"Textures\Textures2");
            var bigTextures = content.Load<Texture2D>(@"Textures\BigTextures");

            var potImage = new StaticImageDisplay(textures2, GetPotImageSourceRectangle());
            
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

                        if (!soundPlayed && Game1.Camera.IsObjectVisible(this.CollisionRectangle))
                        {
                            SoundManager.PlaySlowFlame();
                            soundPlayed = true;
                        }

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
                        soundPlayed = false;
                    }
                }

            }
            
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }
    }

    public class FlameCannonUp : FlameCannonBase
    {
        public FlameCannonUp(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
        }

        protected override Rectangle GetPotImageSourceRectangle()
        {
            return Helpers.GetTileRect(1, 0);
        }
        protected override Rectangle _collisionRectangle => new Rectangle(-12, -82, 24, 56);
        protected override Rectangle _smallCollisionRectangle => new Rectangle(-12, -42, 24, 16);
    }

    public class FlameCannonDown : FlameCannonBase
    {
        public FlameCannonDown(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            _animations.Rotation = MathHelper.Pi;
            _animations.Offset = new Vector2(-4, 64);
        }

        protected override Rectangle GetPotImageSourceRectangle()
        {
            return Helpers.GetTileRect(2, 0);
        }
        protected override Rectangle _collisionRectangle => new Rectangle(-12, 0, 24, 56);
        protected override Rectangle _smallCollisionRectangle => new Rectangle(-12, 0, 24, 16);
    }

    public class FlameCannonLeft : FlameCannonBase
    {
        public FlameCannonLeft(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            _animations.Rotation = MathHelper.Pi + MathHelper.PiOver2;
            _animations.Offset = new Vector2(-48, 16);
        }

        protected override Rectangle GetPotImageSourceRectangle()
        {
            return Helpers.GetTileRect(1, 1);
        }

        protected override Rectangle _collisionRectangle => new Rectangle(-70, -26, 56, 24);
        protected override Rectangle _smallCollisionRectangle => new Rectangle(-32, -26, 16, 24);
    }

    public class FlameCannonRight : FlameCannonBase
    {
        public FlameCannonRight(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            _animations.Rotation = MathHelper.PiOver2;
            _animations.Offset = new Vector2(48, 16);
        }

        protected override Rectangle GetPotImageSourceRectangle()
        {
            return Helpers.GetTileRect(2, 1);
        }
        protected override Rectangle _collisionRectangle => new Rectangle(16, -28, 56, 24);
        protected override Rectangle _smallCollisionRectangle => new Rectangle(16, -28, 16, 24);
    }
}