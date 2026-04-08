using System;
using MacGame.Behaviors;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace MacGame.Npcs
{
    public class BabyAlien : Npc, IPickupObject
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        public bool IsPickedUp { get; private set; }
        private readonly Player _player;

        // No talky to baby.
        public override bool CanInteract => false;

        public enum BabyAlienState
        {
            WalkingAround,
            PickedUp,
            Tossed
        }

        private BabyAlienState _state = BabyAlienState.WalkingAround;

        private JustIdle _idleBehavior;
        private WalkRandomlyBehavior _walkRandomlyBehavior;

        public BabyAlien(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            _player = player;

            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures2");

            var idle = new AnimationStrip(textures, Helpers.GetTileRect(10, 11), 2, "idle");
            idle.LoopAnimation = true;
            idle.FrameLength = 0.35f;
            animations.Add(idle);

            var walk = new AnimationStrip(textures, Helpers.GetTileRect(12, 11), 2, "walk");
            walk.LoopAnimation = true;
            walk.FrameLength = 0.35f;
            animations.Add(walk);

            SetWorldLocationCollisionRectangle(8, 6);

            _walkRandomlyBehavior = new WalkRandomlyBehavior("idle", "walk");
            _idleBehavior = new JustIdle("idle");
            Behavior = _walkRandomlyBehavior;
        }

        public override Rectangle ConversationSourceRectangle => Helpers.GetReallyBigTileRect(5, 4);

        public override void InitiateConversation() { }

        public bool CanBePickedUp => Enabled && !IsPickedUp;

        public void Pickup()
        {
            IsPickedUp = true;
            isTileColliding = false;
            IsAffectedByGravity = false;
            SoundManager.PlaySound("Pickup");
        }

        public void Drop()
        {
            IsPickedUp = false;
            isTileColliding = true;
            IsAffectedByGravity = true;
            Velocity = _player.Velocity;
            Velocity += new Vector2(50 * (_player.IsFacingRight() ? 1 : -1), 0);
            MoveToIgnoreCollisions();
        }

        public void MoveToPlayer()
        {
            WorldLocation = _player.WorldLocation + new Vector2(16 * (_player.Flipped ? -1 : 1), -8);
        }

        public void Kick(bool isStraightUp)
        {
            if (isStraightUp)
            {
                Velocity = _player.Velocity + new Vector2(0, -600);
            }
            else
            {
                Velocity = _player.Velocity + new Vector2(200 * (_player.IsFacingRight() ? 1 : -1), -200);
            }
            EffectsManager.SmallEnemyPop(WorldCenter);
            SoundManager.PlaySound("Kick");
        }

        public void BreakAndReset()
        {
            // Do nothing, no break and reset.
        }

        public override void Update(GameTime gameTime, float elapsed)
        {
            if (_state == BabyAlienState.WalkingAround)
            {
                if (IsPickedUp)
                {
                    _state = BabyAlienState.PickedUp;
                    Behavior = _idleBehavior;
                }
            }
            else if (_state == BabyAlienState.PickedUp)
            {
                // Track the player while held
                Velocity = Vector2.Zero;
                WorldLocation = _player.WorldLocation + new Vector2(16 * (_player.Flipped ? -1 : 1), -8);

                if (!IsPickedUp)
                {
                    _state = BabyAlienState.Tossed;
                }
            }
            else if (_state == BabyAlienState.Tossed)
            {
                // Apply friction so the alien doesn't slide forever
                if (OnGround && Velocity.X != 0)
                {
                    velocity.X -= velocity.X * 3.5f * elapsed;
                    if (Math.Abs(velocity.X) < 15f)
                    {
                        velocity.X = 0;
                    }
                }

                if (IsPickedUp)
                {
                    // weird case but you can go from tossed to picked up if you
                    // pick the baby up while he's still sliding.
                    _state = BabyAlienState.PickedUp;
                    Behavior = _idleBehavior;
                }
                else if (OnGround && Math.Abs(Velocity.X) < 5f)
                {
                    // Revert back to walking after sliding for a bit.
                    Velocity = Vector2.Zero;
                    _state = BabyAlienState.WalkingAround;
                    Behavior = _walkRandomlyBehavior;
                    _walkRandomlyBehavior.Reset();
                }
            }

            base.Update(gameTime, elapsed);
        }
    }
}
