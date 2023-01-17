using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TDC.Core.Manager;
using TDC.UI.Windowing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TDC.Player.Upgrades
{
    public class PlayerUpgradeWindow : Window
    {
        [SerializeField] public Button SpellUpgrades;
        [SerializeField] public Button Loadouts;
        [SerializeField] public GameObject SpellUpgradesPanel;
        [SerializeField] public GameObject LoadoutsPanel;
        [SerializeField] public ScrollRect UpgradeView;
        [SerializeField] public PlayerUpgradeElementUI UpgradeElementTemplate;
        [SerializeField] public TextMeshProUGUI Money;
        [SerializeField] public Button Close;

        private List<PlayerUpgradeElementUI> _UpgradeElements = new List<PlayerUpgradeElementUI>();

        public async void Start()
        {
            await GameManager.CachePlayer();
            await OnOpen(false);
            SpellUpgrades.onClick.AddListener(() => { SpellUpgradesPanel.SetActive(true); LoadoutsPanel.SetActive(false); });
            Loadouts.onClick.AddListener(() => { SpellUpgradesPanel.SetActive(false); LoadoutsPanel.SetActive(true); });
            Close.onClick.AddListener(() => {
                GameManager.AudioManager.SetParameterOverTime("ShopMusic", "fade", 0, 1);
                GameManager.SceneLoader.LoadScene("Main Menu");
            });
        }

        public async override Task OnOpen(bool shouldPlayAnimation)
        {
            try
            {
                PlayerStats stats = await GameManager.PlayerCharacter.GetPlayerStats();
                if (stats != null)
                {
                    Initialize(stats);
                }
            } catch (Exception e)
            {
                throw e;
            }
        }

        public override Task<bool> OnClose(bool shouldPlayAnimation)
        {
            Uninitialize();
            return Task.FromResult(true);
        }

        private void Initialize(PlayerStats stats)
        {
            foreach (var kvp in stats.SpellUpgrades)
            {
                PlayerUpgradeElementUI copy = Instantiate(UpgradeElementTemplate, Vector3.zero, Quaternion.identity);

                copy.Initialize(kvp.Value, stats);
                copy.gameObject.SetActive(true);
                copy.transform.SetParent(UpgradeView.content);
                copy.transform.localScale = new Vector3(1, 1, 1);

                _UpgradeElements.Add(copy);
            }
        }

        private void Uninitialize()
        {
            foreach (var upgradeElement in _UpgradeElements)
            {
                Destroy(upgradeElement);
            }

            _UpgradeElements.Clear();
        }

        private async void Update()
        {
            Money.text = string.Format("Balance: <br> {0}", (await GameManager.PlayerCharacter.GetPlayerStats()).Currency.Value);
        }
    }
}