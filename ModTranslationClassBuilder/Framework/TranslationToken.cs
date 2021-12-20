namespace Pathoschild.Stardew.ModTranslationClassBuilder.Framework
{
    /// <summary>A translation placeholder used in a translation entry.</summary>
    internal class TranslationToken
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The raw placeholder token name.</summary>
        public string Key { get; }

        /// <summary>The normalized name for a parameter which sets the token value.</summary>
        public string ParameterName { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="key">The raw placeholder token name.</param>
        /// <param name="parameterName">The normalized name for a parameter which sets the token value.</param>
        public TranslationToken(string key, string parameterName)
        {
            this.Key = key;
            this.ParameterName = parameterName;
        }
    }
}
