using TDC.Core.Manager;
using TMPro;
using UnityEngine;

namespace TDC
{
    public class MoneyCounter : MonoBehaviour
    {
        public TextMeshProUGUI MoneyText;

        private async void Start()
        {
            await GameManager.PlayerInitialised.WaitAsync();
            (await GameManager.PlayerCharacter.GetPlayerStats()).Currency.OnValueSet += UpdateText;
            UpdateText((await GameManager.PlayerCharacter.GetPlayerStats()).Currency.Value);
        }

        private async void OnDestroy()
        {
            (await GameManager.PlayerCharacter.GetPlayerStats()).Currency.OnValueSet -= UpdateText;
        }

        private void UpdateText(int money)
        {
            MoneyText.SetText(money.ToString());
        }
    }
}
