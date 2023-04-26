﻿using Melville.Parsing.Streams.Bases;

namespace Melville.Parsing.Streams;

/// <summary>
/// This class acts like a memorystream, except that it uses a list of buffers instead of resizing the buffer.
/// </summary>
public class MultiBufferStream : DefaultBaseStream
    {
        private readonly MultiBuffer multiBuffer;
        private MultiBufferPosition position;

        private MultiBufferStream(MultiBuffer multiBuffer): base(true, true, true)
        {
            this.multiBuffer = multiBuffer;
            position = multiBuffer.StartOfStream();
        }

        /// <summary>
        /// Create a MultDufferStream
        /// </summary>
        /// <param name="blockLength">The default block length when the stream creates blocks.</param>
        public MultiBufferStream(int blockLength = 4096) : this(new MultiBuffer(blockLength))
        {
        }

        /// <summary>
        /// Create a multibufferstream that contains the given data
        /// </summary>
        /// <param name="firstBuffer"></param>
        public MultiBufferStream(byte[] firstBuffer) : this(new MultiBuffer(firstBuffer))
        {
        }

        /// <inheritdoc />
        public override int Read(Span<byte> buffer) =>
            UpdatePosition(multiBuffer.Read(position, buffer));

        private int UpdatePosition(in MultiBufferPosition newPosition)
        {
            var ret = (int)(newPosition.StreamPosition - position.StreamPosition);
            position = newPosition;
            return ret;
        }

        /// <inheritdoc />
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            UpdatePosition(multiBuffer.Write(position, buffer));
        }

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin) => 
            Position = offset + SeekOriginLocation(origin);

        private long SeekOriginLocation(SeekOrigin origin) => origin switch
        {
            SeekOrigin.Current => Position,
            SeekOrigin.End => Length,
            _ => 0
        };

        /// <inheritdoc />
        public override void SetLength(long value) => multiBuffer.SetLength(value);

        /// <inheritdoc />
        public override long Length => multiBuffer.Length;

        /// <inheritdoc />
        public override long Position
        {
            get => position.StreamPosition;
            set => position = multiBuffer.FindPosition(value);
        }

        /// <summary>
        /// Create a reader that has its own unique position pointer into the buffer.
        /// </summary>
        /// <returns></returns>
        public MultiBufferStream CreateReader() => new MultiBufferReader(multiBuffer);


        private class MultiBufferReader : MultiBufferStream
        {
            public MultiBufferReader(MultiBuffer multiBuffer) : base(multiBuffer)
            {
            }

            public override void Write(ReadOnlySpan<byte> buffer) =>
                throw new NotSupportedException();

            public override bool CanWrite => false;
        }

    }