using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Schema.NET;

namespace recipe_importer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(args[0]);

            using (FileStream zipToOpen = new FileStream(@args[0], FileMode.Open))
            {
                var recipes = RecipeKeeperImporter.GetSchemaRecipes(zipToOpen);

                string recipesJsonStr = SchemaSerializer.SerializeObject(recipes);
                
                // FIXME: line andings are currently not write correct to file 
                // correct the line endings for host OS
                //recipesJsonStr = Regex.Replace(recipesJsonStr, @"\\r\\n?|\\n", Environment.NewLine);

                File.WriteAllText("recipes.json", recipesJsonStr, Encoding.UTF8);
                Console.WriteLine(recipesJsonStr); 
            }

            Console.WriteLine("Import completed!");
        }
    }
}
