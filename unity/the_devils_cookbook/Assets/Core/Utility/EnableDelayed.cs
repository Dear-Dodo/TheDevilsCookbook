using System.Threading.Tasks;
using UnityAsync;
using UnityEngine;

namespace TDC.Core.Utility
{
    public class EnableDelayed : MonoBehaviour
    {
        [System.Serializable]
        public class Entry
        {
            public GameObject Object;
            public float Delay;
            public bool Enable;
        }

        public bool PlayOnStart;
        public Entry[] Objects;

        public void Start()
        {
            if (PlayOnStart)
            {
                Run();
            }
        }

        public void Run()
        {
            foreach (var obj in Objects)
            {
                EvaluateComponent(obj);
            }
        }

        private async Task EvaluateComponent(Entry obj)
        {
            await Await.Seconds(obj.Delay);
            obj.Object.SetActive(obj.Enable);
        }
    }
}