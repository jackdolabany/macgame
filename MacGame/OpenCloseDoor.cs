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
            Idle, Opening, Closing
        }

        /// <summary>
        /// If you selected a hint when entering the door, store it here.
        /// </summary>
        private int? hintIndex;

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
            return $"You need {CoinsNeeded} coins to unlock this door. This is some bonus message. How is that character thing coming along?";
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
            else if (this.State == DoorState.Opening)
            {
                if (DoorAnimations.animations[DoorAnimations.CurrentAnimationName].FinishedPlaying)
                {
                    pauseBeforeTransitionTimer = 0.5f;
                    this.State = DoorState.Closing;
                    DoorAnimations.Play("close");
                }
            }
            else if (this.State == DoorState.Closing)
            {
                if (DoorAnimations.CurrentAnimation!.currentFrameIndex == 2 || DoorAnimations.CurrentAnimation!.FinishedPlaying)
                {
                    // hide Mac because he just got shut in the door.
                    _player.IsInvisible = true;
                    if (pauseBeforeTransitionTimer > 0)
                    {
                        pauseBeforeTransitionTimer -= elapsed;
                    }
                    if (pauseBeforeTransitionTimer <= 0)
                    {
                        GlobalEvents.FireDoorEntered(this, this.GoToMap, this.GoToDoorName, this.Name, hintIndex);
                        this.State = DoorState.Idle;
                    }
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
        public void OpenThenCloseThenTransition(int? hintIndex = null)
        {
            DoorAnimations.Play("open");
            this.State = DoorState.Opening;
            this.hintIndex = hintIndex;
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
                }
                // TODO: Play a sound effect here.
            }
            else if (IsToSubworld)
            {
                GlobalEvents.FireSubWorldDoorEntered(this, this.Name, this.GoToMap);
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
    }
}
