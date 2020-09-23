using System.Collections.Generic;

namespace Pathoschild.Stardew.ModTranslationClassBuilder.Framework
{
    /// <summary>The constant values used by the translation class builder.</summary>
    internal static class Constants
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The maximum length of the translation excerpt added to a method's XML docs.</summary>
        public const int MaxExcerptLength = 500;

        /// <summary>The string substitutions to make in translation excerpts added to a method's XML docs.</summary>
        public static IDictionary<string, string> ReplaceExcerptStrings = new Dictionary<string, string>
        {
            ["<"] = "&lt;",
            [">"] = "&gt;",
            ["\r"] = "\\r",
            ["\n"] = "\\n"
        };
    }
}
