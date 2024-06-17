using MacGame.Behaviors;
using MacGame.DisplayComponents;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using System.Collections.Generic;
using TileEngine;
using static MacGame.ConversationManager;

namespace MacGame.Npcs
{
    public class Mouse : Npc
    {
        AnimationDisplay animations => (AnimationDisplay)DisplayComponent;
        List<ConversationChoice> choices;

        public bool GiveCoin { get; private set; }

        public Mouse(ContentManager content, int cellX, int cellY, Player player, Camera camera)
            : base(content, cellX, cellY, player, camera)
        {
            DisplayComponent = new AnimationDisplay();

            var textures = content.Load<Texture2D>(@"Textures\Textures");
            var idle = new AnimationStrip(textures, Helpers.GetTileRect(4, 14), 2, "idle");
            idle.LoopAnimation = true;
            idle.FrameLength = 0.5f;
            animations.Add(idle);

            SetCenteredCollisionRectangle(8, 8);

            animations.Play("idle");

            choices = new List<ConversationChoice>();
            choices.Add(new ConversationChoice("take my tacos", () => {
                if (Game1.Player.Tacos >= 100)
                {
                    Game1.Player.Tacos = 0;
                    GiveCoin = true;
                    ConversationManager.AddMessage("Wow! Those tacos were absolutely delicious! Take this", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
                }
                else
                {
                    ConversationManager.AddMessage("Are you kidding me? My hunger is insatiable! I could eat 100 tacos! If you have less than 100 tacos don't even talk to me.", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
                }
            }));
            choices.Add(new ConversationChoice("sounds rough", () => {
                ConversationManager.AddMessage("I could live without tacos, but is that really living?", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
            }));
        }

        public override Rectangle ConversationSourceRectangle => Helpers.GetReallyBigTileRect(3, 0);

        public override void Update(GameTime gameTime, float elapsed)
        {

            base.Update(gameTime, elapsed);
        }

        public override void InitiateConversation()
        {
            ConversationManager.AddMessage("I'm sick of cheese. I need tacos!", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
            if (Game1.Player.Tacos > 0)
            {
                ConversationManager.AddMessage("", PlayerConversationRectangle, ConversationManager.ImagePosition.Left, choices);
            }
        }
    }
}
