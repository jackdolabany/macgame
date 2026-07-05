using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class BigShooterShip : EnemyShipBase
    {
        private AnimationDisplay _animations => (AnimationDisplay)DisplayComponent;

        private enum State { Unseen, MovingToCenter, Pausing, Shooting, FlyingOff }
        private State _state = State.Unseen;

        private float _lockedScreenOffsetX;
        private const float MoveSpeed = 60f;

        private float _pauseTimer;
        private const float PauseDuration = 2f;

        private float _shootTimer;
        private const float ShootDuration = 2f;

        private float _fireTimer;
        private const float FireInterval = 0.15f;
        private bool _fireFromTop = true;

        private int _shotCycles;
        private const int ShotCyclesBeforeFlyOff = 4;

        private const float ShotSpeed = 180f;


        public BigShooterShip(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;

            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\BigTextures");

            var idle = new AnimationStrip(textures, Helpers.GetBigTileRect(12, 1), 1, "idle");
            idle.LoopAnimation = true;
            idle.FrameLength = 0.2f;
            _animations.Add(idle);

            var shoot = new AnimationStrip(textures, Helpers.GetBigTileRect(13, 1), 2, "shoot");
            shoot.LoopAnimation = true;
            shoot.FrameLength = 0.1f;
            _animations.Add(shoot);

            _animations.Play("idle");

            SetInitialHealth(6);
            Attack = 1;

            SetCenteredCollisionRectangle(16, 16, 12, 12);
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (!Alive || !Game1.Camera.IsObjectVisible(CollisionRectangle))
            {
                base.Update(gameTime, elapsed);
                return;
            }

            switch (_state)
            {
                case State.Unseen:
                    _state = State.MovingToCenter;
                    break;

                case State.MovingToCenter:
                {
                    Velocity = new Vector2(-MoveSpeed, 0);
                    var vp = Game1.Camera.ViewPort;
                    if (CollisionCenter.X <= vp.Right - vp.Width / 3f)
                    {
                        Velocity = Vector2.Zero;
                        _lockedScreenOffsetX = WorldLocation.X - Game1.Camera.Position.X;
                        _pauseTimer = 0f;
                        _state = State.Pausing;
                    }
                    break;
                }

                case State.Pausing:
                    Velocity = Vector2.Zero;
                    WorldLocation = new Vector2(Game1.Camera.Position.X + _lockedScreenOffsetX, WorldLocation.Y);
                    _pauseTimer += elapsed;
                    if (_pauseTimer >= PauseDuration)
                    {
                        _shootTimer = 0f;
                        _fireTimer = 0f;
                        _fireFromTop = true;
                        _animations.Play("shoot");
                        _state = State.Shooting;
                    }
                    break;

                case State.Shooting:
                    Velocity = Vector2.Zero;
                    WorldLocation = new Vector2(Game1.Camera.Position.X + _lockedScreenOffsetX, WorldLocation.Y);
                    _shootTimer += elapsed;
                    _fireTimer += elapsed;

                    if (_fireTimer >= FireInterval)
                    {
                        _fireTimer -= FireInterval;
                        var gunPos = _fireFromTop
                            ? new Vector2(CollisionRectangle.Left, CollisionRectangle.Top + 12)
                            : new Vector2(CollisionRectangle.Left, CollisionRectangle.Bottom + 4);
                        ShotManager.FireMediumShot(gunPos, new Vector2(-ShotSpeed, 0), this);
                        SoundManager.PlaySound("Shoot");
                        _fireFromTop = !_fireFromTop;
                    }

                    if (_shootTimer >= ShootDuration)
                    {
                        _shotCycles++;
                        if (_shotCycles >= ShotCyclesBeforeFlyOff)
                        {
                            _animations.Play("idle");
                            _state = State.FlyingOff;
                        }
                        else
                        {
                            _pauseTimer = 0f;
                            _animations.Play("idle");
                            _state = State.Pausing;
                        }
                    }
                    break;

                case State.FlyingOff:
                    Velocity = new Vector2(MoveSpeed, 0);
                    break;
            }

            base.Update(gameTime, elapsed);
        }
    }
}
