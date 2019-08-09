using Dapper.Contrib.Extensions;

namespace InstaHashtagGrabber.Model.Database
{
    [Table("MediaImageUrl")]
    public class ResultMediaImageUrl
    {
        public string MediaCode { get; set; }
        public string Url { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public int CarouselIndex { get; set; }
    }
}
