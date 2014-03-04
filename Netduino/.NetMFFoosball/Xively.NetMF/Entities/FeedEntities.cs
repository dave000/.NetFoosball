using System;
using Microsoft.SPOT;

namespace Xively.NetMF.Entities
{
    public class Feed
    {
        public long FeedId { get; private set; }

        public virtual string AsResource()
        {
            return "/feeds/" + FeedId;
        }

        public Feed(long feedId)
        {
            FeedId = feedId;
        }
    }

    public class DataStream : Feed
    {
        public string DataStreamId { get; private set; }

        public DataStream(long feedId, string dataStreamId) : base(feedId)
        {
            DataStreamId = dataStreamId;
        }

        public override string AsResource()
        {
            return base.AsResource() + "/datastreams/" + DataStreamId;
        }
    }
}
