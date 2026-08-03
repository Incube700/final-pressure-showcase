// Curated FINAL PRESSURE portfolio sample. Public namespace adapted for presentation.
using System;

namespace FinalPressure.Gameplay.Features.Audio
{
    public enum GameplayAudioMixState
    {
        Gameplay,
        Paused,
        FinalPressure,
        FinalBoss
    }

    public interface IGameplayMusicOutput
    {
        void PlayRunSequence();
        void PlayFinalPressureStinger();
        void PlayFinalBossStinger();
        void SetMixState(GameplayAudioMixState state);
        void SetMusicPaused(bool paused);
        void StopMusic(float fadeSeconds);
        void ResetMusic();
    }

    /// <summary>
    /// Narrow run-state owner for music. The presentation output advances the
    /// authored run tracks in order; gameplay events only select mixer states.
    /// </summary>
    public sealed class GameplayMusicService : IDisposable
    {
        public const float ResultFadeOutSeconds = 1f;

        private enum MusicPhase
        {
            Run,
            FinalPressure,
            FinalBoss
        }

        private readonly IGameplayMusicOutput _output;

        private bool _runActive;
        private MusicPhase _phase;
        private bool _paused;
        private bool _isDisposed;

        public GameplayMusicService(IGameplayMusicOutput output)
        {
            _output = output;
        }

        public void StartRun()
        {
            if (_isDisposed || _runActive)
                return;

            _runActive = true;
            _phase = MusicPhase.Run;
            _paused = false;
            _output?.ResetMusic();
            _output?.SetMusicPaused(false);
            _output?.SetMixState(GameplayAudioMixState.Gameplay);
            _output?.PlayRunSequence();
        }

        public void PrepareRun()
        {
            if (_isDisposed)
                return;

            _runActive = false;
            _phase = MusicPhase.Run;
            _paused = false;
            _output?.SetMusicPaused(false);
            _output?.ResetMusic();
            _output?.SetMixState(GameplayAudioMixState.Gameplay);
        }

        public void EnterFinalPressure()
        {
            if (_isDisposed || _runActive == false || _phase != MusicPhase.Run)
                return;

            _phase = MusicPhase.FinalPressure;
            if (_paused == false)
                _output?.SetMixState(GameplayAudioMixState.FinalPressure);
            _output?.PlayFinalPressureStinger();
        }

        public void EnterFinalBoss()
        {
            if (_isDisposed || _runActive == false || _phase == MusicPhase.FinalBoss)
                return;

            _phase = MusicPhase.FinalBoss;
            if (_paused == false)
                _output?.SetMixState(GameplayAudioMixState.FinalBoss);
            _output?.PlayFinalBossStinger();
        }

        public void SetPaused(bool paused)
        {
            if (_isDisposed || _runActive == false || _paused == paused)
                return;

            _paused = paused;
            if (paused)
            {
                _output?.SetMixState(GameplayAudioMixState.Paused);
                _output?.SetMusicPaused(true);
                return;
            }

            _output?.SetMixState(GetMixState());
            _output?.SetMusicPaused(false);
        }

        public void EndRun(float fadeSeconds = ResultFadeOutSeconds)
        {
            if (_isDisposed || _runActive == false)
                return;

            _runActive = false;
            _paused = false;
            _output?.SetMusicPaused(false);
            _output?.StopMusic(fadeSeconds);
        }

        private GameplayAudioMixState GetMixState()
        {
            return _phase switch
            {
                MusicPhase.FinalBoss => GameplayAudioMixState.FinalBoss,
                MusicPhase.FinalPressure => GameplayAudioMixState.FinalPressure,
                _ => GameplayAudioMixState.Gameplay
            };
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            EndRun(0f);
            _output?.ResetMusic();
            _isDisposed = true;
        }
    }
}
