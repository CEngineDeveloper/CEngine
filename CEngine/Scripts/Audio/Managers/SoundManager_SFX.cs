using UnityEngine;
namespace CYM.Audio
{

    public partial class SoundManager : MonoSingleton<SoundManager>
    {

        /// <summary>
        /// Sets the SFX cap.
        /// </summary>
        /// <param name='cap'>
        /// Cap.
        /// </param>
        public static void SetSFXCap(int cap)
        {
            Ins.capAmount = cap;
        }

        /// <summary>
        /// Plays the SFX on an owned & pooled object, will default the location to (0,0,0), pitch to SoundManager.Ins.pitchSFX, volume to SoundManager.Ins.volumeSFX
        /// </summary>
        /// <returns>
        /// The resulting <a href="http://docs.unity3d.com/ScriptReference/AudioSource.html">AudioSource</a>.
        /// </returns>
        /// <param name='clip'>
        /// Clip.
        /// </param>
        /// <param name='looping'>
        /// Whether it is looping.
        /// </param>
        /// <param name='delay'>
        /// Delay.
        /// </param>
        /// <param name='volume'>
        /// Volume. If set to float.MaxValue, it will become the default volume currently set.
        /// </param>
        /// <param name='pitch'>
        /// Pitch. If set to float.MaxValue, it will become the default pitch currently set.
        /// </param>
        /// <param name='location'>
        /// Location.
        /// </param>
        /// <param name='runOnEndFunction'>
        /// Run on end function.
        /// </param>
        /// <param name='duckingSetting'>
        /// Ducking setting.
        /// </param>
        /// <param name='duckVolume'>
        /// Duck volume.
        /// </param>
        /// <param name='duckPitch'>
        /// Duck pitch.
        /// </param>
        public static AudioSource PlaySFX(AudioClip clip, bool looping = false, float delay = 0f, float volume = float.MaxValue, float pitch = float.MaxValue, Vector3 location = default(Vector3), SongCallBack runOnEndFunction = null, SoundDuckingSetting duckingSetting = SoundDuckingSetting.DoNotDuck, float duckVolume = 0f, float duckPitch = 1f)
        {
            if (Ins.offTheSFX || Ins.isPaused)
                return null;

            if (clip == null)
                return null;

            if (volume == float.MaxValue)
                volume = Ins.volumeSFX;

            if (pitch == float.MaxValue)
                pitch = Ins.pitchSFX;

            return Ins.PlaySFXAt(clip, volume, pitch, location, false, "", looping, delay, runOnEndFunction, duckingSetting, duckVolume, duckPitch);
        }

        /// <summary>
        /// Plays the SFX on an owned & pooled object by clipname reference on the SoundManager, will default the location to (0,0,0), pitch to SoundManager.Ins.pitchSFX, volume to SoundManager.Ins.volumeSFX
        /// </summary>
        /// <returns>
        /// The resulting <a href="http://docs.unity3d.com/ScriptReference/AudioSource.html">AudioSource</a>.
        /// </returns>
        /// <param name='clipName'>
        /// Name of the clip on the SoundManager.
        /// </param>
        /// <param name='looping'>
        /// Whether it is looping.
        /// </param>
        /// <param name='delay'>
        /// Delay.
        /// </param>
        /// <param name='volume'>
        /// Volume. If set to float.MaxValue, it will become the default volume currently set.
        /// </param>
        /// <param name='pitch'>
        /// Pitch. If set to float.MaxValue, it will become the default pitch currently set.
        /// </param>
        /// <param name='location'>
        /// Location.
        /// </param>
        /// <param name='runOnEndFunction'>
        /// Run on end function.
        /// </param>
        /// <param name='duckingSetting'>
        /// Ducking setting.
        /// </param>
        /// <param name='duckVolume'>
        /// Duck volume.
        /// </param>
        /// <param name='duckPitch'>
        /// Duck pitch.
        /// </param>
        public static AudioSource PlaySFX(string clipName, bool looping = false, float delay = 0f, float volume = float.MaxValue, float pitch = float.MaxValue, Vector3 location = default(Vector3), SongCallBack runOnEndFunction = null, SoundDuckingSetting duckingSetting = SoundDuckingSetting.DoNotDuck, float duckVolume = 0f, float duckPitch = 1f)
        {
            if (Ins.offTheSFX || Ins.isPaused)
                return null;

            if (!SoundManager.ClipNameIsValid(clipName))
                return null;

            if (volume == float.MaxValue)
                volume = Ins.volumeSFX;

            if (pitch == float.MaxValue)
                pitch = Ins.pitchSFX;

            return Ins.PlaySFXAt(SoundManager.Load(clipName), volume, pitch, location, false, "", looping, delay, runOnEndFunction, duckingSetting, duckVolume, duckPitch);
        }

        /// <summary>
        /// Plays the SFX IFF other SFX with the same cappedID are not over the cap limit. Will default the location to (0,0,0), pitch to SoundManager.Ins.pitchSFX, volume to SoundManager.Ins.volumeSFX
        /// </summary>
        /// <returns>
        /// The resulting <a href="http://docs.unity3d.com/ScriptReference/AudioSource.html">AudioSource</a>.
        /// </returns>
        /// <param name='clip'>
        /// Clip.
        /// </param>
        /// <param name='cappedID'>
        /// Capped ID.
        /// </param>
        /// <param name='volume'>
        /// Volume.
        /// </param>
        /// <param name='pitch'>
        /// Pitch.
        /// </param>
        /// <param name='location'>
        /// Location.
        /// </param>
        public static AudioSource PlayCappedSFX(AudioClip clip, string cappedID, float volume = float.MaxValue, float pitch = float.MaxValue, Vector3 location = default(Vector3))
        {
            if (Ins.offTheSFX || Ins.isPaused)
                return null;

            if (clip == null)
                return null;

            if (string.IsNullOrEmpty(cappedID))
                return null;

            if (volume == float.MaxValue)
                volume = Ins.volumeSFX;

            if (pitch == float.MaxValue)
                pitch = Ins.pitchSFX;

            // Play the clip if not at capacity
            if (!Ins.IsAtCapacity(cappedID, clip.name))
                return Ins.PlaySFXAt(clip, volume, pitch, location, true, cappedID);
            else
                return null;
        }

        /// <summary>
        /// Plays the SFX IFF other SFX with the same cappedID are not over the cap limit. Will default the location to (0,0,0), pitch to SoundManager.Ins.pitchSFX, volume to SoundManager.Ins.volumeSFX
        /// </summary>
        /// <returns>
        /// The resulting <a href="http://docs.unity3d.com/ScriptReference/AudioSource.html">AudioSource</a>.
        /// </returns>
        /// <param name='clipName'>
        /// Clip name.
        /// </param>
        /// <param name='cappedID'>
        /// Capped ID.
        /// </param>
        /// <param name='volume'>
        /// Volume.
        /// </param>
        /// <param name='pitch'>
        /// Pitch.
        /// </param>
        /// <param name='location'>
        /// Location.
        /// </param>
        public static AudioSource PlayCappedSFX(string clipName, string cappedID, float volume = float.MaxValue, float pitch = float.MaxValue, Vector3 location = default(Vector3))
        {
            if (Ins.offTheSFX || Ins.isPaused)
                return null;

            if (!SoundManager.ClipNameIsValid(clipName))
                return null;

            if (string.IsNullOrEmpty(cappedID))
                return null;

            if (volume == float.MaxValue)
                volume = Ins.volumeSFX;

            if (pitch == float.MaxValue)
                pitch = Ins.pitchSFX;

            // Play the clip if not at capacity
            if (!Ins.IsAtCapacity(cappedID, clipName))
                return Ins.PlaySFXAt(SoundManager.Load(clipName), volume, pitch, location, true, cappedID);
            else
                return null;
        }

        /// <summary>
        /// Plays the SFX IFF other SFX with the same cappedID are not over the cap limit. Will default the pitch to SoundManager.Ins.pitchSFX, volume to SoundManager.Ins.volumeSFX
        /// </summary>
        /// <returns>
        /// The resulting <a href="http://docs.unity3d.com/ScriptReference/AudioSource.html">AudioSource</a>.
        /// </returns>
        /// <param name='aS'>
        /// <a href="http://docs.unity3d.com/ScriptReference/AudioSource.html">AudioSource</a> to play on.
        /// </param>
        /// <param name='clip'>
        /// Clip.
        /// </param>
        /// <param name='cappedID'>
        /// Capped ID.
        /// </param>
        /// <param name='volume'>
        /// Volume.
        /// </param>
        /// <param name='pitch'>
        /// Pitch.
        /// </param>
        public static AudioSource PlayCappedSFX(AudioSource aS, AudioClip clip, string cappedID, float volume = float.MaxValue, float pitch = float.MaxValue)
        {
            if (Ins.offTheSFX || Ins.isPaused)
                return null;

            if (clip == null || aS == null)
                return null;

            if (string.IsNullOrEmpty(cappedID))
                return null;

            if (volume == float.MaxValue)
                volume = Ins.volumeSFX;

            if (pitch == float.MaxValue)
                pitch = Ins.pitchSFX;

            // Play the clip if not at capacity
            if (!Ins.IsAtCapacity(cappedID, clip.name))
            {
                // Keep reference of unownedsfx objects
                Ins.CheckInsertionIntoUnownedSFXObjects(aS);

                return Ins.PlaySFXOn(aS, clip, volume, pitch, true, cappedID);
            }
            else
                return null;
        }

        /// <summary>
        /// Plays the SFX IFF other SFX with the same cappedID are not over the cap limit. Will default the pitch to SoundManager.Ins.pitchSFX, volume to SoundManager.Ins.volumeSFX
        /// </summary>
        /// <returns>
        /// The resulting <a href="http://docs.unity3d.com/ScriptReference/AudioSource.html">AudioSource</a>.
        /// </returns>
        /// <param name='aS'>
        /// <a href="http://docs.unity3d.com/ScriptReference/AudioSource.html">AudioSource</a> to play on.
        /// </param>
        /// <param name='clipName'>
        /// Clip name.
        /// </param>
        /// <param name='cappedID'>
        /// Capped ID.
        /// </param>
        /// <param name='volume'>
        /// Volume.
        /// </param>
        /// <param name='pitch'>
        /// Pitch.
        /// </param>
        public static AudioSource PlayCappedSFX(AudioSource aS, string clipName, string cappedID, float volume = float.MaxValue, float pitch = float.MaxValue)
        {
            if (Ins.offTheSFX || Ins.isPaused)
                return null;

            if (!SoundManager.ClipNameIsValid(clipName) || aS == null)
                return null;

            if (string.IsNullOrEmpty(cappedID))
                return null;

            if (volume == float.MaxValue)
                volume = Ins.volumeSFX;

            if (pitch == float.MaxValue)
                pitch = Ins.pitchSFX;

            // Play the clip if not at capacity
            if (!Ins.IsAtCapacity(cappedID, clipName))
            {
                // Keep reference of unownedsfx objects
                Ins.CheckInsertionIntoUnownedSFXObjects(aS);

                return Ins.PlaySFXOn(aS, SoundManager.Load(clipName), volume, pitch, true, cappedID);
            }
            else
                return null;
        }

        /// <summary>
        /// Plays the SFX another audiosource of your choice, will default the looping to false, pitch to SoundManager.Ins.pitchSFX, volume to SoundManager.Ins.volumeSFX
        /// </summary>
        /// <returns>
        /// The resulting <a href="http://docs.unity3d.com/ScriptReference/AudioSource.html">AudioSource</a>.
        /// </returns>
        /// <param name='aS'>
        /// <a href="http://docs.unity3d.com/ScriptReference/AudioSource.html">AudioSource</a> to play on.
        /// </param>
        /// <param name='clip'>
        /// Clip.
        /// </param>
        /// <param name='looping'>
        /// Looping.
        /// </param>
        /// <param name='delay'>
        /// Delay.
        /// </param>
        /// <param name='volume'>
        /// Volume.
        /// </param>
        /// <param name='pitch'>
        /// Pitch.
        /// </param>
        /// <param name='runOnEndFunction'>
        /// Run on end function.
        /// </param>
        /// <param name='duckingSetting'>
        /// Ducking setting.
        /// </param>
        /// <param name='duckVolume'>
        /// Duck volume.
        /// </param>
        /// <param name='duckPitch'>
        /// Duck pitch.
        /// </param>
        public static AudioSource PlaySFX(AudioSource aS, AudioClip clip, bool looping = false, float delay = 0f, float volume = float.MaxValue, float pitch = float.MaxValue, SongCallBack runOnEndFunction = null, SoundDuckingSetting duckingSetting = SoundDuckingSetting.DoNotDuck, float duckVolume = 0f, float duckPitch = 1f)
        {
            if (Ins.offTheSFX || Ins.isPaused)
                return null;

            if ((clip == null) || (aS == null))
                return null;

            if (volume == float.MaxValue)
                volume = Ins.volumeSFX;

            if (pitch == float.MaxValue)
                pitch = Ins.pitchSFX;

            // Keep reference of unownedsfx objects
            Ins.CheckInsertionIntoUnownedSFXObjects(aS);

            return Ins.PlaySFXOn(aS, clip, volume, pitch, false, "", looping, delay, runOnEndFunction, duckingSetting, duckVolume, duckPitch);
        }

        /// <summary>
        /// Plays the SFX another audiosource of your choice, will default the looping to false, pitch to SoundManager.Ins.pitchSFX, volume to SoundManager.Ins.volumeSFX
        /// </summary>
        /// <returns>
        /// The resulting <a href="http://docs.unity3d.com/ScriptReference/AudioSource.html">AudioSource</a>.
        /// </returns>
        /// <param name='aS'>
        /// <a href="http://docs.unity3d.com/ScriptReference/AudioSource.html">AudioSource</a> to play on.
        /// </param>
        /// <param name='clipName'>
        /// Clip name.
        /// </param>
        /// <param name='looping'>
        /// Looping.
        /// </param>
        /// <param name='delay'>
        /// Delay.
        /// </param>
        /// <param name='volume'>
        /// Volume.
        /// </param>
        /// <param name='pitch'>
        /// Pitch.
        /// </param>
        /// <param name='runOnEndFunction'>
        /// Run on end function.
        /// </param>
        /// <param name='duckingSetting'>
        /// Ducking setting.
        /// </param>
        /// <param name='duckVolume'>
        /// Duck volume.
        /// </param>
        /// <param name='duckPitch'>
        /// Duck pitch.
        /// </param>
        public static AudioSource PlaySFX(AudioSource aS, string clipName, bool looping = false, float delay = 0f, float volume = float.MaxValue, float pitch = float.MaxValue, SongCallBack runOnEndFunction = null, SoundDuckingSetting duckingSetting = SoundDuckingSetting.DoNotDuck, float duckVolume = 0f, float duckPitch = 1f)
        {
            if (Ins.offTheSFX || Ins.isPaused)
                return null;

            if ((!SoundManager.ClipNameIsValid(clipName)) || (aS == null))
                return null;

            if (volume == float.MaxValue)
                volume = Ins.volumeSFX;

            if (pitch == float.MaxValue)
                pitch = Ins.pitchSFX;

            // Keep reference of unownedsfx objects
            Ins.CheckInsertionIntoUnownedSFXObjects(aS);

            return Ins.PlaySFXOn(aS, SoundManager.Load(clipName), volume, pitch, false, "", looping, delay, runOnEndFunction, duckingSetting, duckVolume, duckPitch);
        }

        /// <summary>
        /// Stops the SFX on another audiosource
        /// </summary>
        /// <param name='aS'>
        /// <a href="http://docs.unity3d.com/ScriptReference/AudioSource.html">AudioSource</a> to stop.
        /// </param>
        public static void StopSFXObject(AudioSource aS)
        {
            if (aS == null)
                return;

            if (aS.isPlaying)
                aS.Stop();

            if (Ins.delayedAudioSources.ContainsKey(aS))
                Ins.delayedAudioSources.Remove(aS);
        }

        /// <summary>
        /// Plays the SFX another gameObject of your choice, will default the looping to false, pitch to SoundManager.Ins.pitchSFX, volume to SoundManager.Ins.volumeSFX
        /// </summary>
        /// <returns>
        /// The resulting <a href="http://docs.unity3d.com/ScriptReference/AudioSource.html">AudioSource</a>.
        /// </returns>
        /// <param name='gO'>
        /// GameObject to play on.
        /// </param>
        /// <param name='clip'>
        /// Clip.
        /// </param>
        /// <param name='looping'>
        /// Looping.
        /// </param>
        /// <param name='delay'>
        /// Delay.
        /// </param>
        /// <param name='volume'>
        /// Volume.
        /// </param>
        /// <param name='pitch'>
        /// Pitch.
        /// </param>
        /// <param name='runOnEndFunction'>
        /// Run on end function.
        /// </param>
        /// <param name='duckingSetting'>
        /// Ducking setting.
        /// </param>
        /// <param name='duckVolume'>
        /// Duck volume.
        /// </param>
        /// <param name='duckPitch'>
        /// Duck pitch.
        /// </param>
        public static AudioSource PlaySFX(GameObject gO, AudioClip clip, bool looping = false, float delay = 0f, float volume = float.MaxValue, float pitch = float.MaxValue, SongCallBack runOnEndFunction = null, SoundDuckingSetting duckingSetting = SoundDuckingSetting.DoNotDuck, float duckVolume = 0f, float duckPitch = 1f)
        {
            if (Ins.offTheSFX || Ins.isPaused)
                return null;

            if ((clip == null) || (gO == null))
                return null;

            if (gO.GetComponent<AudioSource>() == null)
                gO.AddComponent<AudioSource>();

            if (volume == float.MaxValue)
                volume = Ins.volumeSFX;

            if (pitch == float.MaxValue)
                pitch = Ins.pitchSFX;

            return PlaySFX(gO.GetComponent<AudioSource>(), clip, looping, delay, volume, pitch, runOnEndFunction, duckingSetting, duckVolume, duckPitch);
        }

        /// <summary>
        /// Plays the SFX another gameObject of your choice, will default the looping to false, pitch to SoundManager.Ins.pitchSFX, volume to SoundManager.Ins.volumeSFX
        /// </summary>
        /// <returns>
        /// The resulting <a href="http://docs.unity3d.com/ScriptReference/AudioSource.html">AudioSource</a>.
        /// </returns>
        /// <param name='gO'>
        /// GameObject to play on.
        /// </param>
        /// <param name='clipName'>
        /// Clip name.
        /// </param>
        /// <param name='looping'>
        /// Looping.
        /// </param>
        /// <param name='delay'>
        /// Delay.
        /// </param>
        /// <param name='volume'>
        /// Volume.
        /// </param>
        /// <param name='pitch'>
        /// Pitch.
        /// </param>
        /// <param name='runOnEndFunction'>
        /// Run on end function.
        /// </param>
        /// <param name='duckingSetting'>
        /// Ducking setting.
        /// </param>
        /// <param name='duckVolume'>
        /// Duck volume.
        /// </param>
        /// <param name='duckPitch'>
        /// Duck pitch.
        /// </param>
        public static AudioSource PlaySFX(GameObject gO, string clipName, bool looping = false, float delay = 0f, float volume = float.MaxValue, float pitch = float.MaxValue, SongCallBack runOnEndFunction = null, SoundDuckingSetting duckingSetting = SoundDuckingSetting.DoNotDuck, float duckVolume = 0f, float duckPitch = 1f)
        {
            if (Ins.offTheSFX || Ins.isPaused)
                return null;

            if ((!SoundManager.ClipNameIsValid(clipName)) || (gO == null))
                return null;

            if (gO.GetComponent<AudioSource>() == null)
                gO.AddComponent<AudioSource>();

            if (volume == float.MaxValue)
                volume = Ins.volumeSFX;

            if (pitch == float.MaxValue)
                pitch = Ins.pitchSFX;

            return PlaySFX(gO.GetComponent<AudioSource>(), SoundManager.Load(clipName), looping, delay, volume, pitch, runOnEndFunction, duckingSetting, duckVolume, duckPitch);
        }

        /// <summary>
        /// Stops the SFX on another gameObject
        /// </summary>
        /// <param name='gO'>
        /// GameObject to stop.
        /// </param>
        public static void StopSFXObject(GameObject gO)
        {
            if (gO == null)
                return;

            StopSFXObject(gO.GetComponent<AudioSource>());
        }

        /// <summary>
        /// Stops all SFX.
        /// </summary>
        public static void StopSFX()
        {
            Ins._StopSFX();
        }

        /// <summary>
        /// Plays the SFX in a loop on another audiosource of your choice.  This function is cattered more towards customizing a loop.
        /// You can set the loop to end when the object dies or a maximum duration, whichever comes first.
        /// tillDestroy defaults to true, pitch to SoundManager.Ins.pitchSFX, volume to SoundManager.Ins.volumeSFX, maxDuration to 0f
        /// </summary>
        /// <returns>
        /// The resulting <a href="http://docs.unity3d.com/ScriptReference/AudioSource.html">AudioSource</a>.
        /// </returns>
        /// <param name='aS'>
        /// <a href="http://docs.unity3d.com/ScriptReference/AudioSource.html">AudioSource</a> to play on.
        /// </param>
        /// <param name='clip'>
        /// Clip.
        /// </param>
        /// <param name='tillDestroy'>
        /// Till destroyed?
        /// </param>
        /// <param name='volume'>
        /// Volume.
        /// </param>
        /// <param name='pitch'>
        /// Pitch.
        /// </param>
        /// <param name='maxDuration'>
        /// Max duration.
        /// </param>
        /// <param name='runOnEndFunction'>
        /// Run on end function.
        /// </param>
        /// <param name='duckingSetting'>
        /// Ducking setting.
        /// </param>
        /// <param name='duckVolume'>
        /// Duck volume.
        /// </param>
        /// <param name='duckPitch'>
        /// Duck pitch.
        /// </param>
        public static AudioSource PlaySFXLoop(AudioSource aS, AudioClip clip, bool tillDestroy = true, float volume = float.MaxValue, float pitch = float.MaxValue, float maxDuration = 0f, SongCallBack runOnEndFunction = null, SoundDuckingSetting duckingSetting = SoundDuckingSetting.DoNotDuck, float duckVolume = 0f, float duckPitch = 1f)
        {
            if (Ins.offTheSFX || Ins.isPaused)
                return null;

            if ((clip == null) || (aS == null))
                return null;

            if (volume == float.MaxValue)
                volume = Ins.volumeSFX;

            if (pitch == float.MaxValue)
                pitch = Ins.pitchSFX;

            Ins.CheckInsertionIntoUnownedSFXObjects(aS);

            return Ins.PlaySFXLoopOn(aS, clip, tillDestroy, volume, pitch, maxDuration, runOnEndFunction, duckingSetting, duckVolume, duckPitch);
        }

        /// <summary>
        /// Plays the SFX in a loop on another audiosource of your choice.  This function is cattered more towards customizing a loop.
        /// You can set the loop to end when the object dies or a maximum duration, whichever comes first.
        /// tillDestroy defaults to true, pitch to SoundManager.Ins.pitchSFX, volume to SoundManager.Ins.volumeSFX, maxDuration to 0f
        /// </summary>
        /// <returns>
        /// The resulting <a href="http://docs.unity3d.com/ScriptReference/AudioSource.html">AudioSource</a>.
        /// </returns>
        /// <param name='aS'>
        /// <a href="http://docs.unity3d.com/ScriptReference/AudioSource.html">AudioSource</a> to play on.
        /// </param>
        /// <param name='clipName'>
        /// Clip name.
        /// </param>
        /// <param name='tillDestroy'>
        /// Till destroyed?
        /// </param>
        /// <param name='volume'>
        /// Volume.
        /// </param>
        /// <param name='pitch'>
        /// Pitch.
        /// </param>
        /// <param name='maxDuration'>
        /// Max duration.
        /// </param>
        /// <param name='runOnEndFunction'>
        /// Run on end function.
        /// </param>
        /// <param name='duckingSetting'>
        /// Ducking setting.
        /// </param>
        /// <param name='duckVolume'>
        /// Duck volume.
        /// </param>
        /// <param name='duckPitch'>
        /// Duck pitch.
        /// </param>
        public static AudioSource PlaySFXLoop(AudioSource aS, string clipName, bool tillDestroy = true, float volume = float.MaxValue, float pitch = float.MaxValue, float maxDuration = 0f, SongCallBack runOnEndFunction = null, SoundDuckingSetting duckingSetting = SoundDuckingSetting.DoNotDuck, float duckVolume = 0f, float duckPitch = 1f)
        {
            if (Ins.offTheSFX || Ins.isPaused)
                return null;

            if ((!SoundManager.ClipNameIsValid(clipName)) || (aS == null))
                return null;

            if (volume == float.MaxValue)
                volume = Ins.volumeSFX;

            if (pitch == float.MaxValue)
                pitch = Ins.pitchSFX;

            Ins.CheckInsertionIntoUnownedSFXObjects(aS);

            return Ins.PlaySFXLoopOn(aS, SoundManager.Load(clipName), tillDestroy, volume, pitch, maxDuration, runOnEndFunction, duckingSetting, duckVolume, duckPitch);
        }

        /// <summary>
        /// Plays the SFX in a loop on another gameObject of your choice.  This function is cattered more towards customizing a loop.
        /// You can set the loop to end when the object dies or a maximum duration, whichever comes first.
        /// tillDestroy defaults to true, pitch to SoundManager.Ins.pitchSFX, volume to SoundManager.Ins.volumeSFX, maxDuration to 0f
        /// </summary>
        /// <returns>
        /// The resulting <a href="http://docs.unity3d.com/ScriptReference/AudioSource.html">AudioSource</a>.
        /// </returns>
        /// <param name='gO'>
        /// GameObject to play on.
        /// </param>
        /// <param name='clip'>
        /// Clip.
        /// </param>
        /// <param name='tillDestroy'>
        /// Till destroyed?
        /// </param>
        /// <param name='volume'>
        /// Volume.
        /// </param>
        /// <param name='pitch'>
        /// Pitch.
        /// </param>
        /// <param name='maxDuration'>
        /// Max duration.
        /// </param>
        /// <param name='runOnEndFunction'>
        /// Run on end function.
        /// </param>
        /// <param name='duckingSetting'>
        /// Ducking setting.
        /// </param>
        /// <param name='duckVolume'>
        /// Duck volume.
        /// </param>
        /// <param name='duckPitch'>
        /// Duck pitch.
        /// </param>
        public static AudioSource PlaySFXLoop(GameObject gO, AudioClip clip, bool tillDestroy = true, float volume = float.MaxValue, float pitch = float.MaxValue, float maxDuration = 0f, SongCallBack runOnEndFunction = null, SoundDuckingSetting duckingSetting = SoundDuckingSetting.DoNotDuck, float duckVolume = 0f, float duckPitch = 1f)
        {
            if (Ins.offTheSFX || Ins.isPaused)
                return null;

            if ((clip == null) || (gO == null))
                return null;

            if (gO.GetComponent<AudioSource>() == null)
                gO.AddComponent<AudioSource>();

            if (volume == float.MaxValue)
                volume = Ins.volumeSFX;

            if (pitch == float.MaxValue)
                pitch = Ins.pitchSFX;

            Ins.CheckInsertionIntoUnownedSFXObjects(gO.GetComponent<AudioSource>());

            return Ins.PlaySFXLoopOn(gO.GetComponent<AudioSource>(), clip, tillDestroy, volume, pitch, maxDuration, runOnEndFunction, duckingSetting, duckVolume, duckPitch);
        }

        /// <summary>
        /// Plays the SFX in a loop on another gameObject of your choice.  This function is cattered more towards customizing a loop.
        /// You can set the loop to end when the object dies or a maximum duration, whichever comes first.
        /// tillDestroy defaults to true, pitch to SoundManager.Ins.pitchSFX, volume to SoundManager.Ins.volumeSFX, maxDuration to 0f
        /// </summary>
        /// <returns>
        /// The resulting <a href="http://docs.unity3d.com/ScriptReference/AudioSource.html">AudioSource</a>.
        /// </returns>
        /// <param name='gO'>
        /// GameObject to play on.
        /// </param>
        /// <param name='clipName'>
        /// Clip name.
        /// </param>
        /// <param name='tillDestroy'>
        /// Till destroyed?
        /// </param>
        /// <param name='volume'>
        /// Volume.
        /// </param>
        /// <param name='pitch'>
        /// Pitch.
        /// </param>
        /// <param name='maxDuration'>
        /// Max duration.
        /// </param>
        /// <param name='runOnEndFunction'>
        /// Run on end function.
        /// </param>
        /// <param name='duckingSetting'>
        /// Ducking setting.
        /// </param>
        /// <param name='duckVolume'>
        /// Duck volume.
        /// </param>
        /// <param name='duckPitch'>
        /// Duck pitch.
        /// </param>
        public static AudioSource PlaySFXLoop(GameObject gO, string clipName, bool tillDestroy = true, float volume = float.MaxValue, float pitch = float.MaxValue, float maxDuration = 0f, SongCallBack runOnEndFunction = null, SoundDuckingSetting duckingSetting = SoundDuckingSetting.DoNotDuck, float duckVolume = 0f, float duckPitch = 1f)
        {
            if (Ins.offTheSFX || Ins.isPaused)
                return null;

            if ((!SoundManager.ClipNameIsValid(clipName)) || (gO == null))
                return null;

            if (gO.GetComponent<AudioSource>() == null)
                gO.AddComponent<AudioSource>();

            if (volume == float.MaxValue)
                volume = Ins.volumeSFX;

            if (pitch == float.MaxValue)
                pitch = Ins.pitchSFX;

            Ins.CheckInsertionIntoUnownedSFXObjects(gO.GetComponent<AudioSource>());

            return Ins.PlaySFXLoopOn(gO.GetComponent<AudioSource>(), SoundManager.Load(clipName), tillDestroy, volume, pitch, maxDuration, runOnEndFunction, duckingSetting, duckVolume, duckPitch);
        }

        /// <summary>
        /// Sets mute on all the SFX to 'toggle' value. Returns the result.
        /// </summary>
        /// <returns>
        /// If SFX is NOW muted or not.
        /// </returns>
        /// <param name='toggle'>
        /// The mute to set.
        /// </param>
        public static bool MuteSFX(bool toggle)
        {
            Ins.mutedSFX = toggle;
            return Ins.mutedSFX;
        }

        /// <summary>
        /// Toggles mute on SFX. Returns the result.
        /// </summary>
        /// <returns>
        /// If the SFX is NOW muted or not.
        /// </returns>
        public static bool MuteSFX()
        {
            return MuteSFX(!Ins.mutedSFX);
        }

        /// <summary>
        /// Determines whether this Ins is SFX muted.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this Ins is SFX muted; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsSFXMuted()
        {
            return Ins.mutedSFX;
        }

        /// <summary>
        /// Sets the maximum volume of SFX in the game relative to the global volume.
        /// </summary>
        /// <param name='setVolume'>
        /// Set volume.
        /// </param>
        public static void SetVolumeSFX(float setVolume)
        {
            setVolume = Mathf.Clamp01(setVolume);

            float currentPercentageOfVolume;
            currentPercentageOfVolume = Ins.volumeSFX / Ins.maxSFXVolume;

            Ins.maxSFXVolume = setVolume * Ins.maxVolume;

            if (float.IsNaN(currentPercentageOfVolume) || float.IsInfinity(currentPercentageOfVolume))
                currentPercentageOfVolume = 1f;

            Ins.volumeSFX = Ins.maxSFXVolume * currentPercentageOfVolume;
        }
        /* COMING SOON
        public static void SetVolumeSFX(float setVolume, string groupName)
        {

        }
        */
        /// <summary>
        /// Sets the volume of a certain group of AudioSources. Can set to ignore the max SFX volume.
        /// </summary>
        /// <param name='setVolume'>
        /// Set volume.
        /// </param>
        /// <param name='ignoreMaxSFXVolume'>
        /// Ignore max SFX volume?
        /// </param>
        /// <param name='audioSources'>
        /// Audio sources.
        /// </param>
        public static void SetVolumeSFX(float setVolume, bool ignoreMaxSFXVolume, params AudioSource[] audioSources)
        {
            setVolume = Mathf.Clamp01(setVolume);
            float newVolume = ignoreMaxSFXVolume ? setVolume : (setVolume * Ins.maxSFXVolume);

            foreach (AudioSource audioSource in audioSources)
                audioSource.volume = newVolume;
        }
        /// <summary>
        /// Sets the volume of a certain group of SFX Objects. Can set to ignore the max SFX volume.
        /// </summary>
        /// <param name='setVolume'>
        /// Set volume.
        /// </param>
        /// <param name='ignoreMaxSFXVolume'>
        /// Ignore max SFX volume.
        /// </param>
        /// <param name='sfxObjects'>
        /// SFX objects.
        /// </param>
        public static void SetVolumeSFX(float setVolume, bool ignoreMaxSFXVolume, params GameObject[] sfxObjects)
        {
            setVolume = Mathf.Clamp01(setVolume);
            float newVolume = ignoreMaxSFXVolume ? setVolume : (setVolume * Ins.maxSFXVolume);

            foreach (GameObject sfxObject in sfxObjects)
                sfxObject.GetComponent<AudioSource>().volume = newVolume;
        }

        /// <summary>
        /// Gets the SFX volume.
        /// </summary>
        /// <returns>
        /// The SFX volume.
        /// </returns>
        public static float GetVolumeSFX()
        {
            return Ins.maxSFXVolume;
        }

        /// <summary>
        /// Sets the pitch of SFX in the game.
        /// </summary>
        /// <param name='setPitch'>
        /// Set pitch.
        /// </param>
        public static void SetPitchSFX(float setPitch)
        {
            Ins.pitchSFX = setPitch;
        }
        /* COMING SOON
        public static void SetPitchSFX(float setPitch, string groupName)
        {

        }
        */
        /// <summary>
        /// Sets the pitch of a certain group of AudioSources.
        /// </summary>
        /// <param name='setPitch'>
        /// Set pitch.
        /// </param>
        /// <param name='audioSources'>
        /// Audio sources.
        /// </param>
        public static void SetPitchSFX(float setPitch, params AudioSource[] audioSources)
        {
            foreach (AudioSource audioSource in audioSources)
                audioSource.pitch = setPitch;
        }
        /// <summary>
        /// Sets the pitch of a certain group of SFX Objects.
        /// </summary>
        /// <param name='setPitch'>
        /// Set pitch.
        /// </param>
        /// <param name='sfxObjects'>
        /// Sfx objects.
        /// </param>
        public static void SetPitchSFX(float setPitch, params GameObject[] sfxObjects)
        {
            foreach (GameObject sfxObject in sfxObjects)
                sfxObject.GetComponent<AudioSource>().pitch = setPitch;
        }

        /// <summary>
        /// Gets the SFX pitch.
        /// </summary>
        /// <returns>
        /// The SFX pitch.
        /// </returns>
        public static float GetPitchSFX()
        {
            return Ins.pitchSFX;
        }

        /////////////////////////////////////////////////////

        /// <summary>
        /// Saves the SFX to the SoundManager prefab for easy access for frequently used SFX.  Will register the SFX to the group.
        /// </summary>
        /// <param name='clip'>
        /// Clip.
        /// </param>
        /// <param name='grpName'>
        /// Group name.
        /// </param>
        public static void SaveSFX(AudioClip clip, string grpName)
        {
            if (clip == null)
                return;

            SFXGroup grp = Ins.GetGroupByGroupName(grpName);
            if (grp == null)
                Debug.LogWarning("The SFXGroup, " + grpName + ", does not exist. Creating it as a new group");

            SaveSFX(clip);
            Ins.AddClipToGroup(clip.name, grpName);
        }

        /// <summary>
        /// Saves the SFX to the SoundManager prefab for easy access for frequently used SFX.  Will register the SFX to the group specified.
        /// If the group doesn't exist, it will be added to SoundManager.
        /// </summary>
        /// <param name='clip'>
        /// Clip.
        /// </param>
        /// <param name='grp'>
        /// Group.
        /// </param>
        public static void SaveSFX(AudioClip clip, SFXGroup grp)
        {
            if (clip == null)
                return;

            if (grp != null)
            {
                if (!Ins.groups.ContainsKey(grp.groupName))
                {
                    Ins.groups.Add(grp.groupName, grp);
#if UNITY_EDITOR
                    Ins.sfxGroups.Add(grp);
#endif
                }
                else if (Ins.groups[grp.groupName] != grp)
                    Debug.LogWarning("The SFXGroup, " + grp.groupName + ", already exists. This new group will not be added.");
            }

            SaveSFX(clip);
            Ins.AddClipToGroup(clip.name, grp.groupName);
        }

        /// <summary>
        /// Saves the SFX to the SoundManager prefab for easy access for frequently used SFX.
        /// </summary>
        /// <param name='clips'>
        /// Clips.
        /// </param>
        public static void SaveSFX(params AudioClip[] clips)
        {
            foreach (AudioClip clip in clips)
            {
                if (clip == null)
                    continue;

                if (!Ins.allClips.ContainsKey(clip.name))
                {
                    string clipName = clip.name;
                    Ins.allClips.Add(clipName, clip);
                    Ins.prepools.Add(clipName, 0);
                    Ins.baseVolumes.Add(clipName, 1f);
                    Ins.volumeVariations.Add(clipName, 0f);
                    Ins.pitchVariations.Add(clipName, 0f);
#if UNITY_EDITOR
                    Ins.storedSFXs.Add(clip);
                    Ins.sfxPrePoolAmounts.Add(0);
                    Ins.sfxBaseVolumes.Add(1f);
                    Ins.sfxVolumeVariations.Add(0f);
                    Ins.sfxPitchVariations.Add(0f);
                    //Ins.showSFXDetails.Add(false);
#endif
                }
            }
        }

        /// <summary>
        /// Deletes all SFX from the SoundManager.
        /// </summary>
        public static void DeleteSFX()
        {
            Ins.allClips.Clear();
            Ins.prepools.Clear();
            Ins.baseVolumes.Clear();
            Ins.volumeVariations.Clear();
            Ins.pitchVariations.Clear();
            Ins.clipsInGroups.Clear();
            Ins.clipToGroupKeys.Clear();
            Ins.clipToGroupValues.Clear();
            foreach (SFXGroup grp in Ins.sfxGroups)
                grp.clips.Clear();
#if UNITY_EDITOR
            Ins.storedSFXs.Clear();
            Ins.sfxPrePoolAmounts.Clear();
            Ins.sfxBaseVolumes.Clear();
            Ins.sfxVolumeVariations.Clear();
            Ins.sfxPitchVariations.Clear();
            //Ins.showSFXDetails.Clear();
#endif
        }

        /// <summary>
        /// Deletes certain SFX from the SoundManager.
        /// </summary>
        /// <param name='clips'>
        /// Clips.
        /// </param>
        public static void DeleteSFX(params AudioClip[] clips)
        {
            foreach (AudioClip clip in clips)
            {
                if (clip == null)
                    continue;

                if (!Ins.allClips.ContainsKey(clip.name))
                {
                    string clipName = clip.name;
                    Ins.allClips.Remove(clipName);
                    Ins.prepools.Remove(clipName);
                    Ins.baseVolumes.Remove(clipName);
                    Ins.volumeVariations.Remove(clipName);
                    Ins.pitchVariations.Remove(clipName);
                    Ins.RemoveClipFromGroup(clipName);
#if UNITY_EDITOR
                    int index = Ins.storedSFXs.IndexOf(clip);
                    if (index == -1) continue;
                    Ins.storedSFXs.RemoveAt(index);
                    Ins.sfxPrePoolAmounts.RemoveAt(index);
                    Ins.sfxBaseVolumes.RemoveAt(index);
                    Ins.sfxVolumeVariations.RemoveAt(index);
                    Ins.sfxPitchVariations.RemoveAt(index);
                    //Ins.showSFXDetails.RemoveAt(index);
#endif
                }
            }
        }
        /// <summary>
        /// Deletes certain SFX from the SoundManager referenced by name.
        /// </summary>
        /// <param name='clipNames'>
        /// Clip names.
        /// </param>
        public static void DeleteSFX(params string[] clipNames)
        {
            foreach (string clipName in clipNames)
            {
                if (string.IsNullOrEmpty(clipName))
                    continue;

                if (!Ins.allClips.ContainsKey(clipName))
                {
                    AudioClip clip = Ins.allClips[clipName];
                    Ins.allClips.Remove(clipName);
                    Ins.prepools.Remove(clipName);
                    Ins.baseVolumes.Remove(clipName);
                    Ins.volumeVariations.Remove(clipName);
                    Ins.pitchVariations.Remove(clipName);
                    Ins.RemoveClipFromGroup(clipName);
#if UNITY_EDITOR
                    if (clip == null) continue;
                    int index = Ins.storedSFXs.IndexOf(clip);
                    if (index == -1) continue;
                    Ins.storedSFXs.RemoveAt(index);
                    Ins.sfxPrePoolAmounts.RemoveAt(index);
                    Ins.sfxBaseVolumes.RemoveAt(index);
                    Ins.sfxVolumeVariations.RemoveAt(index);
                    Ins.sfxPitchVariations.RemoveAt(index);
                    //Ins.showSFXDetails.RemoveAt(index);
#endif
                }
            }
        }

        /// <summary>
        /// Applies the attributes available in the editor to SFX. Attributes like prepool amount, base volume, volume variation, and pitch variation.
        /// </summary>
        /// <param name='clip'>
        /// Clip.
        /// </param>
        /// <param name='prepool'>
        /// Prepool amount.
        /// </param>
        /// <param name='baseVolume'>
        /// Base volume.
        /// </param>
        /// <param name='volumeVariation'>
        /// Volume variation.
        /// </param>
        /// <param name='pitchVariation'>
        /// Pitch variation.
        /// </param>
        public static void ApplySFXAttributes(AudioClip clip, int prepool, float baseVolume, float volumeVariation, float pitchVariation)
        {
            if (clip == null || !Ins.allClips.ContainsKey(clip.name) || Ins.allClips[clip.name] != clip)
                return;

            string clipName = clip.name;

            int oldPrepool = Ins.prepools[clipName];
            Ins.prepools[clipName] = prepool;
            Ins.baseVolumes[clipName] = baseVolume;
            Ins.volumeVariations[clipName] = volumeVariation;
            Ins.pitchVariations[clipName] = pitchVariation;


            SFXPoolInfo info = null;
            if (Ins.ownedPools.ContainsKey(clip))
            {
                info = Ins.ownedPools[clip];
                if (info != null)
                {
                    info.prepoolAmount = prepool;
                    info.baseVolume = baseVolume;
                    info.volumeVariation = volumeVariation;
                    info.pitchVariation = pitchVariation;
                }
            }

#if UNITY_EDITOR
            int index = Ins.storedSFXs.IndexOf(clip);
            Ins.sfxPrePoolAmounts[index] = prepool;
            Ins.sfxBaseVolumes[index] = baseVolume;
            Ins.sfxVolumeVariations[index] = volumeVariation;
            Ins.sfxPitchVariations[index] = pitchVariation;
#endif

            if (oldPrepool < prepool)
                Ins.PrePoolClip(clip, prepool - oldPrepool);
        }
        /// <summary>
        /// Applies the attributes available in the editor to SFX, referenced by clip name. Attributes like prepool amount, base volume, volume variation, and pitch variation.
        /// </summary>
        /// <param name='clipName'>
        /// Clip name.
        /// </param>
        /// <param name='prepool'>
        /// Prepool amount.
        /// </param>
        /// <param name='baseVolume'>
        /// Base volume.
        /// </param>
        /// <param name='volumeVariation'>
        /// Volume variation.
        /// </param>
        /// <param name='pitchVariation'>
        /// Pitch variation.
        /// </param>
        public static void ApplySFXAttributes(string clipName, int prepool, float baseVolume, float volumeVariation, float pitchVariation)
        {
            if (string.IsNullOrEmpty(clipName) || !ClipNameIsValid(clipName))
                return;

            ApplySFXAttributes(Ins.allClips[clipName], prepool, baseVolume, volumeVariation, pitchVariation);
        }

        /// <summary>
        /// Creates the SFX group and adds it to SoundManager.
        /// </summary>
        /// <returns>
        /// The SFX group.
        /// </returns>
        /// <param name='grpName'>
        /// Group name.
        /// </param>
        /// <param name='capAmount'>
        /// Cap amount.
        /// </param>
        public static SFXGroup CreateSFXGroup(string grpName, int capAmount)
        {
            if (!Ins.groups.ContainsKey(grpName))
            {
                SFXGroup grp = new SFXGroup(grpName, capAmount);
                Ins.groups.Add(grpName, grp);
#if UNITY_EDITOR
                Ins.sfxGroups.Add(grp);
#endif
                return grp;
            }
            Debug.LogWarning("This group already exists. Cannot add it.");
            return null;
        }

        /// <summary>
        /// Creates the SFX group and adds it to SoundManager.
        /// </summary>
        /// <returns>
        /// The SFX group.
        /// </returns>
        /// <param name='grpName'>
        /// Group name.
        /// </param>
        public static SFXGroup CreateSFXGroup(string grpName)
        {
            if (!Ins.groups.ContainsKey(grpName))
            {
                SFXGroup grp = new SFXGroup(grpName);
                Ins.groups.Add(grpName, grp);
#if UNITY_EDITOR
                Ins.sfxGroups.Add(grp);
#endif
                return grp;
            }
            Debug.LogWarning("This group already exists. Cannot add it.");
            return null;
        }

        /// <summary>
        /// Moves a clip to the specified SFXGroup. If the group doesn't exist, it will make the group.
        /// </summary>
        /// <param name='clipName'>
        /// Clip name.
        /// </param>
        /// <param name='newGroupName'>
        /// New group name.
        /// </param>
        public static void MoveToSFXGroup(string clipName, string newGroupName)
        {
            Ins.SetClipToGroup(clipName, newGroupName);
        }
        /// <summary>
        /// Removes a clip from a SFXGroup.
        /// </summary>
        /// <param name='clipName'>
        /// Clip name.
        /// </param>
        public static void RemoveFromSFXGroup(string clipName)
        {
            Ins.RemoveClipFromGroup(clipName);
        }

        /// <summary>
        /// Loads a random SFX from a specified SFXGroup.
        /// </summary>
        /// <returns>
        /// The random clip.
        /// </returns>
        /// <param name='grpName'>
        /// Group name.
        /// </param>
        public static AudioClip LoadFromGroup(string grpName)
        {
            SFXGroup grp = Ins.GetGroupByGroupName(grpName);
            if (grp == null)
            {
                Debug.LogError("There is no group by this name: " + grpName + ".");
                return null;
            }

            AudioClip result = null;

            // check if clips is empty
            if (grp.clips.Count == 0)
            {
                Debug.LogWarning("There are no clips in this group: " + grpName);
                return null;
            }

            // Get random clip from list
            result = grp.clips[Random.Range(0, grp.clips.Count)];

            // return result
            return result;
        }

        /// <summary>
        /// Loads all SFX from a specified SFXGroup.
        /// </summary>
        /// <returns>
        /// The all clips from the group.
        /// </returns>
        /// <param name='grpName'>
        /// Group name.
        /// </param>
        public static AudioClip[] LoadAllFromGroup(string grpName)
        {
            SFXGroup grp = Ins.GetGroupByGroupName(grpName);
            if (grp == null)
            {
                Debug.LogError("There is no group by this name, " + grpName + ".");
                return null;
            }

            // check if group is empty
            if (grp.clips.Count == 0)
            {
                Debug.LogWarning("There are no clips in this group: " + grpName);
                return null;
            }

            // return all clips in array
            return grp.clips.ToArray();
        }

        /// <summary>
        /// Load the specified clipname, at a custom path if you do not want to use resourcesPath.
        /// If custompath fails or is empty/null, it will query the stored SFXs.  If that fails, it'll query the default
        /// resourcesPath.  If all else fails, it'll return null.
        /// </summary>
        /// <returns>
        /// The clip.
        /// </returns>
        /// <param name='clipname'>
        /// Clip name.
        /// </param>
        /// <param name='customPath'>
        /// Custom path.
        /// </param>
        public static AudioClip Load(string clipname, string customPath)
        {
            AudioClip result = null;

            // Attempt to use custom path if provided
            if (!string.IsNullOrEmpty(customPath))
                if (customPath[customPath.Length - 1] == '/')
                    result = (AudioClip)Resources.Load(customPath.Substring(0, customPath.Length) + "/" + clipname);
                else
                    result = (AudioClip)Resources.Load(customPath + "/" + clipname);

            if (result)
                return result;

            // If custom path fails, attempt to find it in our stored SFXs
            if (Ins.allClips.ContainsKey(clipname))
                result = Ins.allClips[clipname];

            if (result)
                return result;

            // If it is not in our stored SFX, attempt to find it in our default resources path
            result = (AudioClip)Resources.Load(Ins.resourcesPath + "/" + clipname);

            return result;
        }

        /// <summary>
        /// Load the specified clipname from the stored SFXs.  If that fails, it'll query the default
        /// resourcesPath.  If all else fails, it'll return null.
        /// </summary>
        /// <returns>
        /// The clip.
        /// </returns>
        /// <param name='clipname'>
        /// Clipname.
        /// </param>
        public static AudioClip Load(string clipname)
        {
            return Load(clipname, "");
        }

        /// <summary>
        /// Resets the SFX object to default values.
        /// </summary>
        /// <param name='sfxObj'>
        /// SFX object.
        /// </param>
        public static void ResetSFXObject(GameObject sfxObj)
        {
            AudioSource sfxAudio = sfxObj.GetComponent<AudioSource>();
            if (sfxAudio == null)
                return;

            sfxAudio.outputAudioMixerGroup = null;
            sfxAudio.mute = false;
            sfxAudio.bypassEffects = false;
            sfxAudio.bypassListenerEffects = false;
            sfxAudio.bypassReverbZones = false;
            sfxAudio.playOnAwake = false;
            sfxAudio.loop = false;

            sfxAudio.priority = 128;
            sfxAudio.volume = 1f;
            sfxAudio.pitch = 1f;
            sfxAudio.panStereo = 0f;
            sfxAudio.reverbZoneMix = 1f;

            sfxAudio.dopplerLevel = 1f;
            sfxAudio.rolloffMode = AudioRolloffMode.Logarithmic;
            sfxAudio.minDistance = 1f;
            sfxAudio.spatialBlend = Ins.defaultSFXSpatialBlend;
            sfxAudio.spread = 0f;
            sfxAudio.maxDistance = 500f;
        }
        /// <summary>
        /// Crossfade between two AudioSources.
        /// </summary>
        /// <param name='duration'>
        /// Duration.
        /// </param>
        /// <param name='fromSource'>
        /// From source.
        /// </param>
        /// <param name='toSource'>
        /// To source.
        /// </param>
        /// <param name='runOnEndFunction'>
        /// Run on end function.
        /// </param>
        public static void Crossfade(float duration, AudioSource fromSource, AudioSource toSource, SongCallBack runOnEndFunction = null)
        {
            Ins.StartCoroutine(Ins.XFade(duration, fromSource, toSource, runOnEndFunction));
        }
        /// <summary>
        /// Crossfade between two SFX Objects.
        /// </summary>
        /// <param name='duration'>
        /// Duration.
        /// </param>
        /// <param name='fromSFXObject'>
        /// From SFX object.
        /// </param>
        /// <param name='toSFXObject'>
        /// To SFX object.
        /// </param>
        /// <param name='runOnEndFunction'>
        /// Run on end function.
        /// </param>
        public static void Crossfade(float duration, GameObject fromSFXObject, GameObject toSFXObject, SongCallBack runOnEndFunction = null)
        {
            Crossfade(duration, fromSFXObject.GetComponent<AudioSource>(), toSFXObject.GetComponent<AudioSource>(), runOnEndFunction);
        }
        /// <summary>
        /// Cross in an AudioSource.
        /// </summary>
        /// <param name='duration'>
        /// Duration.
        /// </param>
        /// <param name='source'>
        /// Source.
        /// </param>
        /// <param name='runOnEndFunction'>
        /// Run on end function.
        /// </param>
        public static void CrossIn(float duration, AudioSource source, SongCallBack runOnEndFunction = null)
        {
            Ins.StartCoroutine(Ins.XIn(duration, source, runOnEndFunction));
        }
        /// <summary>
        /// Cross in a SFX Object
        /// </summary>
        /// <param name='duration'>
        /// Duration.
        /// </param>
        /// <param name='sfxObject'>
        /// Sfx object.
        /// </param>
        /// <param name='runOnEndFunction'>
        /// Run on end function.
        /// </param>
        public static void CrossIn(float duration, GameObject sfxObject, SongCallBack runOnEndFunction = null)
        {
            CrossIn(duration, sfxObject.GetComponent<AudioSource>(), runOnEndFunction);
        }
        /// <summary>
        /// Cross out an AudioSource.
        /// </summary>
        /// <param name='duration'>
        /// Duration.
        /// </param>
        /// <param name='source'>
        /// Source.
        /// </param>
        /// <param name='runOnEndFunction'>
        /// Run on end function.
        /// </param>
        public static void CrossOut(float duration, AudioSource source, SongCallBack runOnEndFunction = null)
        {
            Ins.StartCoroutine(Ins.XOut(duration, source, runOnEndFunction));
        }
        /// <summary>
        /// Cross out a SFX Object.
        /// </summary>
        /// <param name='duration'>
        /// Duration.
        /// </param>
        /// <param name='sfxObject'>
        /// Sfx object.
        /// </param>
        /// <param name='runOnEndFunction'>
        /// Run on end function.
        /// </param>
        public static void CrossOut(float duration, GameObject sfxObject, SongCallBack runOnEndFunction = null)
        {
            CrossOut(duration, sfxObject.GetComponent<AudioSource>(), runOnEndFunction);
        }
    }

}