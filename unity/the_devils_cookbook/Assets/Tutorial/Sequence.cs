using System.Threading.Tasks;
using UnityEngine;

namespace TDC.Tutorial
{
    public abstract class Sequence : MonoBehaviour
    {
        public abstract Task Run();
    }
}
