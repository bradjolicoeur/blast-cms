using blastcms.ArticleScanService.CaptureMeta;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace blastcms.ArticleScanService.Tests
{
    [TestFixture]
    public class CaptureYouTubeMetaTests
    {
        private CaptureYouTubeMeta _sut;

        [SetUp]
        public void SetUp()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["YouTubeApiKey"] = "test-key"
                })
                .Build();

            _sut = new CaptureYouTubeMeta(config);
        }

        [Test]
        public void GetVideoId_watch_url_returns_video_id()
        {
            var id = _sut.GetVideoId("https://www.youtube.com/watch?v=u0k6Qg6mAV4");

            Assert.That(id, Is.EqualTo("u0k6Qg6mAV4"));
        }

        [Test]
        public void GetVideoId_live_url_returns_video_id()
        {
            var id = _sut.GetVideoId("https://www.youtube.com/live/u0k6Qg6mAV4");

            Assert.That(id, Is.EqualTo("u0k6Qg6mAV4"));
        }

        [Test]
        public void GetVideoId_live_url_with_query_string_returns_video_id()
        {
            var id = _sut.GetVideoId("https://www.youtube.com/live/u0k6Qg6mAV4?feature=share");

            Assert.That(id, Is.EqualTo("u0k6Qg6mAV4"));
        }

        [Test]
        public void GetVideoId_short_url_returns_video_id()
        {
            var id = _sut.GetVideoId("https://youtu.be/u0k6Qg6mAV4");

            Assert.That(id, Is.EqualTo("u0k6Qg6mAV4"));
        }

        [Test]
        public void GetVideoId_short_url_with_query_string_returns_video_id()
        {
            var id = _sut.GetVideoId("https://youtu.be/u0k6Qg6mAV4?si=abc123");

            Assert.That(id, Is.EqualTo("u0k6Qg6mAV4"));
        }

        [Test]
        public void GetVideoId_invalid_url_throws_argument_exception()
        {
            Assert.Throws<ArgumentException>(() => _sut.GetVideoId("https://www.youtube.com/channel/UCxyz"));
        }
    }
}
