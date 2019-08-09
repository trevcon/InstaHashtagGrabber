using Dapper.Contrib.Extensions;

namespace InstaHashtagGrabber.Model.Database
{
    [Table("MediaVideoUrl")]
    public class ResultMediaVideoUrl
    {
        public string MediaCode { get; set; }
        public string Url { get; set; }
        public double Length { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public int Type { get; set; }
        public int CarouselIndex { get; set; }
    }
}
