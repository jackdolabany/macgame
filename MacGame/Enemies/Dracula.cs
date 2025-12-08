using MacGame.DisplayComponents;
using MacGame.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
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
        float dissappearTimerGoal = 1.6f;

        public enum DraculaState
        {
            Sitting,
            Attacking,
            Dying,
            Dead
        }

        public DraculaState _state = DraculaState.Sitting;

        Rectangle _hittableCollisionRectangle;

        Player _player;

        public List<DraculaFireball> FireBalls;
        public List<DraculaBat> Bats;
        private List<DraculaDeathBat> DeathBats;

        // Used to create the flash effect when Drac disappears.
        private Rectangle whiteSource;
        private Rectangle blueSource;
        private const float flashEffectTimerGrowGoal = 0.7f;
        private const float flashEffectTimerShrinkGoal = 0.35f;
        private float flashEffectTimer = flashEffectTimerGrowGoal;
        private Vector2 flashEffectLocation;

        private WineGlass _wineGlass;

        // Must be behind the player while sitting
        private float _sittingDrawDepth;
        private float _regularDrawDepth;

        private enum FlashEffectState
        {
            None,
            Growing,
            Shrinking
        }

        private FlashEffectState _flashEffectState = FlashEffectState.None;

        public Dracula(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {

            _player = player;

            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\ReallyBigTextures");

            var sitting = new AnimationStrip(textures, Helpers.GetReallyBigTileRect(1, 5), 1, "sitting");
            sitting.LoopAnimation = false;
            sitting.FrameLength = 0.05f;
            animations.Add(sitting);

            var tossWine = new AnimationStrip(textures, Helpers.GetReallyBigTileRect(2, 5), 1, "tossWine");
            tossWine.LoopAnimation = false;
            tossWine.FrameLength = 0.4f;
            animations.Add(tossWine);

            var idle = new AnimationStrip(textures, Helpers.GetReallyBigTileRect(0, 2), 1, "idle");
            idle.LoopAnimation = false;
            idle.FrameLength = 0.05f;
            animations.Add(idle);

            animations.Play("sitting");

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

            // Store the current value since he'll sometimes be not hittable.
            // we can toggle this rectangle empty as needed.
            _hittableCollisionRectangle = collisionRectangle;
            collisionRectangle = Rectangle.Empty;

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

            _wineGlass = new WineGlass(content, 0 , 0);
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
                _state = DraculaState.Dying;
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

                // We'll set this for now, but we can wait until they collect the sock to save it.
                Game1.StorageState.HasBeatenDracula = true;

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

            Sock.CollectOrRevealAction = () =>
            {
                GlobalEvents.FireDoorEntered(this, "World3GhostHouse", "DraculaExit", "FromDracula", Game1.TransitionType.SlowFade);
            };

            // Set the locations he'll teleport to
            var offset = 6 * TileMap.TileSize;

            var mapWidth = Game1.CurrentLevel.Map.MapWidth * TileMap.TileSize;
            middleLocation = new Vector2(mapWidth / 2f, WorldLocation.Y);
            leftLocation = new Vector2(middleLocation.X - offset, middleLocation.Y);
            rightLocation = new Vector2(middleLocation.X + offset, middleLocation.Y);

            locations = new[] { leftLocation, middleLocation, rightLocation };

            // To start, move drac so he's sitting in his chair.
            this.WorldLocation += new Vector2(0, -96);

            // Adjust the wine glass based on his new chair location.
            _sittingDrawDepth = _player.DrawDepth + (20 * Game1.MIN_DRAW_INCREMENT);
            _regularDrawDepth = this.DrawDepth;
            this.SetDrawDepth(_sittingDrawDepth);
            _wineGlass.SetDrawDepth(this.DrawDepth - Game1.MIN_DRAW_INCREMENT);
            _wineGlass.WorldLocation = new Vector2(this.WorldLocation.X - 24, this.WorldLocation.Y - 40);

            // Start the conversation.
            if (Game1.LevelState.HasHeardDraculaConversation)
            {
                _state = DraculaState.Attacking;
                _wineGlass.TossGlass();
            }
            else
            {
                var DraculaConversationSourceRect = Helpers.GetReallyBigTileRect(3, 1);
                TimerManager.AddNewTimer(3f, () =>
                {
                    ConversationManager.AddMessage("Behold! I am Dracula, the dark prince. I am evil made flesh.", DraculaConversationSourceRect, ConversationManager.ImagePosition.Right);
                    ConversationManager.AddMessage("Hi, I'm Mac.", ConversationManager.PlayerSourceRectangle, ConversationManager.ImagePosition.Left, null, () =>
                    {
                        _wineGlass.TossGlass();
                        animations.Play("tossWine").FollowedBy("sitting");
                    });

                    TimerManager.AddNewTimer(1.2f, () =>
                    {
                        ConversationManager.AddMessage("What is a Mac? A miserable little pile of pixels. Have at you!", DraculaConversationSourceRect, ConversationManager.ImagePosition.Right, null, () =>
                        {
                            _state = DraculaState.Attacking;
                            Game1.LevelState.HasHeardDraculaConversation = true;
                        });
                    });
                });
            }
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

            // The point where the flash effect goes from growing to shrinking.
            // This is when Dracula will move.
            bool isFlashEffectMaxed = false;

            if (_flashEffectState != FlashEffectState.None)
            {
                flashEffectTimer += elapsed;

                if (_flashEffectState == FlashEffectState.Growing)
                {
                    if (flashEffectTimer > flashEffectTimerGrowGoal)
                    {
                        _flashEffectState = FlashEffectState.Shrinking;
                        flashEffectTimer = 0;
                        isFlashEffectMaxed = true;
                    }
                }

                if (_flashEffectState == FlashEffectState.Shrinking)
                {
                    if (flashEffectTimer > flashEffectTimerShrinkGoal)
                    {
                        _flashEffectState = FlashEffectState.None;
                        flashEffectTimer = 0;
                    }
                }
            }

            if (_state == DraculaState.Attacking)
            {
                var isOnScreen = worldLocation != offScreenLocation;

                disappearTimer += elapsed;

                if (disappearTimer > dissappearTimerGoal)
                {
                    SoundManager.PlaySound("Disappear");
                    flashEffectTimer = 0f;
                    _flashEffectState = FlashEffectState.Growing;

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
                    disappearTimer = 0f;
                }

                if (isFlashEffectMaxed)
                {
                    disappearTimer = 0f;
                    if (isOnScreen)
                    {
                        worldLocation = offScreenLocation;
                        animations.Play("idle");
                    }
                    else
                    {
                        SetDrawDepth(_regularDrawDepth);
                        this.worldLocation = flashEffectLocation;
                        Flipped = _player.WorldLocation.X < this.worldLocation.X;
                        animations.Play("openCape");
                    }
                }

                // You can only hit him or be hit when the flash effect is no longer on screen.
                if (isOnScreen && _flashEffectState == FlashEffectState.Shrinking && collisionRectangle == Rectangle.Empty)
                {
                    CollisionRectangle = _hittableCollisionRectangle;
                }
                else if (!isOnScreen || _flashEffectState == FlashEffectState.Growing || _state != DraculaState.Attacking)
                {
                    CollisionRectangle = Rectangle.Empty;
                }
            }

            if (_state == DraculaState.Dying)
            {
                revealSockTimer += elapsed;
                if (revealSockTimer > 4f)
                {
                    Sock.Enabled = true;
                    Sock.FadeIn();
                    revealSockTimer = 0f;
                    _state = DraculaState.Dead;
                }
            }

            int previousAnimationFrame = animations.CurrentAnimation.currentFrameIndex;

            base.Update(gameTime, elapsed);

            _wineGlass.Update(gameTime, elapsed);

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

            _wineGlass.Draw(spriteBatch);

            if (_flashEffectState != FlashEffectState.None && flashEffectTimer < flashEffectTimerGrowGoal)
            {
                const int fullWidth = 100;

                int currentWidth;
                if (_flashEffectState == FlashEffectState.Growing)
                {
                    currentWidth = (fullWidth * (flashEffectTimer / flashEffectTimerGrowGoal)).ToInt();
                }
                else
                {
                    currentWidth = fullWidth - (fullWidth * (flashEffectTimer / flashEffectTimerShrinkGoal)).ToInt();
                }

                // Always in front of Drac and Mac. We mess around with the draw depths so just consider both.
                float flashEffectDepth = Math.Min(this.DrawDepth, _player.DrawDepth) - Game1.MIN_DRAW_INCREMENT * 10;

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