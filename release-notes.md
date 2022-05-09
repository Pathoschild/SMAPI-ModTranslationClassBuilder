← [README](README.md)

# Release notes
## 2.0.1
Released 08 May 2022.

* The generated token arguments are now marked nullable.

## 2.0.0
Released 19 December 2021.

* Updated for .NET 5 in Stardew Valley 1.5.5.
* Added nullable annotations to generated class.
* You no longer need a `.tt` template in your project.
* You no longer need to compile the project before using `I18n`.
* The package no longer writes the generated class to your project folder by default.

**Update note:**  
To update a project which uses an older version of the package:
1. delete the `I18n.tt` and `I18n.cs` files;
2. remove `<BundleExtraAssemblies>ThirdParty</BundleExtraAssemblies>` from your `.csproj` file if you only added it for this project;
3. update the package;
4. if you customized the settings, see the [new usage docs](README.md).

## 1.0.1
Released 11 October 2021.

* Added support for 64-bit in Stardew Valley 1.5.5.

## 1.0.0
Released 23 September 2020.

* Initial implementation.
* Added builder arguments to configure output (`className`, `classModifiers`, `addGetByKey`, `addKeyMap`).
* Added automatic namespace based on location within project.
