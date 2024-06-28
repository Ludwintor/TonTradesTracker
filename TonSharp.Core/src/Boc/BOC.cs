using System.Diagnostics;
using System.Numerics;

namespace TonSharp.Core
{
    public static class BOC
    {
        private const uint BOC_ID = 0xb5ee9c72;

        public static Bits Serialize(Cell root, bool hasIndex, bool hasCrc32)
        {
            List<(Cell, int[])> sorted = TopologicalSort(root);

            int cells = sorted.Count;
            int sizeBytes = Math.Max((((IBinaryInteger<uint>)(uint)cells).GetShortestBitLength() + 7) / 8, 1);
            int totalCellsSize = 0;
            Span<int> offsets = hasIndex ? stackalloc int[cells] : [];
            for (int i = 0; i < cells; i++)
            {
                (Cell cell, _) = sorted[i];
                totalCellsSize += 2 + (cell.Bits.Length + 7) / 8 + cell.Refs.Length * sizeBytes;
                if (hasIndex)
                    offsets[i] = totalCellsSize;
            }
            int offsetBytes = Math.Max((((IBinaryInteger<uint>)(uint)totalCellsSize).GetShortestBitLength() + 7) / 8, 1);
            int totalSize = ( // in bits
                4 + // boc id
                1 + // flags and size bytes (1 + 1 + 1 + 2 + 3)
                1 + // offset bytes
                3 * sizeBytes + // cells, roots, absent
                offsetBytes + // total cells size
                sizeBytes + // root id
                (hasIndex ? cells * offsetBytes : 0) + // index
                totalCellsSize + // cells data
                (hasCrc32 ? 4 : 0) // crc32
            ) * 8;

            BitWriter writer = new(totalSize);
            writer.WriteUInt(BOC_ID, 32); // boc id (magic)
            writer.WriteBit(hasIndex); // has index
            writer.WriteBit(hasCrc32); // has crc32
            writer.WriteBit(false); // has cache bits (always 0)
            writer.WriteUInt(0, 2); // flags (always 0)
            writer.WriteUInt(sizeBytes, 3); // size bytes
            writer.WriteUInt(offsetBytes, 8); // offset bytes
            writer.WriteUInt(cells, sizeBytes * 8); // cells count
            writer.WriteUInt(1, sizeBytes * 8); // roots count
            writer.WriteUInt(0, sizeBytes * 8); // absent count (always 0?)
            writer.WriteUInt(totalCellsSize, offsetBytes * 8); // total cells size
            writer.WriteUInt(0, sizeBytes * 8); // root id
            if (hasIndex) // index
                for (int i = 0; i < cells; i++)
                    writer.WriteUInt(offsets[i], offsetBytes * 8);
            foreach ((Cell cell, int[] refIndexer) in sorted)
            {
                (byte d1, byte d2) = cell.GetDescriptors();
                writer.WriteByte(d1);
                writer.WriteByte(d2);
                writer.WriteBitsPadded(cell.Bits);
                foreach (int refIndex in refIndexer)
                    writer.WriteUInt(refIndex, sizeBytes * 8);
            }
            if (hasCrc32) // TODO: implement crc32
                throw new NotImplementedException("hasCrc32 not implemented");

            Bits result = writer.Build();
            Debug.Assert(result.Length == totalSize);
            return result;
        }

        public static Cell[] Deserialize(byte[] boc)
        {
            BitReader header = new(new(boc, 0, boc.Length * 8));
            uint bocId = header.LoadUInt<uint>(32);
            if (bocId != BOC_ID)
                throw new Exception($"Invalid BOC id (magic). Got {bocId:x} but expected {BOC_ID:x}");
            bool hasIndex = header.LoadBit();
            bool hasCrc32 = header.LoadBit();
            header.Skip(1 + 2); // skip hasCacheBits and flags
            byte size = header.LoadUInt<byte>(3);
            byte offset = header.LoadUInt<byte>(8);
            int cells = header.LoadUInt<int>(size * 8);
            int roots = header.LoadUInt<int>(size * 8);
            header.Skip(size * 8); // skip absent
            int totalCellsSize = header.LoadUInt<int>(offset * 8);
            Span<int> root = stackalloc int[roots];
            for (int i = 0; i < roots; i++)
                root[i] = header.LoadUInt<int>(size * 8);
            if (hasIndex)
                header.Skip(cells * offset * 8); // we dont need index
            byte[] cellsData = header.LoadBytes(totalCellsSize);
            if (hasCrc32)
            {
                uint crc32 = header.LoadUInt<uint>(32);
                // TODO: Check CRC32 checksum
            }

            BitReader reader = new(new(cellsData, 0, cellsData.Length * 8));
            UnsafeCell[] unsafeCells = new UnsafeCell[cells];
            for (int i = 0; i < cells; i++)
                unsafeCells[i] = ReadCell(reader, size);

            for (int i = unsafeCells.Length - 1; i >= 0; i--)
            {
                UnsafeCell unsafeCell = unsafeCells[i];
                Debug.Assert(unsafeCell.Result == null);
                Cell[] refs = new Cell[unsafeCell.Refs.Length];
                for (int j = 0; j < unsafeCell.Refs.Length; j++)
                {
                    int refIndex = unsafeCell.Refs[j];
                    UnsafeCell child = unsafeCells[refIndex];
                    if (child.Result == null)
                        throw new Exception("Invalid topological order");
                    refs[j] = child.Result;
                }
                unsafeCells[i].Result = new(unsafeCell.IsExotic, unsafeCell.Bits, refs);
            }

            Cell[] rootCells = new Cell[roots];
            for (int i = 0; i < roots; i++)
                rootCells[i] = unsafeCells[root[i]].Result!;
            return rootCells;
        }

        private static List<(Cell Cell, int[] RefIndexer)> TopologicalSort(Cell root)
        {
            Dictionary<Buffer256, int> visited = [];
            List<Cell> sorted = [];
            VisitCell(root);
            Debug.Assert(visited.Count == sorted.Count);
            // visited indexes and sorted array actually reversed
            List<(Cell, int[])> result = new(sorted.Count);
            for (int i = sorted.Count - 1; i >= 0; i--)
            {
                Cell cell = sorted[i];
                int[]? refIndex = null;
                if (cell.Refs.Length > 0)
                {
                    refIndex = new int[cell.Refs.Length];
                    for (int j = 0; j < cell.Refs.Length; j++)
                        refIndex[j] = sorted.Count - visited[cell.Refs[j].Hash] - 1;
                }
                result.Add((cell, refIndex ?? []));
            }

            return result;

            void VisitCell(Cell cell)
            {
                // do we need to protect from non DAG? is it possible for cell to be non DAG?
                for (int i = cell.Refs.Length - 1; i >= 0; i--)
                {
                    Cell child = cell.Refs[i];
                    if (!visited.ContainsKey(child.Hash))
                        VisitCell(child);
                }
                if (visited.TryAdd(cell.Hash, sorted.Count))
                    sorted.Add(cell);
            }
        }

        private static UnsafeCell ReadCell(BitReader reader, int refSize)
        {
            byte refsDescriptor = reader.LoadUInt<byte>(8);
            int refsCount = refsDescriptor & 0b111;
            bool isExotic = (refsDescriptor & 0b1000) != 0;
            bool hasHashes = (refsDescriptor & 0b10000) != 0;
            int level = refsDescriptor >> 5;

            byte bitsDescriptor = reader.LoadUInt<byte>(8);
            int dataBytes = (bitsDescriptor + 1) / 2;
            bool isPadded = (bitsDescriptor & 1) != 0;

            if (hasHashes)
                reader.Skip((level + 1) * (32 + 2)); // skip hashes and depths

            Bits bits = dataBytes > 0
                ? isPadded ? reader.LoadBitsPadded(dataBytes * 8) : reader.LoadBits(dataBytes * 8)
                : Bits.Empty;

            int[] refs = new int[refsCount];
            for (int i = 0; i < refsCount; i++)
                refs[i] = reader.LoadUInt<int>(refSize * 8);

            return new()
            {
                Bits = bits,
                Refs = refs,
                IsExotic = isExotic
            };
        }

        private struct UnsafeCell
        {
            public Bits Bits;
            public int[] Refs;
            public bool IsExotic;
            public Cell? Result;
        }
    }
}
