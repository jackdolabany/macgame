using MacGame.Behaviors;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using TileEngine;

namespace MacGame.Npcs
{
    public class Daisy : Npc
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;
        private Player _player;
        List<InputAction> SalamiShuffleMoves;

        public Daisy(ContentManager content, int cellX, int cellY, Player player, Camera camera) 
            : base(content, cellX, cellY, player, camera)
        {

            _player = player;

            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures2");
            var idle = new AnimationStrip(textures, Helpers.GetTileRect(0, 11), 1, "idle");
            idle.LoopAnimation = true;
            idle.FrameLength = 0.2f;
            animations.Add(idle);

            var walk = new AnimationStrip(textures, Helpers.GetTileRect(0, 11), 2, "walk");
            walk.LoopAnimation = true;
            walk.FrameLength = 0.2f;
            animations.Add(walk);

            SetCenteredCollisionRectangle(8, 8);
           
            Behavior = new WalkRandomlyBehavior("idle", "walk");

            SalamiShuffleMoves = new List<InputAction>();

            // Set up the Salami shuffle moves
            var up1 = new InputAction();
            up1.up = true;
            SalamiShuffleMoves.Add(up1);
            var up2 = new InputAction();
            up2.up = true;
            SalamiShuffleMoves.Add(up2);
            var down1 = new InputAction();
            down1.down = true;
            SalamiShuffleMoves.Add(down1);
            var down2 = new InputAction();
            down2.down = true;
            SalamiShuffleMoves.Add(down2);
            var left1 = new InputAction();
            left1.left = true;
            SalamiShuffleMoves.Add(left1);
            var right1 = new InputAction();
            right1.right = true;
            SalamiShuffleMoves.Add(right1);
            var left2 = new InputAction();
            left2.left = true;
            SalamiShuffleMoves.Add(left2);
            var right2 = new InputAction();
            right2.right = true;
            SalamiShuffleMoves.Add(right2);
            var jump = new InputAction();
            jump.jump = true;
            SalamiShuffleMoves.Add(jump);
        }

        public override Rectangle ConversationSourceRectangle => Helpers.GetReallyBigTileRect(7, 1);

        public override void Update(GameTime gameTime, float elapsed)
        {

            if (!Game1.StorageState.HasDancedForDaisy)
            {
                // Check if the previous moves equal the Salami Shuffle: up, up, down, down, left, right, left, right, jump.

                var priorMoves = _player.InputManager.PreviousUniqueActions;

                var priorMovesArray = priorMoves.ToArray();

                for (int i = 0; i < priorMovesArray.Length - SalamiShuffleMoves.Count + 1; i++)
                {
                    var allMatched = true;
                    for (int j = 0; j < SalamiShuffleMoves.Count; j++)
                    {
                        var priorMove = priorMovesArray[i + j];
                        var salamiMove = SalamiShuffleMoves[j];
                        if (!priorMove.Equals(salamiMove))
                        {
                            allMatched = false;
                            break;
                        }
                    }
                    if (allMatched)
                    {
                        // You did the dance!
                        SoundManager.PlaySound("Reveal");
                        Game1.StorageState.HasDancedForDaisy = true;
                        StorageManager.TrySaveGame();
                    }
                }
            }

            base.Update(gameTime, elapsed);
        }

        public override void InitiateConversation()
        {
            if (Game1.CurrentLevel.Name.EqualsIgnoreCase("World1DaisyHouse"))
            {
                if (!Game1.StorageState.HasDancedForDaisy)
                {
                    ConversationManager.AddMessage("I absolutely hate Molly.", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
                }
                else
                {
                    ConversationManager.AddMessage("What was that dance? Did Molly teach you that? It looks like some stupid thing she'd come up with.", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
                }
            }
            else
            {
                ConversationManager.AddMessage("Hissss.", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
            }
        }
    }
}
