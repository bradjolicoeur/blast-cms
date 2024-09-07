using Azure.Core.Extensions;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel;
using ReverseMarkdown;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace blastcms.ArticleScanService
{
    public class MetaScraperOpenAI : IMetaScraper
    {
        private ILogger<MetaScraperOpenAI> _logger;
        private string _OpenAIKey;
        public MetaScraperOpenAI(ILogger<MetaScraperOpenAI> logger, IConfiguration configuration) 
        { 
            _logger = logger;
            _OpenAIKey = configuration["openai-key"];
        }


        /// <summary>
        /// Uses HtmlAgilityPack  and OpenAI to get the meta information from a url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<MetaInformation> GetMetaDataFromUrl(string url)
        {
            var uri = new Uri(url);
            _logger.LogInformation("Scraping: {url}", url);
            // Get the html from URL specified
            var webGet = new HtmlWeb();
            var document = await webGet.LoadFromWebAsync(url);
            var body = document.DocumentNode.SelectSingleNode("//body");
            var metaTags = document.DocumentNode.SelectNodes("//meta");

            _logger.LogInformation("Captured content from: {url}", url);
            //convert the meta tags to plain text so that it can be added to prompt
            var metaText = ConvertMetaTagsToText(metaTags);

            //Convert the body to markdown text so that we are sending plain text content in prompt
            string markdownText = ConvertToMarkdown(body);

            //Merge the meta and markdown together for the prompt
            string textToSummarize = MergeMetaAndMarkdown(metaText, markdownText);



            string ModelId = "gpt-4o-mini";

            // Create a kernel with OpenAI chat completion
            var builder = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(ModelId, _OpenAIKey);

            Kernel kernel = builder.Build();

            // Create and print out the prompt
            string prompt = $"""
                Consider a JSON schema for Article Summary that includes the following  properties: Author:string, Title:string, Summary:string, KeyWords:string, ImageUrl:string 
    
                Please summarize the the following text in 30 words or less for software engineers as the audience and output in json:
                {textToSummarize}

                # How to respond to this prompt
                    - No other text, just the JSON data
                """;


            _logger.LogInformation("Sending to OpenAI: {url}", url);
#pragma warning disable SKEXP0010
            // Submit the prompt and print out the response
            string response = await kernel.InvokePromptAsync<string>(
                prompt,
                new(new OpenAIPromptExecutionSettings()
                {
                    MaxTokens = 1000,
                    ResponseFormat = "json_object"
                })
                );
#pragma warning restore SKEXP0010

            var responseObject = JsonSerializer.Deserialize<OpenAIResponse>(response);

            var metaInfo = new MetaInformation
            {
                ArticleUrl = url,
                Author = responseObject.Author,
                Title = responseObject.Title,
                Description = responseObject.Summary,
                Keywords = responseObject.KeyWords,
                ImageUrl = responseObject.ImageUrl,
                HasData = true,
                SiteName = uri.Host,
            };

            _logger.LogInformation("Results parsed: {url}", url);

            return metaInfo;
        }

        private class OpenAIResponse
        {
            public string Author { get; set; }
            public string Title { get; set; }
            public string Summary { get; set; }
            public string KeyWords { get; set; }
            public string ImageUrl { get; set; }
        }

        private static string MergeMetaAndMarkdown(string metaText, string markdownText)
        {


            //merge the content and meta text together
            var sbAllText = new StringBuilder();
            sbAllText.AppendLine(metaText);
            sbAllText.AppendLine(markdownText);
            var textToSummarize = sbAllText.ToString();
            return textToSummarize;
        }

        private static string ConvertMetaTagsToText(HtmlNodeCollection metaTags)
        {
            string metaText=string.Empty;
            
            if (metaTags != null)
            {
                var sb = new StringBuilder();
                foreach (var item in metaTags)
                {
                    sb.Append(item.GetAttributeValue("property", ""));
                    sb.Append('|');
                    sb.Append(item.GetAttributeValue("content", ""));
                    sb.AppendLine();
                }
                metaText = sb.ToString();
            }

            return metaText;
        }

        private static string ConvertToMarkdown(HtmlNode body)
        {
            if (body == null)
                return string.Empty;

            var config = new ReverseMarkdown.Config
            {
                UnknownTags = Config.UnknownTagsOption.Drop
            };

            var converter = new ReverseMarkdown.Converter(config);
            string html = body.OuterHtml;

            string markdownText = converter.Convert(html);
            return markdownText;
        }
    }
}
