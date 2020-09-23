namespace Pathoschild.Stardew.ModTranslationClassBuilder.Framework
{
    /// <summary>A parsed entry from a translation file.</summary>
    internal class TranslationEntry
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The raw translation key from the file.</summary>
        public string Key { get; set; }

        /// <summary>The normalized name for a method which wraps access to the translation key.</summary>
        public string MethodName { get; set; }

        /// <summary>The raw translation text from the file.</summary>
        public string TranslationText { get; set; }

        /// <summary>The placeholder tokens used in the translation.</summary>
        public TranslationToken[] Tokens { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get the translation text escaped for use in XML doc comments.</summary>
        public string GetTranslationTextForXmlDoc()
        {
            string text = this.TranslationText;

            if (text.Length > Constants.MaxExcerptLength)
                text = text.Substring(0, Constants.MaxExcerptLength - 3) + "...";

            foreach (var pair in Constants.ReplaceExcerptStrings)
                text = text.Replace(pair.Key, pair.Value);

            return text;
        }
    }
}
