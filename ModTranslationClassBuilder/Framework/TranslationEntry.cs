using System.Linq;

namespace Pathoschild.Stardew.ModTranslationClassBuilder.Framework
{
    /// <summary>A parsed entry from a translation file.</summary>
    internal class TranslationEntry
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The raw translation key from the file.</summary>
        public string Key { get; }

        /// <summary>The normalized name for a method which wraps access to the translation key.</summary>
        public string MethodName { get; }

        /// <summary>The raw translation text from the file.</summary>
        public string TranslationText { get; }

        /// <summary>The placeholder tokens used in the translation.</summary>
        public TranslationToken[] Tokens { get; }

        /// <summary>The format to use when passing token values to the generated <c>GetByKey</c> method.</summary>
        public TokenParameterStyle TokenParameterStyle { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="key">The raw translation key from the file.</param>
        /// <param name="methodName">The normalized name for a method which wraps access to the translation key.</param>
        /// <param name="translationText">The raw translation text from the file.</param>
        /// <param name="tokens">The placeholder tokens used in the translation.</param>
        public TranslationEntry(string key, string methodName, string translationText, TranslationToken[] tokens)
        {
            this.Key = key;
            this.MethodName = methodName;
            this.TranslationText = translationText;
            this.Tokens = tokens;

            if (!tokens.Any())
                this.TokenParameterStyle = TokenParameterStyle.None;
            else if (tokens.All(token => token.ParameterName == token.Key))
                this.TokenParameterStyle = TokenParameterStyle.AnonymousObject;
            else
                this.TokenParameterStyle = TokenParameterStyle.Dictionary;
        }

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
