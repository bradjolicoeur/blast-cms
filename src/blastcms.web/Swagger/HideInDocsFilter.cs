using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace blastcms.web.Swagger
{
    public class HideInDocsFilter : IDocumentFilter
    {

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var pathsToRemove = swaggerDoc.Paths
                .Where(pathItem => pathItem.Key.Contains("tenant"))
                .ToList();

            foreach (var item in pathsToRemove)
            {
                swaggerDoc.Paths.Remove(item.Key);
            }
        }
    }
}
