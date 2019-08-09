using System;
using System.Collections.Generic;
namespace InstaHashtagGrabber.Model
{
    public class Configuration
    {
        public string ConnectionString { get; set; }
        public string InstagramUserName { get; set; }
        public string InstagramPassword { get; set; }
        public string InstagramCsrfToken { get; set; }
        public DateTime MediaMinDate { get; set; }
    }
}
