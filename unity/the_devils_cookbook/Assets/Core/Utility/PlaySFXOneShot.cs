using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace TDC.Core.Utility
{
    public static class SFXHelper
    {
        public static async void PlayOneshot(EventReference soundEvent, GameObject gameObject)
        {
            EventInstance eventInstance = RuntimeManager.CreateInstance(soundEvent);
            eventInstance.set3DAttributes(gameObject.To3DAttributes());
            eventInstance.start();
            eventInstance.release();
        }
    }
}
