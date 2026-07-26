using System.Runtime.Serialization;

namespace TechLeadTools.Protocol
{
    [DataContract]
    public sealed class TltPayload
    {
        [DataMember(Name = "workspace", Order = 1, IsRequired = true)]
        public string Workspace { get; set; } = string.Empty;

        [DataMember(Name = "path", Order = 2, IsRequired = true)]
        public string Path { get; set; } = string.Empty;

        [DataMember(Name = "file", Order = 3, IsRequired = true)]
        public string File { get; set; } = string.Empty;

        [DataMember(Name = "class", Order = 4, IsRequired = true)]
        public string ClassName { get; set; } = string.Empty;

        [DataMember(Name = "startLine", Order = 5, IsRequired = true)]
        public int StartLine { get; set; }

        [DataMember(Name = "endLine", Order = 6, IsRequired = true)]
        public int EndLine { get; set; }
    }
}

