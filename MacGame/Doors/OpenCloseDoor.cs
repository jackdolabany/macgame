using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MacGame.Doors
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

        public int SocksNeeded;

        public bool IsLocked { get; set; }

        private DoorState State;

        // Whether we kick the player out of the door or he walks out nicely.
        private bool isYeet = false;

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
            DisplayComponent = aggDisplay;

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
            DoorAnimations.Play("idle");

            // Need to offset the position of the DrawObject because it gets all screwed up since the
            // graphic for the door is twice the size of the door. That's because it opens/closes and the
            // animation graphic spills to the left of the door.
            DoorAnimations.Offset -= new Vector2(16, 0);
            JailBarAnimations.Offset -= new Vector2(16, 0);
        }

        public override void SetDrawDepth(float depth)
        {
            base.SetDrawDepth(depth);
            JailBarAnimations.DrawDepth = depth - Game1.MIN_DRAW_INCREMENT;
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
                return SocksNeeded > 0;
            }
        }

        public virtual bool CanPlayerUnlock(Player player)
        {
            return player.SockCount >= SocksNeeded;
        }

        public virtual string LockMessage()
        {
            return $"You need {SocksNeeded} socks.";
        }

        private void Unlock()
        {
            IsLocked = false;
            _player.AddUnlockedDoor(Name);
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
                Unlock();
            }

            if (State == DoorState.Idle)
            {
                DoorAnimations.Play("idle");
            }
            else if (State == DoorState.EnterOpening)
            {
                if (DoorAnimations.animations[DoorAnimations.CurrentAnimationName].FinishedPlaying)
                {
                    pauseBeforeTransitionTimer = 0.5f;
                    State = DoorState.EnterClosing;
                    DoorAnimations.Play("close");
                }
            }
            else if (State == DoorState.EnterClosing)
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
                        GlobalEvents.FireDoorEntered(this, GoToMap, GoToDoorName, Name);
                        State = DoorState.Idle;
                    }
                }
            }
            else if (State == DoorState.ExitOpening)
            {

                // Weird hack but we want to show the player early if they walk out the door, late if they
                // are tossed out. It looks better, trust me.
                if (!isYeet)
                {
                    _player.IsInvisible = DoorAnimations.CurrentAnimation.currentFrameIndex < 1;
                }

                if (DoorAnimations.animations[DoorAnimations.CurrentAnimationName].FinishedPlaying)
                {
                    _player.IsInvisible = false;
                    State = DoorState.ExitClosing;
                    DoorAnimations.Play("close");
                    if (isYeet)
                    {
                        _player.SlideOutOfDoor(WorldLocation);
                        SoundManager.PlaySound("KickedOutOfDoor");
                    }
                    isYeet = false;
                }
            }
            else if (State == DoorState.ExitClosing)
            {
                if (DoorAnimations.animations[DoorAnimations.CurrentAnimationName].FinishedPlaying)
                {
                    State = DoorState.Idle;
                    SoundManager.PlaySound("DoorShut");
                }
            }

            base.Update(gameTime, elapsed);

        }

        /// <summary>
        /// Call this to open the door then close it on Mac and transition to the next level.
        /// </summary>
        public void OpenThenCloseThenTransition()
        {
            DoorAnimations.Play("open");
            SoundManager.PlaySound("DoorOpen");
            State = DoorState.EnterOpening;
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
                // locked but the player now has enough socks to open it.
                if (JailBarAnimations.CurrentAnimationName != "open")
                {
                    JailBarAnimations.Play("open");
                    SoundManager.PlaySound("OpenLockedDoor", 0.5f);
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
            State = DoorState.ExitOpening;
            _player.IsInvisible = true;
            _player.PositionForSlideOutOfDoor(WorldLocation);
        }

        public override void ComeOutOfThisDoor(Player player, bool isYeet = false)
        {
            base.ComeOutOfThisDoor(player);

            DoorAnimations.Play("open");
            SoundManager.PlaySound("DoorOpen");
            State = DoorState.ExitOpening;
            player.IsInvisible = true;

            // Door unlocks as you go through it.
            Unlock();
            this.isYeet = isYeet;
            if (isYeet)
            {
                _player.PositionForSlideOutOfDoor(WorldLocation);
            }
        }
    }
}
