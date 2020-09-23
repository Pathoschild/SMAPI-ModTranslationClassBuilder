namespace Pathoschild.Stardew.ModTranslationClassBuilder.Framework
{
    /// <summary>A translation placeholder used in a translation entry.</summary>
    internal class TranslationToken
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The raw placeholder token name.</summary>
        public string Key { get; set; }

        /// <summary>The normalized name for a parameter which sets the token value.</summary>
        public string ParameterName { get; set; }
    }
}
