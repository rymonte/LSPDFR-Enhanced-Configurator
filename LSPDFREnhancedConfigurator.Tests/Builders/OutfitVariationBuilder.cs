using LSPDFREnhancedConfigurator.Models;

namespace LSPDFREnhancedConfigurator.Tests.Builders
{
    /// <summary>
    /// Fluent builder for creating Outfit and OutfitVariation test data
    /// </summary>
    public class OutfitVariationBuilder
    {
        private string _outfitName = "LSPD Class A";
        private string _outfitScriptName = "lspd_class_a";
        private string _variationName = "Officer";
        private string _variationScriptName = "officer";
        private Outfit _parentOutfit = null;

        /// <summary>
        /// Set the outfit name
        /// </summary>
        public OutfitVariationBuilder WithOutfitName(string outfitName)
        {
            _outfitName = outfitName;
            return this;
        }

        /// <summary>
        /// Set the outfit script name
        /// </summary>
        public OutfitVariationBuilder WithOutfitScriptName(string scriptName)
        {
            _outfitScriptName = scriptName;
            return this;
        }

        /// <summary>
        /// Set the variation name
        /// </summary>
        public OutfitVariationBuilder WithVariationName(string variationName)
        {
            _variationName = variationName;
            return this;
        }

        /// <summary>
        /// Set the variation script name
        /// </summary>
        public OutfitVariationBuilder WithVariationScriptName(string scriptName)
        {
            _variationScriptName = scriptName;
            return this;
        }

        /// <summary>
        /// Set the parent outfit
        /// </summary>
        public OutfitVariationBuilder WithParentOutfit(Outfit parentOutfit)
        {
            _parentOutfit = parentOutfit;
            return this;
        }

        /// <summary>
        /// Build the OutfitVariation instance (creates parent if not provided)
        /// </summary>
        public OutfitVariation Build()
        {
            // Create parent outfit if not provided
            if (_parentOutfit == null)
            {
                _parentOutfit = new Outfit(_outfitName, _outfitScriptName);
            }

            var variation = new OutfitVariation(_variationName, _variationScriptName, _parentOutfit);
            _parentOutfit.Variations.Add(variation);

            return variation;
        }

        /// <summary>
        /// Build an Outfit with multiple variations
        /// </summary>
        /// <param name="variationNames">Names of variations to create</param>
        public Outfit BuildOutfitWithVariations(params string[] variationNames)
        {
            var outfit = new Outfit(_outfitName, _outfitScriptName);

            foreach (var variationName in variationNames)
            {
                var scriptName = variationName.ToLowerInvariant().Replace(" ", "_");
                var variation = new OutfitVariation(variationName, scriptName, outfit);
                outfit.Variations.Add(variation);
            }

            return outfit;
        }

        /// <summary>
        /// Create a default LSPD outfit variation for quick testing
        /// </summary>
        public static OutfitVariation CreateDefault()
        {
            return new OutfitVariationBuilder()
                .WithOutfitName("LSPD Class A")
                .WithOutfitScriptName("lspd_class_a")
                .WithVariationName("Officer")
                .WithVariationScriptName("officer")
                .Build();
        }

        /// <summary>
        /// Create a complete LSPD Class A outfit with multiple variations
        /// </summary>
        public static Outfit CreateLSPDClassAOutfit()
        {
            return new OutfitVariationBuilder()
                .WithOutfitName("LSPD Class A")
                .WithOutfitScriptName("lspd_class_a")
                .BuildOutfitWithVariations("Officer", "Officer II", "Officer III");
        }

        /// <summary>
        /// Create a complete LSSD outfit with variations
        /// </summary>
        public static Outfit CreateLSSDOutfit()
        {
            return new OutfitVariationBuilder()
                .WithOutfitName("LSSD Deputy")
                .WithOutfitScriptName("lssd_deputy")
                .BuildOutfitWithVariations("Deputy", "Deputy II");
        }

        /// <summary>
        /// Create a male-specific variation (for gender testing)
        /// </summary>
        public static OutfitVariation CreateMaleVariation()
        {
            return new OutfitVariationBuilder()
                .WithOutfitName("LSPD Class A")
                .WithOutfitScriptName("lspd_class_a")
                .WithVariationName("Officer Male")
                .WithVariationScriptName("officer_m_y")
                .Build();
        }

        /// <summary>
        /// Create a female-specific variation (for gender testing)
        /// </summary>
        public static OutfitVariation CreateFemaleVariation()
        {
            return new OutfitVariationBuilder()
                .WithOutfitName("LSPD Class A")
                .WithOutfitScriptName("lspd_class_a")
                .WithVariationName("Officer Female")
                .WithVariationScriptName("officer_f_y")
                .Build();
        }
    }
}
