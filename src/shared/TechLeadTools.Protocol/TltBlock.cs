namespace TechLeadTools.Protocol
{
    public sealed class TltBlock
    {
        public TltBlock(TltPayload payload, string content)
        {
            Payload = payload;
            Content = content;
        }

        public TltPayload Payload { get; }

        public string Content { get; }
    }
}

