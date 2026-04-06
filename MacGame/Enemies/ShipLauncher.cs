using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class ShipLauncher : Enemy
    {
        private const float LaunchCooldown = 1f;

        private float cooldownTimer = LaunchCooldown;
        private EnemyShipBase _ship;
        private Texture2D textures;

        private enum State
        {
            Cooldown,
            Opening,
            Closing,
            WaitingForShipDeath
        }

        private State currentState = State.Cooldown;

        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        public ShipLauncher(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            textures = content.Load<Texture2D>(@"Textures\BigTextures");

            DisplayComponent = new AnimationDisplay();

            var idle = new AnimationStrip(textures, Helpers.GetBigTileRect(7, 9), 1, "idle");
            idle.LoopAnimation = false;
            animations.Add(idle);

            var open = new AnimationStrip(textures, Helpers.GetBigTileRect(7, 9), 4, "open");
            open.LoopAnimation = false;
            open.FrameLength = 0.12f;
            animations.Add(open);

            var close = (AnimationStrip)open.Clone();
            close.Name = "close";
            close.Reverse = true;
            animations.Add(close);

            var dead = new AnimationStrip(textures, Helpers.GetBigTileRect(11, 9), 1, "dead");
            dead.LoopAnimation = false;
            animations.Add(dead);

            animations.Play("idle");

            isEnemyTileColliding = false;
            isTileColliding = false;
            Attack = 1;
            Health = 3;
            IsAffectedByGravity = false;
            IsAffectedByForces = false;
            IsAbleToMoveOutsideOfWorld = false;
            InvincibleTimeAfterBeingHit = 0.1f;

            SetWorldLocationCollisionRectangle(14, 6);

            _ship = new EnemyShip(content, cellX, cellY, player, camera);
            _ship.Enabled = false;
            AddEnemyInConstructor(_ship);
        }

        public override void Kill()
        {
            EffectsManager.AddExplosion(WorldCenter, false);
            Dead = true;
            PlayDeathSound();

            // Don't call the base method so we keep drawing the destroyed texture.
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (Alive)
            {
                switch (currentState)
                {
                    case State.Cooldown:
                        if (IsOnScreen())
                        {
                            cooldownTimer -= elapsed;
                            if (cooldownTimer <= 0)
                            {
                                currentState = State.Opening;
                                animations.Play("open");
                            }
                        }
                        break;

                    case State.Opening:
                        if (animations.CurrentAnimation!.FinishedPlaying)
                        {
                            _ship.Revive(WorldLocation);
                            currentState = State.Closing;
                            animations.Play("close");
                        }
                        break;

                    case State.Closing:
                        if (animations.CurrentAnimation!.FinishedPlaying)
                        {
                            currentState = State.WaitingForShipDeath;
                            animations.Play("idle");
                        }
                        break;

                    case State.WaitingForShipDeath:
                        if (_ship.Dead || !_ship.Enabled)
                        {
                            cooldownTimer = LaunchCooldown;
                            currentState = State.Cooldown;
                        }
                        break;
                }
            }
            else
            {
                animations.PlayIfNotAlreadyPlaying("dead");
            }

            base.Update(gameTime, elapsed);
        }
    }
}
