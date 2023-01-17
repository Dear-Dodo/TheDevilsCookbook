using TDC.Core.Manager;
using UnityAsync;
using UnityEngine;
using UnityEngine.UI;
using Utility;

namespace TDC.Patrons
{
    public class PatronWindow : MonoBehaviour
    {
        public Patron Occupant;
        public GameObject SpawnPoint;
        public GameObject MiddleOfWindow;
        public GameObject Destination;
        public Image Identifier;

        [HideInInspector]
        public bool Avaliable;

        public Sprite WindowIdentificationSprite;
        public Texture MinimapIDIcon;
        public Vector3 MinimapIconOffset;

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawSphere(transform.position + transform.localToWorldMatrix.MultiplyVector(MinimapIconOffset), 1);
        }

        public void Awake() {
            Identifier.sprite = WindowIdentificationSprite;
            Avaliable = true;
            GameManager.RunOnInitialisation(() => GameManager.PatronManager?.PatronWindows.Add(this));

            MinimapIconOffset = GetComponent<Collider>().bounds.center - transform.position;
        }

        public void SetOccupant(Patron patron)
        {
            Avaliable = false;
            patron.transform.position = SpawnPoint.transform.position;
            patron.transform.right = (SpawnPoint.transform.position - MiddleOfWindow.transform.position).xoz().normalized;
            Occupant = patron;
        }

        public async void RemoveOccupant(float cooldown)
        {
            Occupant = null;
            await Await.Seconds(cooldown);
            Avaliable = true;
        }
    }
}