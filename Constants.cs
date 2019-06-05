namespace Frostbyte
{
    public sealed class Constants
    {
        public const string CLIENT_ID_TWITCH = "jzkbprff40iqj646a697cyrvl0zt2m6";

        public const string CLIENT_ID_SOUNDCLOUD = "a3dd183a357fcff9a6943c0d65664087";

        public const string TWITCH_ACCESS_TOKEN =
            "https://api.twitch.tv/api/channels/{0}/access_token?adblock=false&need_https=true&platform=web&player_type=site";

        public const string PATTERN_SOUNDCLOUD_SCRIPT = "https://[A-Za-z0-9-.]+/assets/app-[a-f0-9-]+\\.js";

        public const string PATTERN_SOUNDCLOUD_CLIENT_ID = "/,client_id:\"([a-zA-Z0-9-_]+)\"/";

        public const string PATTERN_YOUTUBE_ID =
            @"(?:youtube\.com\/(?:[^\/]+\/.+\/|(?:v|e(?:mbed)?)\/|.*[?&]v=)|youtu\.be\/)([^""&?\/ ]{11})";

        public const string PATTERN_URL_SOUNDCLOUD = @"/^https?:\/\/(soundcloud\.com|snd\.sc)\/(.*)$/";
    }
}