using System;
using System.Linq;
using System.Threading.Tasks;
using Frostbyte.Entities;
using Frostbyte.Entities.Audio;
using Frostbyte.Entities.Enums;
using Frostbyte.Handlers;
using Frostbyte.Sources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TagLib.Id3v2;

namespace Frostbyte.Tests
{
    [TestClass]
    public class SourceTests
    {
        private readonly ISearchProvider _youTube = new YouTubeSource();
        private readonly ISearchProvider _soundCloud = new SoundCloudSource();

        [TestMethod]
        public async Task TestYouTube() 
            => CheckRestResult(await _youTube.SearchAsync("Test"));
        
        [TestMethod]
        public async Task TestSoundCloud() 
            => CheckRestResult(await _soundCloud.SearchAsync("Test"));

        private static void CheckRestResult(RESTEntity result)
        {
            Assert.IsFalse(result.LoadType == LoadType.LoadFailed, "Load failed");
            Assert.IsTrue(
                result.AudioItems.OfType<Track>()
                    .All(track => track.Id != null),
                "One of the track's ID equals to null");
        }
    }
}