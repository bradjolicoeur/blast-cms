using blastcms.web.Data;
using blastcms.web.Helpers;
using Marten;
using Marten.Linq;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.Handlers
{
    public class GetPodcastEpisodesContent
    {
        public class Query : IRequest<PagedData>
        {
            public int Skip { get; internal set; }
            public int Take { get; internal set; }
            public int CurrentPage { get; internal set; }
            public string Search { get; internal set; }
            public string PodcastId { get; internal set; }
            public string Tag { get; internal set; }

            public Query(int skip, int take, int currentPage, string search = null, string tag = null, string podcastId = null)
            {
                Skip = skip;
                Take = take;
                CurrentPage = currentPage;
                Search = search;
                Tag = tag;
                PodcastId = podcastId;
            }
        }


        public class PagedData : IPagedData<Model>
        {
            public PagedData(IEnumerable<Model> episodes, long count, int page)
            {
                Data = episodes;
                Count = count;
                Page = page;
            }

            public IEnumerable<Model> Data { get; }
            public long Count { get; }
            public int Page { get; }
        }

        public class Model
        {
            public Guid Id { get; set; }
            public Guid PodcastId { get; set; }
            public string Title { get; set; }
            public string Author { get; set; }
            public HashSet<String> Tags { get; set; }
            public DateTime PublishedDate { get; set; }
            public ImageFile Image { get; set; }
            public string Summary { get; set; }
            public string Content { get; set; }
            public int Episode { get; set; }
            public string Duration { get; set; }
            public string Mp3Url { get; set; }
            public string YouTubeUrl { get; set; }
            public string Slug { get; set; }

            public Podcast Podcast { get; set; } = null;

            public Model(PodcastEpisode episode, Podcast podcast)
            {
                Id = episode.Id;
                PodcastId = episode.PodcastId;
                Title = episode.Title;
                Author = episode.Author;
                Tags = episode.Tags;
                PublishedDate = episode.PublishedDate;
                Image = episode.Image;
                Summary = episode.Summary;
                Content = episode.Content;
                Episode = episode.Episode;
                Duration = episode.Duration;
                Mp3Url = episode.Mp3Url;
                YouTubeUrl = episode.YouTubeUrl;
                Slug = episode.Slug;
                Podcast = podcast;
            }
            
        }


        public class Handler : IRequestHandler<Query, PagedData>
        {
            private readonly ISessionFactory _sessionFactory;

            public Handler(ISessionFactory sessionFactory)
            {
                _sessionFactory = sessionFactory;
            }

            public async Task<PagedData> Handle(Query request, CancellationToken cancellationToken)
            {
                using var session = _sessionFactory.QuerySession();
                {
                    var dict = new Dictionary<Guid, Podcast>();
                    var query = session.Query<PodcastEpisode>()
                        .Stats(out QueryStatistics stats)
                        .Include(x => x.PodcastId, dict)

                        .If(!string.IsNullOrWhiteSpace(request.Search), x => x.Where(q => q.Title.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                                || q.Author.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                                || q.Slug.Contains(request.Search, StringComparison.OrdinalIgnoreCase)))

                        .If(!string.IsNullOrWhiteSpace(request.Tag), x => x.Where(q => q.Tags != null && q.Tags.Contains(request.Tag)))
                        .If(!string.IsNullOrWhiteSpace(request.PodcastId), x => x.Where(q => q.PodcastId == Guid.Parse(request.PodcastId)))

                        .Skip(request.Skip)
                        .Take(request.Take)
                        .OrderByDescending(o => o.PublishedDate).AsQueryable();

                    var podcastList = dict.Values.ToList();
                    
                    var data = await query.ToListAsync(token: cancellationToken);

                    var merged = data.Select(s => new Model(s, podcastList.Where(q => q.Id == s.PodcastId).FirstOrDefault())).ToList();

                    return new PagedData(merged, stats.TotalResults, request.CurrentPage);
                }
            }

        }
    }
}
