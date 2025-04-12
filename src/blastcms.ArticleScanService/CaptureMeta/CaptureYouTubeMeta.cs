using Microsoft.Extensions.Configuration;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3.Data;
using System.Text;
using System;

namespace blastcms.ArticleScanService.CaptureMeta
{
    public class CaptureYouTubeMeta(IConfiguration configuration) : ICaptureMeta
    {
       
        public async Task<CaptureMetaResult> GetMeta(string url)
        {
            var apiKey = configuration["YouTubeApiKey"];
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = apiKey,
                ApplicationName = "Blast CMS"
            });

            var videoId = GetVideoId(url);

            var request = youtubeService.Videos.List("snippet");
            request.Id = videoId;

            var channelsListResponse = await request.ExecuteAsync();

            var snippet = channelsListResponse.Items[0].Snippet;

            var sb = new StringBuilder();
            sb.Append("Title|");
            sb.AppendLine(snippet.Title);
            sb.Append("ChannelTitle|");
            sb.AppendLine(snippet.ChannelTitle);
            sb.Append("Description|");
            sb.AppendLine(snippet.Description);
            sb.Append("Title|");
            sb.AppendLine(snippet.Title);
            sb.Append("Thumbnail|");
            sb.AppendLine(snippet.Thumbnails.Default__.Url);

            return new CaptureMetaResult(sb.ToString());
        }

        private string GetVideoId(string url)
        {
            var uri = new System.Uri(url);
            
            // Handle youtu.be format
            if (uri.Host.Contains("youtu.be"))
            {
                // Extract video ID from the path
                return uri.AbsolutePath.TrimStart('/').Split('?')[0];
            }
            
            // Handle regular youtube.com format
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var videoId = query["v"];
            
            if (string.IsNullOrEmpty(videoId))
            {
                throw new ArgumentException("Could not extract video ID from URL");
            }
            
            return videoId;
        }

    }
}
