using Dapper.Contrib.Extensions;

namespace InstaHashtagGrabber.Model.Database
{
    [Table("MediaProductTag")]
    public class ResultMediaProductTag
    {
        public string MediaCode { get; set; }
        public string Name { get; set; }
        public string ExternalUrl { get; set; }
        public string MerchantUserName { get; set; }
        public string MainImageUri { get; set; }
        public string FullPrice { get; set; }
    }
}
