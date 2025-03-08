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
using blastcms.ArticleScanService.CaptureMeta;

namespace blastcms.ArticleScanService
{
    public class MetaScraperOpenAI : IMetaScraper
    {
        private ILogger<MetaScraperOpenAI> _logger;
        private string _OpenAIKey;
        private readonly ICaptureMetaFactory _captureMetaFactory;

        public MetaScraperOpenAI(ILogger<MetaScraperOpenAI> logger, IConfiguration configuration, ICaptureMetaFactory captureMetaFactory) 
        { 
            _logger = logger;
            _OpenAIKey = configuration["OPENAI_KEY"];
            _captureMetaFactory = captureMetaFactory;
        }


        /// <summary>
        /// Uses HtmlAgilityPack  and OpenAI to get the meta information from a url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<MetaInformation> GetMetaDataFromUrl(string url)
        {
            var uri = new Uri(url);
            var captureMeta = _captureMetaFactory.GetCaptureMeta(url);

            var captureResults = await captureMeta.GetMeta(url);

            string ModelId = "gpt-4o-mini";

            // Create a kernel with OpenAI chat completion
            var builder = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(ModelId, _OpenAIKey);

            Kernel kernel = builder.Build();

            // Create and print out the prompt
            string prompt = $"""
                Consider a JSON schema for Article Summary that includes the following  properties: Author:string, Title:string, Summary:string, KeyWords:string, ImageUrl:string 
    
                Please summarize the the following text in 30 words or less for software engineers as the audience and output in json:
                {captureResults.Data}

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


    }
}
