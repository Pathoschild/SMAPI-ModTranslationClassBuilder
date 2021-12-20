**ModTranslationClassBuilder** autogenerates a strongly-typed class to access [`i18n`
translation files](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Translation)
from your [SMAPI](https://smapi.io/) mod code.

## Contents
* [Why does this exist?](#why-does-this-exist)
* [Usage](#usage)
  * [First-time setup](#first-time-setup)
  * [Conventions](#conventions)
* [Customization](#customization)
* [See also](#see-also)

## Why does this exist?
<dl>
<dt>Without the package:</dt>
<dd>

Mods use code like this to read their translations:
```c#
string text = helper.Translation.Get("range-value", new { min = 1, max = 5 });
```

Unfortunately there's no validation at this point; if the key is `range` (not `range-value`) or the
token name is `minimum` (not `min`), you won't know until you test that part of the mod in-game and
see an error message.

That also means that after changing the translation files, you need to manually search the code for
anywhere that referenced the translations to update them. That gets pretty tedious with larger
mods, which might have hundreds of translations used across dozens of files.

</dd>
<dt>With the package:</dt>
<dd>

This package lets you write code like this instead:
```c#
string text = I18n.RangeValue(min: 1, max: 5);
```

Since it's strongly typed, it's validated immediately as you type. For example, if you accidentally
typed `I18n.RangeValues` instead, you'll see an immediate error that `RangeValues` doesn't exist
without needing to test it in-game (or even compile the mod).

</dd>
</dl>

See the [test mod](TestMod) for an example of the generated class in an actual mod.

## Usage
### First-time setup
1. [Install the NuGet package](https://www.nuget.org/packages/Pathoschild.Stardew.ModTranslationClassBuilder).
2. In your mod's `Entry` method, add this line:
   ```c#
   I18n.Init(helper.Translation);
   ```
3. If needed, click _Build > Rebuild Solution_ to regenerate the `I18n` class.

That's it! Now you can immediately use `I18n` anywhere in your mod code. The class will be updated
whenever you rebuild the project.

### Conventions
* The class uses your assembly name as the default namespace; so if your mod project is `YourMod`,
  then the generated file will be `YourMod.I18n`. You can [change that](#customization) if needed.
* Translation keys are converted to CamelCase, with `.` changed to `_` to help group categories.

  For example:

  key in `i18n/default.json` | method
  -------------------------- | --------------------------
  `ready`                    | `I18n.Ready()`
  `ready-now`                | `I18n.ReadyNow()`
  `generic.ready-now`        | `I18n.Generic_ReadyNow()`

## Customization
You can configure the `I18n` class using a `<PropertyGroup>` section in your mod's `.csproj` file.
Each property must be prefixed with `TranslationClassBuilder_`. For example, this changes the class
name to `Translations`:

```xml
<PropertyGroup>
   <TranslationClassBuilder_ClassName>Translations</TranslationClassBuilder_ClassName>
</PropertyGroup>
```

Main options:

argument         | description | default value
---------------- | ----------- | ------------
`AddGetByKey`    | Whether to add a method to fetch a translation by its key, like `I18n.GetByKey("ready-now")` | `false`
`AddKeyMap`      | Whether to add a nested static class to access translation keys like `I18n.Keys.ReadyNow`. | `false`
`ClassName`      | The name of the generated class. | `I18n`
`Namespace`      | The namespace for the generated class. | _project's root namespace_

Advanced options:

argument         | description | default value
---------------- | ----------- | ------------
`ClassModifiers` | The [access modifiers](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/access-modifiers) to apply to the generated class (e.g. to make it public). | `internal static`
`CreateBackup`   | Whether to add a backup of the generated class to the project folder in a `Generated` subfolder. If it's disabled, the generated file will be hidden and excluded from source control. | `false`

## See also
* [Release notes](release-notes.md)
