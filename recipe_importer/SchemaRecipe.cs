
namespace recipe_importer
{
    using System;
    using System.Collections.Generic;

    class NutritionInformation
    {
        public string calories{ get; set; }
        public string fatContent{ get; set; }
    }

    class SchemaRecipe 
    {
        public string name{ get; set; }
        public string image{ get; set; }
        public string recipeYield{ get; set; }
        public List<string> recipeInstructions{ get; set; }
        public List<string> recipeIngredient{ get; set; }
        public string author{ get; set; }
        public TimeSpan prepTime{ get; set; }
        public TimeSpan cookTime{ get; set; }
        public NutritionInformation nutrition{ get; set; }
    }

}