namespace blastcms.ArticleScanService
{
    public class MetaInformation
    {
        public bool HasData { get; set; }
        public string ArticleUrl { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public string Keywords { get; set; }
        public string ImageUrl { get; set; }
        public string SiteName { get; set; }

        public MetaInformation(string url)
        {
            ArticleUrl = url;
            HasData = false;
        }
        public MetaInformation() { }

    }
}
