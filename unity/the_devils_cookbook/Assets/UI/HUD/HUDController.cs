using System.Threading.Tasks;
using TDC.UI.Windowing;

namespace TDC.UI.HUD
{
    public class HUDController : Window
    {
        private bool _CurrentlyHidden;
        
        public override Task OnOpen(bool shouldPlayAnimation)
        {
            return Task.CompletedTask;
        }

        public override Task<bool> OnClose(bool shouldPlayAnimation)
        {
            return Task.FromResult(true);
        }
    }
}