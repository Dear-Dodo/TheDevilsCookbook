using FMOD.Studio;
using FMODUnity;
using System;
using System.Threading.Tasks;
using TDC.Core.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TDC.UI
{
    public class LoseScreen : MonoBehaviour
    {
        public GameObject Panel;
        public TextMeshProUGUI OrdersCompletedText;
        public TextMeshProUGUI OrderCompletionText;
        public TextMeshProUGUI CatchBonusText;
        public TextMeshProUGUI TotalMoneyText;
        public TextMeshProUGUI TotalMoneyCharecterText;
        public Button ShopButton;
        public Button RetryButton;
        public EventReference LoseSFX;

        private Func<bool, Task> _OnLevelEnd;

        // Start is called before the first frame update
        private void Start()
        {
            GameManager.RunOnInitialisation(Initialise);

        }

        private void Initialise()
        {
            Panel.SetActive(false);
            _OnLevelEnd = OnLevelEnd;
            GameManager.OnLevelEnd += _OnLevelEnd;

            if (GameManager.SceneLoader.TryGetScene("Shop", out SceneEntry _))
            {
                ShopButton.onClick.AddListener(async () => {
                    GameManager.AudioManager.SetParameterOverTime("LevelMusic", "fade", 0, 1);
                    await GameManager.SceneLoader.LoadScene("Shop");
                    GameManager.AudioManager.SetParameter("ShopMusic", "fade", 1);
                    GameManager.AudioManager.PlaySound("ShopMusic");
                });
            }
            else
            {
                ShopButton.onClick.AddListener(() => { GameManager.SceneLoader.LoadScene("Main Menu"); });
            }

            RetryButton.onClick.AddListener(async () => {
                GameManager.StartLevel(GameManager.CurrentLevel);
            });
        }
        
        private void OnDestroy()
        {
            GameManager.OnLevelEnd -= _OnLevelEnd;
        }

        private async Task OnLevelEnd(bool win)
        {
            if (!win)
            {
                Panel.SetActive(true);
                EventInstance SFX = RuntimeManager.CreateInstance(LoseSFX);
                SFX.start();
                SFX.release();
                int OrderCompletion = GameManager.CurrentLevelData.CurrencyEarned;
                int CatchBonus = GameManager.CurrentLevelData.CatchBonus;

                OrdersCompletedText.text = string.Format("Completed {0} of {1} Orders", GameManager.OrderManager.CompletedOrders.Count, GameManager.CurrentLevelData.OrderCount);
                OrderCompletionText.text = string.Format("Order Completion: {0}", OrderCompletion);
                CatchBonusText.text = string.Format("Catch Bonus: {0}", CatchBonus);
                TotalMoneyText.text = string.Format("Total: {0}", (OrderCompletion + CatchBonus ));
                TotalMoneyCharecterText.text = string.Format("Balance: {0}", (await GameManager.PlayerCharacter.GetPlayerStats()).Currency.Value);
            }
        }

        // Update is called once per frame
        private void Update()
        {
        }
    }
}