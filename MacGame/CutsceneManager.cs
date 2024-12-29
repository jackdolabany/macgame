using System;
using MacGame.DisplayComponents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MacGame
{
    /// <summary>
    /// A class to help manage any "cutscenes" where the player has no control over the character
    /// or NPCs do custom things.
    /// </summary>
    public static class CutsceneManager
    {

        private static AnimationDisplay _collectible;

        public enum CutsceneType
        {
            None,
            Intro
        }

        public static CutsceneType CurrentCutscene = CutsceneType.None;
        public static Vector2 CollectiblePosition = Vector2.Zero;

        public static float FadeInTimer = 0;
        public static float FadeOutTimer = 0;

        public static void Initialize(ContentManager content)
        {
            _collectible = new AnimationDisplay();
            var star = new AnimationStrip(Game1.BigTileTextures, Helpers.GetBigTileRect(3, 5), 5, "star");
            star.LoopAnimation = true;
            star.FrameLength = 0.2f;
            star.Oscillate = true;
            _collectible.Add(star);
            var moon = new AnimationStrip(Game1.BigTileTextures, Helpers.GetBigTileRect(3, 4), 8, "moon");
            moon.LoopAnimation = true;
            moon.FrameLength = 0.2f;
            _collectible.Add(moon);
            var sock = new AnimationStrip(Game1.BigTileTextures, Helpers.GetBigTileRect(3, 6), 8, "sock");
            sock.LoopAnimation = true;
            sock.FrameLength = 0.2f;
            _collectible.Add(sock);

            _collectible.TintColor = Color.Transparent;
        }

        public static void ShowMoon()
        {
            _collectible.PlayIfNotAlreadyPlaying("moon");
            FadeInTimer = 0.5f;
        }

        public static void ShowSock()
        {
            _collectible.PlayIfNotAlreadyPlaying("sock");
            FadeInTimer = 0.5f;
        }

        public static void ShowStar()
        {
            _collectible.PlayIfNotAlreadyPlaying("star");
            FadeInTimer = 0.5f;
        }

        public static void HideCollectable()
        {
            FadeOutTimer = 0.5f;
        }

        public static void Update(GameTime gameTime, float elapsed)
        {
            if (CurrentCutscene == CutsceneType.Intro)
            {
                _collectible.Update(gameTime, elapsed);

                if (FadeInTimer > 0)
                {
                    FadeInTimer -= elapsed;
                    if (FadeInTimer <= 0)
                    {
                        FadeInTimer = 0;
                    }

                    // Adjust the opacity of the collectible to fade in.
                    var fadePercent = 1 - (FadeInTimer / 0.5f);
                    _collectible.TintColor = Color.Lerp(Color.Transparent, Color.White, fadePercent);
                }

                if (FadeOutTimer > 0)
                {
                    FadeOutTimer -= elapsed;
                    if (FadeOutTimer <= 0)
                    {
                        FadeOutTimer = 0;
                    }

                    // Adjust the opacity of the collectible to fade out.
                    var fadePercent = 1 - (FadeOutTimer / 0.5f);
                    _collectible.TintColor = Color.Lerp(Color.White, Color.Transparent, fadePercent);
                }
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            if (CurrentCutscene == CutsceneType.Intro)
            {
                _collectible.Draw(spriteBatch, CollectiblePosition, false);
            }
        }
    }
}
