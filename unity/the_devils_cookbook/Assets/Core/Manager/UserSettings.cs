using System.Threading.Tasks;
using UnityEngine;

namespace TDC.Core.Manager
{
    public class UserSettings : GameManagerSubsystem
    {
        public enum DashDirectionMode
        {
            TowardsMovement,
            TowardsCursor
        }

        public DashDirectionMode DashMode { get; set; }

        private bool _VSync;
        public bool VSync
        {
            get => _VSync;
            set
            {
                QualitySettings.vSyncCount = value ? 1 : 0;
                _VSync = value;
            }
        }

        public const int MinFrameRateLimit = 30;
        private int _FrameRateLimit;
        public int FrameRateLimit
        {
            get => _FrameRateLimit;
            set
            {
                int clamped = Mathf.Max(value, MinFrameRateLimit);
                Application.targetFrameRate = clamped;
                _FrameRateLimit = clamped;
            }
        }
        

        protected override Task OnInitialise()
        {
            VSync = SystemInfo.operatingSystemFamily != OperatingSystemFamily.Linux;
            FrameRateLimit = Screen.currentResolution.refreshRate;
            return Task.CompletedTask;
        }

        protected override void Reset() {}
    }
}