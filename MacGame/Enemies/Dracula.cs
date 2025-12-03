using System;
using System.Collections.Generic;
using MacGame.DisplayComponents;
using MacGame.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Enemies
{
    public class Dracula : Enemy
    {

        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        const int MaxHealth = 6;
        
        /// <summary>
        /// After death the boss will reveal a sock.
        /// </summary>
        private Sock Sock;
        float revealSockTimer = 0f;

        private bool _isInitialized = false;

        Vector2 middleLocation;
        Vector2 leftLocation;
        Vector2 rightLocation;
        Vector2[] locations;
        Vector2 offScreenLocation;

        float disappearTimer = 0f;

        public enum DraculaState
        {
            Idle,
            Attacking,
            Dying,
            Dead
        }

        public DraculaState state = DraculaState.Idle;

        Rectangle _hittableCollisionRectangle;

        Player _player;

        public List<DraculaFireball> FireBalls;
        public List<DraculaBat> Bats;
        private List<DraculaDeathBat> DeathBats;

        // Used to create the flash effect when Drac disappears.
        private Rectangle whiteSource;
        private Rectangle blueSource;
        private const float flashEffectTimerGoal = 0.7f;
        private float flashEffectTimer = flashEffectTimerGoal;
        private Vector2 flashEffectLocation;

        public Dracula(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {

            _player = player;

            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\ReallyBigTextures");
            var idle = new AnimationStrip(textures, Helpers.GetReallyBigTileRect(0, 2), 1, "idle");
            idle.LoopAnimation = false;
            idle.FrameLength = 0.05f;
            animations.Add(idle);

            animations.Play("idle");

            var openCape = new AnimationStrip(textures, Helpers.GetReallyBigTileRect(0, 2), 2, "openCape");
            openCape.LoopAnimation = false;
            openCape.FrameLength = 0.7f;
            animations.Add(openCape);

            isEnemyTileColliding = false;
            Attack = 1;

            Health = MaxHealth;

            // TODO: Undo
            //Health = 1;

            IsAffectedByGravity = false;
            IsAbleToMoveOutsideOfWorld = true;
            IsAbleToSurviveOutsideOfWorld = true;
            isTileColliding = false;
            CanBeJumpedOn = true;
            CanBeHitWithWeapons = true;

            SetWorldLocationCollisionRectangle(8, 20);

            // we'll set the collision rectangle to empty so that the player can walk through the boss
            // And reset it after you talk to drac
            _hittableCollisionRectangle = collisionRectangle;
            CollisionRectangle = Rectangle.Empty;

            FireBalls = new List<DraculaFireball>();
            for (int i = 0; i < 3; i++)
            {
                var fireball = new DraculaFireball(content, 0, 0, player, camera);
                fireball.Enabled = false;
                FireBalls.Add(fireball);
            }
            ExtraEnemiesToAddAfterConstructor.AddRange(FireBalls);

            Bats = new List<DraculaBat>();
            for (int i = 0; i < 2; i++)
            {
                var bat = new DraculaBat(content, 0, 0, player, camera);
                bat.Enabled = false;
                Bats.Add(bat);
            }
            ExtraEnemiesToAddAfterConstructor.AddRange(Bats);

            DeathBats = new List<DraculaDeathBat>();
            for (int i = 0; i < 20; i++)
            {
                var bat = new DraculaDeathBat(content, 0, 0, player, camera);
                bat.Enabled = false;
                DeathBats.Add(bat);
            }
            ExtraEnemiesToAddAfterConstructor.AddRange(DeathBats);


            var flashEffectTile = Helpers.GetTileRect(2, 23);
            // The blue is the left 2 pixels of this tile.
            blueSource = new Rectangle(flashEffectTile.X, flashEffectTile.Y, 2 * Game1.TileScale, Game1.TileSize);
            
            // Shift the blue source over 2 pixels to get the white.
            whiteSource = blueSource;
            whiteSource.X += 2 * Game1.TileScale;
        }

        public override void TakeHit(GameObject attacker, int damage, Vector2 force)
        {
            if (IsTempInvincibleFromBeingHit)
            {
                return;
            }

            SoundManager.PlaySound("HitEnemy2");

            Health -= damage;

            if (Health == 4)
            {
                Bats[0].WorldLocation = this.CollisionCenter;
                Bats[0].Enabled = true;
                Bats[0].Alive = true;
                Bats[0].Velocity = new Vector2(200, 200);
                SoundManager.PlaySound("BatChirp");
            }

            if (Health == 2)
            {
                Bats[1].WorldLocation = this.CollisionCenter;
                Bats[1].Enabled = true;
                Bats[1].Alive = true;
                Bats[1].Velocity = new Vector2(-200, 200);
                SoundManager.PlaySound("BatChirp");
            }

            if (!IsTempInvincibleFromBeingHit)
            {
                InvincibleTimer += 2f;
            }

            if (Health <= 0)
            {
                // DEATH!!!
                state = DraculaState.Dying;
                Dead = true;
                Enabled = false;
                this.velocity = Vector2.Zero;

                foreach (var fireBall in FireBalls)
                {
                    fireBall.Kill();
                }
                foreach (var bat in Bats)
                {
                    bat.Kill();
                }

                SoundManager.PlaySound("DracDeath");

                foreach (var bat in DeathBats)
                {
                    bat.Enabled = true;
                    bat.Alive = true;
                    bat.WorldLocation = this.CollisionCenter;
                    var speed = Game1.Randy.Next(150, 400);
                    var randomDirection = Game1.Randy.NextVector();
                    bat.Velocity = randomDirection * speed;
                }

            }
        }

        public override void Kill()
        {
            Enabled = false;
            base.Kill();
        }

        public void Initialize()
        {

            foreach (var item in Game1.CurrentLevel.Items)
            {
                if (item is Sock)
                {
                    Sock = item as Sock;
                    break;
                }
            }

            if (Sock == null)
            {
                throw new Exception("You need a sock in the level!");
            }

            Sock.Enabled = false;

            TimerManager.AddNewTimer(1.5f, () =>
            {
                state = DraculaState.Attacking;
            });

            var offset = 6 * TileMap.TileSize;

            var mapWidth = Game1.CurrentLevel.Map.MapWidth * TileMap.TileSize;

            middleLocation = new Vector2(mapWidth / 2f, WorldLocation.Y);
            leftLocation = new Vector2(middleLocation.X - offset, middleLocation.Y);
            rightLocation = new Vector2(middleLocation.X + offset, middleLocation.Y);

            locations = new[] { leftLocation, middleLocation, rightLocation };

            offScreenLocation = new Vector2(-500, -500);
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (!_isInitialized)
            {
                Initialize();
                _isInitialized = true;
            }

            Game1.DrawBossHealth = true;
            Game1.MaxBossHealth = MaxHealth;
            Game1.BossHealth = Health;
            Game1.BossName = "Dracula";

            if (flashEffectTimer < flashEffectTimerGoal)
            {
                flashEffectTimer += elapsed;
            }

            if (state == DraculaState.Attacking)
            {
                var isOnScreen = worldLocation != offScreenLocation;

                disappearTimer += elapsed;

                if (disappearTimer > 1.6f && flashEffectTimer >= flashEffectTimerGoal)
                {
                    SoundManager.PlaySound("Disappear");
                    flashEffectTimer = 0f;

                    if (isOnScreen)
                    {
                        flashEffectLocation = this.CollisionCenter;
                    }
                    else
                    {
                        // Get a random location
                        var locationIndex = Game1.Randy.Next(0, locations.Length);
                        flashEffectLocation = locations[locationIndex];
                    }
                    
                }

                if (disappearTimer > 2f)
                {
                    disappearTimer = 0f;
                    if (isOnScreen)
                    {
                        worldLocation = offScreenLocation;
                        animations.Play("idle");
                    }
                    else
                    {
                        this.worldLocation = flashEffectLocation;
                        Flipped = _player.WorldLocation.X < this.worldLocation.X;
                        animations.Play("openCape");
                    }
                }

                // You can only hit him or be hit when the flash effect is no longer on screen.
                if (isOnScreen && flashEffectTimer >= flashEffectTimerGoal)
                {
                    CollisionRectangle = _hittableCollisionRectangle;
                }
                else
                {
                    CollisionRectangle = Rectangle.Empty;
                }
            }

            if (state == DraculaState.Dying)
            {
                revealSockTimer += elapsed;
                if (revealSockTimer > 3f)
                {
                    Sock.Enabled = true;
                    Sock.FadeIn();
                    revealSockTimer = 0f;
                    state = DraculaState.Dead;
                }
            }

            int previousAnimationFrame = animations.CurrentAnimation.currentFrameIndex;

            base.Update(gameTime, elapsed);

            if (animations.CurrentAnimationName == "openCape" && previousAnimationFrame == 0 && animations.CurrentAnimation.currentFrameIndex == 1)
            {
                // Shoot fireballs
                SoundManager.PlaySound("Fire", 1f, -0.3f);
                const float yVelocityDecrement = 40f;
                var currentYVelocity = yVelocityDecrement;
                foreach (var fireball in FireBalls)
                {
                    fireball.WorldLocation = CollisionCenter;
                    fireball.Enabled = true;
                    fireball.Alive = true;
                    var flippedMultiplier = Flipped ? -1 : 1;
                    fireball.Velocity = new Vector2(250 * flippedMultiplier, currentYVelocity);
                    currentYVelocity -= yVelocityDecrement;
                }
            }

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            // Draw the flash effect for when Dracula is disappearing.
            if (flashEffectTimer < flashEffectTimerGoal)
            {
                const int fullWidth = 100;
                var currentWidth = (fullWidth * (flashEffectTimer / flashEffectTimerGoal)).ToInt();

                float flashEffectDepth = this.DrawDepth - Game1.MIN_DRAW_INCREMENT * 10;

                Rectangle blueDestination = new Rectangle((flashEffectLocation.X - (currentWidth / 2f)).ToInt(), 0, currentWidth, Game1.CurrentLevel.Map.MapHeightInPixels);

                spriteBatch.Draw(Game1.TileTextures, blueDestination, blueSource, Color.White, 0f, Vector2.Zero, SpriteEffects.None, flashEffectDepth);


                flashEffectDepth -= (Game1.MIN_DRAW_INCREMENT * 10);

                Rectangle whiteDestination = blueDestination;
                whiteDestination.X += 2 * Game1.TileScale;
                whiteDestination.Width -= 4 * Game1.TileScale;


                spriteBatch.Draw(Game1.TileTextures, whiteDestination, whiteSource, Color.White, 0f, Vector2.Zero, SpriteEffects.None, flashEffectDepth);
            }
        }
    }
}