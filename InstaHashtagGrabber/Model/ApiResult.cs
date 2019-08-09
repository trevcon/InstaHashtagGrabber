using System.Collections.Generic;
using InstaHashtagGrabber.Model.Database;

namespace InstaHashtagGrabber.Model
{
    public class ApiResult
    {
        public bool MoreAvailable { get; set; }
        public string NextMaxId { get; set; }
        public IEnumerable<ResultMedia> Medias { get; set; }
    }
}
