using Dapper.Contrib.Extensions;

namespace InstaHashtagGrabber.Model.Database
{
    [Table("MediaUserTag")]
    public class ResultMediaUserTag
    {
        public string MediaCode { get; set; }
        public string UserName { get; set; }
    }
}
