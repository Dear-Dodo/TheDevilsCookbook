using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using System;

namespace TDC.Core.Manager
{
    [Serializable]
    public class AudioEmmitter
    {
        public string name;
        public StudioEventEmitter emitter;
        public List<ParameterTarget> parameters;
    }

    public class ParameterTarget
    {
        public string name;
        public float oldValue;
        public float targetValue;
        public float lerpTime;
        public float currentLerpTime;
    }
    public class AudioManager : MonoBehaviour
    {
        public List<AudioEmmitter> AudioEvents = new List<AudioEmmitter>();
        private FMOD.Studio.Bus Music;
        private FMOD.Studio.Bus SFX;
        private FMOD.Studio.Bus Master;
        public float MusicVolume = 0.5f;
        public float SFXVolume = 0.5f;
        public float MasterVolume = 0.75f;

        private void Awake()
        {
            Music = RuntimeManager.GetBus("bus:/Master/Music");
            SFX = RuntimeManager.GetBus("bus:/Master/SFX");
            Master = RuntimeManager.GetBus("bus:/Master");
            for (int i = 0; i < AudioEvents.Count; i++)
            {
                if (AudioEvents[i].parameters == null)
                    AudioEvents[i].parameters = new List<ParameterTarget>();
            }
        }

        private void Update()
        {
            Music.setVolume(MusicVolume);
            SFX.setVolume(SFXVolume);
            Master.setVolume(MasterVolume);
            for (int i = 0; i < AudioEvents.Count; i++)
            {
                for (int j = 0; j < AudioEvents[i].parameters.Count; j++)
                {
                    ParameterTarget parameter = AudioEvents[i].parameters[j];
                    if (parameter.currentLerpTime > 0)
                    {
                        AudioEvents[i].emitter.SetParameter(
                            parameter.name,
                            Mathf.Lerp(parameter.oldValue, parameter.targetValue, 1 - parameter.currentLerpTime/parameter.lerpTime)
                        );
                        parameter.currentLerpTime -= Time.unscaledDeltaTime;
                    }
                    else
                    {
                        AudioEvents[i].emitter.SetParameter(parameter.name, parameter.targetValue);
                        AudioEvents[i].parameters.Remove(parameter);
                    }
                }
            }
        }

        public AudioEmmitter GetSound(string Name)
        {
            for (int i = 0; i < AudioEvents.Count; i++)
            {
                if (AudioEvents[i].name.Equals(Name))
                {
                    return AudioEvents[i];
                }
            }
            throw new IndexOutOfRangeException();
        }

        public void PlaySound(string SoundEffect)
        {
            AudioEmmitter sound = GetSound(SoundEffect);
            if(sound.emitter.IsPlaying())
            {
                sound.emitter.Stop();
            }
            sound.emitter.Play();

        }

        public void StopSound(string SoundEffect)
        {
            GetSound(SoundEffect).emitter.Stop();
        }

        public void SetParameter(string SoundEffect,string Parameter, float value)
        {
            GetSound(SoundEffect).emitter.SetParameter(Parameter, value);
        }

        public void SetParameter(string SoundEffect, string Parameter, int value)
        {
            GetSound(SoundEffect).emitter.SetParameter(Parameter, value);
        }

        public void SetGlobalParameter(string Parameter, float value)
        {
            RuntimeManager.StudioSystem.setParameterByName(Parameter, value);
        }

        public void SetGlobalParameter(string Parameter, int value)
        {
            RuntimeManager.StudioSystem.setParameterByName(Parameter, value);
        }

        public void SetParameterOverTime(string SoundEffect, string Parameter, float value, float time)
        {
            AudioEmmitter sound = GetSound(SoundEffect);
            for (int i = 0; i < sound.parameters.Count; i++)
            {
                ParameterTarget parameter = sound.parameters[i];
                if (parameter.name.Equals(Parameter))
                {
                    sound.emitter.EventInstance.getParameterByName(Parameter, out parameter.oldValue);
                    parameter.targetValue = value;
                    parameter.lerpTime = time;
                    parameter.currentLerpTime = time;
                    return;
                }
            }
            ParameterTarget newParameter = new ParameterTarget();
            newParameter.name = Parameter;
            sound.emitter.EventInstance.getParameterByName(Parameter, out newParameter.oldValue);
            newParameter.targetValue = value;
            newParameter.lerpTime = time;
            newParameter.currentLerpTime = time;
            sound.parameters.Add(newParameter);
        }

        public void SetParameterOverTime(string SoundEffect, string Parameter, int value, float time)
        {
            SetParameterOverTime(SoundEffect, Parameter, (float)value,time);
        }

        public void MasterVolumeLevel(float volume)
        {
            MasterVolume = volume;
        }

        public void MusicVolumeLevel(float volume)
        {
            MusicVolume = volume;
        }

        public void SFXVolumeLevel(float volume)
        {
            SFXVolume = volume;
        }
    }
}
