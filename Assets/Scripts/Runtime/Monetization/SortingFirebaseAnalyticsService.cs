#if SORTING_FIREBASE
using System.Collections.Generic;
using Firebase;
using Firebase.Analytics;
using UnityEngine;

//
public sealed class SortingFirebaseAnalyticsService : ISortingAnalyticsService
{
    private bool ready;
    private readonly Queue<System.Action> pending = new Queue<System.Action>();

    public SortingFirebaseAnalyticsService()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.Result != DependencyStatus.Available)
            {
                Debug.LogWarning($"[Firebase] Dependencies not available: {task.Result}");
                return;
            }
            ready = true;
            while (pending.Count > 0)
            {
                pending.Dequeue()?.Invoke();
            }
        });
    }

    public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
    {
        void Send()
        {
            if (parameters == null || parameters.Count == 0)
            {
                FirebaseAnalytics.LogEvent(eventName);
                return;
            }

            List<Parameter> list = new List<Parameter>(parameters.Count);
            foreach (KeyValuePair<string, object> pair in parameters)
            {
                switch (pair.Value)
                {
                    case null: list.Add(new Parameter(pair.Key, "")); break;
                    case string s: list.Add(new Parameter(pair.Key, s)); break;
                    case int i: list.Add(new Parameter(pair.Key, i)); break;
                    case long l: list.Add(new Parameter(pair.Key, l)); break;
                    case float f: list.Add(new Parameter(pair.Key, f)); break;
                    case double d: list.Add(new Parameter(pair.Key, d)); break;
                    case bool b: list.Add(new Parameter(pair.Key, b ? 1 : 0)); break;
                    default: list.Add(new Parameter(pair.Key, pair.Value.ToString())); break;
                }
            }
            FirebaseAnalytics.LogEvent(eventName, list.ToArray());
        }

        if (ready) Send();
        else pending.Enqueue(Send);
    }
}
#endif
