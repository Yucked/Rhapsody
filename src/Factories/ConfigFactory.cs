using System.IO;
using System.Threading.Tasks;
using Frostbyte.Entities;
using Frostbyte.Entities.Enums;
using Frostbyte.Misc;

namespace Frostbyte.Factories
{
    public sealed class ConfigFactory
    {
        private const string PATH = "./Config.json";

        public void BuildConfigAsync()
        {
            if (File.Exists(PATH))
            {
                LogFactory.Warning<ConfigFactory>("Configuration already exists.");
                return;
            }

            var config = new Configuration
            {
                Server = new ServerConfig
                {
                    Hostname = "127.0.0.1",
                    Port = 8080,
                    UseRandomPort = true,
                    Authorization = "FrostyGhosty",
                    MaxReconnectTries = 10,
                    ReconnectInterval = 5000,
                    BufferSize = 256
                },
                LogType = LogType.Debug,
                Audio = new AudioConfig
                {
                    OpusVoiceType = OpusVoiceType.Music,
                    Sources = new SourcesConfig
                    {
                        SoundCloud = true,
                        BandCamp = true
                    }
                }
            };

            var serialize = config.Serialize();
            File.WriteAllBytes(PATH, serialize.ToArray());
            LogFactory.Information<Configuration>("Created new configuration.");
        }

        public async Task<Configuration> LoadConfigAsync()
        {
            var bytes = await File.ReadAllBytesAsync(PATH)
                .ConfigureAwait(false);

            LogFactory.Information<Configuration>("Loaded configuration.");
            return bytes.Deserialize<Configuration>();
        }
    }
}