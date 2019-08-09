using Dapper.Contrib.Extensions;

namespace InstaHashtagGrabber.Model.Database
{
    [Table("MediaTag")]
    public class ResultMediaTag
    {
        public string MediaCode { get; set; }
        public string Tag { get; set; }
        public int Type { get; set; }
    }
}
