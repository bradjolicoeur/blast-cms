using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace blastcms.web.Swagger
{
    public class InjectSamples : Swashbuckle.AspNetCore.SwaggerGen.IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {

            var path = swaggerDoc.Paths.Where(x => x.Key.Contains("blogarticle")).First().Value;

            string source = @"PetStore.v1.Pet pet = new PetStore.v1.Pet();
                        pet.setApiKey(""your api key"");
                        pet.petType = PetStore.v1.Pet.TYPE_DOG;
                        pet.name = ""Rex"";
                        // set other fields
                        PetStoreResponse response = pet.create();
                        if (response.statusCode == HttpStatusCode.Created)
                        {
                            // Successfully created
                        }
                        else
                        {
                            // Something wrong -- check response for errors
                            Console.WriteLine(response.getRawResponse());
                        }";
            // need to check if extension already exists, otherwise swagger 
            // tries to re-add it and results in error  


            path.Parameters.FirstOrDefault().Extensions.Add("x-codeSamples", new OpenApiObject
            {
                {"lang", new OpenApiString("C#")},
                {"label", new OpenApiString("")},
                {"source", new OpenApiString(source)},
            });
        }
    }
}
