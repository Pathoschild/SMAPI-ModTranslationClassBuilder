using System;
using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace Pathoschild.Stardew.TestMod
{
    /// <summary>Get translations from the mod's <c>i18n</c> folder.</summary>
    /// <remarks>This is auto-generated from the <c>i18n/default.json</c> file when the T4 template is saved.</remarks>
    [GeneratedCode("TextTemplatingFileGenerator", "1.0.0")]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Deliberately named for consistency and to match translation conventions.")]
    internal static class I18n
    {
        /*********
        ** Fields
        *********/
        /// <summary>The mod's translation helper.</summary>
        private static ITranslationHelper Translations;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="translations">The mod's translation helper.</param>
        public static void Init(ITranslationHelper translations)
        {
            I18n.Translations = translations;
        }

        /// <summary>Get a translation equivalent to "yesterday".</summary>
        public static string Yesterday()
        {
            return I18n.GetByKey("yesterday");
        }

        /// <summary>Get a translation equivalent to "ready now!".</summary>
        public static string ReadyNow()
        {
            return I18n.GetByKey("ready-now");
        }

        /// <summary>Get a translation equivalent to "{{value}}%".</summary>
        /// <param name="value">The value to inject for the <c>{{value}}</c> token.</param>
        public static string Percentage(object value)
        {
            return I18n.GetByKey("percentage", new { value });
        }

        /// <summary>Get a translation equivalent to "Some text that\nhas newlines".</summary>
        public static string EdgeCases_TextWithNewlines()
        {
            return I18n.GetByKey("edge-cases.text-with-newlines");
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get a translation by its key.</summary>
        /// <param name="key">The translation key.</param>
        /// <param name="tokens">An object containing token key/value pairs. This can be an anonymous object (like <c>new { value = 42, name = "Cranberries" }</c>), a dictionary, or a class instance.</param>
        private static string GetByKey(string key, object tokens = null)
        {
            if (I18n.Translations == null)
                throw new InvalidOperationException($"You must call {nameof(I18n)}.{nameof(I18n.Init)} from the mod's entry method before reading translations.");
            return I18n.Translations.Get(key, tokens);
        }
    }
}

