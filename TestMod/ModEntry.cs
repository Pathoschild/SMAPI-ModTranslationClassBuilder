using StardewModdingAPI;

namespace Pathoschild.Stardew.TestMod
{
    /// <inheritdoc />
    public class ModEntry : Mod
    {
        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            I18n.Init(helper.Translation);
        }
    }
}
