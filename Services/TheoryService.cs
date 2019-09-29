using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Config = Concept.Configuration.Configuration;

namespace Concept.Services
{
    public class TheoryService
    {
        private readonly Config _config;

        public TheoryService(Config config)
        {
            _config = config;
        }
    }
}