using System;
using System.Collections.Generic;
using System.Linq;

namespace Parang.UI
{
    public class UIEventManager<T> : Singleton<UIEventManager<T>> where T : struct
    {
        private readonly Dictionary<UIStackBehaviour, List<(T, Action<object[]>)>> refers = new();
        private readonly Dictionary<T, List<Action<object[]>>> actions = new();

        public void Register(UIStackBehaviour ui, T evt, Action<object[]> action)
        {
            if (!refers.TryGetValue(ui, out var refer))
            {
                refer = new List<(T, Action<object[]>)>();
                refers.Add(ui, refer);
            }
            refer.Add((evt, action));
            if (actions.ContainsKey(evt))
                actions[evt].Add(action);
            else
                actions.Add(evt, new List<Action<object[]>> { action });
        }

        public void Register(UIStackBehaviour ui, Dictionary<T, Action<object[]>> evts)
        {
            if (!refers.TryGetValue(ui, out var refer))
            {
                refer = new List<(T, Action<object[]>)>();
                refers.Add(ui, refer);
            }

            foreach (var evt in evts)
            {
                refer.Add((evt.Key, evt.Value));
                if (actions.ContainsKey(evt.Key))
                    actions[evt.Key].Add(evt.Value);
                else
                    actions.Add(evt.Key, new List<Action<object[]>> { evt.Value });
            }
        }

        public void Unregister(UIStackBehaviour ui)
        {
            if (refers.TryGetValue(ui, out var refer))
            {
                foreach (var r in refer)
                    actions[r.Item1].Remove(r.Item2);
            }
            refers.Remove(ui);
        }

        public void Event(T evt, params object[] args)
        {
            if (actions.TryGetValue(evt, out var action))
            {
                foreach (var a in action.ToList()) a.Invoke(args);
            }
        }
    }
}
