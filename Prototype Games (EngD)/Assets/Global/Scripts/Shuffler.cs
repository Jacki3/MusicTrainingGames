using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Shuffler
{
    public static void Shuffle<T>(this IList<int> ts, int rootNote)
    {
        ts
            .SkipWhile(x => x != rootNote)
            .Concat(ts.TakeWhile(x => x != rootNote));
    }
}
