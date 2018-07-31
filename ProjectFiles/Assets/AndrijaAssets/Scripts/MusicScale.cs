using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using System;

/// <summary>
/// Class that contains a series of inter-tone offsets,
/// representing a custom tone collection
/// used for sound sampling of selected tones only
/// 
/// Scale is implemented as a class, not an enum, 
/// because this will enable easy addition of custom scales in the future
/// 
/// TODO: decide on preserving scales in a text file, 
/// TODO: or PlayerPrefs (simple option for web player build)
/// </summary>
[Serializable]
public class MusicScale
{
    /// <summary>
    /// Name of this MusicScale
    /// </summary>
    public string Name;
    
    /// <summary>
    /// Tone offsets in this MusicScale.
    /// In relative mode (AreToneOffsetsAbsolute == false) 
    /// first tone offset is offset from base tone,
    /// subsequent offsets are offsets from previous tones.
    /// In absolute mode (AreToneOffsetsAbsolute == false)
    /// all the offsets are absolute from base tone
    /// </summary>
    public List<int> ToneOffsets;
    
    /// <summary>
    /// Contains existing Music scales
    /// Initially contains 7 main MusicScales in relative offset
    /// Major alternate name is Ionian, 
    /// Minor alternate name is Aeolian    /// 
    /// </summary>
    public static Dictionary<string,MusicScale> MusicScales = new Dictionary<string,MusicScale>()
    {
        { "Major", new MusicScale(){Name = "Major", ToneOffsets = new List<int>(){2,2,1,2,2,2,1}}}, 
        { "Dorian", new MusicScale(){Name = "Dorian", ToneOffsets = new List<int>(){2,1,2,2,2,1,2}}},  
        { "Phrygian", new MusicScale(){Name = "Phrygian", ToneOffsets = new List<int>(){1,2,2,2,1,2,2}}},  
        { "Lydian", new MusicScale(){Name = "Lydian", ToneOffsets = new List<int>(){2,2,2,1,2,2,1} }},  
        { "Mixolydian", new MusicScale(){Name = "Mixolydian", ToneOffsets = new List<int>(){2,2,1,2,2,1,2}} },  
        { "Minor", new MusicScale(){Name = "Minor", ToneOffsets = new List<int>(){2,1,2,2,1,2,2} }},  
        { "Locrian", new MusicScale(){Name = "Locrian", ToneOffsets = new List<int>(){1,2,2,1,2,2,2} }}  		
    };    

   
    
    // returns merged tone name and octave
    public string GetRandomTone(ToneRange toneRange, MusicTone baseTone)
    {
        List<string> toneNames = GetToneNames(baseTone, toneRange);
        
        return toneNames[Random.Range(0, toneNames.Count)];
    }

    // Get all the tone names from scale
    public List<string> GetToneNames(MusicTone baseTone, ToneRange toneRange)
    {
        List<string> toneNames = GetToneNames(baseTone, toneRange.MinOctave, toneRange.ToneCount);

        for (int i = 0; i < toneNames.Count; i++)
        {
            if (!toneRange.IsToneInRange(toneNames[i]))
            {
                toneNames.RemoveAt(i);
                i--;
            }
        }

        return toneNames;
    }

    /// <summary>
    /// Gets names of tones made from MusicTone and its octave index
    /// </summary>
    /// <param name="baseTone">Starting tone of the scale</param>
    /// <param name="startOctave">Index of starting tone octave</param>
    /// <param name="toneCount">Number of tones, including starting tone</param>
    /// <returns></returns>
    public List<string> GetToneNames(MusicTone baseTone, int startOctave, int toneCount)
    {
        List<string> tones = new List<string>();
        tones.Add(baseTone.ToString() + "_" + startOctave.ToString());

        List<int> offsets = MusicUtilities.GetAbsoluteListFromRelativeList(this.ToneOffsets, toneCount - 1);

        int currentOctave;
        foreach (int toneOffset in offsets)
        {
            currentOctave = startOctave + toneOffset / 12;
            tones.Add(((MusicTone)(toneOffset % 12)).ToString() + "_" + currentOctave.ToString());
        }

        return tones;
    }
}

public enum ScaleEnum{Major,Dorian,Phrygian,Lydian,Mixolydian,Minor,Locrian}

[Serializable]
public class ToneInfo
{
    public MusicTone Tone;
    public int Octave;
    public ToneDuration Duration;

    // TODO: debate: float vs DateTime
    public float TimeOfPlaySinceStartup;
}

public enum MusicTone
{
    C = 0,
    Csharp = 1,
    D = 2,
    Dsharp = 3, 
    E = 4,
    F = 5,
    Fsharp = 6,
    G = 7,
    Gsharp = 8,
    A = 9, 
    Asharp = 10,
    H = 11
};

public enum ToneDuration
{
    Whole = 1,
    Half = 2,
    Quarter = 4,
    Eighth = 8,
    Sixteenth = 16,
    ThirtySecond = 32
}

public static class MusicUtilities
{
    public static void StringToToneOctave(string toneString, out MusicTone tone, out int octave)
    {
        string[] toneData = toneString.Split('_');
        tone = (MusicTone) Enum.Parse(typeof (MusicTone), toneData[0]);
        octave = int.Parse(toneData[1]);
    }

    public static List<int> GetAbsoluteListFromAbsoluteList(List<int> offsets, int tones)
    {
        List<int> absOffsets = new List<int>();

        int seriesOffset = 0;
        while (tones > 0)
        {
            for (int i = 0; i < offsets.Count; i++)
            {
                absOffsets.Add(offsets[i] + seriesOffset);
                tones--;

                if(tones == 0)
                    break;
            }
            // offset the series offset by total tone count, 12
            seriesOffset += System.Enum.GetValues(typeof(MusicTone)).Length;
        }

        return absOffsets;
    }

    public static List<int> GetAbsoluteListFromRelativeList(List<int> offsets, int tones)
    {
        List<int> absOffsets = new List<int>();

        int seriesOffset = 0;
        while (tones > 0)
        {
            for (int i = 0; i < offsets.Count; i++)
            {
                seriesOffset += offsets[i];
                absOffsets.Add(seriesOffset);
                tones--;

                if (tones == 0)
                    break;
            }
        }

        return absOffsets;
    }

    #region Music Scale Load and Save in PlayerPrefs
   
    private static string PPMusicScaleNamePrefix = "_MusicScaleName";
    private static string PPMusicScaleTonePrefix = "_MusicTone";

    public static Dictionary<string,MusicScale> LoadMusicScales()
    {
        Dictionary<string, MusicScale> MusicScales = new Dictionary<string, MusicScale>();

        int scaleNameIndex = 0;
        while (PlayerPrefs.HasKey(PPMusicScaleNamePrefix + scaleNameIndex.ToString()))
        {
            string scaleName = PlayerPrefs.GetString(PPMusicScaleNamePrefix + scaleNameIndex.ToString());
            MusicScale scale = LoadMusicScale(scaleName);
            MusicScales.Add(scaleName, scale);
            scaleNameIndex++;
        }

        return MusicScales;
    }

    public static void SaveMusicScales(Dictionary<string, MusicScale> musicScales)
    {
        int scaleIndex = 0;
        foreach (KeyValuePair<string, MusicScale> keyValuePair in musicScales)
        {
            PlayerPrefs.SetString(PPMusicScaleNamePrefix + scaleIndex.ToString(), keyValuePair.Key);
            SaveMusicScale(keyValuePair.Value);
            scaleIndex++;
        }
    }

    public static void ClearMusicScales()
    {
        int scaleNameIndex = 0;
        while (PlayerPrefs.HasKey(PPMusicScaleNamePrefix + scaleNameIndex.ToString()))
        {
            string scaleName = PlayerPrefs.GetString(PPMusicScaleNamePrefix + scaleNameIndex.ToString());
            DeleteMusicScale(scaleName);
            scaleNameIndex++;
        }    
    }

    public static MusicScale LoadMusicScale(string scaleName)
    {
        MusicScale scale = new MusicScale();
        scale.Name = scaleName;
        
        scale.ToneOffsets = new List<int>();
        int toneIndex = 0;
        while (PlayerPrefs.HasKey(scaleName + PPMusicScaleTonePrefix + toneIndex.ToString()))
        {
            int offset = PlayerPrefs.GetInt(scaleName + PPMusicScaleTonePrefix + toneIndex.ToString());
            scale.ToneOffsets.Add(offset);
            toneIndex++;
        }

        return scale;
    }

    public static void SaveMusicScale(MusicScale scale)
    {
        DeleteMusicScale(scale.Name);

        for (int i = 0; i < scale.ToneOffsets.Count; i++)
        {
            PlayerPrefs.SetInt(scale.Name + PPMusicScaleTonePrefix + i.ToString(), scale.ToneOffsets[i]);
        }
    }

    public static void DeleteMusicScale(string scaleName)
    {
        int toneIndex = 0;
        while (PlayerPrefs.HasKey(scaleName + PPMusicScaleTonePrefix + toneIndex.ToString()))
        {
            PlayerPrefs.DeleteKey(scaleName + PPMusicScaleTonePrefix + toneIndex.ToString());
            toneIndex++;
        }
    }

    #endregion
}

[Serializable]
public class ToneRange
{
    // e.g. D2 -> C0 is first tone, C1 is 12th, C2 24th, D2 26th
    public MusicTone MinTone;
    public int MinOctave;
	
	public int MinToneIndex
	{
		get { return (int) MinTone + MinOctave*12; }
	}

    public MusicTone MaxTone;
    public int MaxOctave;
	
    // INCLUSIVE
	public int MaxToneIndex
	{
		get { return (int) MaxTone + MaxOctave*12; }
	}

    public int ToneCount
    {
        get { return MaxToneIndex - MinToneIndex + 1; }
    }

    public int GetRandomTone()
    {
        // Random range int has exclusive maximum, therefore +1
        return Random.Range(MinToneIndex, MaxToneIndex+1);
    }

    public bool IsToneInRange(MusicTone tone, int octave)
    {
        int toneIndex = (int) tone + octave*12;

        if (toneIndex >= MinToneIndex && toneIndex <= MaxToneIndex)
            return true;
        else
            return false;
    }

    public bool IsToneInRange(string toneString)
    {
        MusicTone tone;
        int octave;
        MusicUtilities.StringToToneOctave(toneString,out tone, out octave);

        return IsToneInRange(tone, octave);
    }
}