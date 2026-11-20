using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace com.Plapamaru.Shared.Audio
{
    [CreateAssetMenu(menuName = "Shared/Audio/Audio Config", fileName = "AudioConfig")]
    public class AudioConfig : ScriptableObject
    {
        [Serializable]
        public struct Volume
        {
            public string Parameter;
            [Range(0f, 1f)] public float Default;
        }

        [SerializeField] private AudioMixer _mixer;
        [SerializeField] private AudioMixerGroup _effectsGroup;
        [SerializeField] private AudioMixerGroup _musicGroup;
        [SerializeField, Min(1)] private int _effectVoices = 8;
        [SerializeField] private Volume[] _volumes = Array.Empty<Volume>();

        public AudioMixer Mixer => _mixer;
        public AudioMixerGroup EffectsGroup => _effectsGroup;
        public AudioMixerGroup MusicGroup => _musicGroup;
        public int EffectVoices => _effectVoices;
        public IReadOnlyList<Volume> Volumes => _volumes;
    }
}
