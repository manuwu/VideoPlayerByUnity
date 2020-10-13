using System;
using UnityEngine;

namespace UMP
{
    [RequireComponent(typeof(AudioSource))]
    public abstract class AudioOutput : MonoBehaviour
    {
        public enum AudioChannels
        {
            Both,
            Left,
            Right
        };

        [SerializeField]
        private AudioChannels _audioChannel;

        public AudioChannels AudioChannel
        {
            get { return _audioChannel; }
            set { _audioChannel = value; }
        }

        private int _id;

        public int Id
        {
            get { return _id; }
        }

        private AudioSource _audioSource;

        public AudioSource AudioSource
        {
            get {
                if (_audioSource == null)
                    _audioSource = GetComponent<AudioSource>();

                return _audioSource;
            }
        }

        private float[] _data;

        internal float[] Data
        {
            get { return _data; }
            set
            {
                _data = value;

                if (_outputDataListener != null && _data != null)
                    _outputDataListener(_data, _audioChannel);
            }
        }

        internal void Init()
        {
            _id = GetInstanceID();
        }

        public void Play()
        {
            if (_audioSource == null)
                _audioSource = GetComponent<AudioSource>();

            _audioSource.Play();
        }

        public void Pause()
        {
            if (_audioSource == null)
                _audioSource = GetComponent<AudioSource>();

            _audioSource.Pause();
        }

        public void Stop()
        {
            if (_audioSource == null)
                _audioSource = GetComponent<AudioSource>();

            _audioSource.Stop();
        }

        public void RemoveAllListeners()
        {
            if (_audioFilterReadListener != null)
            {
                foreach (Action<int, float[], AudioChannels> eh in _audioFilterReadListener.GetInvocationList())
                    _audioFilterReadListener -= eh;
            }

            if (_outputDataListener != null)
            {
                foreach (Action<float[], AudioChannels> eh in _outputDataListener.GetInvocationList())
                    _outputDataListener -= eh;
            }
        }

        private event Action<float[], AudioChannels> _outputDataListener;

        public event Action<float[], AudioChannels> OutputDataListener
        {
            add
            {
                _outputDataListener = (Action<float[], AudioChannels>)Delegate.Combine(_outputDataListener, value);
            }
            remove
            {
                if (_outputDataListener != null)
                    _outputDataListener = (Action<float[], AudioChannels>)Delegate.Remove(_outputDataListener, value);
            }
        }

        private event Action<int, float[], AudioChannels> _audioFilterReadListener;

        internal event Action<int, float[], AudioChannels> AudioFilterReadListener
        {
            add
            {
                _audioFilterReadListener = (Action<int, float[], AudioChannels>)Delegate.Combine(_audioFilterReadListener, value);
            }
            remove
            {
                if (_audioFilterReadListener != null)
                    _audioFilterReadListener = (Action<int, float[], AudioChannels>)Delegate.Remove(_audioFilterReadListener, value);
            }
        }
        
        /// Native Unity "AudioSource" callback
        private void OnAudioFilterRead(float[] data, int nbChannels)
        {
            if (_audioFilterReadListener != null)
                _audioFilterReadListener(Id, data, _audioChannel);
        }
    }
}
