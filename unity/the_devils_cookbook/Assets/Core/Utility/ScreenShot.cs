using System.Collections;
using System.IO;
using TDC.Core.Manager;
using UnityEngine;

namespace TDC.Core.Utility
{
    public class ScreenShot : MonoBehaviour
    {
        private bool isPerformingScreenGrab;
        private int screenshotCount = 0;

        private void Start()
        {
            System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/Screenshots");
            string[] files = Directory.GetFiles(Application.persistentDataPath + "/Screenshots");
            foreach (string fileName in files)
            {
                if (int.TryParse(fileName.Substring((Application.persistentDataPath + "/Screenshots/Screenshot_").Length,
                    fileName.Length - (Application.persistentDataPath + "/Screenshots/Screenshot_").Length - 4),out int number) &&
                    number >= screenshotCount)
                {
                    screenshotCount = number + 1;
                }
            }
        }

        void Update()
        {
            if (GameManager.PlayerControls.UI.Screenshot.WasPerformedThisFrame())
            {
                StartCoroutine(TakeScreenShot());
            }
        }

        IEnumerator TakeScreenShot()
        {

            yield return new WaitForEndOfFrame();

            int width = Screen.width;
            int height = Screen.height;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);

            // Read screen contents into the texture
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            // Encode texture into PNG
            byte[] bytes = tex.EncodeToPNG();
            Destroy(tex);
            if (!File.Exists(Application.persistentDataPath + "/Screenshots/Screenshot_" + screenshotCount + ".png"))
                File.Create(Application.persistentDataPath + "/Screenshots/Screenshot_" + screenshotCount + ".png").Close();
            yield return new WaitUntil(() => { return File.Exists(Application.persistentDataPath + "/Screenshots/Screenshot_" + screenshotCount + ".png"); });
            File.WriteAllBytes(Application.persistentDataPath + "/Screenshots/Screenshot_" + screenshotCount + ".png", bytes);
            Debug.Log(Application.persistentDataPath + "/Screenshots/Screenshot_" + screenshotCount + ".png" + " Saved");
            screenshotCount++;
            isPerformingScreenGrab = false;
        }
    }
}
