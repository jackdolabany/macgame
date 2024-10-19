using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
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

            SetCenteredCollisionRectangle(6, 14);
            IsAffectedByGravity = false;
            IsAbleToSurviveOutsideOfWorld = true;

            animations.Play("idle");
        }

        public override void Update(GameTime gameTime, float elapsed)
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

            base.Update(gameTime, elapsed);
        }

        public override Rectangle ConversationSourceRectangle => Helpers.GetReallyBigTileRect(1, 0);

        public override void InitiateConversation()
        {
            ConversationManager.AddMessage("Hi I'm Mac!", Helpers.GetReallyBigTileRect(0, 0), ConversationManager.ImagePosition.Left);
            ConversationManager.AddMessage("My name is Ottis. I am a good boy. Go in this door and see if you can fetch all of the socks for me.", this.ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
        }
    }
}
