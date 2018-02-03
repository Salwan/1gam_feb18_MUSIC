using System;
using System.Collections;
using System.Collections.Generic;

public static class Randomizer {
    public static Random rng = new Random();

    /// <summary>
    /// Shuffles a list.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    public static void Shuffle<T>(this IList<T> list) {
        int n = list.Count;
        while(n > 1) {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
