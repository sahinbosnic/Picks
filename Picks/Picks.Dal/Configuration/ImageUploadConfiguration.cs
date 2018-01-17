using System;
using System.Collections.Generic;
using System.Text;

namespace Picks.Dal.Configuration
{
    public class ImageUploadConfiguration
    {
        public string BasePath { get; set; }
        public int MaxSize { get; set; }
        public int Quality { get; set; }
    }
}