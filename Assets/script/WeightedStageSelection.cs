using System;
using System.Collections.Generic;

// Pure selection logic: deterministic in tests and independent of Unity state.
public static class WeightedStageSelection
{
    public static List<T> Draw<T>(IEnumerable<T> source, int count, Func<T, double> weight,
        Func<T, string> identity, Func<double> random)
    {
        var candidates = new List<T>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        if (source == null || count <= 0) return candidates;
        foreach (T item in source)
        {
            double value = weight(item);
            string key = identity(item);
            if (value > 0 && !double.IsInfinity(value) && !string.IsNullOrEmpty(key) && seen.Add(key))
                candidates.Add(item);
        }
        var result = new List<T>();
        while (result.Count < count && candidates.Count > 0)
        {
            double total = 0;
            foreach (T item in candidates) total += weight(item);
            double roll = Math.Max(0, Math.Min(1, random())) * total;
            int selected = candidates.Count - 1;
            for (int i = 0; i < candidates.Count; i++)
            {
                roll -= weight(candidates[i]);
                if (roll < 0) { selected = i; break; }
            }
            result.Add(candidates[selected]);
            candidates.RemoveAt(selected);
        }
        return result;
    }
}
