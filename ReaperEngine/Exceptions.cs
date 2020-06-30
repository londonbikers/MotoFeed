using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPanther.Aggregator.Reaper.Exceptions
{
    /// <summary>
    /// Represents any issues with xml feed formats.
    /// </summary>
    public class FeedFormatException : Exception
    {
        public FeedFormatException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Represents any issues with xml feed contents.
    /// </summary>
    public class FeedContentsException : Exception
    {
        public FeedContentsException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Represents any issues with the primary task of scanning sources.
    /// </summary>
    public class SourceScanException : Exception
    {
        public SourceScanException(string message) : base(message)
        {
        }
    }
}