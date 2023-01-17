using System;
using System.IO;
using UnityEngine;

namespace TDC.Core.Utility
{
    public class PlayerLogCopier : MonoBehaviour
    {
        private void OnApplicationQuit()
        {
            if (Application.platform == RuntimePlatform.WindowsPlayer)
            {
                var envPath =
                    $"%USERPROFILE%\\AppData\\LocalLow\\{Application.companyName}\\{Application.productName}\\Player.log";
                string absolutePath = Environment.ExpandEnvironmentVariables(envPath);
                File.Copy(absolutePath, $"./TDC_{Application.version}.log");
            }
        }
    }
}
