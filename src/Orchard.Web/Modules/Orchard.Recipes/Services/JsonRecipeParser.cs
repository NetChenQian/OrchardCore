﻿using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchard.Recipes.Models;
using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Orchard.Recipes.Services
{
    public class JsonRecipeParser : IRecipeParser
    {
        public RecipeDescriptor ParseRecipe(IFileInfo recipeFile)
        {
            var serializer = new JsonSerializer();

            using (StreamReader streamReader = new StreamReader(recipeFile.CreateReadStream()))
            {
                using (JsonTextReader reader = new JsonTextReader(streamReader))
                {
                    return serializer.Deserialize<RecipeDescriptor>(reader);
                }
            }
        }

        public void ProcessRecipe(
            IFileInfo recipeFile, 
            Action<RecipeDescriptor, RecipeStepDescriptor> action)
        {
            var descriptor = ParseRecipe(recipeFile);

            var serializer = new JsonSerializer();

            using (StreamReader streamReader = new StreamReader(recipeFile.CreateReadStream()))
            {
                using (JsonTextReader reader = new JsonTextReader(streamReader))
                {
                    // Go to Steps, then iterate.
                    while (reader.Read()) {
                        if (reader.Path == "steps" && reader.TokenType == JsonToken.StartArray)
                        {

                            int stepId = 0;
                            while (reader.Read() && reader.Depth > 1)
                            {
                                if (reader.Depth == 2)
                                {
                                    var child = JToken.Load(reader);
                                    action(descriptor, new RecipeStepDescriptor
                                    {
                                        Id = (stepId++).ToString(CultureInfo.InvariantCulture),
                                        RecipeName = descriptor.Name,
                                        Name = child.Value<string>("name"),
                                        Step = child
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
