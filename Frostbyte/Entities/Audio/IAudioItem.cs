using System;
using System.Collections.Generic;
using System.Text;

namespace Frostbyte.Entities.Audio
{
    public interface IAudioItem
    {
        string Id { get; set; }
        
        Author Author { get; set; }

        string Url { get; set; }
    }
}
