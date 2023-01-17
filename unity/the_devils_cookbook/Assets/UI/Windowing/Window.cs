using System.Threading.Tasks;
using UnityEngine;

namespace TDC.UI.Windowing
{
    public interface IWindow
    {
        Task OnOpen(bool shouldPlayAnimation);
        Task<bool> OnClose(bool shouldPlayAnimation);
    }

    public abstract class Window : MonoBehaviour, IWindow
    {
        public abstract Task OnOpen(bool shouldPlayAnimation);
        public abstract Task<bool> OnClose(bool shouldPlayAnimation);
    }
}
