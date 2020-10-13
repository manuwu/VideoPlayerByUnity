using System;
using System.Collections.Generic;
using UnityEngine;

namespace UMP
{
    internal class PlayerManagerAudios
    {
        private AudioOutput[] _audioOutputs;

        public PlayerManagerAudios(AudioOutput[] audioOutputs)
        {
            _audioOutputs = audioOutputs;

            if (_audioOutputs != null && IsValid)
            {
                foreach (var audioOutput in _audioOutputs)
                    audioOutput.Init();
            }
        }

        public void AddListener(Action<int, float[], AudioOutput.AudioChannels> listener)
        {
            if (_audioOutputs != null && IsValid)
            {
                foreach (var audioOutput in _audioOutputs)
                {
                    audioOutput.AudioFilterReadListener += listener;
                }
            }
        }

        public void RemoveAllListeners()
        {
            if (_audioOutputs != null && IsValid)
            {
                foreach (var audioOutput in _audioOutputs)
                {
                    audioOutput.RemoveAllListeners();
                }
            }
        }

        public AudioOutput[] AudioOutputs
        {
            get { return _audioOutputs; }
        }

        public AudioSource[] AudioSources
        {
            get
            {
                var audioSources = new List<AudioSource>();

                if (_audioOutputs != null && IsValid)
                {
                    foreach (var audioOutput in _audioOutputs)
                        audioSources.Add(audioOutput.AudioSource);
                }

                return audioSources.ToArray();
            }
        }

        public bool IsValid
        {
            get
            {
                if (_audioOutputs != null)
                {
                    foreach (var audioOutput in _audioOutputs)
                    {
                        if (audioOutput == null || audioOutput.AudioSource == null)
                            return false;
                    }
                }

                return true;
            }
        }

        public bool OutputsDataUpdated
        {
            get
            {
                if (_audioOutputs != null && IsValid)
                {
                    foreach (var audioOutput in _audioOutputs)
                    {
                        if (audioOutput.Data == null)
                            return false;
                    }
                }

                return true;
            }
        }

        public bool SetOutputData(int id, float[] data)
        {
            if (_audioOutputs != null && IsValid)
            {
                foreach (var audioOutput in _audioOutputs)
                {
                    if (audioOutput.Id == id)
                    {
                        audioOutput.Data = data;
                        return true;
                    }
                }
            }

            return false;
        }

        public void ResetOutputsData()
        {
            if (_audioOutputs != null && IsValid)
            {
                foreach (var audioOutput in _audioOutputs)
                    audioOutput.Data = null;
            }
        }

        public void Play()
        {
            if (_audioOutputs != null && IsValid)
            {
                foreach (var audioOutput in _audioOutputs)
                    audioOutput.Play();
            }
        }

        public void Pause()
        {
            if (_audioOutputs != null && IsValid)
            {
                foreach (var audioOutput in _audioOutputs)
                    audioOutput.Pause();
            }
        }

        public void Stop()
        {
            if (_audioOutputs != null && IsValid)
            {
                foreach (var audioOutput in _audioOutputs)
                    audioOutput.Stop();
            }
        }
    }
}
