using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework;

namespace MacGame
{
    public static class SoundManager
    {
        private static SoundEffectInstance seiMinecart;
        private static SoundEffectInstance seiSlowFlame;
        private static SoundEffectInstance seiCharging;
        private static SoundEffectInstance seiFullyCharged;

        public static Dictionary<string, SoundEffect> Sounds { get; set; }
        public static Dictionary<string, Song> Songs { get; set; }

        private static ContentManager content;
        private const int MaxSounds = 32;
        private static SoundEffectInstance[] _playingSounds = new SoundEffectInstance[MaxSounds];

        /// <summary>
        /// Gets the name of the currently playing song, or null if no song is playing.
        /// </summary>
        public static string CurrentSongName { get; private set; }

        private static Song _currentSong = null;

        private static bool _isMusicPaused = false;
        private static bool _isFading = false;
        private static MusicFadeEffect _fadeEffect;

        public static bool Enabled = true;

        /// <summary>
        /// Gets whether a song is playing or paused (i.e. not stopped).
        /// </summary>
        public static bool IsSongActive { get { return _currentSong != null && MediaPlayer.State != MediaState.Stopped; } }

        public static bool IsSongPaused { get { return _currentSong != null && _isMusicPaused; } }

        public static float SoundEffectVolume = 1f;
        public static float MusicVolume = 1f;

        // Buzzsaws click regularly, but I only want the click to happen once even if multiple are on screen.
        public static float buzzsawTimer = 0f;
        public static float buzzsawTimerGoal = 0.15f;

        // This class will set this to false after every update, but the buzzsaws can set it true.
        public static bool IsBuzzsawOnScreen = false;

        /// <summary>
        /// Loads a Song into the AudioManager.
        /// </summary>
        public static void LoadSong(string songName)
        {
            LoadSong(songName, @"Music\" + songName);
        }

        /// <summary>
        /// Loads a Song into the AudioManager.
        /// </summary>
        /// <param name="songName">Name of the song to load</param>
        /// <param name="songPath">Path to the song asset file</param>
        private static void LoadSong(string songName, string songPath)
        {
            if (Songs.ContainsKey(songName))
            {
                throw new InvalidOperationException(string.Format("Song '{0}' has already been loaded", songName));
            }

            var song = content.Load<Song>(songPath);

            Songs.Add(songName, song);
        }

        public static void LoadSound(string name)
        {
            var soundEffect = content.Load<SoundEffect>(@"SoundEffects\" + name);
            Sounds.Add(name, soundEffect);
        }

        // Acquires an open sound slot.
        private static int GetAvailableSoundIndex()
        {
            for (int i = 0; i < _playingSounds.Length; ++i)
            {
                if (_playingSounds[i] == null)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Plays the sound of the given name with the given parameters.
        /// </summary>
        /// <param name="soundName">Name of the sound</param>
        /// <param name="volume">Volume, 0.0f to 1.0f</param>
        /// <param name="pitch">Pitch, -1.0f (down one octave) to 1.0f (up one octave)</param>
        public static void PlaySound(string soundName, float volume = 1f, float pitch = 0f)
        {
            SoundEffect sound;

            if (!SoundManager.Sounds.TryGetValue(soundName, out sound))
            {
                if (Game1.IS_DEBUG)
                {
                    throw new ArgumentException($"Sound '{soundName}' not found");
                }
                else
                {
                    return;
                }
            }

            int index = GetAvailableSoundIndex();

            if (index != -1)
            {
                _playingSounds[index] = sound.CreateInstance();
                _playingSounds[index].Volume = volume * SoundEffectVolume;
                _playingSounds[index].Pitch = pitch;
                _playingSounds[index].Pan = 0f;
                _playingSounds[index].Play();
            }
        }

        public static void Initialize(ContentManager content)
        {
            SoundManager.content = content;
            Sounds = new Dictionary<string, SoundEffect>();
            Songs = new Dictionary<string, Song>();

            // Sound Effects
            LoadSound("AlertBox");
            LoadSound("Bark");
            LoadSound("Climb");
            LoadSound("SockCollected");
            LoadSound("ConversationStart");
            LoadSound("DoorOpen");
            LoadSound("DoorShut");
            LoadSound("Dig");
            LoadSound("HarshHit");
            LoadSound("Health");
            LoadSound("HitEnemy");
            LoadSound("Jump");
            LoadSound("KickedOutOfDoor");
            LoadSound("MacDeath");
            LoadSound("MenuChoice");
            LoadSound("OpenLockedDoor");
            LoadSound("Reveal");
            LoadSound("ShootFromCannon");
            LoadSound("TacoCollected");
            LoadSound("TakeHit");
            LoadSound("TypeLetter");
            LoadSound("Click");
            LoadSound("Explosion");
            LoadSound("Break");
            LoadSound("Splash");
            LoadSound("AlienDisappear");
            LoadSound("GooseBallBounce");
            LoadSound("GooseHonk");
            LoadSound("GooseHit");
            LoadSound("Swim");
            LoadSound("PlatformBreak");
            LoadSound("Pickup");
            LoadSound("Kick");
            LoadSound("Bounce");
            LoadSound("MinecartLand");
            LoadSound("MinecartLandMetal");
            LoadSound("MinecartJump");
            LoadSound("PowerUp");
            LoadSound("ChestOpen");
            LoadSound("CatBossHit");
            LoadSound("CatBossJump");
            LoadSound("CatBossShoot");
            LoadSound("SockReveal");
            LoadSound("Disappear");
            LoadSound("BatChirp");
            LoadSound("Fire");
            LoadSound("HitEnemy2");
            LoadSound("DracDeath");
            LoadSound("Minecart");
            LoadSound("Unlock");
            LoadSound("Shoot");
            LoadSound("Shoot2");
            LoadSound("Shrink");
            LoadSound("Grow");
            LoadSound("Crackle");
            LoadSound("Fall");
            LoadSound("Electric");
            LoadSound("SlowFlame");
            LoadSound("ChargedShot");
            LoadSound("GhostSound");
            LoadSound("JumpQuick");
            LoadSound("MurdererDeath");
            LoadSound("Hurt");
            LoadSound("Sickle");
            LoadSound("BlockAppear");
            LoadSound("DracPart1");
            LoadSound("DracPart2");
            LoadSound("GlassBreak");


            // Charging the ship's mega laser.
            LoadSound("Charging");
            LoadSound("FullyCharged");

            seiMinecart = Sounds["Minecart"].CreateInstance();
            seiSlowFlame = Sounds["SlowFlame"].CreateInstance();
            seiSlowFlame.Volume = 0.5f;

            seiCharging = Sounds["Charging"].CreateInstance();
            seiFullyCharged = Sounds["FullyCharged"].CreateInstance();

            // Music.
            LoadSong("Stage1");
            LoadSong("BossFight");

            MusicVolume = 0.1f;
        }


        public static void PlayCharging()
        {
            if (seiCharging.State != SoundState.Playing)
            {
                seiCharging.Volume = 0.5f;
                seiCharging.Play();
            }
        }

        public static void StopCharging()
        {
            if (seiCharging.State == SoundState.Playing)
            {
                seiCharging.Stop();
            }
        }

        public static void PlayFullyCharged()
        {
            if (seiFullyCharged.State != SoundState.Playing)
            {
                seiFullyCharged.Volume = 0.15f;

                // Vary the pitch slightly so it doesn't sound so droning.
                var randomFloat = (float)(Game1.Randy.NextFloat() * 0.4 - 0.2);

                seiFullyCharged.Pitch = randomFloat;
                seiFullyCharged.Play();
            }
        }

        public static void StopFullyCharged()
        {
            if (seiFullyCharged.State == SoundState.Playing)
            {
                seiFullyCharged.Stop();
            }
        }

        public static void PlayMinecart()
        {
            if (!(seiMinecart.State == SoundState.Playing))
            {
                seiMinecart.Volume = 0.05f;
                //seiMinecart.Pitch = GetVariedPitch(0.1f);
                seiMinecart.Play();
            }
        }

        public static void StopMinecart()
        {
            if (seiMinecart.State == SoundState.Playing)
            {
                seiMinecart.Stop();
            }
        }

        public static void PlayMinecartLanded()
        {
            SoundManager.PlaySound("MinecartLand", 0.4f);
            SoundManager.PlaySound("MinecartLandMetal", 0.3f);
        }

        public static void PlayMinecartJump()
        {
            SoundManager.PlaySound("Jump");
        }

        /// <summary>
        /// Plays this sound effect, but at most one time.
        /// </summary>
        public static void PlaySlowFlame()
        {
            if (seiSlowFlame.State != SoundState.Playing)
            {
                seiSlowFlame.Play();
            }
        }

        /// <summary>
        /// Called per loop unless Enabled is set to false.
        /// </summary>
        /// <param name="gameTime">Time elapsed since last frame</param>
        public static void Update(float elapsed)
        {
            for (int i = 0; i < _playingSounds.Length; ++i)
            {
                if (_playingSounds[i] != null && _playingSounds[i].State == SoundState.Stopped)
                {
                    _playingSounds[i].Dispose();
                    _playingSounds[i] = null;
                }
            }

            if (_currentSong != null && MediaPlayer.State == MediaState.Stopped)
            {
                _currentSong = null;
                CurrentSongName = null;
                _isMusicPaused = false;
            }

            if (_isFading && !_isMusicPaused)
            {
                if (_currentSong != null && MediaPlayer.State == MediaState.Playing)
                {
                    if (_fadeEffect.Update(elapsed))
                    {
                        _isFading = false;
                        if (_fadeEffect.TargetVolume == 0)
                        {
                            // if it has faded completely out stop it so that it will start from the beginning next time.
                            StopSong();
                        }
                    }

                    MediaPlayer.Volume = _fadeEffect.GetVolume();
                }
                else
                {
                    _isFading = false;
                }
            }

            if (IsBuzzsawOnScreen)
            {
                buzzsawTimer += elapsed;
                if (buzzsawTimer >= buzzsawTimerGoal)
                {
                    PlaySound("Crackle", 0.1f);
                    buzzsawTimer -= buzzsawTimerGoal;
                }
                IsBuzzsawOnScreen = false;
            }
        }

        /// <summary>
        /// Starts playing the song with the given name. If it is already playing, this method
        /// does nothing. If another song is currently playing, it is stopped first.
        /// </summary>
        /// <param name="songName">Name of the song to play</param>
        /// <param name="loop">True if song should loop, false otherwise</param>
        public static void PlaySong(string songName, bool loop = false, float volume = 1f)
        {
            if (CurrentSongName != songName)
            {
                if (_currentSong != null)
                {
                    MediaPlayer.Stop();
                }

                if (!Songs.TryGetValue(songName, out _currentSong))
                {
                    if (Game1.IS_DEBUG)
                    {
                        throw new ArgumentException($"Song '{songName}' not found");
                    }
                    else
                    {
                        // Don't ever throw an exception in the production build.
                        return;
                    }
                }

                CurrentSongName = songName;

                _isMusicPaused = false;
                MediaPlayer.IsRepeating = loop;
                MediaPlayer.Play(_currentSong);
                if (!Enabled)
                {
                    MediaPlayer.Pause();
                }
            }
            else
            {
                // if the same song is paused, resume it
                ResumeSong();
            }
            CancelFade(FadeCancelOptions.Current);
            MediaPlayer.Volume = volume * MusicVolume;
        }

        /// <summary>
        /// Pauses the currently playing song. This is a no-op if the song is already paused,
        /// or if no song is currently playing.
        /// </summary>
        public static void PauseSong()
        {
            if (_currentSong != null && !_isMusicPaused)
            {
                if (Enabled) MediaPlayer.Pause();
                _isMusicPaused = true;
            }
        }

        /// <summary>
        /// Resumes the currently paused song. This is a no-op if the song is not paused,
        /// or if no song is currently playing.
        /// </summary>
        public static void ResumeSong()
        {
            if (_currentSong != null && _isMusicPaused)
            {
                if (Enabled) MediaPlayer.Resume();
                _isMusicPaused = false;
            }
        }

        /// <summary>
        /// Stops the currently playing song. This is a no-op if no song is currently playing.
        /// </summary>
        public static void StopSong()
        {
            if (_currentSong != null && MediaPlayer.State != MediaState.Stopped)
            {
                MediaPlayer.Stop();
                _isMusicPaused = false;
                CurrentSongName = "";
                _currentSong = null;
            }
        }

        public static void FadeOutSong()
        {
            FadeSong(0f, 3f);
        }

        /// <summary>
        /// Smoothly transition between two volumes.
        /// </summary>
        /// <param name="targetVolume">Target volume, 0.0f to 1.0f</param>
        /// <param name="duration">Length of volume transition</param>
        public static void FadeSong(float targetVolume, float duration)
        {
            if (duration <= 0)
            {
                throw new ArgumentException("Duration must be a positive value");
            }

            _fadeEffect = new MusicFadeEffect(MediaPlayer.Volume, targetVolume, duration);
            _isFading = true;
        }

        /// <summary>
        /// Stop the current fade.
        /// </summary>
        /// <param name="option">Options for setting the music volume</param>
        public static void CancelFade(FadeCancelOptions option)
        {
            if (_isFading)
            {
                switch (option)
                {
                    case FadeCancelOptions.Source: MediaPlayer.Volume = _fadeEffect.SourceVolume; break;
                    case FadeCancelOptions.Target: MediaPlayer.Volume = _fadeEffect.TargetVolume; break;
                }

                _isFading = false;
            }
        }

    }

    public struct MusicFadeEffect
    {
        public float SourceVolume;
        public float TargetVolume;

        private float _time;
        private float _duration;

        public MusicFadeEffect(float sourceVolume, float targetVolume, float duration)
        {
            SourceVolume = sourceVolume;
            TargetVolume = targetVolume;
            _time = 0;
            _duration = duration;
        }

        public bool Update(float time)
        {
            _time += time;

            if (_time >= _duration)
            {
                _time = _duration;
                return true;
            }

            return false;
        }

        public float GetVolume()
        {
            return MathHelper.Lerp(SourceVolume, TargetVolume, _time / _duration);
        }
    }

    /// <summary>
    /// Options for AudioManager.CancelFade
    /// </summary>
    public enum FadeCancelOptions
    {
        /// <summary>
        /// Return to pre-fade volume
        /// </summary>
        Source,
        /// <summary>
        /// Snap to fade target volume
        /// </summary>
        Target,
        /// <summary>
        /// Keep current volume
        /// </summary>
        Current
    }
}
