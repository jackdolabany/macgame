﻿using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MacGame
{

    /// <summary>
    ///  A door that opens and closes. As opposed to a doorway that is always open.
    /// </summary>
    public class OpenCloseDoor : Door
    {
        /// <summary>
        /// If the door is coming from a hub world and going to a sub world display the hints from that sub world.
        /// </summary>
        public bool IsToSubworld = false;

        public int CoinsNeeded;

        public bool IsLocked { get; set; }

        private DoorState State;

        public enum DoorState
        {
            /// <summary>
            /// Door is shut waiting to be opened.
            /// </summary>
            Idle, 

            /// <summary>
            /// Mac is entering the door and it's opening.
            /// </summary>
            EnterOpening, 

            /// <summary>
            /// Mac is entering the door and it's closing.
            /// </summary>
            EnterClosing, 

            /// <summary>
            /// The door is opening to kick Mac out.
            /// </summary>
            ExitOpening, 

            /// <summary>
            /// Mac has been kicked out and the door is closing.
            /// </summary>
            ExitClosing
        }

        /// <summary>
        /// Set this to something to pause for a bit before transitioning to the next level after the door closes.
        /// </summary>
        float pauseBeforeTransitionTimer = 0;

        private AnimationDisplay DoorAnimations;
        private AnimationDisplay JailBarAnimations;

        public OpenCloseDoor(ContentManager content, int cellX, int cellY, Player player, Camera camera) 
            : base(content, cellX, cellY, player, camera)
        {
            DoorAnimations = new AnimationDisplay();
            JailBarAnimations = new AnimationDisplay();
            var aggDisplay = new AggregateDisplay(new DisplayComponent[] { DoorAnimations, JailBarAnimations });
            this.DisplayComponent = aggDisplay;

            var textures = content.Load<Texture2D>(@"Textures\BigTextures");
            
            var idle = new AnimationStrip(textures, DoorImageTextureSourceRectangle, 1, "idle");
            idle.LoopAnimation = false;
            idle.FrameLength = 0.15f;
            DoorAnimations.Add(idle);

            var open = new AnimationStrip(textures, DoorImageTextureSourceRectangle, 3, "open");
            open.LoopAnimation = false;
            open.FrameLength = 0.15f;
            DoorAnimations.Add(open);

            var close = new AnimationStrip(textures, DoorImageTextureSourceRectangle, 3, "close");
            close.LoopAnimation = false;
            close.FrameLength = 0.15f;
            close.Reverse = true;
            DoorAnimations.Add(close);

            var closedJailBars = new AnimationStrip(textures, Helpers.GetBigTileRect(0, 6), 1, "closed");
            closedJailBars.LoopAnimation = false;
            JailBarAnimations.Add(closedJailBars);

            var openJailBars = new AnimationStrip(textures, Helpers.GetBigTileRect(0, 6), 3, "open");
            openJailBars.LoopAnimation = false;
            openJailBars.FrameLength = 0.15f;
            JailBarAnimations.Add(openJailBars);

            State = DoorState.Idle;
            DoorAnimations.Play("Idle");
        }

        public override void SetDrawDepth(float depth)
        {
            base.SetDrawDepth(depth);
            this.JailBarAnimations.DrawDepth = depth - Game1.MIN_DRAW_INCREMENT;
        }

        /// <summary>
        /// Override this for inherited classes that have doors that look different. Like red/blue/green doors.
        /// </summary>
        public virtual Rectangle DoorImageTextureSourceRectangle
        {
            get
            {
                return Helpers.GetBigTileRect(0, 2);
            }
        }

        public virtual bool IsInitiallyLocked
        {
            get
            {
                return CoinsNeeded > 0;
            }
        }

        public virtual bool CanPlayerUnlock(Player player)
        {
            return player.CricketCoinCount >= CoinsNeeded;
        }

        public virtual string LockMessage()
        {
            return $"You need {CoinsNeeded} coins to unlock this door.";
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (IsLocked && JailBarAnimations.CurrentAnimation == null)
            {                 
                JailBarAnimations.Play("closed");
            }
            
            // Hide the jail bars if the door is unlocked.
            if (!IsLocked)
            {
                JailBarAnimations.TintColor = Color.White * 0;
            }

            if (JailBarAnimations.CurrentAnimationName == "open" && JailBarAnimations.CurrentAnimation!.FinishedPlaying)
            {
                IsLocked = false;
                _player.AddUnlockedDoor(Name);
            }

            if (this.State == DoorState.Idle)
            {
                DoorAnimations.Play("idle");
            }
            else if (this.State == DoorState.EnterOpening)
            {
                if (DoorAnimations.animations[DoorAnimations.CurrentAnimationName].FinishedPlaying)
                {
                    pauseBeforeTransitionTimer = 0.5f;
                    this.State = DoorState.EnterClosing;
                    DoorAnimations.Play("close");
                }
            }
            else if (this.State == DoorState.EnterClosing)
            {
                if (DoorAnimations.CurrentAnimation!.currentFrameIndex == 2 || DoorAnimations.CurrentAnimation!.FinishedPlaying)
                {
                    // hide Mac because he just got shut in the door.
                    if (!_player.IsInvisible)
                    {
                        SoundManager.PlaySound("DoorShut");
                        _player.IsInvisible = true;
                    }

                    if (pauseBeforeTransitionTimer > 0)
                    {
                        pauseBeforeTransitionTimer -= elapsed;
                    }
                    if (pauseBeforeTransitionTimer <= 0)
                    {
                        GlobalEvents.FireDoorEntered(this, this.GoToMap, this.GoToDoorName, this.Name);
                        this.State = DoorState.Idle;
                    }
                }
            }
            else if (this.State == DoorState.ExitOpening)
            {
                _player.IsInvisible = true;
                if (DoorAnimations.animations[DoorAnimations.CurrentAnimationName].FinishedPlaying)
                {
                    _player.IsInvisible = false;
                    _player.SlideOutOfDoor(this.WorldLocation);
                    this.State = DoorState.ExitClosing;
                    DoorAnimations.Play("close");
                }
            }
            else if (this.State == DoorState.ExitClosing)
            {
                if (DoorAnimations.animations[DoorAnimations.CurrentAnimationName].FinishedPlaying)
                {
                    this.State = DoorState.Idle;
                    SoundManager.PlaySound("DoorShut");
                }
            }

            base.Update(gameTime, elapsed);

            // Need to offset the position of the DrawObject because it gets all screwed up since the
            // graphic for the door is twice the size of the door. That's because it opens/closes and the
            // animation graphic spills to the left of the door.
            DoorAnimations.WorldLocation -= new Vector2(16, 0);
            JailBarAnimations.WorldLocation -= new Vector2(16, 0);
        }

        /// <summary>
        /// Call this to open the door then close it on Mac and transition to the next level.
        /// </summary>
        public void OpenThenCloseThenTransition()
        {
            DoorAnimations.Play("open");
            SoundManager.PlaySound("DoorOpen");
            this.State = DoorState.EnterOpening;
            GlobalEvents.FireBeginDoorEnter(this, EventArgs.Empty);
        }

        public override void PlayerTriedToOpen(Player player)
        {
            if (!CanPlayerUnlock(player))
            {
                ConversationManager.AddMessage(LockMessage());
            }
            else if (IsLocked)
            {
                // locked but the player now has enough coins to open it.
                if (JailBarAnimations.CurrentAnimationName != "open")
                {
                    JailBarAnimations.Play("open");
                    SoundManager.PlaySound("OpenLockedDoor");
                }
            }
            else
            {
                OpenThenCloseThenTransition();
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public override void PlayerSlidingOut()
        {
            DoorAnimations.Play("open");
            SoundManager.PlaySound("DoorOpen");
            this.State = DoorState.ExitOpening;
            _player.IsInvisible = true;
            _player.PositionForSlideOutOfDoor(this.WorldLocation);
        }
    }
}
