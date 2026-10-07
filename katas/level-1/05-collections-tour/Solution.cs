namespace Katas.L1.Collections;

public static class CollectionKata
{
    private static readonly char[] Separators = [' ', ',', '.', '!', '?', ';', ':'];

    public static Dictionary<string, int> WordFrequency(string text)
    {
        var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var word in text.Split(Separators, StringSplitOptions.RemoveEmptyEntries))
        {
            counts[word] = counts.GetValueOrDefault(word) + 1;
        }

        return counts;
    }

    public static int? FirstDuplicate(IEnumerable<int> values)
    {
        var seen = new HashSet<int>();
        foreach (var value in values)
        {
            if (!seen.Add(value))
            {
                return value;
            }
        }

        return null;
    }

    public static bool IsBalanced(string input)
    {
        var expected = new Stack<char>();
        foreach (var c in input)
        {
            switch (c)
            {
                case '(': expected.Push(')'); break;
                case '[': expected.Push(']'); break;
                case '{': expected.Push('}'); break;
                case ')' or ']' or '}':
                    if (!expected.TryPop(out var closing) || closing != c)
                    {
                        return false;
                    }
                    break;
            }
        }

        return expected.Count == 0;
    }

    public static int[] Frame(IEnumerable<int> middle, int edge) => [edge, .. middle, edge];

    public static List<int> EliminationOrder(int count, int step)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(step);

        var circle = new Queue<int>(Enumerable.Range(1, count));
        var removed = new List<int>(count);

        while (circle.Count > 0)
        {
            for (var i = 1; i < step; i++)
            {
                circle.Enqueue(circle.Dequeue());
            }

            removed.Add(circle.Dequeue());
        }

        return removed;
    }

    public static SortedDictionary<int, List<string>> GroupByLength(IEnumerable<string> words)
    {
        var groups = new SortedDictionary<int, List<string>>();
        foreach (var word in words)
        {
            if (!groups.TryGetValue(word.Length, out var bucket))
            {
                bucket = [];
                groups[word.Length] = bucket;
            }

            bucket.Add(word);
        }

        return groups;
    }
}
