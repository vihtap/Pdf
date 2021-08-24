﻿using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using Melville.INPC;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Model.Primitives
{
    public partial class CountingPipeReader : PipeReader
    {
        [DelegateTo()] private PipeReader inner;
        public long Position { get; private set; }
        private ReadOnlySequence<byte> currentSequence;

        public CountingPipeReader(PipeReader inner)
        {
            this.inner = inner;
        }

#warning -- DelegateTo needs to handle in out and ref parameters as well as obsolete attributes
        public override bool TryRead(out ReadResult result)
        {
            var succeeded = inner.TryRead(out result);
            if (succeeded)
            {
                currentSequence = result.Buffer;
            }
            return succeeded;
        }
        public override async ValueTask<ReadResult> ReadAsync(
            CancellationToken cancellationToken = default)
        {
            var ret = await inner.ReadAsync(cancellationToken);
            currentSequence = ret.Buffer;
            return ret;
        }

        public void MarkSequenceAsExamined() =>
            AdvanceTo(currentSequence.Start, currentSequence.End);

        public override void AdvanceTo(SequencePosition consumed)
        {
            IncrementPosition(consumed);
            inner.AdvanceTo(consumed);
        }

        public override void AdvanceTo(SequencePosition consumed, SequencePosition examined)
        {
            IncrementPosition(consumed);
            inner.AdvanceTo(consumed, examined);
        }

        private void IncrementPosition(SequencePosition consumed)
        {
            Position += currentSequence.Slice(0, consumed).Length;
        }
    }
}