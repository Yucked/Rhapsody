﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Frostbyte.Entities;
using Frostbyte.Entities.Results;

namespace Frostbyte.Sources
{
    
    public sealed class HttpSource : SourceBase
    {
        public HttpSource(Configuration config) : base(config)
        {
            Prefix = "htsource";
        }

        public override string Prefix { get; }

        public override ValueTask<Stream> GetStreamAsync(string id)
        {
            throw new NotImplementedException();
        }

        public override ValueTask<SearchResult> SearchAsync(string query)
        {
            throw new NotImplementedException();
        }
    }
}