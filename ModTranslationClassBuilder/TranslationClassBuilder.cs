using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Pathoschild.Stardew.ModTranslationClassBuilder.Framework;

namespace Pathoschild.Stardew.ModTranslationClassBuilder
{
    /// <summary>Generates a strongly-typed wrapper around a mod's <c>i18n</c> translation files.</summary>
    public static class TranslationClassBuilder
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Generate the code for a translation wrapper class.</summary>
        /// <param name="jsonPath">The absolute path to the <c>i18n/default.json</c> file for which to generate a wrapper class.</param>
        /// <param name="className">The name of the class to generate.</param>
        /// <param name="classModifiers">The access modifiers to set for the class.</param>
        /// <param name="addGetByKey">Whether to add a <c>GetByKey</c> method which fetches a translation by its key, bypassing the strongly-typed methods.</param>
        /// <param name="addKeyMap">Whether to add a static class to get constant translation keys.</param>
        public static string Generate(string jsonPath, string className = "I18n", string classModifiers = "internal static", bool addGetByKey = false, bool addKeyMap = false)
        {
            // get metadata
            var reservedNames = new HashSet<string>(TranslationClassBuilder.GetReservedNames(className, addKeyMap));
            TranslationEntry[] entries = TranslationClassBuilder.ReadTranslationFile(jsonPath, reservedNames).ToArray();
            bool usesDictionary = entries.SelectMany(p => p.Tokens).Any(p => p.ParameterName != p.Key);
            string @namespace = (string)System.Runtime.Remoting.Messaging.CallContext.LogicalGetData("NamespaceHint");

            // build output
            StringBuilder output = new StringBuilder();
            output
                .AppendLine("using System;")
                .AppendLine("using System.CodeDom.Compiler;");
            if (usesDictionary)
            {
                output
                    .AppendLine("using System.Collections.Generic;");
            }
            output
                .AppendLine("using System.Diagnostics.CodeAnalysis;")
                .AppendLine("using StardewModdingAPI;")
                .AppendLine()
                .AppendLine($"namespace {@namespace}")
                .AppendLine("{")
                .AppendLine("    /// <summary>Get translations from the mod's <c>i18n</c> folder.</summary>")
                .AppendLine("    /// <remarks>This is auto-generated from the <c>i18n/default.json</c> file when the T4 template is saved.</remarks>")
                .AppendLine(@"    [GeneratedCode(""TextTemplatingFileGenerator"", ""1.0.0"")]")
                .AppendLine(@"    [SuppressMessage(""ReSharper"", ""InconsistentNaming"", Justification = ""Deliberately named for consistency and to match translation conventions."")]")
                .AppendLine($"    {classModifiers} class {className}")
                .AppendLine("    {")
                .AppendLine("        /*********")
                .AppendLine("        ** Fields")
                .AppendLine("        *********/")
                .AppendLine("        /// <summary>The mod's translation helper.</summary>")
                .AppendLine("        private static ITranslationHelper Translations;")
                .AppendLine()
                .AppendLine();

            if (addKeyMap)
            {
                output
                    .AppendLine("        /*********")
                    .AppendLine("        ** Accessors")
                    .AppendLine("        *********/")
                    .AppendLine("        /// <summary>A lookup of available translation keys.</summary>")
                    .AppendLine(@"        [SuppressMessage(""ReSharper"", ""MemberHidesStaticFromOuterClass"", Justification = ""Using the same key is deliberate."")]")
                    .AppendLine("        public static class Keys")
                    .AppendLine("        {");

                for (int i = 0; i < entries.Length; i++)
                {
                    TranslationEntry entry = entries[i];

                    if (i != 0)
                        output.AppendLine();

                    output
                        .AppendLine($@"            /// <summary>The unique key for a translation equivalent to ""{entry.GetTranslationTextForXmlDoc()}"".</summary>")
                        .AppendLine($@"            public const string {entry.MethodName} = ""{entry.Key}"";");
                }

                output
                    .AppendLine("        }")
                    .AppendLine()
                    .AppendLine();
            }

            output
                .AppendLine("        /*********")
                .AppendLine("        ** Public methods")
                .AppendLine("        *********/")
                .AppendLine("        /// <summary>Construct an instance.</summary>")
                .AppendLine(@"        /// <param name=""translations"">The mod's translation helper.</param>")
                .AppendLine("        public static void Init(ITranslationHelper translations)")
                .AppendLine("        {")
                .AppendLine($"            {className}.Translations = translations;")
                .AppendLine("        }");

            foreach (TranslationEntry entry in entries)
            {
                // summary doc
                output
                    .AppendLine()
                    .AppendLine($@"        /// <summary>Get a translation equivalent to ""{entry.GetTranslationTextForXmlDoc()}"".</summary>");

                // param docs
                foreach (var token in entry.Tokens)
                    output.AppendLine($@"        /// <param name=""{TranslationClassBuilder.PrefixIdentifierIfNeeded(token.ParameterName)}"">The value to inject for the <c>{{{{{token.Key}}}}}</c> token.</param>");

                // method
                {
                    string renderedKey = addKeyMap ? $"Keys.{entry.MethodName}" : $@"""{entry.Key}""";
                    string renderedArgs = string.Join(", ", entry.Tokens.Select(token => $"object {token.ParameterName}"));
                    string renderedTokenObj = entry.Tokens.Any() ? $", {TranslationClassBuilder.GenerateTokenParameter(entry.Tokens)}" : "";

                    output
                        .AppendLine($@"        public static string {entry.MethodName}({renderedArgs})")
                        .AppendLine("        {")
                        .AppendLine($@"            return {className}.GetByKey({renderedKey}{renderedTokenObj});")
                        .AppendLine("        }");
                }
            }

            if (addGetByKey)
                output.AppendLine();
            else
            {
                output
                    .AppendLine()
                    .AppendLine()
                    .AppendLine("        /*********")
                    .AppendLine("        ** Private methods")
                    .AppendLine("        *********/");
            }

            output
                .AppendLine("        /// <summary>Get a translation by its key.</summary>")
                .AppendLine(@"        /// <param name=""key"">The translation key.</param>")
                .AppendLine(@"        /// <param name=""tokens"">An object containing token key/value pairs. This can be an anonymous object (like <c>new { value = 42, name = ""Cranberries"" }</c>), a dictionary, or a class instance.</param>")
                .AppendLine($"        {(addGetByKey ? "public" : "private")} static string GetByKey(string key, object tokens = null)")
                .AppendLine("        {")
                .AppendLine($"            if ({className}.Translations == null)")
                .AppendLine($@"                throw new InvalidOperationException($""You must call {{nameof({className})}}.{{nameof({className}.Init)}} from the mod's entry method before reading translations."");")
                .AppendLine($"            return {className}.Translations.Get(key, tokens);")
                .AppendLine("        }");

            output
                .AppendLine("    }")
                .AppendLine("}");

            return output.ToString();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the names which can't be used by a translation key.</summary>
        /// <param name="className">The name of the class to generate.</param>
        /// <param name="addKeyMap">Whether to add a static class to get constant translation keys.</param>
        private static IEnumerable<string> GetReservedNames(string className, bool addKeyMap)
        {
            yield return className;
            yield return "Init";
            yield return "Translations";
            yield return "GetByKey";
            if (addKeyMap)
                yield return "Keys";
        }

        /// <summary>Parse an <c>i18n</c> translation file.</summary>
        /// <param name="jsonPath">The absolute path to the <c>i18n/default.json</c> file for which to generate a wrapper class.</param>
        /// <param name="reservedNames">The names that can't be used for a translation method.</param>
        private static IEnumerable<TranslationEntry> ReadTranslationFile(string jsonPath, ISet<string> reservedNames)
        {
            // read file
            string json = File.ReadAllText(jsonPath);
            var entries = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            // parse entries
            foreach (var entry in entries)
            {
                yield return new TranslationEntry
                {
                    Key = entry.Key,
                    MethodName = TranslationClassBuilder.FormatMethodName(entry.Key, reservedNames),
                    TranslationText = entry.Value,
                    Tokens = TranslationClassBuilder.GetTokenNames(entry.Value).ToArray()
                };
            }
        }

        /// <summary>Get the token names used in a translation value.</summary>
        /// <param name="text">The translation text to parse.</param>
        private static IEnumerable<TranslationToken> GetTokenNames(string text)
        {
            ISet<string> tokens = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (Match match in Regex.Matches(text, @"{{ *([\w\.\-]+?) *}}"))
            {
                string name = match.Groups[1].Value;
                if (tokens.Add(name))
                {
                    yield return new TranslationToken
                    {
                        Key = name,
                        ParameterName = TranslationClassBuilder.PrefixIdentifierIfNeeded(name)
                    };
                }
            }
        }

        /// <summary>Get a valid method name for a translation key.</summary>
        /// <param name="key">The translation key fetched by the method.</param>
        /// <param name="reservedNames">The names that can't be used for a translation method.</param>
        private static string FormatMethodName(string key, ISet<string> reservedNames)
        {
            // convert key to method name (e.g. "some.key-here" => "Some_KeyHere")
            string name;
            {
                StringBuilder str = new StringBuilder();
                bool nextIsCapital = true;
                foreach (char ch in key)
                {
                    if (ch == '.')
                    {
                        nextIsCapital = true;
                        str.Append("_");
                    }
                    else if (!char.IsLetterOrDigit(ch))
                        nextIsCapital = true;
                    else
                    {
                        str.Append(nextIsCapital
                            ? char.ToUpper(ch)
                            : ch
                            );
                        nextIsCapital = false;
                    }
                }
                name = str.ToString();
            }

            // escape duplicate method names
            while (reservedNames.Contains(name))
                name = "_" + name;

            return TranslationClassBuilder.PrefixIdentifierIfNeeded(name);
        }

        /// <summary>Add a <c>_</c> prefix to a method or parameter name if needed.</summary>
        /// <param name="name">The method or parameter name.</param>
        private static string PrefixIdentifierIfNeeded(string name)
        {
            // identifiers must start with a letter or _
            return char.IsDigit(name[0])
                ? "_" + name
                : name;
        }

        /// <summary>Generate the tokens argument for the translation helper.</summary>
        private static string? GenerateTokenParameter(TranslationToken[] tokens)
        {
            // no tokens
            if (!tokens.Any())
                return null;

            // object syntax (if all translation keys are valid property names)
            if (tokens.All(token => token.ParameterName == token.Key))
                return "new { " + string.Join(", ", tokens.Select(token => token.Key)) + " }";

            // full dictionary format
            return "new Dictionary<string, object> { " + string.Join(", ", tokens.Select(token => $@"[""{token.Key}""] = {token.ParameterName}")) + " }";
        }
    }
}
