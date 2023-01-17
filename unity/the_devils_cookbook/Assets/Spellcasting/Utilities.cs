using System;
using System.Collections.Generic;
using System.Linq;
using TDC.AIRefac;
using UnityEngine;

namespace TDC.Spellcasting
{
    public static class Utilities
    {
        public static bool CapsuleCastForAgents(Vector3 position, float radius)
        {
            return Physics.OverlapCapsule(position + Vector3.down, position + Vector3.up, radius)
                .Any(c => c.TryGetComponent(out Agent _));
        }

        public static void PollCapsuleForAgents(ref List<Agent> agents, Vector3 position, float radius)
        {
            agents.Clear();
            foreach (Collider collider in Physics.OverlapCapsule(position + Vector3.down, position + Vector3.up, radius))
            {
                if (!collider.TryGetComponent(out Agent agent)) continue;
                agents.Add(agent);
            }
        }

        public static void PollCapsuleForNewAgents(ref Dictionary<Collider, Agent> agents, Vector3 position, float radius,
            Action<Agent> onNewAgent, Action<Agent> onExistingAgent)
        {
            foreach (Collider collider in Physics.OverlapCapsule(position + Vector3.down, position + Vector3.up, radius))
            {
                if (agents.TryGetValue(collider, out Agent agent))
                {
                    onExistingAgent?.Invoke(agent);
                    continue;
                }
                if (!collider.TryGetComponent(out agent)) continue;
                agents.Add(collider, agent);
                onNewAgent?.Invoke(agent);
            }
        }
    }
}