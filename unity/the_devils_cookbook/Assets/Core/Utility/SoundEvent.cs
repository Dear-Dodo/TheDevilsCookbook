using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TDC
{
    [Serializable]
    public struct AutomationParameter
    {
        public string Name;
        public float Value;
    }
    public class SoundEvent : MonoBehaviour
    {
        public EventReference AudioEvent;

        public List<AutomationParameter> Parameters = new List<AutomationParameter>();

        public void Play()
        {
            EventInstance eventInstance = RuntimeManager.CreateInstance(AudioEvent);
            eventInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
            eventInstance.start();
            eventInstance.release();
        }
    }
}
