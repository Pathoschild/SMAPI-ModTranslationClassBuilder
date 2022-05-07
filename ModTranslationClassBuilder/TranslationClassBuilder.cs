using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using Pathoschild.Stardew.ModTranslationClassBuilder.Framework;

namespace Pathoschild.Stardew.ModTranslationClassBuilder
{
    /// <summary>Generates a strongly-typed wrapper around a mod's <c>i18n</c> translation files.</summary>
    [Generator]
    public class TranslationClassBuilder : ISourceGenerator
    {
        /*********
        ** Public methods
        *********/
        /// <inheritdoc/>
        public void Initialize(GeneratorInitializationContext context) { }

        /// <inheritdoc/>
        public void Execute(GeneratorExecutionContext context)
        {
            try
            {
                // get generator options
                string @namespace = this.ReadOption(context, "Namespace", raw => raw) ?? this.GetRootNamespace(context) ?? "Pathoschild.I18n";
                string className = this.ReadOption(context, "ClassName", raw => raw) ?? "I18n";
                string classModifiers = this.ReadOption(context, "ClassModifiers", raw => raw) ?? "internal static";
                bool addGetByKey = this.ReadOption<bool?>(context, "AddGetByKey", raw => bool.Parse(raw)) ?? false;
                bool addKeyMap = this.ReadOption<bool?>(context, "AddKeyMap", raw => bool.Parse(raw)) ?? false;

                // get translations
                TranslationEntry[] entries = this.ReadTranslationFile(context, new HashSet<string>(this.GetReservedNames(className, addKeyMap)));

                // build output
                StringBuilder output = new();
                output
                    .AppendLine("#nullable enable")
                    .AppendLine("using System;")
                    .AppendLine("using System.CodeDom.Compiler;");

                if (entries.Any(p => p.TokenParameterStyle == TokenParameterStyle.Dictionary))
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
                    .AppendLine("    /// <remarks>This is auto-generated from the <c>i18n/default.json</c> file when the project is compiled.</remarks>")
                    .AppendLine(@"    [GeneratedCode(""TextTemplatingFileGenerator"", ""1.0.0"")]")
                    .AppendLine(@"    [SuppressMessage(""ReSharper"", ""InconsistentNaming"", Justification = ""Deliberately named for consistency and to match translation conventions."")]")
                    .AppendLine($"    {classModifiers} class {className}")
                    .AppendLine("    {")
                    .AppendLine("        /*********")
                    .AppendLine("        ** Fields")
                    .AppendLine("        *********/")
                    .AppendLine("        /// <summary>The mod's translation helper.</summary>")
                    .AppendLine("        private static ITranslationHelper? Translations;")
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
                        output.AppendLine($@"        /// <param name=""{this.PrefixIdentifierIfNeeded(token.ParameterName)}"">The value to inject for the <c>{{{{{token.Key}}}}}</c> token.</param>");

                    // method
                    {
                        string renderedKey = addKeyMap ? $"Keys.{entry.MethodName}" : $@"""{entry.Key}""";
                        string renderedArgs = string.Join(", ", entry.Tokens.Select(token => $"object? {token.ParameterName}"));
                        string renderedTokenObj = entry.Tokens.Any() ? $", {this.GenerateTokenParameter(entry)}" : "";

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
                    .AppendLine($"        {(addGetByKey ? "public" : "private")} static Translation GetByKey(string key, object? tokens = null)")
                    .AppendLine("        {")
                    .AppendLine($"            if ({className}.Translations == null)")
                    .AppendLine($@"                throw new InvalidOperationException($""You must call {{nameof({className})}}.{{nameof({className}.Init)}} from the mod's entry method before reading translations."");")
                    .AppendLine($"            return {className}.Translations.Get(key, tokens);")
                    .AppendLine("        }");

                output
                    .AppendLine("    }")
                    .AppendLine("}")
                    .AppendLine();

                // Add the source code to the compilation
                context.AddSource($"{className}.generated.cs", output.ToString());
            }
            catch (Exception ex)
            {
                this.LogException(context, ex);
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the names which can't be used by a translation key.</summary>
        /// <param name="className">The name of the class to generate.</param>
        /// <param name="addKeyMap">Whether to add a static class to get constant translation keys.</param>
        private IEnumerable<string> GetReservedNames(string className, bool addKeyMap)
        {
            yield return className;
            yield return "Init";
            yield return "Translations";
            yield return "GetByKey";
            if (addKeyMap)
                yield return "Keys";
        }

        /// <summary>Read the translation entries from the context.</summary>
        /// <param name="context">The source generator execution context.</param>
        /// <param name="reservedNames">The names that can't be used for a translation method.</param>
        private TranslationEntry[] ReadTranslationFile(GeneratorExecutionContext context, ISet<string> reservedNames)
        {
            foreach (AdditionalText file in context.AdditionalFiles)
            {
                // skip if not i18n/default.json
                if (!Path.GetFullPath(file.Path).EndsWith(Path.Combine("i18n", "default.json"), StringComparison.OrdinalIgnoreCase))
                    continue;

                // read file
                string json = File.ReadAllText(file.Path);
                var rawEntries = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                // parse entries
                TranslationEntry[] entries = rawEntries
                    .Select(entry => new TranslationEntry(
                        key: entry.Key,
                        methodName: this.FormatMethodName(entry.Key, reservedNames),
                        translationText: entry.Value,
                        tokens: this.GetTokenNames(entry.Value).ToArray()
                    ))
                    .ToArray();
                if (!entries.Any())
                    this.LogDiagnostic(context, Diagnostics.NoTranslationEntries);

                return entries;
            }

            // none found
            this.LogDiagnostic(context, Diagnostics.NoTranslationFile);
            return Array.Empty<TranslationEntry>();
        }

        /// <summary>Get the token names used in a translation value.</summary>
        /// <param name="text">The translation text to parse.</param>
        private IEnumerable<TranslationToken> GetTokenNames(string text)
        {
            ISet<string> tokens = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (Match match in Regex.Matches(text, @"{{ *([\w\.\-]+?) *}}"))
            {
                string name = match.Groups[1].Value;
                if (tokens.Add(name))
                    yield return new TranslationToken(name, this.PrefixIdentifierIfNeeded(name));
            }
        }

        /// <summary>Get a valid method name for a translation key.</summary>
        /// <param name="key">The translation key fetched by the method.</param>
        /// <param name="reservedNames">The names that can't be used for a translation method.</param>
        private string FormatMethodName(string key, ISet<string> reservedNames)
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

            return this.PrefixIdentifierIfNeeded(name);
        }

        /// <summary>Add a <c>_</c> prefix to a method or parameter name if needed.</summary>
        /// <param name="name">The method or parameter name.</param>
        private string PrefixIdentifierIfNeeded(string name)
        {
            // identifiers must start with a letter or _
            return char.IsDigit(name[0])
                ? "_" + name
                : name;
        }

        /// <summary>Generate the tokens argument for the translation helper.</summary>
        /// <param name="entry">The translation entry.</param>
        private string? GenerateTokenParameter(TranslationEntry entry)
        {
            return entry.TokenParameterStyle switch
            {
                TokenParameterStyle.AnonymousObject => "new { " + string.Join(", ", entry.Tokens.Select(token => token.Key)) + " }",
                TokenParameterStyle.Dictionary => "new Dictionary<string, object> { " + string.Join(", ", entry.Tokens.Select(token => $@"[""{token.Key}""] = {token.ParameterName}")) + " }",
                _ => null
            };
        }

        /// <summary>Get the project's root namespace, if available.</summary>
        /// <param name="context">The source generator execution context.</param>
        private string? GetRootNamespace(GeneratorExecutionContext context)
        {
            return context.AnalyzerConfigOptions.GlobalOptions.TryGetValue($"build_property.RootNamespace", out string? rootNamespace) && !string.IsNullOrWhiteSpace(rootNamespace)
                ? rootNamespace
                : context.Compilation.AssemblyName;
        }

        /// <summary>Read a translation class builder option from the project build properties.</summary>
        /// <typeparam name="T">The expected option value type.</typeparam>
        /// <param name="context">The source generator execution context.</param>
        /// <param name="name">The name of the option to read, without the <c>TranslationClassBuilder_</c> prefix.</param>
        /// <param name="parse">Parse the raw string representation into the expected value type.</param>
        private T? ReadOption<T>(GeneratorExecutionContext context, string name, Func<string, T> parse)
        {
            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue($"build_property.TranslationClassBuilder_{name}", out string? raw) && !string.IsNullOrWhiteSpace(raw))
            {
                try
                {
                    return parse(raw);
                }
                catch (Exception ex)
                {
                    string? typeName = typeof(T).FullName;
                    if (typeof(T) == typeof(bool?))
                        typeName = "boolean";

                    this.LogInvalidOption(context, name, $"can't parse value '{raw}' as a {typeName}. Technical details: {ex.ToString().Replace("\r", "").Replace("\n", "\\n")}");
                }
            }

            return default(T?);
        }

        /// <summary>Log a diagnostic error message for an unhandled exception.</summary>
        /// <param name="context">The source generation context.</param>
        /// <param name="exception">The exception to log.</param>
        private void LogException(GeneratorExecutionContext context, Exception exception)
        {
            string details = exception.ToString().Replace("\r", "").Replace("\n", "\\n"); // error messages don't allow newlines
            this.LogDiagnostic(context, Diagnostics.UnhandledException, details);
        }

        /// <summary>Log a diagnostic message for an invalid build property option in the project file.</summary>
        /// <param name="context">The source generation context.</param>
        /// <param name="optionName">The name of the invalid option.</param>
        /// <param name="errorPhrase">The error phrase indicating why parsing the option failed.</param>
        private void LogInvalidOption(GeneratorExecutionContext context, string optionName, string errorPhrase)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(Diagnostics.InvalidProjectOption, Location.None, context.Compilation.AssemblyName, optionName, errorPhrase)
            );
        }

        /// <summary>Log a diagnostic message.</summary>
        /// <param name="context">The source generation context.</param>
        /// <param name="descriptor">The diagnostic message descriptor.</param>
        /// <param name="args">The message format arguments.</param>
        private void LogDiagnostic(GeneratorExecutionContext context, DiagnosticDescriptor descriptor, params object[] args)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(descriptor, Location.None, args)
            );
        }
    }
}
