using System.Collections.Concurrent;

namespace Cosmium.Engine.Infrastructure.Extensions;

/// <summary>
/// Collection manipulation helpers for quantum mechanics and scientific computing.
/// Provides convenient methods for working with arrays, lists, and other collections.
/// </summary>
public static class CollectionExtensions
{
    #region Array Extensions

    /// <summary>
    /// Creates a deep copy of an array.
    /// </summary>
    /// <typeparam name="T">The type of elements in the array.</typeparam>
    /// <param name="array">The array to copy.</param>
    /// <returns>A deep copy of the array.</returns>
    public static T[] DeepCopy<T>(this T[] array) where T : ICloneable
    {
        return array.Select(item => (T)item.Clone()).ToArray();
    }

    /// <summary>
    /// Creates a shallow copy of an array.
    /// </summary>
    /// <typeparam name="T">The type of elements in the array.</typeparam>
    /// <param name="array">The array to copy.</param>
    /// <returns>A shallow copy of the array.</returns>
    public static T[] ShallowCopy<T>(this T[] array)
    {
        var copy = new T[array.Length];
        Array.Copy(array, copy, array.Length);
        return copy;
    }

    /// <summary>
    /// Fills an array with a specified value.
    /// </summary>
    /// <typeparam name="T">The type of elements in the array.</typeparam>
    /// <param name="array">The array to fill.</param>
    /// <param name="value">The value to fill with.</param>
    /// <returns>The filled array.</returns>
    public static T[] Fill<T>(this T[] array, T value)
    {
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = value;
        }
        return array;
    }

    /// <summary>
    /// Fills an array using a generator function.
    /// </summary>
    /// <typeparam name="T">The type of elements in the array.</typeparam>
    /// <param name="array">The array to fill.</param>
    /// <param name="generator">Function that generates values based on index.</param>
    /// <returns>The filled array.</returns>
    public static T[] Fill<T>(this T[] array, Func<int, T> generator)
    {
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = generator(i);
        }
        return array;
    }

    /// <summary>
    /// Creates a sub-array from the specified range.
    /// </summary>
    /// <typeparam name="T">The type of elements in the array.</typeparam>
    /// <param name="array">The source array.</param>
    /// <param name="startIndex">The starting index.</param>
    /// <param name="length">The length of the sub-array.</param>
    /// <returns>A new array containing the specified range.</returns>
    public static T[] SubArray<T>(this T[] array, int startIndex, int length)
    {
        if (startIndex < 0 || startIndex >= array.Length)
            throw new ArgumentOutOfRangeException(nameof(startIndex));
        
        if (length < 0 || startIndex + length > array.Length)
            throw new ArgumentOutOfRangeException(nameof(length));

        var result = new T[length];
        Array.Copy(array, startIndex, result, 0, length);
        return result;
    }

    /// <summary>
    /// Reshapes a 1D array into a 2D array.
    /// </summary>
    /// <typeparam name="T">The type of elements in the array.</typeparam>
    /// <param name="array">The 1D array.</param>
    /// <param name="rows">Number of rows.</param>
    /// <param name="columns">Number of columns.</param>
    /// <returns>A 2D array.</returns>
    public static T[,] Reshape<T>(this T[] array, int rows, int columns)
    {
        if (array.Length != rows * columns)
            throw new ArgumentException("Array length must equal rows * columns");

        var result = new T[rows, columns];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                result[i, j] = array[i * columns + j];
            }
        }
        return result;
    }

    /// <summary>
    /// Flattens a 2D array into a 1D array (row-major order).
    /// </summary>
    /// <typeparam name="T">The type of elements in the array.</typeparam>
    /// <param name="array">The 2D array.</param>
    /// <returns>A flattened 1D array.</returns>
    public static T[] Flatten<T>(this T[,] array)
    {
        int rows = array.GetLength(0);
        int columns = array.GetLength(1);
        var result = new T[rows * columns];
        
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                result[i * columns + j] = array[i, j];
            }
        }
        return result;
    }

    #endregion

    #region List Extensions

    /// <summary>
    /// Adds multiple items to a list.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list to add items to.</param>
    /// <param name="items">The items to add.</param>
    /// <returns>The list for method chaining.</returns>
    public static List<T> AddMany<T>(this List<T> list, params T[] items)
    {
        list.AddRange(items);
        return list;
    }

    /// <summary>
    /// Removes items from a list based on a predicate.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list to remove items from.</param>
    /// <param name="predicate">The predicate to test items.</param>
    /// <returns>The number of items removed.</returns>
    public static int RemoveWhere<T>(this List<T> list, Func<T, bool> predicate)
    {
        return list.RemoveAll(item => predicate(item));
    }

    /// <summary>
    /// Partitions a list into chunks of specified size.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list to partition.</param>
    /// <param name="chunkSize">The size of each chunk.</param>
    /// <returns>An enumerable of chunks.</returns>
    public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> list, int chunkSize)
    {
        if (chunkSize <= 0)
            throw new ArgumentException("Chunk size must be positive", nameof(chunkSize));

        var enumerator = list.GetEnumerator();
        while (enumerator.MoveNext())
        {
            yield return GetChunk(enumerator, chunkSize);
        }
    }

    private static IEnumerable<T> GetChunk<T>(IEnumerator<T> enumerator, int chunkSize)
    {
        int count = 0;
        do
        {
            yield return enumerator.Current;
            count++;
        }
        while (count < chunkSize && enumerator.MoveNext());
    }

    /// <summary>
    /// Finds the index of the maximum element in a list.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list to search.</param>
    /// <param name="comparer">Optional comparer for elements.</param>
    /// <returns>The index of the maximum element.</returns>
    public static int IndexOfMax<T>(this IEnumerable<T> list, IComparer<T>? comparer = null)
    {
        comparer ??= Comparer<T>.Default;
        var enumerated = list.ToList();
        
        if (!enumerated.Any())
            throw new InvalidOperationException("Cannot find maximum of empty sequence");

        int maxIndex = 0;
        T maxValue = enumerated[0];
        
        for (int i = 1; i < enumerated.Count; i++)
        {
            if (comparer.Compare(enumerated[i], maxValue) > 0)
            {
                maxIndex = i;
                maxValue = enumerated[i];
            }
        }
        
        return maxIndex;
    }

    /// <summary>
    /// Finds the index of the minimum element in a list.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list to search.</param>
    /// <param name="comparer">Optional comparer for elements.</param>
    /// <returns>The index of the minimum element.</returns>
    public static int IndexOfMin<T>(this IEnumerable<T> list, IComparer<T>? comparer = null)
    {
        comparer ??= Comparer<T>.Default;
        var enumerated = list.ToList();
        
        if (!enumerated.Any())
            throw new InvalidOperationException("Cannot find minimum of empty sequence");

        int minIndex = 0;
        T minValue = enumerated[0];
        
        for (int i = 1; i < enumerated.Count; i++)
        {
            if (comparer.Compare(enumerated[i], minValue) < 0)
            {
                minIndex = i;
                minValue = enumerated[i];
            }
        }
        
        return minIndex;
    }

    #endregion

    #region Dictionary Extensions

    /// <summary>
    /// Gets a value from a dictionary or returns a default value if the key doesn't exist.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    /// <param name="dictionary">The dictionary to search.</param>
    /// <param name="key">The key to look for.</param>
    /// <param name="defaultValue">The default value to return if key not found.</param>
    /// <returns>The value associated with the key or the default value.</returns>
    public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default!)
    {
        return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
    }

    /// <summary>
    /// Adds or updates a value in a dictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    /// <param name="dictionary">The dictionary to update.</param>
    /// <param name="key">The key.</param>
    /// <param name="value">The value to add or update.</param>
    /// <returns>The dictionary for method chaining.</returns>
    public static IDictionary<TKey, TValue> AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        dictionary[key] = value;
        return dictionary;
    }

    /// <summary>
    /// Merges another dictionary into this one.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    /// <param name="dictionary">The target dictionary.</param>
    /// <param name="other">The source dictionary to merge.</param>
    /// <param name="overwrite">Whether to overwrite existing keys.</param>
    /// <returns>The target dictionary for method chaining.</returns>
    public static IDictionary<TKey, TValue> Merge<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, 
        IDictionary<TKey, TValue> other, bool overwrite = true)
    {
        foreach (var kvp in other)
        {
            if (overwrite || !dictionary.ContainsKey(kvp.Key))
            {
                dictionary[kvp.Key] = kvp.Value;
            }
        }
        return dictionary;
    }

    #endregion

    #region Parallel Extensions

    /// <summary>
    /// Processes a collection in parallel with a specified degree of parallelism.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to process.</param>
    /// <param name="action">The action to perform on each element.</param>
    /// <param name="maxDegreeOfParallelism">Maximum degree of parallelism.</param>
    public static void ForEachParallel<T>(this IEnumerable<T> collection, Action<T> action, int maxDegreeOfParallelism = -1)
    {
        var options = new ParallelOptions();
        if (maxDegreeOfParallelism > 0)
        {
            options.MaxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        Parallel.ForEach(collection, options, action);
    }

    /// <summary>
    /// Processes a collection in parallel and returns results.
    /// </summary>
    /// <typeparam name="TSource">The type of elements in the source collection.</typeparam>
    /// <typeparam name="TResult">The type of elements in the result collection.</typeparam>
    /// <param name="collection">The collection to process.</param>
    /// <param name="selector">The function to apply to each element.</param>
    /// <param name="maxDegreeOfParallelism">Maximum degree of parallelism.</param>
    /// <returns>A collection of results.</returns>
    public static IEnumerable<TResult> SelectParallel<TSource, TResult>(this IEnumerable<TSource> collection, 
        Func<TSource, TResult> selector, int maxDegreeOfParallelism = -1)
    {
        var options = new ParallelOptions();
        if (maxDegreeOfParallelism > 0)
        {
            options.MaxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        var results = new ConcurrentBag<TResult>();
        Parallel.ForEach(collection, options, item => results.Add(selector(item)));
        
        return results;
    }

    #endregion

    #region Quantum-Specific Collections

    /// <summary>
    /// Creates a tensor product of two collections.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collections.</typeparam>
    /// <param name="collection1">The first collection.</param>
    /// <param name="collection2">The second collection.</param>
    /// <param name="combiner">Function to combine elements from both collections.</param>
    /// <returns>The tensor product collection.</returns>
    public static IEnumerable<TResult> TensorProduct<T, TResult>(this IEnumerable<T> collection1, 
        IEnumerable<T> collection2, Func<T, T, TResult> combiner)
    {
        var list1 = collection1.ToList();
        var list2 = collection2.ToList();
        
        foreach (var item1 in list1)
        {
            foreach (var item2 in list2)
            {
                yield return combiner(item1, item2);
            }
        }
    }

    /// <summary>
    /// Generates all permutations of a collection.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to permute.</param>
    /// <returns>All permutations of the collection.</returns>
    public static IEnumerable<IEnumerable<T>> GetPermutations<T>(this IEnumerable<T> collection)
    {
        var list = collection.ToList();
        if (list.Count <= 1)
        {
            yield return list;
            yield break;
        }

        for (int i = 0; i < list.Count; i++)
        {
            var element = list[i];
            var remaining = list.Take(i).Concat(list.Skip(i + 1));
            
            foreach (var permutation in GetPermutations(remaining))
            {
                yield return new[] { element }.Concat(permutation);
            }
        }
    }

    /// <summary>
    /// Generates all combinations of a specified size from a collection.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to generate combinations from.</param>
    /// <param name="size">The size of each combination.</param>
    /// <returns>All combinations of the specified size.</returns>
    public static IEnumerable<IEnumerable<T>> GetCombinations<T>(this IEnumerable<T> collection, int size)
    {
        var list = collection.ToList();
        
        if (size == 0)
        {
            yield return Enumerable.Empty<T>();
            yield break;
        }

        if (size > list.Count)
            yield break;

        for (int i = 0; i < list.Count; i++)
        {
            var element = list[i];
            var remaining = list.Skip(i + 1);
            
            foreach (var combination in GetCombinations(remaining, size - 1))
            {
                yield return new[] { element }.Concat(combination);
            }
        }
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Checks if a collection is null or empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to check.</param>
    /// <returns>True if the collection is null or empty.</returns>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? collection)
    {
        return collection == null || !collection.Any();
    }

    /// <summary>
    /// Executes an action on each element of a collection and returns the collection.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to iterate over.</param>
    /// <param name="action">The action to perform on each element.</param>
    /// <returns>The original collection for method chaining.</returns>
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> collection, Action<T> action)
    {
        foreach (var item in collection)
        {
            action(item);
        }
        return collection;
    }

    /// <summary>
    /// Shuffles the elements of a list randomly.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list to shuffle.</param>
    /// <param name="random">Optional random number generator.</param>
    /// <returns>The shuffled list.</returns>
    public static IList<T> Shuffle<T>(this IList<T> list, Random? random = null)
    {
        random ??= new Random();
        
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
        
        return list;
    }

    #endregion
}
