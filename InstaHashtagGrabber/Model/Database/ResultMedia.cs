using System;
using System.Collections.Generic;
using Dapper.Contrib.Extensions;

namespace InstaHashtagGrabber.Model.Database
{
    [Table("Media")]
    public class ResultMedia
    {
        [ExplicitKey]
        public string Code { get; set; }
        public string MainTag { get; set; }
        public string Pk { get; set; }
        public string InstaIdentifier { get; set; }
        public string Title { get; set; }
        public string Caption { get; set; }
        public string LangCode { get; set; }
        public double? LangCodeScore { get; set; }
        public string SecondLangCode { get; set; }
        public double? SecondLangCodeScore { get; set; }
        public string UserName { get; set; }
        public long UserPK { get; set; }
        public long LikesCount { get; set; }
        public long CommentsCount { get; set; }
        public string CommentsCountText { get; set; }
        [Write(false)]
        public IEnumerable<ResultMediaTag> Tags { get; set; }
        public long UserFollowersCount { get; set; }
        public long MutualFollowers { get; set; }
        public DateTime Date { get; set; }
        public int DateYear { get; set; }
        public string DateMonthLabel { get; set; }
        public string DateWeekLabel { get; set; }
        public int DateMonth { get; set; }
        public int DateWeek { get; set; }
        public string LocationShortName { get; set; }
        public string LocationName { get; set; }
        public string LocationAddress { get; set; }
        public string LocationCity { get; set; }
        public long LocationFacebookPlacesId { get; set; }
        public double LocationLat { get; set; }
        public double LocationLng { get; set; }
        public double VideoDuration { get; set; }
        public int ViewCount { get; set; }
        public int MediaType { get; set; }
        public bool HasImage { get; set; }
        public bool HasVideo { get; set; }
        public bool IsMultiPost { get; set; }
        public string ProductType { get; set; }
        public int NumberOfQualities { get; set; }
        public int ImageCount { get; set; }
        public int VideoCount { get; set; }
        [Write(false)]
        public IEnumerable<ResultMediaImageUrl> ResultMediaImageUrls { get; set; }
        [Write(false)]
        public IEnumerable<ResultMediaVideoUrl> ResultMediaVideoUrls { get; set; }
        [Write(false)]
        public IEnumerable<ResultMediaUserTag> ResultMediaUserTags { get; set; }
        [Write(false)]
        public IEnumerable<ResultMediaProductTag> ResultMediaProductTags { get; set; }
    }
}
