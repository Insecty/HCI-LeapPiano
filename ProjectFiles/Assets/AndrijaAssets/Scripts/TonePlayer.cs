using UnityEngine;
using System.Collections.Generic;

public class TonePlayer : Singleton<TonePlayer>
{
    #region Singleton Necessities
    
    protected TonePlayer ()
    {
        PlayedTonesCache = new List<ToneInfo>(PlayedTonesCacheSize);

        List<string> tones = MusicScale.MusicScales[Scale.ToString()].GetToneNames(BaseTone, ToneRange);

        //foreach (string tone in tones)
        //{
            //Debug.Log("Available: " + tone);
       // }
    }
	// new private static TonePlayer Instance { get { return null; } }
    // this prevents calling Instance before Manager's Awake

    #endregion

    #region Variables

    public int PlayedTonesCacheSize = 50;

    public List<ToneInfo> PlayedTonesCache;

    public ScaleEnum Scale = ScaleEnum.Major;

    public ToneRange ToneRange = new ToneRange(){MinTone = MusicTone.C, MinOctave = 2, MaxTone = MusicTone.E, MaxOctave = 6};

    public MusicTone BaseTone = MusicTone.C;

    public ToneDuration ToneDuration = ToneDuration.Quarter;

    public string CurrentInstrument = "Piano";

    private const string Underline = "_";

    #endregion

    #region Mono Behaviour specific methods

    // Use this for initialization
	void Start () 
    {
	  
    }
	
	// Update is called once per frame
	void Update () 
    {

    }

    #endregion

    #region Tone Player methods
    
    // plays random tone from current instrument, current duration, current scale, and current tone range
    public void PlayTone()
    {
        // calls function down
        PlayTone(CurrentInstrument, ToneDuration, BaseTone, ToneRange, Scale.ToString());
    }

    // play random tone from given variables - scale and range
    public void PlayTone(string instrument, ToneDuration duration, MusicTone baseTone, ToneRange toneRange, string scale)
    {
        string toneString = MusicScale.MusicScales[scale].GetRandomTone(toneRange, baseTone);
        MusicTone tone;
        int octave;
        MusicUtilities.StringToToneOctave(toneString, out tone, out octave);

        PlayTone(instrument, duration, tone, octave);
    }

    // PLAYS A SPECIFIC TONE 
    public void PlayTone(string instrument, ToneDuration duration, MusicTone tone, int octave)
    {
        string toneClipName = instrument + Underline + duration.ToString() + Underline + tone + Underline + octave;

        Debug.Log(toneClipName);

        ToneInfo info = new ToneInfo()
        {
            Tone = tone,
            Octave = octave,
            Duration = duration,
            TimeOfPlaySinceStartup = Time.realtimeSinceStartup
        };
        RecordToneInfo(info);
        AudioController.Play(toneClipName);
    }

    /// <summary>
    /// Plays the specific tone with current instrument
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="tone"></param>
    /// <param name="octave"></param>
    public void PlayTone(ToneDuration duration, MusicTone tone, int octave)
    {
        PlayTone(CurrentInstrument, duration, tone, octave);
    }

    /// <summary>
    /// Plays the specific tone with current instrument and current duration
    /// </summary>
    /// <param name="tone"></param>
    /// <param name="octave"></param>
    public void PlayTone(MusicTone tone, int octave)
    {
        PlayTone(CurrentInstrument, ToneDuration, tone, octave);
    }

    public void RecordToneInfo(ToneInfo toneInfo)
    {
        if (PlayedTonesCache.Count == PlayedTonesCacheSize)
        {
            PlayedTonesCache.RemoveAt(0);
        }
        PlayedTonesCache.Add(toneInfo);
    }

    #endregion
}
