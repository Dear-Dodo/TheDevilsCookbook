using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TDC.Core.Manager;
using TDC.Core.Type;
using TDC.ThirdParty.SerializableDictionary;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TDC.UI.Windowing
{
    [Serializable]
    public class WindowManager : GameManagerSubsystem
    {
        public const uint HistoryLimit = 5;

        [SerializeField] private SerializableDictionary<string, WindowEntry> _Windows;
        private Dictionary<Type, WindowEntry> _WindowsByType;
        private readonly SortedList<int, List<WindowNode>> _ActiveWindows = new SortedList<int, List<WindowNode>>();
        private readonly SortedList<int, DropOutStack<List<WindowEntry>>> _History = new SortedList<int, DropOutStack<List<WindowEntry>>>();

        // TODO: Clear active windows on scene change

        public async Task CloseAll(int layer, bool addToHistory = true)
        {
            if (!_ActiveWindows.ContainsKey(layer)) return;

            if (addToHistory)
            {
                if (!_History.ContainsKey(layer)) _History.Add(layer, new DropOutStack<List<WindowEntry>>(HistoryLimit));
                _History[layer].Push(_ActiveWindows[layer].Select(n => n.Entry).ToList());
            }
            List<Task<bool>> closeTasks = _ActiveWindows[layer].Select(node => node.Close(false)).ToList();
            await Task.WhenAll(closeTasks);
        }

        public async Task Close(Window toClose)
        {
            Type winType = toClose.GetType();
            if (!_WindowsByType.TryGetValue(winType, out WindowEntry entry)) 
                throw new KeyNotFoundException($"No window of type {winType} registered.");
            
            var notOpenError = $"Window of type {winType} was not open on layer {entry.InitialLayer}";
            if (!_ActiveWindows.TryGetValue(entry.InitialLayer, out List<WindowNode> nodes))
                throw new KeyNotFoundException(notOpenError);
            
            WindowNode node = nodes.Find(n => n.Entry == entry);
            if (node == null) throw new KeyNotFoundException(notOpenError);

            nodes.Remove(node);
            await node.Close(false);
        }

        public async Task<WindowNode> OpenReplace(int layer, string windowKey)
        {
            WindowEntry entry = GetWindowEntry(windowKey);

            if (_ActiveWindows.ContainsKey(layer)) await CloseAll(layer);

            return await OpenInternal(layer, entry);
        }

        public async Task<WindowNode> OpenAdditive(int layer, string windowKey)
        {
            WindowEntry entry = GetWindowEntry(windowKey);
            return await OpenInternal(layer, entry);
        }

        public async Task<WindowNode> OpenSubwindow(WindowNode parent, string windowKey)
        {
            WindowEntry toOpen = GetWindowEntry(windowKey);

            WindowNode node = CreateNode(toOpen);
            parent.Subwindows.Add(node);
            node.WindowInstance.transform.SetParent(parent.WindowInstance.transform);
            await node.WindowInstance.OnOpen(true);
            return node;
        }

        public async Task<WindowNode> OpenReplace(string key)
        {
            WindowEntry entry = GetWindowEntry(key);
            int layer = entry.InitialLayer;
            if (_ActiveWindows.ContainsKey(layer)) await CloseAll(layer);
            return await OpenInternal(layer, entry);
        }

        public async Task<WindowNode> OpenAdditive(string key)
        {
            WindowEntry toOpen = GetWindowEntry(key);
            return await OpenInternal(toOpen.InitialLayer, toOpen);
        }

        public async Task NavigateBack(int layer)
        {
            if (!_History.ContainsKey(layer) || !_History[layer].TryPop(out List<WindowEntry> history)) return;
            await CloseAll(layer);

            List<Task<WindowNode>> openTasks = history.Select(entry => OpenInternal(layer, entry)).ToList();
            await Task.WhenAll(openTasks);
        }

        private static WindowNode CreateNode(WindowEntry entry)
        {
            Window window;
            if (entry.IsSingle)
            {
                // Instance, Find in scene, Create
                window = entry.Instance
                    ? entry.Instance
                    : (Window)Object.FindObjectOfType(entry.Window.GetType()) ??
                      Object.Instantiate(entry.Window.gameObject).GetComponent<Window>();
                entry.Instance = window;
            }
            else window = Object.Instantiate(entry.Window.gameObject).GetComponent<Window>();

            return new WindowNode()
            {
                Entry = entry,
                WindowInstance = window
            };
        }

        private async Task<WindowNode> OpenInternal(int layer, WindowEntry entry)
        {
            WindowNode node = CreateNode(entry);

            if (!_ActiveWindows.ContainsKey(layer)) _ActiveWindows.Add(layer, new List<WindowNode>());
            _ActiveWindows[layer].Add(node);
            node.WindowInstance.gameObject.SetActive(true);
            await node.WindowInstance.OnOpen(true);
            return node;
        }

        private WindowEntry GetWindowEntry(string key)
        {
            if (!_Windows.TryGetValue(key, out WindowEntry entry))
                throw new KeyNotFoundException($"No window with key '{key}' was registered.");
            return entry;
        }

        protected override Task OnInitialise()
        {
            _WindowsByType = new Dictionary<Type, WindowEntry>(_Windows.Count);
            foreach (WindowEntry entry in _Windows.Values)
            {
                _WindowsByType.Add(entry.Window.GetType(), entry);
            }

            return Task.CompletedTask;
        }

        protected override void Reset()
        {
            _ActiveWindows.Clear();
            _History.Clear();
        }
    }
}