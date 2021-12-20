namespace Pathoschild.Stardew.ModTranslationClassBuilder.Framework
{
    /// <summary>The format to use when passing token values to the generated <c>GetByKey</c> method.</summary>
    internal enum TokenParameterStyle
    {
        /// <summary>The translation has no tokens.</summary>
        None,

        /// <summary>Pass the tokens as an anonymous object like <c>new { tokenA, token B }</c>. This is the default format.</summary>
        AnonymousObject,

        /// <summary>Pass the tokens as a dictionary instance like <c>new Dictionary&lt;string, object&gt; { ["tokenA"] = tokenA, ["tokenB"] = tokenB }</c>. This format is only used when a token name isn't valid as a field name.</summary>
        Dictionary
    }
}
