using System.Collections.Generic;
using com.Plapamaru.Shared.Singletons;
using UnityEngine;

namespace com.Plapamaru.Shared.Audio
{
    public class AudioManager : MonoBehaviourSingleton<AudioManager>
    {
        private const string PrefsPrefix = "Audio.";
        private const float MinVolume = 0.0001f;

        [SerializeField] private AudioConfig _config;

        private readonly Dictionary<string, float> _volumes = new Dictionary<string, float>();
        private AudioSource[] _effects;
        private AudioSource _music;
        private int _nextEffect;

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this)
                return;

            var voices = _config != null ? _config.EffectVoices : 8;
            _effects = new AudioSource[voices];
            for (var i = 0; i < voices; i++)
                _effects[i] = CreateSource($"Effect{i}", _config != null ? _config.EffectsGroup : null);

            _music = CreateSource("Music", _config != null ? _config.MusicGroup : null);
            _music.loop = true;

            if (_config != null)
            {
                foreach (var volume in _config.Volumes)
                    _volumes[volume.Parameter] = PlayerPrefs.GetFloat(PrefsPrefix + volume.Parameter, volume.Default);
            }
        }

        private void Start()
        {
            foreach (var pair in _volumes)
                ApplyVolume(pair.Key, pair.Value);
        }

        public void PlayEffect(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (clip == null)
                return;

            var source = NextEffectSource();
            source.clip = clip;
            source.volume = volume;
            source.pitch = pitch;
            source.Play();
        }

        public void PlayMusic(AudioClip clip, float volume = 1f)
        {
            if (_music.clip == clip && _music.isPlaying)
                return;

            _music.clip = clip;
            _music.volume = volume;
            if (clip != null)
                _music.Play();
            else
                _music.Stop();
        }

        public void StopMusic()
        {
            _music.Stop();
            _music.clip = null;
        }

        public float GetVolume(string parameter)
        {
            return _volumes.TryGetValue(parameter, out var volume) ? volume : 1f;
        }

        public void SetVolume(string parameter, float volume)
        {
            volume = Mathf.Clamp01(volume);
            _volumes[parameter] = volume;
            PlayerPrefs.SetFloat(PrefsPrefix + parameter, volume);
            ApplyVolume(parameter, volume);
        }

        private void ApplyVolume(string parameter, float volume)
        {
            if (_config == null || _config.Mixer == null)
                return;

            if (!_config.Mixer.SetFloat(parameter, Mathf.Log10(Mathf.Max(volume, MinVolume)) * 20f))
                Debug.LogWarning($"{nameof(AudioManager)}: '{parameter}' is not an exposed parameter of {_config.Mixer.name}.", this);
        }

        private AudioSource NextEffectSource()
        {
            for (var i = 0; i < _effects.Length; i++)
            {
                var index = (_nextEffect + i) % _effects.Length;
                if (!_effects[index].isPlaying)
                {
                    _nextEffect = (index + 1) % _effects.Length;
                    return _effects[index];
                }
            }

            var stolen = _effects[_nextEffect];
            _nextEffect = (_nextEffect + 1) % _effects.Length;
            return stolen;
        }

        private AudioSource CreateSource(string sourceName, UnityEngine.Audio.AudioMixerGroup group)
        {
            var go = new GameObject(sourceName);
            go.transform.SetParent(transform, false);
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.ignoreListenerPause = true;
            source.spatialBlend = 0f;
            source.outputAudioMixerGroup = group;
            return source;
        }
    }
}
