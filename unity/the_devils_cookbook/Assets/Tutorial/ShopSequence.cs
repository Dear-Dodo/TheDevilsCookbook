using System.Threading.Tasks;
using TDC.Core.Manager;
using TDC.Player;
using TDC.UI.Dialogue;
using UnityEngine;

namespace TDC.Tutorial
{
    public class ShopSequence : Sequence
    {
        [SerializeField] private DialogueData _ShopDialogue;

        public async void Start()
        {
            await GameManager.InitialisedAsync.WaitAsync();
            await Run();
        }

        public override async Task Run()
        {
            await GameManager.PlayerInitialised.WaitAsync();
            PlayerStats playerStats = await GameManager.PlayerCharacter.GetPlayerStats();

            if (!playerStats.ShopTutorialPlayed)
            {
                await GameManager.DialogueSystem.Run(_ShopDialogue);
                playerStats.ShopTutorialPlayed = true;
            }
        }
    }
}