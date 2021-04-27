﻿using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Threading;
using DMK.Core;
using DMK.DMath;
using DMK.Reflection;
using JetBrains.Annotations;

namespace DMK.Scriptables {
[CreateAssetMenu(menuName = "AudioTrack")]
public class AudioTrack : ScriptableObject, IAudioTrackInfo {
    public AudioClip clip = null!;

    public string key = "";
    public string trackPlayLocation = "";
    public string title = "";
    public LocalizedStringReference musicRoomDescription = null!;
    public float volume = .2f;
    public float pitch = 1f;
    public bool stopOnPause;
    public bool loop;

    public Vector2 loopSeconds;
    public float startWithTime = 0f;
    [ReflectInto(typeof(Pred))]
    public string? musicRoomRequirement;
    public bool showInMusicRoom = true;

    public AudioClip Clip => clip;
    public float Volume => volume;
    public float Pitch => pitch;
    public bool StopOnPause => stopOnPause;
    public bool Loop => loop;
    public Vector2 LoopSeconds => loopSeconds;
    public float StartTime => startWithTime;
    public string Title => title;
    public string TrackPlayLocation => trackPlayLocation;

    public LocalizedString MusicRoomDescription => musicRoomDescription.Value;
    public bool? DisplayInMusicRoom =>
        showInMusicRoom ?
            (bool?)(ReflWrap<Pred>.MaybeWrap(musicRoomRequirement)
                ?.Invoke(ParametricInfo.Zero) ?? true) :
            null;
}
}