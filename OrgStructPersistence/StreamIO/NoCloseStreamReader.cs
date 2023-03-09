using System.IO;
using System.Text;

namespace OrgStructPersistence.StreamIO
{
    /// <summary>
    /// Encapsulates a stream reader which does not close the underlying stream on disposal.
    /// Based on example by Aaron Murgatroyd (https://stackoverflow.com/questions/1187700/does-disposing-a-streamwriter-close-the-underlying-stream)
    /// </summary>
    public class NoCloseStreamReader : StreamReader
    {
        /// <summary>
        /// Creates a new stream reader object.
        /// </summary>
        /// <param name="stream">The underlying stream to write to.</param>
        /// <param name="encoding">The encoding for the stream.</param>
        public NoCloseStreamReader(Stream stream, Encoding encoding) : base(stream, encoding) { }

        /// <summary>
        /// Creates a new stream reader object using default encoding.
        /// </summary>
        /// <param name="stream">The underlying stream to read from.</param>
        /// <param name="encoding">The encoding for the stream.</param>
        public NoCloseStreamReader(Stream stream) : base(stream) { }

        /// <summary>
        /// Disposes of the stream reader.
        /// </summary>
        /// <param name="disposing">True to dispose managed objects.</param>
        protected override void Dispose(bool disposeManaged)
        {
            // Dispose the stream reader but pass false to the dispose
            // method to stop it from closing the underlying stream
            base.Dispose(false);
        }
    }
}
