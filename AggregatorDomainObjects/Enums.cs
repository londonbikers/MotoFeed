using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPanther.Aggregator
{
    public enum SourceStatus
    {
        New = 0,
        Active = 1,
        Inactive = 2,
        NotResponding = 3
    }

    public enum CategoryStatus
    {
        Inactive = 0,
        Active = 1
    }

    public enum FeedFormat
    {
        Rss0Point90,
        Rss0Point91,
        Rss0Point92,
        Rss1Point0,
        Rss2Point0,
        Atom0Point3,
        Atom1Point0,
        RDF
    }
}