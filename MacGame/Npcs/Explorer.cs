using MacGame.Behaviors;
using MacGame.DisplayComponents;
using MacGame.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using System.Numerics;
using TileEngine;

namespace MacGame.Npcs
{
    public class Explorer : Npc
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;

        public Explorer(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures2");

            var idle = new AnimationStrip(textures, Helpers.GetTileRect(8, 31), 1, "idle");
            idle.LoopAnimation = true;
            idle.FrameLength = 0.2f;
            animations.Add(idle);

            var walk = new AnimationStrip(textures, Helpers.GetTileRect(8, 31), 2, "walk");
            walk.LoopAnimation = true;
            walk.FrameLength = 0.2f;
            animations.Add(walk);

            SetWorldLocationCollisionRectangle(8, 8);

            Behavior = new WalkRandomlyBehavior("idle", "walk");
        }

        public override Rectangle ConversationSourceRectangle => Helpers.GetReallyBigTileRect(8, 15);

        public override void InitiateConversation()
        {
            ISay("Looks like you've found all of the treasure in here!");
        }

        public override void CheckPlayerInteractions(Player player)
        {
            if (ConversationOverrides.Any())
            {
                // This NPC is special. If we found everything in the level, we're going to cancel this 
                // convo override and replace it with a default message that's in InitiateConversation().
                if (HasFoundEverything())
                {
                    ConversationOverrides.Clear();
                }
            }

            base.CheckPlayerInteractions(player);
        }

        /// <summary>
        /// Sorry, this method is insanely hacky!
        /// </summary>
        private bool HasFoundEverything()
        {
            var levelNumber = Game1.CurrentLevel.LevelNumber;

            // Check if they have all of the Socks and Drac parts
            foreach(var item in Game1.CurrentLevel.Items)
            {

                if (item is Sock)
                {
                    if (!((Sock)item).IsCollected)
                    {
                        return false;
                    }
                }

                if (item is DraculaPart)
                {
                    if (item is DraculaHeart && !Game1.StorageState.HasDraculaHeart)
                    {
                        return false;
                    }
                    else if (item is DraculaSkull && !Game1.StorageState.HasDraculaSkull)
                    {
                        return false;
                    }
                    else if (item is DraculaRib && !Game1.StorageState.HasDraculaRib)
                    {
                        return false;
                    }
                    else if (item is DraculaEye && !Game1.StorageState.HasDraculaEye)
                    {
                        return false;
                    }
                    else if (item is DraculaTeeth && !Game1.StorageState.HasDraculaTeeth)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
