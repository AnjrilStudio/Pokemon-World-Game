using Anjril.PokemonWorld.Common.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


class ChunkMatrix<T>
{
    private Dictionary<Position, T[,]> chunks;
    private int chunksize;

    public ChunkMatrix(int chunksize)
    {
        chunks = new Dictionary<Position, T[,]>();
        this.chunksize = chunksize;
    }

    public T this[int x, int y]
    {
        get
        {
            var segment = GetSegment(x, y, chunksize);
            if (!chunks.ContainsKey(segment))
            {
                return default(T);
            }

            var chunk = chunks[segment];

            return chunk[x % chunksize, y % chunksize];
        }

        set
        {
            var segment = GetSegment(x, y, chunksize);
            if (!chunks.ContainsKey(segment))
            {
                chunks.Add(segment, new T[chunksize, chunksize]);
            }

            var chunk = chunks[segment];
            chunk[x % chunksize, y % chunksize] = value;
        }
    }

    private static Position GetSegment(int x, int y, int chunksize)
    {
        var px = x / chunksize;
        var py = y / chunksize;

        return new Position(px, py);
    }

}
