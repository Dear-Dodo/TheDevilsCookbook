using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TDC.Cooking;
using TDC.Core.Extension;
using TDC.Core.Manager;
using TDC.Core.Utility;
using TDC.ThirdParty.SerializableDictionary;
using UnityAsync;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TDC.Patrons
{
    [Serializable]
    public class PatronManager : GameManagerSubsystem
    {
        [Serializable]
        public struct PatronPrefabEntry
        {
            public List<Patron> Prefabs;
            public List<Color> BaseColours;
            public List<Color> TailColours;
        }

        public SerializableDictionary<Difficulty, PatronPrefabEntry> PatronPrefabs;

        [Header("Do not modify. For debug purposes.")]
        public List<PatronWindow> PatronWindows = new List<PatronWindow>();

        public List<Patron> Patrons = new List<Patron>();

        public Action OnPatronRemoved;

        public Action OnAllPatronsRemoved;
        public SerializableDictionary<Difficulty, int> DifficultyRewards;

        public bool CreatePatron(Recipe recipe, out Patron patron, PatronWindow window = null)
        {
            patron = null;
            if (window?.Avaliable == false)
            {
                Debug.LogError($"CreatePatron was called with an explicit window that was unavailable.");
                return false;
            }
            if (PatronPrefabs.TryGetValue(recipe.Difficulty, out PatronPrefabEntry pool))
            {
                Patron patronObject = pool.Prefabs.Random(GameManager.GameRandom);
                window ??= GetRandomAvaliableWindow();
                if (window != null && patronObject != null)
                {
                    patron = Object.Instantiate(patronObject.gameObject).GetComponent<Patron>();
                    Renderer patronRenderer = patron.gameObject.GetComponentInChildren<Renderer>();
                    Material patronMat = Object.Instantiate(patronRenderer.material);
                    patronMat.SetColor("_BaseColor", pool.BaseColours.Random(GameManager.GameRandom));
                    patronMat.SetColor("_TailColor", pool.TailColours.Random(GameManager.GameRandom));
                    patronRenderer.material = patronMat;
                    window.SetOccupant(patron);
                    Patrons.Add(patron);
                    return true;
                }
            }
            return false;
        }

        public async Task RemovePatron(Patron patron, bool leave)
        {
            if (TryGetWindow(patron, out PatronWindow window))
            {
                window.RemoveOccupant(GameManager.CurrentLevelData.WindowCooldown);

                if (leave)
                {
                    float money = (1 - patron.Patience);
                    money *= DifficultyRewards[patron.Order.Food.Difficulty];
                    GameManager.CurrentLevelData.CurrencyEarned += (int)money;
                    (await GameManager.PlayerCharacter.GetPlayerStats()).Currency.Value += (int)money;
                    await patron.PatronLeave();
                    if (!Patrons.Remove(patron))
                    {
                        Debug.LogError($"Failed to remove patron: {patron.name}.");
                        return;
                    }
                    if (Patrons.Count == 0 && !GameManager.LevelRunning)
                    {
                        OnAllPatronsRemoved?.Invoke();
                    }
                    await Await.Seconds(5);
                    Object.Destroy(patron.gameObject);
                    return;
                }
                await patron.PatronAttack();
                OnPatronRemoved?.Invoke();
                if (!Patrons.Remove(patron))
                {
                    Debug.LogError($"Failed to remove patron: {patron.name}.");
                    return;
                }

                if (Patrons.Count == 0 && !GameManager.LevelRunning)
                {
                    OnAllPatronsRemoved?.Invoke();
                }
            }
        }

        public bool TryGetWindow(Patron patron, out PatronWindow window)
        {
            window = GetWindow(patron);
            return window != null;
        }

        public PatronWindow GetWindow(Patron patron) =>
            PatronWindows.FirstOrDefault(patronWindow => patronWindow.Occupant == patron);

        public PatronWindow GetRandomAvaliableWindow() => GetAvaliableWindows()?.Random();

        public PatronWindow GetAvaliableWindow() => PatronWindows.FirstOrDefault(window => window.Avaliable);

        public PatronWindow[] GetAvaliableWindows() => PatronWindows.Where(window => window.Avaliable).ToArray();

        public PatronWindow[] GetOccupiedWindows() => PatronWindows.Where(window => !window.Avaliable).ToArray();

        protected override Task OnInitialise() => Task.CompletedTask;

        protected override void Reset()
        {
            PatronWindows.Clear();
            Patrons.Clear();
        }
    }
}