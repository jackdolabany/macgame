using MacGame.Behaviors;
using MacGame.DisplayComponents;
using MacGame.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame.Npcs
{
    public class Nerd : Npc
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;
        private Player _player;
        private bool _isFallen = false;
        private Sock _nerdSock;
        private bool _isInitialized = false;
        private Vector2 _startingPosition;
        private bool _movingRight = true;
        private const float MOVE_DISTANCE = 12f;
        private const float MOVE_SPEED = 40f;

        public Nerd(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            _player = player;
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures2");

            // Idle animation with 2 frames at location (4, 34)
            var idle = new AnimationStrip(textures, Helpers.GetTileRect(4, 34), 2, "idle");
            idle.LoopAnimation = true;
            idle.FrameLength = 0.5f;
            animations.Add(idle);

            var fall = new AnimationStrip(textures, Helpers.GetTileRect(6, 34), 1, "fall");
            fall.LoopAnimation = true;
            fall.FrameLength = 0.5f;
            animations.Add(fall);

            SetWorldLocationCollisionRectangle(8, 8);

            _startingPosition = this.WorldLocation;

            animations.Play("idle");
        }

        public override Rectangle ConversationSourceRectangle => Helpers.GetReallyBigTileRect(6, 5);

        public override void InitiateConversation()
        {
            if (!Game1.StorageState.IsNerdHitByMac)
            {
                ConversationManager.AddMessage("Mac help! These guys bully me every day.", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
            }
            else if (!_nerdSock.IsCollected && !_nerdSock.Enabled)
            {
                // Hit but sock not yet shown or collected.
                var revealSock = () =>
                {
                    _nerdSock.FadeIn();
                };
                ConversationManager.AddMessage("Thanks. That really hurt but I know you only threw it so hard to show these bullies how cruel they were acting. Here take this and no more lessons.", ConversationSourceRectangle, ConversationManager.ImagePosition.Right, null, revealSock);
            }
            else
            {
                // Hit and sock collected.
                ConversationManager.AddMessage("That really hurt, but thanks. I guess.", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
            }
                
        }

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (!_isInitialized)
            {
                // Find a sock named "NerdSock" in the level
                foreach (var item in Game1.CurrentLevel.Items)
                {
                    if (item is Sock)
                    {
                        var sock = (Sock)item;
                        if (sock.Name == "NerdSock")
                        {
                            _nerdSock = sock;
                            break;
                        }
                    }
                }

                if (!_nerdSock.IsCollected)
                {
                    _nerdSock.Enabled = false;
                }

                _isInitialized = true;
            }

            base.Update(gameTime, elapsed);

            // Move back and forth while bullies are still throwing fruit
            if (!Game1.StorageState.IsNerdHitByMac)
            {
                // Move left or right
                if (_movingRight)
                {
                    this.WorldLocation = new Vector2(this.WorldLocation.X + MOVE_SPEED * elapsed, this.WorldLocation.Y);

                    // Check if we've moved far enough right
                    if (this.WorldLocation.X >= _startingPosition.X + MOVE_DISTANCE)
                    {
                        _movingRight = false;
                        Flipped = true;
                    }
                }
                else
                {
                    this.WorldLocation = new Vector2(this.WorldLocation.X - MOVE_SPEED * elapsed, this.WorldLocation.Y);

                    // Check if we've moved far enough left
                    if (this.WorldLocation.X <= _startingPosition.X - MOVE_DISTANCE)
                    {
                        _movingRight = true;
                        Flipped = false;
                    }
                }
            }

            // Check collision with player's apples
            if (!Game1.StorageState.IsNerdHitByMac && !_isFallen && _player.Apples != null)
            {
                foreach (var apple in _player.Apples.RawList)
                {
                    if (apple.Enabled && this.CollisionRectangle.Contains(apple.WorldCenter))
                    {
                        // Nerd gets hit and falls
                        _isFallen = true;
                        animations.Play("fall");
                        apple.Smash();
                        Flipped = apple.WorldCenter.X > this.WorldCenter.X;
                        SoundManager.PlaySound("ShootFromCannon");

                        // Save that Mac hit the nerd
                        Game1.StorageState.IsNerdHitByMac = true;
                        StorageManager.TrySaveGame();

                        break;
                    }
                }
            }
        }
    }
}
