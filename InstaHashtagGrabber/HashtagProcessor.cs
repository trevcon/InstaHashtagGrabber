using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading.Tasks;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using Dapper.Contrib.Extensions;
using NTextCat;
using InstaHashtagGrabber.Model;
using InstaHashtagGrabber.Model.Database;

namespace InstaHashtagGrabber
{
    public class HashtagProcessor
    {
        private IInstaApi instaApi;
        private RankedLanguageIdentifier languageIdentifier;
        private Configuration configuration;
        DateTime minDate;
        bool dateLimitReached;

        public async Task Initialize(Configuration configuration)
        {
            this.configuration = configuration;
            minDate = configuration.MediaMinDate;
            dateLimitReached = false;

            var factory = new RankedLanguageIdentifierFactory();
            languageIdentifier = factory.Load(Utils.GetPath(@"packages\NTextCat.0.2.1.30\Core14.profile.xml"));

            instaApi = InstaApiBuilder.CreateBuilder()
                .SetUser(new UserSessionData()
                {
                    UserName = configuration.InstagramUserName,
                    Password = configuration.InstagramPassword,
                    CsrfToken = configuration.InstagramCsrfToken
                })
                .SetRequestDelay(RequestDelay.FromSeconds(0, 0))
                .Build();


            await instaApi.LoginAsync();
        }

        public async Task GetHashtagMedia(string tag)
        {
            await GetHashtagMedia(tag, "", 0, 0);
        }

        public async Task GetHashtagMedia(string tag, string startMaxId, long startPageCount, long startTotal)
        {
            try
            {
                string nextMaxId = startMaxId;
                long pageCount = startPageCount;
                long total = startTotal;
                bool hasNext = true;
                while (!dateLimitReached && hasNext)
                {
                    pageCount++;
                    var nextData = await GetNextPage(tag, nextMaxId);
                    if (nextData != null)
                    {
                        total += nextData.Medias.LongCount();
                        Console.WriteLine($"time: {DateTime.Now.ToLongTimeString()}, next: {nextData.Medias.LongCount()}, page: {pageCount}, total: {total}");
                        await InsertMedias(nextData.Medias);
                        hasNext = nextData.MoreAvailable;
                        nextMaxId = nextData.NextMaxId;

                        var savePoint = new SavePoint()
                        {
                            NextMaxId = nextMaxId,
                            PageCount = pageCount,
                            Total = total
                        };
                        System.IO.File.WriteAllText(Utils.GetPath($"data\\{tag}_savepoint.json"), Newtonsoft.Json.JsonConvert.SerializeObject(savePoint, Newtonsoft.Json.Formatting.Indented));
                    }
                    else
                    {
                        Console.WriteLine($"error - no data");
                        System.IO.File.AppendAllText(Utils.GetPath($"logs\\{tag}_log.log"), $"error - no data");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"error - {e.Message}");
                System.IO.File.AppendAllText(Utils.GetPath($"logs\\{tag}_log.log"), $"error - {e.Message}");
                return;
            }
        }

        public async Task<ApiResult> GetNextPage(string tag, string startFromMaxId)
        {
            var paging = InstagramApiSharp.PaginationParameters.MaxPagesToLoad(10);
            if (!string.IsNullOrEmpty(startFromMaxId))
            {
                paging.StartFromMaxId(startFromMaxId);
            }

            var tryCount = 0;
            while (tryCount < 10)
            {
                tryCount++;
                var data = await instaApi.HashtagProcessor.GetRecentHashtagMediaListAsync(tag, paging);
                if (data.Succeeded)
                {
                    if (data.Value != null && data.Value.Medias != null)
                    {
                        return new ApiResult()
                        {
                            NextMaxId = data.Value.NextMaxId,
                            MoreAvailable = data.Value.MoreAvailable,
                            Medias = data.Value.Medias.Where(m => m.TakenAt >= minDate).Select(m =>
                            {
                                return MapInstaMedia(m, tag);
                            })
                        };
                    }
                }
                else
                {
                    Console.WriteLine($"error - {data.Info.Exception.Message}");
                    System.IO.File.AppendAllText(Utils.GetPath($"logs\\{tag}_log.log"), data.Info.Exception.ToString());
                    Console.WriteLine($"wait 60 sec, attempt {tryCount} of 10");
                    await Task.Delay(60000);
                }
            }

            return null;
        }

        public ResultMedia MapInstaMedia(InstaMedia m, string tag)
        {
            var langResult = languageIdentifier.Identify(m.Caption?.Text ?? "").ToArray();
            int intCommentsCount = 0;
            int.TryParse(m.CommentsCount, out intCommentsCount);
            return new ResultMedia()
            {
                MainTag = tag,
                Pk = m.Pk,
                Code = m.Code,
                InstaIdentifier = m.InstaIdentifier,
                Title = m.Title,
                Caption = m.Caption?.Text,
                LangCode = langResult.FirstOrDefault()?.Item1.Iso639_2T,
                LangCodeScore = langResult.FirstOrDefault()?.Item2,
                SecondLangCode = langResult[1]?.Item1.Iso639_2T,
                SecondLangCodeScore = langResult[1]?.Item2,
                Date = m.TakenAt,
                DateYear = m.TakenAt.Year,
                DateMonth = m.TakenAt.Month,
                DateWeek = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(m.TakenAt, CalendarWeekRule.FirstDay, DayOfWeek.Monday),
                DateMonthLabel = $"{m.TakenAt.Year}/{m.TakenAt.Month}",
                DateWeekLabel = $"{m.TakenAt.Year}/{CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(m.TakenAt, CalendarWeekRule.FirstDay, DayOfWeek.Monday)}",
                UserName = m.User.UserName,
                UserPK = m.User.Pk,
                LikesCount = m.LikesCount,
                CommentsCount = intCommentsCount,
                CommentsCountText = m.CommentsCount,
                UserFollowersCount = m.User.FollowersCount,
                MutualFollowers = m.User.MutualFollowers,
                LocationShortName = m.Location?.ShortName ?? "",
                LocationName = m.Location?.Name ?? "",
                LocationCity = m.Location?.City ?? "",
                LocationAddress = m.Location?.Address ?? "",
                LocationFacebookPlacesId = m.Location?.FacebookPlacesId ?? -1,
                LocationLat = m.Location?.Lat ?? 0,
                LocationLng = m.Location?.Lng ?? 0,
                VideoDuration = m.VideoDuration,
                ViewCount = m.ViewCount,
                MediaType = (int)m.MediaType,
                HasVideo = m.MediaType == InstaMediaType.Video || (m.Carousel?.Any(i => i.MediaType == InstaMediaType.Video || (i.Videos?.Any() ?? false)) ?? false),
                HasImage = m.MediaType == InstaMediaType.Image || (m.Carousel?.Any(i => i.MediaType == InstaMediaType.Image || (i.Images?.Any() ?? false)) ?? false),
                ImageCount = m.MediaType == InstaMediaType.Image ? 1 : m.MediaType == InstaMediaType.Video ? 0 : m.Carousel.Count(c => c.MediaType == InstaMediaType.Image),
                VideoCount = m.MediaType == InstaMediaType.Video ? 1 : m.MediaType == InstaMediaType.Image ? 0 : m.Carousel.Count(c => c.MediaType == InstaMediaType.Video),
                IsMultiPost = m.IsMultiPost,
                ProductType = m.ProductType,
                NumberOfQualities = m.NumberOfQualities,
                Tags = GetHashtags(m.Caption?.Text ?? "").Select(t => new ResultMediaTag()
                {
                    MediaCode = m.Code,
                    Tag = t,
                    Type = 1
                }).Concat(m.PreviewComments.Where(c => c.UserId == m.User.Pk).Select(c => c.Text ?? "").SelectMany(t => GetHashtags(t)).Select(t => new ResultMediaTag()
                {
                    MediaCode = m.Code,
                    Tag = t,
                    Type = 2
                })).Concat(m.PreviewComments.Where(c => c.UserId != m.User.Pk).Select(c => c.Text ?? "").SelectMany(t => GetHashtags(t)).Select(t => new ResultMediaTag()
                {
                    MediaCode = m.Code,
                    Tag = t,
                    Type = 3
                })),
                ResultMediaProductTags = m.ProductTags.Select(t => new ResultMediaProductTag()
                {
                    MediaCode = m.Code,
                    Name = t.Product?.Name ?? "",
                    ExternalUrl = t.Product?.ExternalUrl ?? "",
                    MerchantUserName = t.Product?.Merchant?.Username ?? "",
                    MainImageUri = t.Product?.MainImage?.FirstOrDefault()?.Uri ?? "",
                    FullPrice = t.Product?.FullPrice ?? ""
                }),
                ResultMediaVideoUrls = (m.Carousel?.Any(i => i.MediaType == InstaMediaType.Video || (i.Videos?.Any() ?? false)) ?? false)
                    ? m.Videos.Select(v => new ResultMediaVideoUrl()
                    {
                        MediaCode = m.Code,
                        Url = v.Uri,
                        Length = v.Length,
                        Height = v.Height,
                        Width = v.Width,
                        Type = v.Type,
                        CarouselIndex = -1
                    }).Concat(m.Carousel.SelectMany((i, index) => i.Videos.Select(v => new ResultMediaVideoUrl()
                    {
                        MediaCode = m.Code,
                        Url = v.Uri,
                        Length = v.Length,
                        Height = v.Height,
                        Width = v.Width,
                        Type = v.Type,
                        CarouselIndex = index
                    })))
                    : m.Videos.Select(v => new ResultMediaVideoUrl()
                    {
                        MediaCode = m.Code,
                        Url = v.Uri,
                        Length = v.Length,
                        Height = v.Height,
                        Width = v.Width,
                        Type = v.Type,
                        CarouselIndex = -1
                    }),
                ResultMediaImageUrls = (m.Carousel?.Any(i => i.MediaType == InstaMediaType.Image || (i.Images?.Any() ?? false)) ?? false)
                    ? m.Images.Select(i => new ResultMediaImageUrl()
                    {
                        MediaCode = m.Code,
                        Url = i.Uri,
                        Height = i.Height,
                        Width = i.Width,
                        CarouselIndex = -1
                    }).Concat(m.Carousel.SelectMany((i, index) => i.Images.Select(v => new ResultMediaImageUrl()
                    {
                        MediaCode = m.Code,
                        Url = v.Uri,
                        Height = v.Height,
                        Width = v.Width,
                        CarouselIndex = index
                    })))
                    : m.Images.Select(i => new ResultMediaImageUrl()
                    {
                        MediaCode = m.Code,
                        Url = i.Uri,
                        Height = i.Height,
                        Width = i.Width,
                        CarouselIndex = -1
                    }),
                ResultMediaUserTags = m.UserTags.Select(u => new ResultMediaUserTag()
                {
                    MediaCode = m.Code,
                    UserName = u.User.UserName
                })
            };
        }

        private async Task InsertMedias(IEnumerable<ResultMedia> medias)
        {
            using (System.Data.IDbConnection conn = new SqlConnection(configuration.ConnectionString))
            {
                conn.Open();

                await conn.InsertAsync(medias);
                await conn.InsertAsync(medias.SelectMany(m => m.Tags));
                await conn.InsertAsync(medias.SelectMany(m => m.ResultMediaUserTags));
                await conn.InsertAsync(medias.SelectMany(m => m.ResultMediaProductTags));
                await conn.InsertAsync(medias.SelectMany(m => m.ResultMediaImageUrls));
                await conn.InsertAsync(medias.SelectMany(m => m.ResultMediaVideoUrls));
                
                conn.Close();
            }
        }

        private static IEnumerable<string> GetHashtags(string text)
        {
            var hashtagRegex = new System.Text.RegularExpressions.Regex(@"#\w+");
            var matches = hashtagRegex.Matches(text);
            var tags = new List<string>();
            for (int i = 0; i < matches.Count; i++)
            {
                tags.Add(matches[i].Value);
            }
            return tags;
        }
    }
}
