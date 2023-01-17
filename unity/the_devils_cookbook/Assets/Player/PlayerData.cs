using UnityEngine;

namespace TDC.Player
{
    [System.Serializable]
    public class PlayerData
    {
        [SerializeField] public PlayerStats PlayerStats;

        public void Initialize()
        {
            PlayerStats.Initialize();
        }

    }
}