using MacGame.Behaviors;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using TileEngine;

namespace MacGame.Npcs
{
    /// <summary>
    /// An npc who can talk to the player.
    /// </summary>
    public class Ottie : Npc
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        Vector2 OriginalPosition;

        public float actionTimer = 0.0f;
        public float actionTimeLimit = 3.0f;

        private MoveToLocation _moveToLocation;

        public enum OttieState
        {
            Stationary,
            WalkAroundAimlessly,
            GoToLocation
        }

        private OttieState _state = OttieState.WalkAroundAimlessly;

        public Ottie(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            WorldLocation = new Vector2(cellX * TileMap.TileSize + TileMap.TileSize / 2, (cellY + 1) * TileMap.TileSize);
            OriginalPosition = WorldLocation;

            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\BigTextures");
            var idle = new AnimationStrip(textures, Helpers.GetBigTileRect(0, 7), 2, "idle");
            idle.LoopAnimation = true;
            idle.FrameLength = 0.5f;
            animations.Add(idle);

            var walk = new AnimationStrip(textures, Helpers.GetBigTileRect(2, 7), 2, "walk");
            walk.LoopAnimation = true;
            walk.FrameLength = 0.2f;
            animations.Add(walk);

            var bark = new AnimationStrip(textures, Helpers.GetBigTileRect(4, 7), 1, "bark");
            bark.LoopAnimation = false;
            bark.FrameLength = 0.2f;
            animations.Add(bark);

            var look = new AnimationStrip(textures, Helpers.GetBigTileRect(5, 7), 1, "look");
            look.LoopAnimation = false;
            look.FrameLength = 0.8f;
            animations.Add(look);

            Enabled = true;

            SetWorldLocationCollisionRectangle(6, 14);
            IsAffectedByGravity = false;
            IsAbleToSurviveOutsideOfWorld = true;

            animations.Play("idle");

            _moveToLocation = new MoveToLocation(this, 100, "idle", "walk", "walk", "walk");

        }

        private bool _didInitiateIntroConveration = false;

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (_state == OttieState.GoToLocation)
            {
                _moveToLocation.Update(this, gameTime, elapsed);

                var ottisIsAtLocation = _moveToLocation.IsAtFinalLocation;

                var macIsAtLocation = Game1.Player.IsAtLocation();

                if (!_didInitiateIntroConveration
                    && ottisIsAtLocation
                    && this.animations.CurrentAnimationName == "idle"
                    && macIsAtLocation)
                {
                    _didInitiateIntroConveration = true;

                    // Ottis
                    ConversationManager.AddMessage("Hi! I'm Ottis, I'm a good boy.", this.ConversationSourceRectangle, ConversationManager.ImagePosition.Right);

                    // Mac
                    ConversationManager.AddMessage("I'm Mac and I could use a good person to help me get back home.", Helpers.GetReallyBigTileRect(0, 0), ConversationManager.ImagePosition.Left);
                    ConversationManager.AddMessage("I thought this was going to be a straight forward game, like jump on a flagpole. Maybe save a princess.", Helpers.GetReallyBigTileRect(0, 0), ConversationManager.ImagePosition.Left);

                    var showStar = () =>
                    {
                        CutsceneManager.CollectiblePosition = WorldLocation + new Vector2(-58, -58);
                        CutsceneManager.CurrentCutscene = CutsceneManager.CutsceneType.Intro;
                        
                        TimerManager.AddNewTimer(1f, () => CutsceneManager.ShowStar());

                        TimerManager.AddNewTimer(2.5f, () =>
                        {
                            // Ottis
                            ConversationManager.AddMessage("I can get you back home, just find me the lost Magic Stars.", this.ConversationSourceRectangle, ConversationManager.ImagePosition.Right);

                            // Mac
                            ConversationManager.AddMessage("Hey that's just like a game I know!", Helpers.GetReallyBigTileRect(0, 0), ConversationManager.ImagePosition.Left);

                            var showMoon = () => {

                                CutsceneManager.HideCollectable();
                                TimerManager.AddNewTimer(1f, () => CutsceneManager.ShowMoon());

                                TimerManager.AddNewTimer(2.5f, () =>
                                {

                                    ConversationManager.AddMessage("Instead, find me these magic moons. Hidden throughout the land by mysterious...", this.ConversationSourceRectangle, ConversationManager.ImagePosition.Right);

                                    var showSock = () =>
                                    {
                                        CutsceneManager.HideCollectable();
                                        TimerManager.AddNewTimer(1f, () => CutsceneManager.ShowSock());

                                        TimerManager.AddNewTimer(2.5f, () =>
                                        {
                                            // Ottis
                                            ConversationManager.AddMessage("Tell you what, I'm a dog and I like stinky socks. Find me the magic stinky gym socks.", this.ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
                                           
                                            var finish = () =>
                                            {
                                                CutsceneManager.CurrentCutscene = CutsceneManager.CutsceneType.None;
                                                CutsceneManager.HideCollectable();
                                                GlobalEvents.FireIntroComplete();
                                            };

                                            // Mac
                                            ConversationManager.AddMessage("I'll do it!", Helpers.GetReallyBigTileRect(0, 0), ConversationManager.ImagePosition.Left, completeAction: finish);
                                        });
                                    };

                                    // Mac
                                    ConversationManager.AddMessage("That sounds familiar too.", Helpers.GetReallyBigTileRect(0, 0), ConversationManager.ImagePosition.Left, completeAction: showSock);
                                });

                            };

                            // Ottis
                            ConversationManager.AddMessage("Oh snap, don't want to get sued.", this.ConversationSourceRectangle, ConversationManager.ImagePosition.Right, completeAction: showMoon);
                        });

                    };

                    // Ottis
                    ConversationManager.AddMessage("No, it can't be like that. We'll get sued.", this.ConversationSourceRectangle, ConversationManager.ImagePosition.Right, completeAction: showStar);

                }
            }
            else if (_state == OttieState.WalkAroundAimlessly)
            {

                // Randomly walk left and right. Randomly bark. Randomly go idle.
                actionTimer += elapsed;
                if (actionTimer >= actionTimeLimit)
                {
                    actionTimer = 0.0f;
                    velocity.X = 0;

                    int action = Game1.Randy.Next(0, 4);
                    if (action == 0 || animations.CurrentAnimationName == "walk")
                    {
                        animations.Play("idle");
                    }
                    else if (action == 1)
                    {
                        animations.Play("walk");

                        velocity.X = 20;
                        Flipped = false;
                        if (WorldLocation.X > OriginalPosition.X)
                        {
                            velocity.X *= -1;
                            Flipped = true;
                        }
                    }
                    else if (action == 2)
                    {
                        animations.Play("bark").FollowedBy("idle");
                        SoundManager.PlaySound("Bark");

                    }
                    else if (action == 3)
                    {
                        animations.Play("look").FollowedBy("idle");
                    }
                }
            }
            base.Update(gameTime, elapsed);
        }

        public override Rectangle ConversationSourceRectangle => Helpers.GetReallyBigTileRect(1, 0);

        public override void InitiateConversation()
        {
            var message = Game1.Randy.Next(1, 5);
            switch(message)
            {
                case 1:
                    ConversationManager.AddMessage("My name is Ottis and I am a good boy.", this.ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
                    break;
                case 2:
                    ConversationManager.AddMessage("Fetch me socks and I'll unlock doors.", this.ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
                    break;
                case 3:
                    ConversationManager.AddMessage("Fetch me socks and I'll bring you home.", this.ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
                    break;
                case 4:
                    ConversationManager.AddMessage("Tell me I'm a good boy.", this.ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
                    break;
                default:
                    Game1.ThrowDebugException("Invalid message index");
                    break;
            }
        }

        public void GoToLocation(Vector2 location)
        {
            _state = OttieState.GoToLocation;
            _moveToLocation.SetTargetLocation(location);
        }

        public void BeStationary()
        {
            _state = OttieState.Stationary;
        }
    }
}
