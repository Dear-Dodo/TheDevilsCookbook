using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Object = UnityEngine.Object;

namespace TDC.UI.Windowing
{
    public class WindowNode
    {
        public Window WindowInstance;
        public WindowEntry Entry;
        public List<WindowNode> Subwindows = new List<WindowNode>();
        public event Action OnClosed;

        public async Task<bool> Close(bool waitForSubwindows)
        {
            foreach (WindowNode node in Subwindows)
            {
                if (waitForSubwindows) await node.Close(true);
                else node.CloseImmediate();
            }

            bool closed = await WindowInstance.OnClose(true);
            if (closed)
            {
                if (!Entry.IsSingle) Object.Destroy(WindowInstance.gameObject);
                else WindowInstance.gameObject.SetActive(false);
                OnClosed?.Invoke();
            }
            return closed;
        }

        public void CloseImmediate()
        {
            foreach (WindowNode node in Subwindows)
            {
                node.CloseImmediate();
            }

            WindowInstance.OnClose(false);
            if (!Entry.IsSingle) Object.Destroy(WindowInstance.gameObject);
            else WindowInstance.gameObject.SetActive(false);
        }
    }
}