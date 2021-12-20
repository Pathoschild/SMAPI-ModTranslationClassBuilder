using Microsoft.CodeAnalysis;

namespace Pathoschild.Stardew.ModTranslationClassBuilder.Framework
{
    /// <summary>Diagnostic messages which can be logged by the translation class builder.</summary>
    internal static class Diagnostics
    {
        /*********
        ** Accessors
        *********/
        /// <summary>A diagnostic message logged when an unhandled exception occurs during code generation.</summary>
        /// <remarks>This diagnostic has one format parameter: the exception details.</remarks>
        public static readonly DiagnosticDescriptor UnhandledException = new DiagnosticDescriptor(
            id: "TCB0001",
            title: "An exception was thrown by the translation class builder",
            messageFormat: "An exception was thrown by the translation class builder. Technical details: {0}",
            category: "TranslationClassBuilder",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

        /// <summary>A diagnostic message logged when the <c>i18n/default.json</c> file wasn't found.</summary>
        /// <remarks>This diagnostic has no format parameters.</remarks>
        public static readonly DiagnosticDescriptor NoTranslationFile = new DiagnosticDescriptor(
            id: "TCB0002",
            title: "No 'i18n/default.json' file was found",
            messageFormat: "No i18n/default.json file was found. Make sure the file exists and you added an AdditionalFiles entry for it to your project file. See usage at https://github.com/Pathoschild/SMAPI-ModTranslationClassBuilder#readme.",
            category: "TranslationClassBuilder",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

        /// <summary>A diagnostic message logged when the <c>i18n/default.json</c> file wasn't found.</summary>
        /// <remarks>This diagnostic has no format parameters.</remarks>
        public static readonly DiagnosticDescriptor NoTranslationEntries = new DiagnosticDescriptor(
            id: "TCB0003",
            title: "No translations were found in the i18n/default.json file",
            messageFormat: "No translations were found in the i18n/default.json file",
            category: "TranslationClassBuilder",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

        /// <summary>A diagnostic message logged when the <c>AdditionalFiles</c> entry in the project file has an invalid option.</summary>
        /// <remarks>This diagnostic has three format parameters: the assembly name, option name, and error phrase.</remarks>
        public static readonly DiagnosticDescriptor InvalidProjectOption = new DiagnosticDescriptor(
            id: "TCB0004",
            title: "The AdditionalFiles entry in the project file has an invalid option format",
            messageFormat: "The 'TranslationClassBuilder_{1}' build property in {0}.csproj is invalid: {2}",
            category: "TranslationClassBuilder",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );
    }
}
