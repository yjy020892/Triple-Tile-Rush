using System.Collections.Generic;
using System.Text;
using UnityEngine;

public interface ISortingAnalyticsService
{
    void LogEvent(string eventName, Dictionary<string, object> parameters = null);
}

public sealed class SortingMockAnalyticsService : ISortingAnalyticsService
{
    public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
    {
        if (parameters == null || parameters.Count == 0)
        {
            Debug.Log($"[Analytics/Mock] {eventName}");
            return;
        }

        StringBuilder builder = new StringBuilder();
        bool first = true;
        foreach (KeyValuePair<string, object> pair in parameters)
        {
            if (!first)
            {
                builder.Append(", ");
            }

            builder.Append(pair.Key);
            builder.Append('=');
            builder.Append(pair.Value);
            first = false;
        }

        Debug.Log($"[Analytics/Mock] {eventName} {{{builder}}}");
    }
}
