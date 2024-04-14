using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Security.Cryptography.X509Certificates;

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

        public bool IsLocked
        {
            get
            {
                return _player.CricketCoinCount < CoinsNeeded;
            }
        }

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

        private AnimationDisplay Animations => (AnimationDisplay)DisplayComponent;

        public OpenCloseDoor(ContentManager content, int cellX, int cellY, Player player, Camera camera) 
            : base(content, cellX, cellY, player, camera)
        {
            var animations = new AnimationDisplay();
            this.DisplayComponent = animations;

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var idle = new AnimationStrip(textures, new Rectangle(4 * Game1.TileSize, 10 * Game1.TileSize, 16, 16), 1, "idle");
            idle.LoopAnimation = false;
            idle.FrameLength = 0.15f;
            animations.Add(idle);
            
            var open = new AnimationStrip(textures, new Rectangle(4 * Game1.TileSize, 10 * Game1.TileSize, 16, 16), 3, "open");
            open.LoopAnimation = false;
            open.FrameLength = 0.15f;
            animations.Add(open);

            var close = new AnimationStrip(textures, new Rectangle(4 * Game1.TileSize, 10 * Game1.TileSize, 16, 16), 1, "close");
            close.LoopAnimation = false;
            close.FrameLength = 0.15f;
            close.Reverse = true;
            animations.Add(close);

        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (this.State == DoorState.Idle)
            {
                Animations.Play("idle");
            }
            else if (this.State == DoorState.Opening)
            {
                if (Animations.animations[Animations.currentAnimationName].FinishedPlaying)
                {
                    pauseBeforeTransitionTimer = 1f;
                    this.State = DoorState.Closing;
                    Animations.Play("close");
                }
            }
            else if (this.State == DoorState.Closing)
            {
                if (Animations.animations[Animations.currentAnimationName].FinishedPlaying)
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
            Animations.WorldLocation -= new Vector2(4, 0);
        }

        /// <summary>
        /// Call this to open the door then close it on Mac and transition to the next level.
        /// </summary>
        public void OpenThenCloseThenTransition(int? hintIndex = null)
        {
            Animations.Play("open");
            this.State = DoorState.Opening;
            this.hintIndex = hintIndex;
            GlobalEvents.FireBeginDoorEnter(this, EventArgs.Empty);
        }

        public override void PlayerTriedToOpen(Player player)
        {
            if (CoinsNeeded > player.CricketCoinCount)
            {
                ConversationManager.AddMessage($"You need {CoinsNeeded} coins to unlock this door.");
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
    }
}
