using blastcms.web.Data;
using blastcms.web.Helpers;
using blastcms.web.Pages.PodastEpisodes;
using Marten;
using Marten.Linq;
using blastcms.web.Infrastructure;
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
            public string PodcastSlug { get; internal set; }
            public string Tag { get; internal set; }

            public Query(string podcastSlug, int skip, int take, int currentPage, string search = null, string tag = null)
            {
                Skip = skip;
                Take = take;
                CurrentPage = currentPage;
                Search = search;
                Tag = tag;
                PodcastSlug = podcastSlug;
            }
        }


        public class PagedData : IPagedData<PodcastEpisode>
        {
            public PagedData(IEnumerable<PodcastEpisode> episodes, long count, int page, Podcast podcast) : this(episodes, count, page)
            {
                Podcast = podcast;
            }
            public PagedData(IEnumerable<PodcastEpisode> episodes, long count, int page)
            {
                Data = episodes;
                Count = count;
                Page = page;
            }

            public IEnumerable<PodcastEpisode> Data { get; }

            public Podcast Podcast { get; } 

            public long Count { get; }
            public int Page { get; }
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

                    var podcast = await session.Query<Podcast>().FirstOrDefaultAsync(q => q.Slug.Equals(request.PodcastSlug, StringComparison.OrdinalIgnoreCase), token: cancellationToken);

                    if(podcast == null)
                        throw new KeyNotFoundException(request.PodcastSlug + "Not found");

                    var query = session.Query<PodcastEpisode>()
                        .Stats(out QueryStatistics stats)

                        .If(!string.IsNullOrWhiteSpace(request.Search), x => x.Where(q => q.Title.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                                || q.Author.Contains(request.Search, StringComparison.OrdinalIgnoreCase)
                                || q.Slug.Contains(request.Search, StringComparison.OrdinalIgnoreCase)))

                        .If(!string.IsNullOrWhiteSpace(request.Tag), x => x.Where(q => q.Tags != null && q.Tags.Contains(request.Tag)))
              
                        .Where(q => q.PodcastId == podcast.Id)
                        .Skip(request.Skip)
                        .Take(request.Take)
                        .OrderByDescending(o => o.PublishedDate).AsQueryable();

                    var data = await query.ToListAsync(token: cancellationToken);

                    return new PagedData(data, stats.TotalResults, request.CurrentPage, podcast);
                }
            }

        }
    }
}
