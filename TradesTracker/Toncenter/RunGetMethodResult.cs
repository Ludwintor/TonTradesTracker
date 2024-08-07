﻿using System.Globalization;
using System.Text.Json.Serialization;
using TonSharp.Core;

namespace TradesTracker.Toncenter
{
    public sealed class RunGetMethodResult
    {
        private int _stackPosition = 0;

        [JsonPropertyName("gas_used")]
        public ulong GasUsed { get; init; }

        [JsonPropertyName("exit_code")]
        public int ExitCode { get; init; }

        [JsonPropertyName("stack")]
        public StackEntry[] Stack { get; init; } = [];

        public UInt128 ReadUInt()
        {
            if (_stackPosition == Stack.Length)
                throw new IndexOutOfRangeException("Stack overflow");

            StackEntry entry = Stack[_stackPosition];
            if (entry.Type != "num")
                throw new InvalidOperationException($"Got type \"{entry.Type}\" but expected \"num\"");
            UInt128 result = UInt128.Parse(entry.Value.AsSpan()[2..], NumberStyles.HexNumber);
            _stackPosition++;
            return result;
        }

        public Address ReadAddress()
        {
            if (_stackPosition == Stack.Length)
                throw new IndexOutOfRangeException("Stack overflow");

            StackEntry entry = Stack[_stackPosition];
            if (entry.Type != "slice" && entry.Type != "cell")
                throw new InvalidOperationException($"Got type \"{entry.Type}\" but expected \"slice\" or \"cell\"");
            Cell cell = Cell.FromBase64(entry.Value);
            Address result = cell.BeginParse().LoadAddress();
            _stackPosition++;
            return result;
        }
    }
}
