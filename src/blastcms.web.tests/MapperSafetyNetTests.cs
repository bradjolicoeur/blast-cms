using blastcms.ArticleScanService;
using blastcms.UserManagement.Models;
using blastcms.web.Data;
using blastcms.web.Handlers;
using blastcms.web.Handlers.Tenant;
using blastcms.web.Infrastructure;
using blastcms.web.Tenant;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace blastcms.web.tests
{
    public class MapperSafetyNetTests
    {
        [Test]
        public void AlterContentBlock_maps_groups_and_image_in_both_directions()
        {
            var mapper = new AlterContentBlock.SliceMapper();
            var command = new AlterContentBlock.Command
            {
                Id = Guid.NewGuid(),
                Title = "Homepage Promo",
                Groups = new[] { "homepage", "featured" },
                Image = new ImageFile
                {
                    Id = Guid.NewGuid(),
                    Title = "Hero",
                    ImageUrl = "https://cdn.example.com/hero.jpg",
                    ImageStorageName = "hero.jpg",
                    Description = "Hero image",
                    Tags = new HashSet<string> { "hero", "homepage" }
                },
                Body = "<p>Body</p>",
                Slug = "homepage-promo"
            };

            var mapped = mapper.ToContentBlock(command);
            var reverseMapped = mapper.ToCommand(mapped);

            ClassicAssert.AreEqual(command.Title, mapped.Title);
            CollectionAssert.AreEquivalent(command.Groups, mapped.Groups);
            ClassicAssert.AreEqual(command.Image.ImageUrl, mapped.Image.ImageUrl);
            CollectionAssert.AreEquivalent(command.Groups, reverseMapped.Groups);
            ClassicAssert.AreEqual(command.Image.Title, reverseMapped.Image.Title);
            ClassicAssert.AreEqual(command.Slug, reverseMapped.Slug);
        }

        [Test]
        public void AlterContentGroup_maps_id_and_value_in_both_directions()
        {
            var mapper = new AlterContentGroup.SliceMapper();
            var command = new AlterContentGroup.Command
            {
                Id = Guid.NewGuid(),
                Value = "Featured"
            };

            var mapped = mapper.ToContentGroup(command);
            var reverseMapped = mapper.ToCommand(mapped);

            ClassicAssert.AreEqual(command.Id, mapped.Id);
            ClassicAssert.AreEqual(command.Value, mapped.Value);
            ClassicAssert.AreEqual(command.Id, reverseMapped.Id);
            ClassicAssert.AreEqual(command.Value, reverseMapped.Value);
        }

        [Test]
        public void AlterEventVenue_maps_nested_image_in_both_directions()
        {
            var mapper = new AlterEventVenue.SliceMapper();
            var command = new AlterEventVenue.Command
            {
                Id = Guid.NewGuid(),
                VenueName = "The Hideout",
                Address = "123 Main",
                City = "Columbus",
                State = "OH",
                Zip = "43215",
                Latitude = "40.0",
                Longitude = "-83.0",
                WebsiteUrl = "https://venue.example.com",
                Image = new ImageFile
                {
                    Id = Guid.NewGuid(),
                    Title = "Venue",
                    ImageUrl = "https://cdn.example.com/venue.jpg"
                },
                Slug = "the-hideout"
            };

            var mapped = mapper.ToEventVenue(command);
            var reverseMapped = mapper.ToCommand(mapped);

            ClassicAssert.AreEqual(command.VenueName, mapped.VenueName);
            ClassicAssert.AreEqual(command.Image.ImageUrl, mapped.Image.ImageUrl);
            ClassicAssert.AreEqual(command.WebsiteUrl, reverseMapped.WebsiteUrl);
            ClassicAssert.AreEqual(command.Slug, reverseMapped.Slug);
        }

        [Test]
        public void PutEventItem_maps_venue_id_and_enum_values()
        {
            var mapper = new PutEventItem.SliceMapper();
            var venueId = Guid.NewGuid();
            var command = new PutEventItem.Command
            {
                Id = Guid.NewGuid(),
                Title = "Open Mic",
                Body = "Body",
                Summary = "Summary",
                Special = "Special",
                Venue = new EventVenue
                {
                    Id = venueId,
                    VenueName = "The Hideout"
                },
                TicketPrice = "$10",
                Flyer = new ImageFile
                {
                    Id = Guid.NewGuid(),
                    Title = "Flyer",
                    ImageUrl = "https://cdn.example.com/flyer.jpg"
                },
                OpenMicSignup = OpenMicOption.ShowForm.Name,
                EventDate = new DateTime(2026, 5, 1),
                EventTime = TimeSpan.FromHours(19),
                Sponsor = "Blast",
                TicketSaleProvider = TicketSaleProvider.VenueTicket.Name,
                TicketSaleValue = "1234",
                VenueTicketsUrl = "https://venue.example.com/tickets",
                Slug = "open-mic"
            };

            var mapped = mapper.ToEventItem(command);

            ClassicAssert.AreEqual(venueId, mapped.VenueId);
            ClassicAssert.AreEqual(TicketSaleProvider.VenueTicket.Name, mapped.TicketSaleProvider.Name);
            ClassicAssert.AreEqual(OpenMicOption.ShowForm.Name, mapped.OpenMicSignup.Name);
            ClassicAssert.AreEqual(command.EventTime, mapped.EventTime);
        }

        [Test]
        public void AlterPodcast_maps_cover_image_and_published_date_in_both_directions()
        {
            var mapper = new AlterPodcast.SliceMapper();
            var command = new AlterPodcast.Command
            {
                Id = Guid.NewGuid(),
                Title = "Blast Pod",
                Description = "Description",
                PublishedDate = new DateTime(2026, 4, 1),
                PodcastUrl = "https://example.com/podcast",
                RssCategory = "Music",
                RssSubcategory = "Indie",
                CoverImage = new ImageFile
                {
                    Id = Guid.NewGuid(),
                    Title = "Cover",
                    ImageUrl = "https://cdn.example.com/cover.jpg"
                },
                OwnerName = "Blast CMS",
                OwnerEmail = "owner@example.com",
                ExplicitContent = true,
                Slug = "blast-pod"
            };

            var mapped = mapper.ToPodcast(command);
            var reverseMapped = mapper.ToCommand(mapped);

            ClassicAssert.AreEqual(command.PublishedDate, mapped.PublishedDate);
            ClassicAssert.AreEqual(command.CoverImage.ImageUrl, mapped.CoverImage.ImageUrl);
            ClassicAssert.AreEqual(command.OwnerEmail, reverseMapped.OwnerEmail);
            ClassicAssert.AreEqual(command.ExplicitContent, reverseMapped.ExplicitContent);
        }

        [Test]
        public void AlterPodcastEpisode_maps_podcast_id_collection_and_reverse_projection()
        {
            var mapper = new AlterPodcastEpisode.SliceMapper();
            var podcastId = Guid.NewGuid();
            var command = new AlterPodcastEpisode.Command
            {
                Id = Guid.NewGuid(),
                PodcastId = new Guid?[] { podcastId },
                Title = "Episode 1",
                Author = "Blast",
                Tags = new[] { "tag1", "tag2" },
                PublishedDate = new DateTime(2026, 4, 5),
                Image = new ImageFile
                {
                    Id = Guid.NewGuid(),
                    Title = "Episode",
                    ImageUrl = "https://cdn.example.com/episode.jpg"
                },
                Summary = "Summary",
                Content = "Content",
                Episode = 1,
                Duration = "00:45:00",
                Mp3Url = "https://cdn.example.com/episode.mp3",
                YouTubeUrl = "https://youtube.com/watch?v=abc123",
                Slug = "episode-1"
            };

            var mapped = mapper.ToPodcastEpisode(command);
            var reverseMapped = mapper.ToCommand(mapped);

            ClassicAssert.AreEqual(podcastId, mapped.PodcastId);
            CollectionAssert.AreEqual(new Guid?[] { podcastId }, reverseMapped.PodcastId.ToArray());
            ClassicAssert.AreEqual(command.Duration, reverseMapped.Duration);
            ClassicAssert.AreEqual(command.Image.ImageUrl, reverseMapped.Image.ImageUrl);
        }

        [Test]
        public void AlterImageFile_maps_tags_and_urls_in_both_directions()
        {
            var mapper = new AlterImageFile.SliceMapper();
            var command = new AlterImageFile.Command
            {
                Id = Guid.NewGuid(),
                Title = "Hero",
                Tags = new[] { "homepage", "featured" },
                Description = "Homepage hero",
                ImageStorageName = "hero.jpg",
                ImageUrl = "https://cdn.example.com/hero.jpg"
            };

            var mapped = mapper.ToImageFile(command);
            var reverseMapped = mapper.ToCommand(mapped);

            CollectionAssert.AreEquivalent(command.Tags, mapped.Tags);
            ClassicAssert.AreEqual(command.ImageStorageName, mapped.ImageStorageName);
            CollectionAssert.AreEquivalent(command.Tags, reverseMapped.Tags);
            ClassicAssert.AreEqual(command.ImageUrl, reverseMapped.ImageUrl);
        }

        [Test]
        public void PutTenant_maps_all_tenant_configuration_fields()
        {
            var mapper = new PutTenant.SliceMapper();
            var command = new PutTenant.Command
            {
                Id = Guid.NewGuid().ToString(),
                Identifier = "tenant-1",
                Name = "Tenant 1",
                CustomerId = "customer-1",
                ReferenceId = "ref-1",
                IdentityTenantId = "identity-1",
                OpenIdConnectClientId = "client-id",
                OpenIdConnectAuthority = "https://identity.example.com",
                OpenIdConnectClientSecret = "secret",
                ChallengeScheme = "OpenIdConnect"
            };

            var mapped = mapper.ToTenant(command);

            ClassicAssert.AreEqual(command.Id, mapped.Id);
            ClassicAssert.AreEqual(command.Identifier, mapped.Identifier);
            ClassicAssert.AreEqual(command.OpenIdConnectAuthority, mapped.OpenIdConnectAuthority);
            ClassicAssert.AreEqual(command.ChallengeScheme, mapped.ChallengeScheme);
        }

        [Test]
        public void AlterUser_maps_blast_user_in_both_directions()
        {
            var mapper = new AlterUser.SliceMapper();
            var command = new AlterUser.Command
            {
                Id = "user-1",
                FirstName = "Brad",
                LastName = "Jolicoeur",
                Email = "brad@example.com",
                Active = true
            };

            var mapped = mapper.ToBlastUser(command);
            var reverseMapped = mapper.ToCommand(mapped);

            ClassicAssert.AreEqual(command.Email, mapped.Email);
            ClassicAssert.AreEqual(command.Active, mapped.Active);
            ClassicAssert.AreEqual(command.FirstName, reverseMapped.FirstName);
            ClassicAssert.AreEqual(command.LastName, reverseMapped.LastName);
        }

        [Test]
        public async Task MartenTenantStore_get_all_async_maps_and_enriches_tenants()
        {
            var tenant = new BlastTenant
            {
                Id = "tenant-1",
                Identifier = "tenant-identifier",
                Name = "Tenant Name",
                CustomerId = "customer-1",
                ReferenceId = "reference-1",
                IdentityTenantId = "identity-1",
                OpenIdConnectClientId = "client-id",
                OpenIdConnectAuthority = "https://identity.example.com",
                OpenIdConnectClientSecret = "secret",
                ChallengeScheme = "OpenIdConnect",
                AdminTenant = true
            };

            var dispatcher = new Mock<IDispatcher>();
            dispatcher
                .Setup(x => x.Send(It.IsAny<GetTenants.Query>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetTenants.PagedData(new[] { tenant }, 1, 1));

            var sut = new MartenTenantStore(
                dispatcher.Object,
                new HostTenantConfig(BillingProvider.Paddle, AuthenticationProvider.FusionAuth));

            var result = (await sut.GetAllAsync()).Single();

            ClassicAssert.AreEqual(tenant.Id, result.Id);
            ClassicAssert.AreEqual(tenant.Identifier, result.Identifier);
            ClassicAssert.AreEqual(tenant.OpenIdConnectAuthority, result.OpenIdConnectAuthority);
            ClassicAssert.AreEqual(BillingProvider.Paddle, result.BillingProvider);
            ClassicAssert.AreEqual(AuthenticationProvider.FusionAuth, result.AuthenticationProvider);
            ClassicAssert.IsTrue(result.AdminTenant);
        }

        [Test]
        public async Task MartenTenantStore_try_get_async_maps_and_enriches_single_tenant()
        {
            var tenant = new BlastTenant
            {
                Id = "tenant-1",
                Identifier = "tenant-identifier",
                Name = "Tenant Name",
                CustomerId = "customer-1",
                ReferenceId = "reference-1",
                IdentityTenantId = "identity-1",
                OpenIdConnectClientId = "client-id",
                OpenIdConnectAuthority = "https://identity.example.com",
                OpenIdConnectClientSecret = "secret",
                ChallengeScheme = "OpenIdConnect",
                AdminTenant = true
            };

            var dispatcher = new Mock<IDispatcher>();
            dispatcher
                .Setup(x => x.Send(It.IsAny<GetTenant.Query>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetTenant.Model(tenant));

            var sut = new MartenTenantStore(
                dispatcher.Object,
                new HostTenantConfig(BillingProvider.Paddle, AuthenticationProvider.FusionAuth));

            var result = await sut.TryGetAsync(tenant.Id);

            ClassicAssert.AreEqual(tenant.Id, result.Id);
            ClassicAssert.AreEqual(tenant.Name, result.Name);
            ClassicAssert.AreEqual(tenant.OpenIdConnectClientId, result.OpenIdConnectClientId);
            ClassicAssert.AreEqual(BillingProvider.Paddle, result.BillingProvider);
            ClassicAssert.AreEqual(AuthenticationProvider.FusionAuth, result.AuthenticationProvider);
        }

        [Test]
        public async Task ScanAndAddUrlToFeed_handler_maps_meta_and_sets_current_date()
        {
            var articleUrl = $"https://example.com/article-{Guid.NewGuid():N}";
            var meta = new MetaInformation(articleUrl)
            {
                HasData = true,
                Title = "Mapped Title",
                Author = "Mapped Author",
                Description = "Mapped Description",
                Keywords = "music,indie",
                ImageUrl = "https://cdn.example.com/article.jpg",
                SiteName = "Blast"
            };

            var scraper = new Mock<IMetaScraper>();
            scraper
                .Setup(x => x.GetMetaDataFromUrl(articleUrl))
                .ReturnsAsync(meta);

            var sut = new ScanAndAddUrlToFeed.Handler(scraper.Object, Tests.SessionFactory);
            var before = DateTime.UtcNow;

            var result = await sut.Handle(new ScanAndAddUrlToFeed.Command(articleUrl), CancellationToken.None);

            var after = DateTime.UtcNow;

            ClassicAssert.IsNotNull(result);
            ClassicAssert.IsNotNull(result.FeedArticle);
            ClassicAssert.AreEqual(meta.Title, result.FeedArticle.Title);
            ClassicAssert.AreEqual(meta.ArticleUrl, result.FeedArticle.ArticleUrl);
            ClassicAssert.AreEqual(meta.ImageUrl, result.FeedArticle.ImageUrl);
            ClassicAssert.AreEqual(meta.SiteName, result.FeedArticle.SiteName);
            ClassicAssert.GreaterOrEqual(result.FeedArticle.DatePosted, before.AddSeconds(-1));
            ClassicAssert.LessOrEqual(result.FeedArticle.DatePosted, after.AddSeconds(1));

            using var session = Tests.SessionFactory.QuerySession();
            var storedArticle = session.Query<FeedArticle>().First(x => x.ArticleUrl == articleUrl);
            ClassicAssert.AreEqual(meta.Title, storedArticle.Title);
            ClassicAssert.GreaterOrEqual(storedArticle.DatePosted, before.AddSeconds(-1));
            ClassicAssert.LessOrEqual(storedArticle.DatePosted, after.AddSeconds(1));
        }
    }
}
