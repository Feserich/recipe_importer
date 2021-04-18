namespace recipe_importer
{
    using System;
    using System.Collections.Generic;
    using HtmlAgilityPack;
    using System.IO.Compression;
    using System.Xml;
    using System.Text.RegularExpressions;
    using System.IO;

    class RecipeKeeperImporter
    {
        public static List<SchemaRecipe> GetSchemaRecipes(System.IO.Stream zipFile)
        {
            List<SchemaRecipe> recipes = new List<SchemaRecipe>();

            ZipArchive archive = new ZipArchive(zipFile);
            // process the html file in the zip
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if(entry.FullName.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
                {
                    recipes = ParseRecipeKeeperHtml(entry.Open());
                }
            }

            // store the images from the found recipes
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if(entry.FullName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
                {
                    // check if image name is in recipe list

                    //unzippedEntryStream = entry.Open(); // .Open will return a stream
                    // Process entry data here
                }
            }

            return recipes;
        }

        private static List<string> MultiLineNodeToStringList(HtmlNode node)
        {
            List<string> stringList = new List<string>();

            var directionLines = Regex.Split(node.InnerText, "\r\n|\r|\n");
            foreach (var line in directionLines)
            {
                var lineTrimmed = line.Trim();
                if(lineTrimmed != "")
                {
                    stringList.Add(lineTrimmed);
                }
            }

            return stringList;
        }

        private static string ParseName(HtmlNode recipeNode)
        {
            try
            {
                return recipeNode.SelectSingleNode("//h2[contains(@itemprop, 'name')]").InnerText;
            }
            catch (System.Exception)
            {
                return "";
            }
        }

        private static string ParseImagePath(HtmlNode recipeNode)
        {
            try
            {
                return recipeNode.SelectSingleNode("//img[contains(@class, 'recipe-photo')]").Attributes["src"].Value;
            }
            catch (System.Exception)
            {
                return "";
            }
        }

        private static string ParseRecipeYield(HtmlNode recipeNode)
        {
            try
            {
                return recipeNode.SelectSingleNode("//span[contains(@itemprop, 'recipeYield')]").InnerText;
            }
            catch (System.Exception)
            {
                return "";
            }
        }

        private static List<string> ParseInstructions(HtmlNode recipeNode)
        {
            List<string> instructions = new List<string>();

            try
            {
                var instructionNode = recipeNode.SelectSingleNode("//div[contains(@itemprop, 'recipeDirections')]");
                instructions = MultiLineNodeToStringList(instructionNode);
            }
            catch (System.Exception)
            {
                instructions.Add("");
            }

            return instructions;
        }

        private static List<string> ParseIngredients(HtmlNode recipeNode)
        {
            List<string> ingredients = new List<string>();

            try
            {
                var ingredientNode = recipeNode.SelectSingleNode("//div[contains(@itemprop, 'recipeIngredients')]");
                ingredients = MultiLineNodeToStringList(ingredientNode);
            }
            catch (System.Exception)
            {
                ingredients.Add("");
            }

            return ingredients;
        }

        private static string ParseAuthor(HtmlNode recipeNode)
        {
            try
            {
                return recipeNode.SelectSingleNode("//span[contains(@itemprop, 'recipeSource')]").InnerText;
            }
            catch (System.Exception)
            {
                return "";
            }
        }

        private static TimeSpan ParsePreparationTime(HtmlNode recipeNode)
        {
            try
            {
                string timeStr = recipeNode.SelectSingleNode("//meta[contains(@itemprop, 'prepTime')]").Attributes["content"].Value;
                return XmlConvert.ToTimeSpan(timeStr);
            }
            catch (System.Exception)
            {
                return new TimeSpan(0);
            }
        }

        private static TimeSpan ParseCookingTime(HtmlNode recipeNode)
        {
            try
            {
                string timeStr = recipeNode.SelectSingleNode("//meta[contains(@itemprop, 'cookTime')]").Attributes["content"].Value;
                return XmlConvert.ToTimeSpan(timeStr);
            }
            catch (System.Exception)
            {
                return new TimeSpan(0);
            }
        }

        private static string ParseCalories(HtmlNode recipeNode)
        {
            try
            {
                return recipeNode.SelectSingleNode("//span[contains(@itemprop, 'recipeNutCalories')]").InnerText;
            }
            catch (System.Exception)
            {
                return "";
            }
        }


        private static List<SchemaRecipe> ParseRecipeKeeperHtml(System.IO.Stream htmlStream)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.Load(htmlStream);

            var htmlBody = htmlDoc.DocumentNode.SelectSingleNode("//body");
            var recipeNodes =  htmlBody.SelectNodes("//div[contains(@class, 'recipe-details')]");

            List<SchemaRecipe> recipes = new List<SchemaRecipe>();

            foreach(HtmlNode htmlRecipe in recipeNodes)
            {
                // Remove parent node of the current node with all children of it. 
                // Otherwise SelectNode will be executed on all recipes instead of only the current one. 
                htmlRecipe.ParentNode.RemoveChild(htmlRecipe, false);

                string recipeName = ParseName(htmlRecipe);
                string imagePath = ParseImagePath(htmlRecipe);
                string yield = ParseRecipeYield(htmlRecipe);
                List<string> instructions = ParseInstructions(htmlRecipe);
                List<string> ingredients = ParseIngredients(htmlRecipe);
                string recipeAuthor = ParseAuthor(htmlRecipe);
                TimeSpan preparationTime = ParsePreparationTime(htmlRecipe);
                TimeSpan cookingTime = ParseCookingTime(htmlRecipe);
                string recipeCalories = ParseCalories(htmlRecipe);

                List<string> emptyStringList = new List<string>();
                emptyStringList.Add("");

                if((recipeName == "") && instructions.Capacity == emptyStringList.Capacity && instructions[0] == emptyStringList[0])
                {
                    // skipe empty recipe 
                }
                else
                {
                    recipes.Add(new SchemaRecipe 
                    {
                        name = recipeName,
                        image = imagePath,
                        recipeYield = yield,
                        recipeInstructions = instructions,
                        recipeIngredient = ingredients,
                        author = recipeAuthor,
                        prepTime = preparationTime,
                        cookTime = cookingTime,
                        
                        nutrition = new NutritionInformation
                        {
                            calories = recipeCalories
                        }
                    }); 
                }
            }

            return recipes;
        }

    }
}