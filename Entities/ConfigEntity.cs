using System.Text.Json.Serialization;
using Frostbyte.Entities.Enums;

namespace Frostbyte.Entities
{
    public sealed class ConfigEntity
    {
        public int Port { get; set; }
        public string Host { get; set; }
        public string Password { get; set; }
        public SourcesEntity Sources { get; set; }
        public LogLevel LogLevel { get; set; }

        [JsonIgnore]
        internal string Url
        {
            get { return $"http://{Host}:{Port}/"; }
        }
    }
}