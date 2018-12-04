﻿//https://github.com/dotnet/corefx/blob/dd21223aef94836bc18c8f1540bdece5e6ef3881/src/Common/src/CoreLib/System/MemoryDebugView.cs

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;

namespace System
{
    internal sealed class MemoryDebugView<T>
    {
        private readonly ReadOnlyMemory<T> _memory;

        public MemoryDebugView(Memory<T> memory)
        {
            _memory = memory;
        }

        public MemoryDebugView(ReadOnlyMemory<T> memory)
        {
            _memory = memory;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items => _memory.ToArray();
    }
}