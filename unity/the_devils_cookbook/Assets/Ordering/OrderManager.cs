using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TDC.Cooking;
using TDC.Core.Extension;
using TDC.Core.Manager;
using TDC.Items;
using TDC.Level;
using TDC.Patrons;
using UnityAsync;
using UnityEngine;

namespace TDC.Ordering
{
    [Serializable]
    public class OrderManager : GameManagerSubsystem
    {
        public List<StorableObject> Ingredients;
        public List<Order> ActiveOrders = new List<Order>();
        public List<Order> CompletedOrders = new List<Order>();
        public int FailedOrders;
        public int RemainingOrders => GameManager.CurrentLevelData.OrderCount - FailedOrders - CompletedOrders.Count;
        public event Action<Order> OnOrderCreated;
        public event Action<Order> OnOrderDeleted;
        public event Action<Order, bool> OnOrderCompleted;
        
        private CancellationTokenSource _TokenSource = new CancellationTokenSource();
        private Func<bool, Task> _OnLevelEnd;

        /// <summary>
        /// Generate next order from the available recipes.
        /// </summary>
        /// <returns></returns>
        public Task<Order> CreateNextOrder()
        { 
            Recipe recipe = GameManager.CurrentLevelData.RecipePool.Random(GameManager.GameRandom);
            return CreateNextOrder(recipe);
        }

        /// <summary>
        /// Generate the next order with the given <paramref name="recipe"/>.
        /// </summary>
        /// <param name="recipe"></param>
        /// <param name="window"></param>
        /// <returns></returns>
        public async Task<Order> CreateNextOrder(Recipe recipe, PatronWindow window = null)
        {
            if (!GameManager.PatronManager.CreatePatron(recipe, out Patron patron, window)) return null;
            
            await Await.Seconds(patron.SpawnClip.length);
            patron.Ready = true;
            patron.FinishEntrance();
            patron.WindowPosition = patron.RootBone.transform.position;

            window ??= GameManager.PatronManager.GetWindow(patron);
            var order = new Order()
            {
                Food = recipe,
                Time = GameManager.CurrentLevelData.TimeDifficulties[recipe.Difficulty]
            };
            order.Initialise(patron, window);

            ActiveOrders.Add(order);
            OnOrderCreated?.Invoke(order);
            return order;
        }
        
        public async Task CompleteOrder(Order order, bool orderSuccessful)
        {
            if (orderSuccessful)
            {
                CompletedOrders.Add(order);
            } else
            {
                FailedOrders++;
            }

            order.Complete(orderSuccessful);
            
            if (ActiveOrders.Remove(order))
            {
                OnOrderCompleted?.Invoke(order, orderSuccessful);
                OnOrderDeleted?.Invoke(order);
                await order.ProcessRemoval(orderSuccessful);
            }
        }

        public async Task Begin()
        {
            try
            {
                await BeginLevelAutomatic(_TokenSource.Token);
            }
            catch (OperationCanceledException) {}
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }

        public void Cancel()
        {
            _TokenSource.Cancel();
            _TokenSource = new CancellationTokenSource();
        }

        public void CompleteAllOrders()
        {
            for (int i = ActiveOrders.Count - 1; i >= 0; i--)
            {
                Order activeOrder = ActiveOrders[i];
                _ = GameManager.PatronManager.RemovePatron(activeOrder.Patron, true);
                _ = CompleteOrder(activeOrder, false);
            }
        }
        
        public async Task Update()
        {
            for (var index = 0; index < ActiveOrders.Count; index++)
            {
                Order activeOrder = ActiveOrders[index];
                activeOrder.Update();

                if (activeOrder.IsCompleted())
                {
                    await CompleteOrder(activeOrder, activeOrder.Successful);
                }
                if (!GameManager.LevelRunning)
                {
                    GameManager.PatronManager.RemovePatron(activeOrder.Patron,true);
                    await CompleteOrder(activeOrder, false);
                }
            }
        }

        public void BeginLevelManual()
        {
            CompletedOrders = new List<Order>();
            ActiveOrders = new List<Order>();
            FailedOrders = 0;

            _OnLevelEnd = (win) =>
            {
                if (!win) FailedOrders += RemainingOrders;
                GameManager.OnLevelEnd -= _OnLevelEnd;
                return Task.CompletedTask;
            };
            GameManager.OnLevelEnd += _OnLevelEnd;
        }
        public async Task BeginLevelAutomatic(CancellationToken token)
        {
            _OnLevelEnd = (win) => {
                if (win) return Task.CompletedTask;
                Cancel();
                FailedOrders += RemainingOrders;
                GameManager.OnLevelEnd -= _OnLevelEnd;
                return Task.CompletedTask; 
            };
            GameManager.OnLevelEnd += _OnLevelEnd;
            CompletedOrders = new List<Order>();
            ActiveOrders = new List<Order>();
            FailedOrders = 0;

            await CreateNextOrder();
            
            while (!token.IsCancellationRequested)
            {
                LevelData data = GameManager.CurrentLevelData;
                if (data == null)
                {
                    continue;
                }
                if (data.OrderCount == -1 || CompletedOrders.Count + ActiveOrders.Count + FailedOrders < data.OrderCount)
                {
                    float timeDelay = data.OrderSpawnRange.Random();
                    await Await.Seconds(timeDelay).ConfigureAwait(token);

                    token.ThrowIfCancellationRequested();

                    await CreateNextOrder();
                } else
                {
                    break;
                }
            }
        }

        protected override Task OnInitialise()
        {
            return Task.CompletedTask;
        }

        protected override void Reset()
        {
            ActiveOrders.Clear();
            CompletedOrders.Clear();
            Cancel();
        }
    }
}