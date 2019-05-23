﻿using Frostbyte.Entities;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Frostbyte.Sources
{
    public abstract class BaseSource
    {
        protected const string NO_EMBED_URL = "http://noembed.com/embed?url={0}&callback=my_embed_function";

        protected Regex Regex(string pattern)
            => new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public abstract ValueTask<TrackEntity> GetTrackAsync(string query);

        public abstract ValueTask<RESTEntity> PrepareResponseAsync(string query);

        public abstract ValueTask<Stream> GetStreamAsync(TrackEntity track);
    }
}