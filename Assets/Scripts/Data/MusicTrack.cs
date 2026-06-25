using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Sanicball/Music/Track")]
public class MusicTrack : ScriptableObject
{
    [Serializable]
    public class Segment
    {
        public MusicSegmentType type;
        public AudioClip clip;

        public bool loop;
        public bool mustLoopOnce;
        public bool uninterruptible;
    }

    public Segment[] segments;

    public Segment Get(MusicSegmentType type)
    {
        foreach (var s in segments)
            if (s.type == type)
                return s;

        return null;
    }
}