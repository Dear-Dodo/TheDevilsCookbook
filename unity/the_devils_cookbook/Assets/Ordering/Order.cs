using System;
using System.Threading.Tasks;
using Nito.AsyncEx;
using TDC.Cooking;
using TDC.Core.Manager;
using TDC.Patrons;
using UnityEngine;

namespace TDC.Ordering
{
    public class Order
    {
        public Recipe Food;
        public Patron Patron;
        public float ElapsedTime;
        public float Time;
        public bool Successful;

        public bool IsPaused;

        public Action<bool> OnCompleted;
        public AsyncManualResetEvent CompletedAsync = new AsyncManualResetEvent();
        public Action<float, float> OnTimerUpdated;

        public PatronWindow PatronWindow { get; private set; }

        public void Initialise(Patron patron, PatronWindow window)
        {
            Patron = patron;
            PatronWindow = window;
            patron.Order = this;
        }
        // public Order(Patron patron, PatronWindow window)
        // {
        //     Patron = patron;
        //     patron.Order = this;
        //     PatronWindow = window;
        // }

        public void Update()
        {
            if (Patron == null) return;
            if (!Patron.Ready) return;
            if (IsPaused) return;

            ElapsedTime += UnityEngine.Time.deltaTime;
            Patron.Patience = Core.Utility.Math.Percentage(ElapsedTime, Time);
            Patron.transform.position = Vector3.Lerp(Patron.WindowPosition, PatronWindow.Destination.transform.position, Patron.Patience);
            OnTimerUpdated?.Invoke(ElapsedTime, UnityEngine.Time.deltaTime);
        }

        public void Complete(bool successful)
        {
            OnCompleted?.Invoke(successful);
            CompletedAsync.Set();
        }
        
        public async Task ProcessRemoval(bool leave)
        {
            await GameManager.PatronManager.RemovePatron(Patron, leave);
        }

        public bool IsCompleted() => (ElapsedTime >= Time) || Successful;
    }
}