using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace TDC.Core.Utility
{
    public class SFXEmitter : MonoBehaviour
    {
        [SerializeField] public EventReference SoundEvent;

        public void Play()
        {
            EventInstance soundEvent = RuntimeManager.CreateInstance(SoundEvent);
            soundEvent.set3DAttributes(gameObject.To3DAttributes());
            soundEvent.start();
            soundEvent.release();
        }
    }
}