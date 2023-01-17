using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TDC.Ingredient;
using Unity.Mathematics;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TDC.Core.Manager
{
    public class CreatureManager : GameManagerSubsystem
    {
        private static LinkedList<Creature> _ActiveCreatures = new LinkedList<Creature>();
        public static IEnumerable<Creature> ActiveCreatures => _ActiveCreatures;

        private static Dictionary<Creature, LinkedListNode<Creature>> _NodesByCreature =
            new Dictionary<Creature, LinkedListNode<Creature>>();

        public static event Action<Creature> CreatureCreated;
        public static event Action<Creature> CreatureDestroyed;

        public static Creature CreateCreature(Creature prefab, Vector3 position)
        {
            var instance = Object.Instantiate(prefab.gameObject, position, quaternion.identity).GetComponent<Creature>();
            _NodesByCreature.Add(instance, _ActiveCreatures.AddLast(instance));
            CreatureCreated?.Invoke(instance);
            return instance;
        }

        public static void DestroyCreature(Creature target)
        {
            Object.Destroy(target.gameObject);
            if (!_NodesByCreature.TryGetValue(target, out LinkedListNode<Creature> targetNode))
            {
                throw new KeyNotFoundException("Attempted to destroy creature that was not registered");
            }

            _NodesByCreature.Remove(target);
            _ActiveCreatures.Remove(targetNode);
            CreatureDestroyed?.Invoke(target);
        }

        protected override Task OnInitialise()
        {
            GameManager.SceneLoader.OnSceneLoadStarted += ResetOnSceneLoad;
            return Task.CompletedTask;
        }

        private void ResetOnSceneLoad(SceneEntry _)
        {
            _ActiveCreatures.Clear();
            _NodesByCreature.Clear();
        }
    }
}