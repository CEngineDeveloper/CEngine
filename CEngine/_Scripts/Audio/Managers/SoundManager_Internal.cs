using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CYM.Audio
{
    public partial class SoundManager : MonoSingleton<SoundManager>
    {
        #region life
        protected override void Awake()
        {
            base.Awake();
            if (Ins && Ins.gameObject != gameObject)
                Destroy(gameObject);
            else
            {
                gameObject.name = this.name;
                AudioSource[] currentSources = gameObject.GetComponents<AudioSource>();
                foreach (AudioSource source in currentSources)
                    Destroy(source);
                if (Ins)
                {
                    audios = new AudioSource[2];
                    audios[0] = gameObject.AddComponent<AudioSource>();
                    audios[1] = gameObject.AddComponent<AudioSource>();

                    audiosPaused = new bool[2];
                    audiosPaused[0] = audiosPaused[1] = false;

                    audios[0].hideFlags = HideFlags.HideInInspector;
                    audios[1].hideFlags = HideFlags.HideInInspector;

                    SoundManagerTools.Make2D(ref audios[0]);
                    SoundManagerTools.Make2D(ref audios[1]);

                    audios[0].volume = 0f;
                    audios[1].volume = 0f;

                    audios[0].ignoreListenerVolume = true;
                    audios[1].ignoreListenerVolume = true;

                    maxVolume = AudioListener.volume;

                    currentPlaying = CheckWhosPlaying();
                }
                SetupSoundFX();
            }
        }
        #endregion

        #region check
        /// <summary>
        /// Checks the who's not playing.  This will return SOUNDMANAGER_FALSE(-1) IFF both AudioSources are playing.  This is usually in a crossfade situation.
        /// </summary>
        /// <returns>
        /// The index of who's not playing.
        /// </returns>
        private int CheckWhosNotPlaying()
        {
            if (!audios[0].isPlaying) return 0;
            else if (!audios[1].isPlaying) return 1;
            else return SOUNDMANAGER_FALSE;
        }
        /// <summary>
        /// Checks the who's playing.  This will return SOUNDMANAGER_FALSE(-1) IFF no AudioSources are playing.  This is usually in a silent scene situation.
        /// </summary>
        /// <returns>
        /// The index of who's playing.
        /// </returns>
        private int CheckWhosPlaying()
        {
            if (audios[0].isPlaying) return 0;
            else if (audios[1].isPlaying) return 1;
            else return SOUNDMANAGER_FALSE;
        }
        #endregion

        #region control
        /// <summary>
        /// Stops all sound immediately.
        /// </summary>
        private void _StopMusicImmediately()
        {
            StopAllCoroutines();
            CoroutineCrossoutAll = StartCoroutine(CrossoutAll(0f));
        }
        /// <summary>
        /// Crosses out all AudioSources.
        /// </summary>
        private void _StopMusic()
        {
            StopAllCoroutines();
            CoroutineCrossoutAll = StartCoroutine(CrossoutAll(crossDuration));
        }
        /// <summary>
        /// Pause's all sounds
        /// </summary>
        private void _Pause()
        {
            if (!isPaused)
            {
                isPaused = !isPaused;
                if (audios[0].isPlaying)
                    audiosPaused[0] = true;
                audios[0].Pause();
                if (audios[1].isPlaying)
                    audiosPaused[1] = true;
                audios[1].Pause();
                PSFX(true);
            }
        }
        /// <summary>
        /// Unpause's all sounds
        /// </summary>
        private void _UnPause()
        {
            if (isPaused)
            {
                if (audiosPaused[0])
                    audios[0].Play();
                if (audiosPaused[1])
                    audios[1].Play();

                audiosPaused[0] = audiosPaused[1] = false;

                PSFX(false);
                isPaused = !isPaused;
            }
        }
        #endregion

        #region play
        /// <summary>
        /// Plays the clip immediately regardless of a playing SoundConnection, with an option to loop.  Calls an event once the clip is done.
        /// You can resume a SoundConnection afterwards if you so choose, using currentSoundConnection.  However, it will not resume on it's own.
        /// Callbacks will only fire once.
        /// </summary>
        /// <param name='clip2play'>
        /// The clip to play.
        /// </param>
        /// <param name='runOnEndFunction'>
        /// Function to run once the clip is done.  Is added to OnSongEnd
        /// </param>
        /// <param name='loop'>
        /// Whether the clip should loop
        /// </param>
        private void _PlayImmediately(AudioClip clip2play, bool loop, SongCallBack runOnEndFunction)
        {
            if (InternalCallback != null)
                OnSongEnd = InternalCallback;
            StopMusicImmediately();
            InternalCallback = runOnEndFunction;

            if (offTheBGM || isPaused) return;
            SoundConnection sc;
            if (loop)
                sc = new SoundConnection("", PlayMethod.ContinuousPlayThrough, clip2play);
            else
                sc = new SoundConnection("", PlayMethod.OncePlayThrough, clip2play);
            ignoreCrossDuration = true;
            SoundManager.PlayConnection(sc);
        }
        /// <summary>
        /// Plays the clip by crossing out what's currently playing regardless of a playing SoundConnection, with an option to loop.  Calls an event once the clip is done.
        /// You can resume a SoundConnection afterwards if you so choose, using currentSoundConnection.  However, it will not resume on it's own.
        /// Callbacks will only fire once.
        /// </summary>
        /// <param name='clip2play'>
        /// The clip to play.
        /// </param>
        /// <param name='runOnEndFunction'>
        /// Function to run once the clip is done.  Is added to OnSongEnd
        /// </param>
        /// <param name='loop'>
        /// Whether the clip should loop
        /// </param>
        private void _Play(AudioClip clip2play, bool loop, SongCallBack runOnEndFunction)
        {
            if (offTheBGM || isPaused) return;
            if (InternalCallback != null)
                OnSongEnd = InternalCallback;
            InternalCallback = runOnEndFunction;

            SoundConnection sc;
            if (loop)
                sc = new SoundConnection(SceneManager.GetActiveScene().name, PlayMethod.ContinuousPlayThrough, clip2play);
            else
                sc = new SoundConnection(SceneManager.GetActiveScene().name, PlayMethod.OncePlayThrough, clip2play);
            SoundManager.PlayConnection(sc);
        }
        /// <summary>
        /// Plays the SoundConnection right then, regardless of what you put at the level parameter of the SoundConnection.
        /// </summary>
        private void _PlayConnection(SoundConnection sc)
        {
            if (offTheBGM || isPaused) return;
            if (string.IsNullOrEmpty(sc.level))
            {
                int i = 1;
                while (SoundConnectionsContainsThisLevel("CustomConnection" + i.ToString()) != SOUNDMANAGER_FALSE)
                    i++;
                sc.level = "CustomConnection" + i.ToString();
            }
            StopPreviousPlaySoundConnection();
            CoroutinePlaySoundConnection = StartCoroutine(PlaySoundConnection(sc));
        }
        /// <summary>
        /// Plays a SoundConnection on SoundManager that matches the level name.
        /// </summary>
        private void _PlayConnection(string levelName)
        {
            if (offTheBGM || isPaused) return;
            int _indexOf = SoundConnectionsContainsThisLevel(levelName);
            if (_indexOf != SOUNDMANAGER_FALSE)
            {
                StopPreviousPlaySoundConnection();
                CoroutinePlaySoundConnection = StartCoroutine(PlaySoundConnection(soundConnections[_indexOf]));
            }
            else
            {
                CLog.Error("There are no SoundConnections with the name: " + levelName);
            }
        }
        /// <summary>
        /// Plays the clip.  This is a private method because it must be used appropriately with a SoundConnection.
        /// It's logic is dependent on it being used correctly.
        /// </summary>
        /// <param name='clip2play'>
        /// The clip to play.
        /// </param>
        /// <returns>
        /// The index of the new audiosource if the clip is already being played, otherwise SOUNDMANAGER_FALSE
        /// </returns>
        private int _PlayClip(AudioClip clip2play, float clipVolume = 1f)
        {
            if (showDebug) CLog.Log("Playing: " + clip2play.name);
            currentPlaying = CheckWhosPlaying();
            int notPlaying = CheckWhosNotPlaying();
            if (currentPlaying != SOUNDMANAGER_FALSE) //If an AudioSource is playing...
            {
                if (notPlaying != SOUNDMANAGER_FALSE) //If one AudioSources is playing...
                {
                    if ((audios[currentPlaying].clip.Equals(clip2play) && audios[currentPlaying].isPlaying)) //If the current playing source is playing the clip...
                    {
                        if (showDebug) CLog.Log("Already playing BGM, check if crossing out(" + outCrossing[currentPlaying] + ") or in(" + inCrossing[currentPlaying] + ").");
                        if (outCrossing[currentPlaying]) //If that source is crossing out, stop it and cross it back in.
                        {
                            StopAllNonSoundConnectionCoroutines();
                            if (showDebug) CLog.Log("In the process of crossing out, so that is being changed to cross in now.");
                            outCrossing[currentPlaying] = false;
                            CoroutineCrossin = StartCoroutine(Crossin(new object[] { audios[currentPlaying], crossDuration, clipVolume }));
                            return currentPlaying;
                        }
                        else if (movingOnFromSong)
                        {
                            if (showDebug) CLog.Log("Current song is actually done, so crossfading to another Ins of it.");
                            if (audios[notPlaying] == null || audios[notPlaying].clip == null || !audios[notPlaying].clip.Equals(clip2play))
                                audios[notPlaying].clip = clip2play;
                            CoroutineCrossfade = StartCoroutine(Crossfade(new object[] { audios[currentPlaying], audios[notPlaying], crossDuration, clipVolume }));
                            return notPlaying;
                        }
                        return currentPlaying;
                    }
                    else //If the current playing source is not playing the clip, crossfade to the clip normally.
                    {
                        StopAllNonSoundConnectionCoroutines();
                        if (showDebug) CLog.Log("Playing another track, crossfading to that.");
                        audios[notPlaying].clip = clip2play;
                        CoroutineCrossfade = StartCoroutine(Crossfade(new object[] { audios[currentPlaying], audios[notPlaying], crossDuration, clipVolume }));
                        return notPlaying;
                    }
                }
                else //If both AudioSources are playing...
                {
                    int lastPlaying = GetActualCurrentPlayingIndex();
                    if (showDebug) CLog.Log("Both are playing (crossfade situation).");
                    if (clip2play.Equals(audios[0].clip) && clip2play.Equals(audios[1].clip))
                    {
                        if (showDebug) CLog.Log("If clip == clip in audio1 AND audio2, then do nothing and let it finish.");
                        int swapIn = (lastPlaying == 0) ? 0 : 1;

                        if (!audios[0].isPlaying) audios[0].Play();
                        if (!audios[1].isPlaying) audios[1].Play();

                        return swapIn;
                    }
                    else if (clip2play.Equals(audios[0].clip)) //If the clip is the same clip playing in source1...
                    {
                        bool switcheroo = false;
                        if (outCrossing[0] && ((audios[0].clip.samples - audios[0].timeSamples) * 1f) / (audios[0].clip.frequency * 1f) <= crossDuration) // If the clip is crossing out though, cross in the new track started over.
                        {
                            if (showDebug) CLog.Log("Clip == clip in audio1, but it's crossing out so cross it in from the beginning.");
                            audios[1].clip = clip2play;
                            audios[1].timeSamples = 0;
                            switcheroo = true;
                        }
                        else if (showDebug) CLog.Log("If clip == clip in audio1, then just switch them.");

                        int swapIn = (lastPlaying == 0) ? 0 : 1;
                        int swapOut = (swapIn == 0) ? 1 : 0;
                        StopAllNonSoundConnectionCoroutines();
                        if (switcheroo) // Perform the switch if needed
                        {
                            int tempSwap = swapIn;
                            swapIn = swapOut;
                            swapOut = tempSwap;
                        }
                        if (swapIn != 0 || (switcheroo && swapIn != 1)) //If the source crossing out is not the clip OR it is but is crossing out, just continue with crossfade
                        {
                            CoroutineCrossfade = StartCoroutine(Crossfade(new object[] { audios[swapIn], audios[swapOut], crossDuration, clipVolume }));
                        }
                        else
                        { //If the source crossing out is the clip, swap them so that it's now crossing in
                            CoroutineCrossfade = StartCoroutine(Crossfade(new object[] { audios[swapOut], audios[swapIn], crossDuration, clipVolume }));
                        }
                        if (!audios[0].isPlaying) audios[0].Play();
                        if (!audios[1].isPlaying) audios[1].Play();
                        if (swapIn != 0)
                            return swapOut;
                        return swapIn;
                    }
                    else if (clip2play.Equals(audios[1].clip)) //If the clip is the same clip playing in source2...
                    {
                        bool switcheroo = false;
                        if (outCrossing[1] && ((audios[1].clip.samples - audios[1].timeSamples) * 1f) / (audios[1].clip.frequency * 1f) <= crossDuration) // If the clip is crossing out though, cross in the new track started over.
                        {
                            if (showDebug) CLog.Log("Clip == clip in audio2, but it's crossing out so cross it in from the beginning.");
                            audios[0].clip = clip2play;
                            audios[0].timeSamples = 0;
                            switcheroo = true;
                        }
                        else if (showDebug) CLog.Log("If clip == clip in audio2, then just switch them.");

                        int swapIn = (lastPlaying == 0) ? 0 : 1;
                        int swapOut = (swapIn == 0) ? 1 : 0;
                        StopAllNonSoundConnectionCoroutines();
                        if (switcheroo) // Perform the switch if needed
                        {
                            int tempSwap = swapIn;
                            swapIn = swapOut;
                            swapOut = tempSwap;
                        }
                        if (swapIn != 1 || (switcheroo && swapIn != 0)) //If the source crossing out is not the clip, just continue with crossfade
                        {
                            CoroutineCrossfade = StartCoroutine(Crossfade(new object[] { audios[swapIn], audios[swapOut], crossDuration, clipVolume }));
                        }
                        else
                        {
                            //If the source crossing out is the clip, swap them so that it's now crossing in
                            CoroutineCrossfade = StartCoroutine(Crossfade(new object[] { audios[swapOut], audios[swapIn], crossDuration, clipVolume }));
                        };
                        if (!audios[0].isPlaying) audios[0].Play();
                        if (!audios[1].isPlaying) audios[1].Play();
                        if (swapIn != 1)
                            return swapOut;
                        return swapIn;
                    }
                    else
                    { // If the clip is not in either source1 or source2...
                        StopAllNonSoundConnectionCoroutines();
                        if (showDebug) CLog.Log("If clip is in neither, find the louder one and crossfade from that one.");
                        if (audios[0].volume > audios[1].volume) //If source1 is louder than source2, then crossfade from source1.
                        {
                            audios[1].clip = clip2play;
                            CoroutineCrossfade = StartCoroutine(Crossfade(new object[] { audios[0], audios[1], crossDuration, clipVolume }));
                            return 1;
                        }
                        else //If source2 is louder than source1, then crossfade from source2.
                        {
                            audios[0].clip = clip2play;
                            CoroutineCrossfade = StartCoroutine(Crossfade(new object[] { audios[1], audios[0], crossDuration, clipVolume }));
                            return 0;
                        }
                    }
                }
            }
            else //If no AudioSource is playing (silent scene OR paused), crossin to the clip
            {
                if (audiosPaused[0] && audiosPaused[1]) // paused and playing two tracks (crossfading)
                {
                    if (showDebug) CLog.Log("All sound is paused and it's crossfading between two songs. Replace the lower volume song and prepare for crossfade on unpause.");
                    int lesserAudio = (audios[0].volume > audios[1].volume) ? 1 : 0;
                    int greaterAudio = (lesserAudio == 0) ? 1 : 0;
                    audios[lesserAudio].clip = clip2play;
                    CoroutineCrossfade = StartCoroutine(Crossfade(new object[] { audios[greaterAudio], audios[lesserAudio], crossDuration, clipVolume }));
                }
                else if (audiosPaused[0]) // track 1 is paused
                {
                    if (showDebug) CLog.Log("All sound is paused and track1 is playing. Prepare for crossfade on unpause.");
                    audios[1].clip = clip2play;
                    audiosPaused[1] = true;
                    CoroutineCrossfade = StartCoroutine(Crossfade(new object[] { audios[0], audios[1], crossDuration, clipVolume }));
                }
                else if (audiosPaused[1]) // track 2 is paused
                {
                    if (showDebug) CLog.Log("All sound is paused and track2 is playing. Prepare for crossfade on unpause.");
                    audios[0].clip = clip2play;
                    audiosPaused[0] = true;
                    CoroutineCrossfade = StartCoroutine(Crossfade(new object[] { audios[1], audios[0], crossDuration, clipVolume }));
                }
                else // silent scene
                {
                    if (showDebug) CLog.Log("Wasn't playing anything, crossing in.");
                    audios[notPlaying].clip = clip2play;
                    CoroutineCrossin = StartCoroutine(Crossin(new object[] { audios[notPlaying], crossDuration, clipVolume }));
                }
            }
            return SOUNDMANAGER_FALSE;
        }
        #endregion

        #region IEnumerator
        Coroutine CoroutinePlaySoundConnection;
        Coroutine CoroutineCrossfade;
        Coroutine CoroutineCrossout;
        Coroutine CoroutineCrossin;
        Coroutine CoroutineCrossoutAll;

        private IEnumerator Crossfade(object[] param)
        {
            OnCrossInBegin?.Invoke();
            OnCrossInBegin = null;

            OnCrossOutBegin?.Invoke();
            OnCrossOutBegin = null;

            AudioSource a1 = param[0] as AudioSource;
            AudioSource a2 = param[1] as AudioSource;

            int index1 = GetAudioSourceIndex(a1);
            int index2 = GetAudioSourceIndex(a2);

            float duration = (float)param[2];
            if (ignoreCrossDuration)
            {
                ignoreCrossDuration = false;
                duration = 0f;
            }
            float realSongLength = 0f;
            if (a2.clip != null)
                realSongLength = ((a2.clip.samples - a2.timeSamples) * 1f) / (a2.clip.frequency * 1f);
            if (duration - (realSongLength / 2f) > .1f)
            {
                duration = Mathf.Floor((realSongLength / 2f) * 100f) / 100f;
                CLog.Warn("Had to reduce the cross duration to " + duration + " for this transition as the cross duration is longer than half the track length.");
            }
            modifiedCrossDuration = duration;

            float clipVolume = (float)param[3];

            OnSongBegin?.Invoke();
            OnSongBegin = null;

            if (index1 == SOUNDMANAGER_FALSE || index2 == SOUNDMANAGER_FALSE)
                CLog.Warn("You passed an AudioSource that is not used by the SoundManager May cause erratic behavior");
            outCrossing[index1] = true;
            inCrossing[index2] = true;
            outCrossing[index2] = false;
            inCrossing[index1] = false;

            var startTime = Time.realtimeSinceStartup;
            var endTime = startTime + duration;
            if (!a2.isPlaying) a2.Play();
            float a1StartVolume = a1.volume, a2StartVolume = a2.volume, deltaPercent = 0f, a1DeltaVolume = 0f, a2DeltaVolume = 0f, startMaxMusicVolume = maxMusicVolume, volumePercent = 1f;
            bool passedFirstPause = false, passedFirstUnpause = true;
            float pauseTimeRemaining = 0f;
            while (isPaused || passedFirstPause || Time.realtimeSinceStartup < endTime)
            {
                if (isPaused)
                {
                    if (!passedFirstPause)
                    {
                        pauseTimeRemaining = endTime - Time.realtimeSinceStartup;
                        passedFirstPause = true;
                        passedFirstUnpause = false;
                    }
                    yield return new WaitForFixedUpdate();
                }
                else
                {
                    if (!passedFirstUnpause)
                    {
                        float oldEndTime = endTime;
                        endTime = Time.realtimeSinceStartup + pauseTimeRemaining;
                        startTime += (endTime - oldEndTime);
                        passedFirstPause = false;
                        passedFirstUnpause = true;
                    }
                    if (startMaxMusicVolume == 0f)
                        volumePercent = 1f;
                    else
                        volumePercent = maxMusicVolume / startMaxMusicVolume;

                    if (endTime - Time.realtimeSinceStartup > duration)
                    {
                        startTime = Time.realtimeSinceStartup;
                        endTime = startTime + duration;
                    }
                    deltaPercent = ((Time.realtimeSinceStartup - startTime) / duration);
                    a1DeltaVolume = deltaPercent * a1StartVolume;
                    a2DeltaVolume = deltaPercent * (startMaxMusicVolume - a2StartVolume);

                    a1.volume = Mathf.Clamp01((a1StartVolume - a1DeltaVolume) * volumePercent);
                    a2.volume = Mathf.Clamp01((a2DeltaVolume + a2StartVolume) * volumePercent * clipVolume);
                    yield return null;
                }
            }
            a1.volume = 0f;
            a2.volume = maxMusicVolume * clipVolume;
            a1.Stop();
            a1.timeSamples = 0;
            modifiedCrossDuration = crossDuration;
            currentPlaying = CheckWhosPlaying();

            outCrossing[index1] = false;
            inCrossing[index2] = false;

            if (OnSongEnd != null)
            {
                OnSongEnd();
                if (InternalCallback != null)
                    OnSongEnd = InternalCallback;
                else
                    OnSongEnd = null;
                InternalCallback = null;
            }

            if (InternalCallback != null)
                OnSongEnd = InternalCallback;
            InternalCallback = null;

            OnSongBegin?.Invoke();
            OnSongBegin = null;

            SetNextSongInQueue();
        }
        // Crossout from a1 for duration.
        private IEnumerator Crossout(object[] param)
        {
            OnCrossOutBegin?.Invoke();
            OnCrossOutBegin = null;

            AudioSource a1 = param[0] as AudioSource;
            float duration = (float)param[1];
            if (ignoreCrossDuration)
            {
                ignoreCrossDuration = false;
                duration = 0f;
            }
            float realSongLength = 0f;
            if (a1.clip != null)
                realSongLength = ((a1.clip.samples - a1.timeSamples) * 1f) / (a1.clip.frequency * 1f);
            if (duration - (realSongLength / 2f) > .1f)
            {
                duration = Mathf.Floor((realSongLength / 2f) * 100f) / 100f;
                Debug.LogWarning("Had to reduce the cross duration to " + duration + " for this transition as the cross duration is longer than half the track length.");
            }
            modifiedCrossDuration = duration;

            int index1 = GetAudioSourceIndex(a1);

            if (index1 == SOUNDMANAGER_FALSE)
                Debug.LogWarning("You passed an AudioSource that is not used by the SoundManager May cause erratic behavior");
            outCrossing[index1] = true;
            inCrossing[index1] = false;

            var startTime = Time.realtimeSinceStartup;
            var endTime = startTime + duration;
            float maxVolume = a1.volume, deltaVolume = 0f, startMaxMusicVolume = maxMusicVolume, volumePercent = 1f;
            bool passedFirstPause = false, passedFirstUnpause = true;
            float pauseTimeRemaining = 0f;
            while (isPaused || passedFirstPause || Time.realtimeSinceStartup < endTime)
            {
                if (isPaused)
                {
                    if (!passedFirstPause)
                    {
                        pauseTimeRemaining = endTime - Time.realtimeSinceStartup;
                        passedFirstPause = true;
                        passedFirstUnpause = false;
                    }
                    yield return new WaitForFixedUpdate();
                }
                else
                {
                    if (!passedFirstUnpause)
                    {
                        float oldEndTime = endTime;
                        endTime = Time.realtimeSinceStartup + pauseTimeRemaining;
                        startTime += (endTime - oldEndTime);
                        passedFirstPause = false;
                        passedFirstUnpause = true;
                    }
                    if (startMaxMusicVolume == 0f)
                        volumePercent = 1f;
                    else
                        volumePercent = maxMusicVolume / startMaxMusicVolume;

                    if (endTime - Time.realtimeSinceStartup > duration)
                    {
                        startTime = Time.realtimeSinceStartup;
                        endTime = startTime + duration;
                    }
                    deltaVolume = ((Time.realtimeSinceStartup - startTime) / duration) * maxVolume;

                    a1.volume = Mathf.Clamp01((maxVolume - deltaVolume) * volumePercent);
                    yield return null;
                }
            }
            a1.volume = 0f;
            a1.Stop();
            a1.timeSamples = 0;
            modifiedCrossDuration = crossDuration;
            currentPlaying = CheckWhosPlaying();

            outCrossing[index1] = true;

            OnSongEnd?.Invoke();
            OnSongEnd = null;

            InternalCallback?.Invoke();
            InternalCallback = null;
        }
        // Crossout from all AudioSources for a duration.
        private IEnumerator CrossoutAll(float duration)
        {
            if (CheckWhosPlaying() == SOUNDMANAGER_FALSE) yield break;

            OnCrossOutBegin?.Invoke();
            OnCrossOutBegin = null;

            outCrossing[0] = true;
            outCrossing[1] = true;
            inCrossing[0] = false;
            inCrossing[1] = false;

            float realSongLength = 0f;
            if (audios[0].clip != null && audios[1].clip != null)
                realSongLength = Mathf.Max(((audios[0].clip.samples - audios[0].timeSamples) * 1f) / (audios[0].clip.frequency * 1f), ((audios[1].clip.samples - audios[1].timeSamples) * 1f) / (audios[1].clip.frequency * 1f));
            else if (audios[0].clip != null)
                realSongLength = ((audios[0].clip.samples - audios[0].timeSamples) * 1f) / (audios[0].clip.frequency * 1f);
            else if (audios[1].clip != null)
                realSongLength = ((audios[1].clip.samples - audios[1].timeSamples) * 1f) / (audios[1].clip.frequency * 1f);

            if (ignoreCrossDuration)
            {
                ignoreCrossDuration = false;
                duration = 0f;
            }
            if (duration - (realSongLength / 2f) > .1f)
            {
                duration = Mathf.Floor((realSongLength / 2f) * 100f) / 100f;
                Debug.LogWarning("Had to reduce the cross duration to " + duration + " for this transition as the cross duration is longer than half the track length.");
            }
            modifiedCrossDuration = duration;

            var startTime = Time.realtimeSinceStartup;
            var endTime = Time.realtimeSinceStartup + duration;
            float a1MaxVolume = volume1, a2MaxVolume = volume2;
            float deltaPercent = 0f, a1DeltaVolume = 0f, a2DeltaVolume = 0f, startMaxMusicVolume = maxMusicVolume, volumePercent = 1f;
            bool passedFirstPause = false, passedFirstUnpause = true;
            float pauseTimeRemaining = 0f;
            while (isPaused || passedFirstPause || Time.realtimeSinceStartup < endTime)
            {
                if (isPaused)
                {
                    if (!passedFirstPause)
                    {
                        pauseTimeRemaining = endTime - Time.realtimeSinceStartup;
                        passedFirstPause = true;
                        passedFirstUnpause = false;
                    }
                    yield return new WaitForFixedUpdate();
                }
                else
                {
                    if (!passedFirstUnpause)
                    {
                        float oldEndTime = endTime;
                        endTime = Time.realtimeSinceStartup + pauseTimeRemaining;
                        startTime += (endTime - oldEndTime);
                        passedFirstPause = false;
                        passedFirstUnpause = true;
                    }
                    if (startMaxMusicVolume == 0f)
                        volumePercent = 1f;
                    else
                        volumePercent = maxMusicVolume / startMaxMusicVolume;

                    if (endTime - Time.realtimeSinceStartup > duration)
                    {
                        startTime = Time.realtimeSinceStartup;
                        endTime = startTime + duration;
                    }
                    deltaPercent = ((Time.realtimeSinceStartup - startTime) / duration);
                    a1DeltaVolume = deltaPercent * a1MaxVolume;
                    a2DeltaVolume = deltaPercent * a2MaxVolume;

                    volume1 = Mathf.Clamp01((a1MaxVolume - a1DeltaVolume) * volumePercent);
                    volume2 = Mathf.Clamp01((a2MaxVolume - a2DeltaVolume) * volumePercent);
                    yield return null;
                }
            }
            volume1 = volume2 = 0f;
            audios[0].Stop();
            audios[1].Stop();
            audios[0].timeSamples = 0;
            audios[1].timeSamples = 0;
            modifiedCrossDuration = crossDuration;
            currentPlaying = CheckWhosPlaying();

            outCrossing[0] = false;
            outCrossing[1] = false;

            OnSongEnd?.Invoke();
            OnSongEnd = null;

            InternalCallback?.Invoke();
            InternalCallback = null;
        }
        // Crossin from a1 for duration.
        private IEnumerator Crossin(object[] param)
        {
            OnCrossInBegin?.Invoke();
            OnCrossInBegin = null;

            AudioSource a1 = param[0] as AudioSource;

            float duration = (float)param[1];
            if (ignoreCrossDuration)
            {
                ignoreCrossDuration = false;
                duration = 0f;
            }
            float realSongLength = 0f;
            if (a1.clip != null)
                realSongLength = (a1.clip.samples * 1f) / (a1.clip.frequency * 1f);
            if (duration - (realSongLength / 2f) > .1f)
            {
                duration = Mathf.Floor((realSongLength / 2f) * 100f) / 100f;
                Debug.LogWarning("Had to reduce the cross duration to " + duration + " for this transition as the cross duration is longer than half the track length.");
            }
            modifiedCrossDuration = duration;

            float clipVolume = (float)param[2];

            int index1 = GetAudioSourceIndex(a1);

            if (index1 == SOUNDMANAGER_FALSE)
                Debug.LogWarning("You passed an AudioSource that is not used by the SoundManager May cause erratic behavior");
            inCrossing[index1] = true;
            outCrossing[index1] = false;

            var startTime = Time.realtimeSinceStartup;
            var endTime = startTime + duration;
            float a1StartVolume = a1.volume, startMaxMusicVolume = maxMusicVolume, volumePercent = 1f;
            if (!a1.isPlaying)
            {
                a1StartVolume = 0f;
                a1.Play();
            }
            float deltaVolume = 0f;
            bool passedFirstPause = false, passedFirstUnpause = true;
            float pauseTimeRemaining = 0f;
            while (isPaused || passedFirstPause || Time.realtimeSinceStartup < endTime)
            {
                if (isPaused)
                {
                    if (!passedFirstPause)
                    {
                        pauseTimeRemaining = endTime - Time.realtimeSinceStartup;
                        passedFirstPause = true;
                        passedFirstUnpause = false;
                    }
                    yield return new WaitForFixedUpdate();
                }
                else
                {
                    if (!passedFirstUnpause)
                    {
                        float oldEndTime = endTime;
                        endTime = Time.realtimeSinceStartup + pauseTimeRemaining;
                        startTime += (endTime - oldEndTime);
                        passedFirstPause = false;
                        passedFirstUnpause = true;
                    }
                    if (startMaxMusicVolume == 0f)
                        volumePercent = 1f;
                    else
                        volumePercent = maxMusicVolume / startMaxMusicVolume;

                    if (endTime - Time.realtimeSinceStartup > duration)
                    {
                        startTime = Time.realtimeSinceStartup;
                        endTime = startTime + duration;
                    }
                    deltaVolume = ((Time.realtimeSinceStartup - startTime) / duration) * (startMaxMusicVolume - a1StartVolume);

                    a1.volume = Mathf.Clamp01((deltaVolume + a1StartVolume) * volumePercent * clipVolume);
                    yield return null;
                }
            }
            a1.volume = maxMusicVolume * clipVolume;
            modifiedCrossDuration = crossDuration;
            currentPlaying = CheckWhosPlaying();

            inCrossing[index1] = false;

            OnSongBegin?.Invoke();
            OnSongBegin = null;

            if (InternalCallback != null)
                OnSongEnd = InternalCallback;
            InternalCallback = null;

            SetNextSongInQueue();
        }
        // Plays the SoundConnection according to it's PlayMethod.
        private IEnumerator PlaySoundConnection(SoundConnection sc)
        {
            if (isPaused) yield break;
            currentSoundConnection = sc;
            if (sc.soundsToPlay.Count == 0)
            {
                Debug.LogWarning("The SoundConnection for this level has no sounds to play.  Will cross out.");
                CoroutineCrossoutAll = StartCoroutine(CrossoutAll(crossDuration));
                yield break;
            }
            int songPlaying = 0;
            if (skipSongs)
            {
                songPlaying = (songPlaying + skipAmount) % sc.soundsToPlay.Count;
                if (songPlaying < 0) songPlaying += sc.soundsToPlay.Count;
                skipSongs = false;
                skipAmount = 0;
            }


            switch (sc.playMethod)
            {
                case PlayMethod.ContinuousPlayThrough:
                    while (Application.isPlaying)
                    {
                        modifiedCrossDuration = crossDuration;
                        currentSongIndex = songPlaying;
                        _PlayClip(sc.soundsToPlay[songPlaying], sc.baseVolumes[songPlaying]);
                        movingOnFromSong = false;

                        // While the clip is playing, wait until the time left is less than the cross duration
                        currentSource = IsPlaying(sc.soundsToPlay[songPlaying], false);
                        if (currentSource != null)
                            while (ignoreFromLosingFocus || (currentSource.isPlaying || isPaused || currentSource.mute) && ((((currentSource.clip.samples - currentSource.timeSamples) * 1f) / (currentSource.clip.frequency * 1f)) > modifiedCrossDuration))
                            {
                                if (skipSongs)
                                    break;
                                yield return null;
                            }

                        // Then go to the next song.
                        movingOnFromSong = true;
                        if (!skipSongs)
                            songPlaying = (songPlaying + 1) % sc.soundsToPlay.Count;
                        else
                        {
                            songPlaying = (songPlaying + skipAmount) % sc.soundsToPlay.Count;
                            if (songPlaying < 0) songPlaying += sc.soundsToPlay.Count;
                            skipSongs = false;
                            skipAmount = 0;
                        }
                    }
                    break;
                case PlayMethod.ContinuousPlayThroughWithDelay:
                    while (Application.isPlaying)
                    {
                        modifiedCrossDuration = crossDuration;
                        currentSongIndex = songPlaying;
                        _PlayClip(sc.soundsToPlay[songPlaying], sc.baseVolumes[songPlaying]);
                        movingOnFromSong = false;

                        // While the clip is playing, wait until the song is done before moving on with the delay
                        currentSource = IsPlaying(sc.soundsToPlay[songPlaying], false);
                        if (currentSource != null)
                            while (ignoreFromLosingFocus || (currentSource.isPlaying || isPaused || currentSource.mute))
                            {
                                if (skipSongs)
                                    break;
                                yield return null;
                            }

                        // Then go to the next song.
                        movingOnFromSong = true;
                        yield return new WaitForSeconds(sc.delay);
                        if (!skipSongs)
                            songPlaying = (songPlaying + 1) % sc.soundsToPlay.Count;
                        else
                        {
                            songPlaying = (songPlaying + skipAmount) % sc.soundsToPlay.Count;
                            if (songPlaying < 0) songPlaying += sc.soundsToPlay.Count;
                            skipSongs = false;
                            skipAmount = 0;
                        }
                    }
                    break;
                case PlayMethod.ContinuousPlayThroughWithRandomDelayInRange:
                    while (Application.isPlaying)
                    {
                        modifiedCrossDuration = crossDuration;
                        currentSongIndex = songPlaying;
                        _PlayClip(sc.soundsToPlay[songPlaying], sc.baseVolumes[songPlaying]);
                        movingOnFromSong = false;

                        // While the clip is playing, wait until the song is done before moving on with the delay
                        currentSource = IsPlaying(sc.soundsToPlay[songPlaying], false);
                        if (currentSource != null)
                            while (ignoreFromLosingFocus || (currentSource.isPlaying || isPaused || currentSource.mute))
                            {
                                if (skipSongs)
                                    break;
                                yield return null;
                            }

                        // Then go to the next song.
                        movingOnFromSong = true;
                        float randomDelay = Random.Range(sc.minDelay, sc.maxDelay);
                        yield return new WaitForSeconds(randomDelay);
                        if (!skipSongs)
                            songPlaying = (songPlaying + 1) % sc.soundsToPlay.Count;
                        else
                        {
                            songPlaying = (songPlaying + skipAmount) % sc.soundsToPlay.Count;
                            if (songPlaying < 0) songPlaying += sc.soundsToPlay.Count;
                            skipSongs = false;
                            skipAmount = 0;
                        }
                    }
                    break;
                case PlayMethod.OncePlayThrough:
                    while (songPlaying < sc.soundsToPlay.Count)
                    {
                        modifiedCrossDuration = crossDuration;
                        currentSongIndex = songPlaying;
                        _PlayClip(sc.soundsToPlay[songPlaying], sc.baseVolumes[songPlaying]);
                        movingOnFromSong = false;

                        // While the clip is playing, wait until the time left is less than the cross duration
                        currentSource = IsPlaying(sc.soundsToPlay[songPlaying], false);
                        if (currentSource != null)
                            while (ignoreFromLosingFocus || (currentSource.isPlaying || isPaused || currentSource.mute) && ((((currentSource.clip.samples - currentSource.timeSamples) * 1f) / (currentSource.clip.frequency * 1f)) > modifiedCrossDuration))
                            {
                                if (skipSongs)
                                    break;
                                yield return null;
                            }

                        // Then go to the next song.
                        movingOnFromSong = true;
                        if (!skipSongs)
                            songPlaying++;
                        else
                        {
                            songPlaying = (songPlaying + skipAmount);
                            if (songPlaying < 0) songPlaying = 0;
                            skipSongs = false;
                            skipAmount = 0;
                        }
                        if (sc.soundsToPlay.Count <= songPlaying)
                        {
                            CoroutineCrossoutAll = StartCoroutine(CrossoutAll(crossDuration));
                        }
                    }
                    break;
                case PlayMethod.OncePlayThroughWithDelay:
                    while (songPlaying < sc.soundsToPlay.Count)
                    {
                        modifiedCrossDuration = crossDuration;
                        currentSongIndex = songPlaying;
                        _PlayClip(sc.soundsToPlay[songPlaying], sc.baseVolumes[songPlaying]);
                        movingOnFromSong = false;

                        // While the clip is playing, wait until the song is done before moving on with the delay
                        currentSource = IsPlaying(sc.soundsToPlay[songPlaying], false);
                        if (currentSource != null)
                            while (ignoreFromLosingFocus || (currentSource.isPlaying || isPaused || currentSource.mute))
                            {
                                if (skipSongs)
                                    break;
                                yield return null;
                            }

                        // Then go to the next song.
                        movingOnFromSong = true;
                        yield return new WaitForSeconds(sc.delay);
                        if (!skipSongs)
                            songPlaying++;
                        else
                        {
                            songPlaying = (songPlaying + skipAmount);
                            if (songPlaying < 0) songPlaying = 0;
                            skipSongs = false;
                            skipAmount = 0;
                        }
                        if (sc.soundsToPlay.Count <= songPlaying)
                        {
                            CoroutineCrossoutAll = StartCoroutine(CrossoutAll(crossDuration));
                        }
                    }
                    break;
                case PlayMethod.OncePlayThroughWithRandomDelayInRange:
                    while (songPlaying < sc.soundsToPlay.Count)
                    {
                        modifiedCrossDuration = crossDuration;
                        currentSongIndex = songPlaying;
                        _PlayClip(sc.soundsToPlay[songPlaying], sc.baseVolumes[songPlaying]);
                        movingOnFromSong = false;

                        // While the clip is playing, wait until the song is done before moving on with the delay
                        currentSource = IsPlaying(sc.soundsToPlay[songPlaying], false);
                        if (currentSource != null)
                            while (ignoreFromLosingFocus || (currentSource.isPlaying || isPaused || currentSource.mute))
                            {
                                if (skipSongs)
                                    break;
                                yield return null;
                            }

                        // Then go to the next song.
                        movingOnFromSong = true;
                        float randomDelay = Random.Range(sc.minDelay, sc.maxDelay);
                        yield return new WaitForSeconds(randomDelay);
                        if (!skipSongs)
                            songPlaying++;
                        else
                        {
                            songPlaying = (songPlaying + skipAmount);
                            if (songPlaying < 0) songPlaying = 0;
                            skipSongs = false;
                            skipAmount = 0;
                        }
                        if (sc.soundsToPlay.Count <= songPlaying)
                        {
                            CoroutineCrossoutAll = StartCoroutine(CrossoutAll(crossDuration));
                        }
                    }
                    break;
                case PlayMethod.ShufflePlayThrough:
                    SoundManagerTools.ShuffleTwo<AudioClip, float>(ref sc.soundsToPlay, ref sc.baseVolumes);
                    while (Application.isPlaying)
                    {
                        modifiedCrossDuration = crossDuration;
                        currentSongIndex = songPlaying;
                        _PlayClip(sc.soundsToPlay[songPlaying], sc.baseVolumes[songPlaying]);
                        movingOnFromSong = false;

                        // While the clip is playing, wait until the time left is less than the cross duration
                        currentSource = IsPlaying(sc.soundsToPlay[songPlaying], false);
                        if (currentSource != null)
                            while (ignoreFromLosingFocus || (currentSource.isPlaying || isPaused || currentSource.mute) && ((((currentSource.clip.samples - currentSource.timeSamples) * 1f) / (currentSource.clip.frequency * 1f)) > modifiedCrossDuration))
                            {
                                if (skipSongs)
                                    break;
                                yield return null;
                            }

                        // Then go to the next song.
                        movingOnFromSong = true;
                        if (!skipSongs)
                            songPlaying = (songPlaying + 1) % sc.soundsToPlay.Count;
                        else
                        {
                            songPlaying = (songPlaying + skipAmount) % sc.soundsToPlay.Count;
                            if (songPlaying < 0) songPlaying += sc.soundsToPlay.Count;
                            skipSongs = false;
                            skipAmount = 0;
                        }
                        if (songPlaying == 0)
                            SoundManagerTools.ShuffleTwo<AudioClip, float>(ref sc.soundsToPlay, ref sc.baseVolumes);
                    }
                    break;
                case PlayMethod.ShufflePlayThroughWithDelay:
                    SoundManagerTools.ShuffleTwo<AudioClip, float>(ref sc.soundsToPlay, ref sc.baseVolumes);
                    while (Application.isPlaying)
                    {
                        modifiedCrossDuration = crossDuration;
                        currentSongIndex = songPlaying;
                        _PlayClip(sc.soundsToPlay[songPlaying], sc.baseVolumes[songPlaying]);
                        movingOnFromSong = false;

                        // While the clip is playing, wait until the song is done before moving on with the delay
                        currentSource = IsPlaying(sc.soundsToPlay[songPlaying], false);
                        if (currentSource != null)
                            while (ignoreFromLosingFocus || (currentSource.isPlaying || isPaused || currentSource.mute))
                            {
                                if (skipSongs)
                                    break;
                                yield return null;
                            }

                        // Then go to the next song.
                        movingOnFromSong = true;
                        yield return new WaitForSeconds(sc.delay);
                        if (!skipSongs)
                            songPlaying = (songPlaying + 1) % sc.soundsToPlay.Count;
                        else
                        {
                            songPlaying = (songPlaying + skipAmount) % sc.soundsToPlay.Count;
                            if (songPlaying < 0) songPlaying += sc.soundsToPlay.Count;
                            skipSongs = false;
                            skipAmount = 0;
                        }
                        if (songPlaying == 0)
                            SoundManagerTools.ShuffleTwo<AudioClip, float>(ref sc.soundsToPlay, ref sc.baseVolumes);
                    }
                    break;
                case PlayMethod.ShufflePlayThroughWithRandomDelayInRange:
                    SoundManagerTools.ShuffleTwo<AudioClip, float>(ref sc.soundsToPlay, ref sc.baseVolumes);
                    while (Application.isPlaying)
                    {
                        modifiedCrossDuration = crossDuration;
                        currentSongIndex = songPlaying;
                        _PlayClip(sc.soundsToPlay[songPlaying], sc.baseVolumes[songPlaying]);
                        movingOnFromSong = false;

                        // While the clip is playing, wait until the song is done before moving on with the delay
                        currentSource = IsPlaying(sc.soundsToPlay[songPlaying], false);
                        if (currentSource != null)
                            while (ignoreFromLosingFocus || (currentSource.isPlaying || isPaused || currentSource.mute))
                            {
                                if (skipSongs)
                                    break;
                                yield return null;
                            }

                        // Then go to the next song.
                        movingOnFromSong = true;
                        float randomDelay = Random.Range(sc.minDelay, sc.maxDelay);
                        yield return new WaitForSeconds(randomDelay);
                        if (!skipSongs)
                            songPlaying = (songPlaying + 1) % sc.soundsToPlay.Count;
                        else
                        {
                            songPlaying = (songPlaying + skipAmount) % sc.soundsToPlay.Count;
                            if (songPlaying < 0) songPlaying += sc.soundsToPlay.Count;
                            skipSongs = false;
                            skipAmount = 0;
                        }
                        if (songPlaying == 0)
                            SoundManagerTools.ShuffleTwo(ref sc.soundsToPlay, ref sc.baseVolumes);
                    }
                    break;
                default:
                    Debug.LogError("This SoundConnection has an invalid PlayMethod.");
                    break;
            }
            yield break;
        }
        private void StopAllNonSoundConnectionCoroutines()
        {
            if (CoroutineCrossfade != null) StopCoroutine(CoroutineCrossfade);
            if (CoroutineCrossout != null) StopCoroutine(CoroutineCrossout);
            if (CoroutineCrossin != null) StopCoroutine(CoroutineCrossin);
            if (CoroutineCrossoutAll != null) StopCoroutine(CoroutineCrossoutAll);
        }
        private void StopPreviousPlaySoundConnection()
        {
            if (CoroutinePlaySoundConnection != null) StopCoroutine(CoroutinePlaySoundConnection);
        }
        #endregion

        #region is playing
        //�ж���Ƶ�Ƿ��ڲ���
        private AudioSource IsPlaying(AudioClip clip)
        {
            return IsPlaying(clip, true);
        }
        private AudioSource IsPlaying(AudioClip clip, bool regardlessOfCrossOut)
        {
            for (int i = 0; i < audios.Length; i++)
                if ((audios[i].isPlaying && audios[i].clip == clip) && (regardlessOfCrossOut || !outCrossing[i]))
                    return audios[i];
            return null;
        }
        private bool IsPlaying()
        {
            for (int i = 0; i < audios.Length; i++)
                if (audios[i].isPlaying)
                    return true;
            return false;
        }
        #endregion

        #region get
        private int GetAudioSourceIndex(AudioSource source)
        {
            for (int i = 0; i < audios.Length; i++)
                if (source == audios[i])
                    return i;
            return SOUNDMANAGER_FALSE;
        }

        private int GetActualCurrentPlayingIndex()
        {
            if (audios[0].isPlaying && audios[1].isPlaying)
            {
                //Both are playing, so figure out whos going to be playing when its over.
                if (inCrossing[0] && inCrossing[1])
                    return SOUNDMANAGER_FALSE; //not possible!
                else if (inCrossing[0])
                    return 0;
                else if (inCrossing[1])
                    return 1;
                else
                    return SOUNDMANAGER_FALSE; // not possible!
            }
            for (int i = 0; i < audios.Length; i++)
                if (audios[i].isPlaying)
                    return i;
            return SOUNDMANAGER_FALSE;
        }
        #endregion

        #region set
        private void SetNextSongInQueue()
        {
            if (currentSongIndex == -1)
                return;
            if (currentSoundConnection == null || currentSoundConnection.soundsToPlay == null || currentSoundConnection.soundsToPlay.Count <= 0)
                return;
            int nextSongPlaying = (currentSongIndex + 1) % currentSoundConnection.soundsToPlay.Count;
            AudioClip nextSong = currentSoundConnection.soundsToPlay[nextSongPlaying];

            int notPlaying = CheckWhosNotPlaying();
            if (notPlaying >= audios.Length)
                return;
            if (audios[notPlaying] == null || (audios[notPlaying].clip != null && !audios[notPlaying].clip.Equals(nextSong)))
                audios[notPlaying].clip = nextSong;
        }
        #endregion
    }

}