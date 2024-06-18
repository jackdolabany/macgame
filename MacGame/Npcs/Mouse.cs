using MacGame.Behaviors;
using MacGame.DisplayComponents;
using MacGame.Items;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TileEngine;
using static MacGame.ConversationManager;
using static MacGame.Game1;

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

                    SoundManager.PlaySound("CoinCollected", 0.4f);

                    var tacoCoin = CurrentLevel.TacoCoin;

                    tacoCoin.Enabled = true;

                    Game1.IsTacoCoinRevealed = true;

                    ConversationManager.AddMessage("Wow! Those tacos were absolutely delicious! Take this", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);

                }
                else
                {
                    ConversationManager.AddMessage("Are you kidding me? My hunger is insatiable! I could eat 100 tacos! If you have less than 100 tacos don't even talk to me.", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
                }
            }));
            choices.Add(new ConversationChoice("sounds rough", () => {
                ConversationManager.AddMessage("I could live without tacos, but what's the point?", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
            }));
        }

        public override Rectangle ConversationSourceRectangle => Helpers.GetReallyBigTileRect(3, 0);

        public override void Update(GameTime gameTime, float elapsed)
        {

            base.Update(gameTime, elapsed);
        }

        public override void InitiateConversation()
        {
            if (Game1.IsTacoCoinRevealed || CurrentLevel.TacoCoin.AlreadyCollected)
            {

                var rando = Game1.Randy.Next(0, 4);

                switch (rando)
                {
                    case 0:
                        ConversationManager.AddMessage("Thanks for the tacos hombre", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
                        break;
                    case 1:
                        ConversationManager.AddMessage("I live for taco Tuesday. I'd die for taco tuesday", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
                        break;
                    case 2:
                        ConversationManager.AddMessage("People like to say salsa", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
                        break;
                    case 3:
                        ConversationManager.AddMessage("I used to love tacos. I still do, but I used to love them too.", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
                        break;
                    default: 
                        ConversationManager.AddMessage("So long and thanks for all the tacos", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
                        break;
                }
            }
            else
            {
                ConversationManager.AddMessage("I'm sick of cheese. I need tacos!", ConversationSourceRectangle, ConversationManager.ImagePosition.Right);
                if (Game1.Player.Tacos > 0)
                {
                    ConversationManager.AddMessage("", PlayerConversationRectangle, ConversationManager.ImagePosition.Left, choices);
                }
            }
        }
    }
}
