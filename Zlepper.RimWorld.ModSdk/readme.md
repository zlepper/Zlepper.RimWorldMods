# RimWorld ModSdk

This is a dotnet sdk that allows you to create mods for RimWorld with less messing with configurations.

## Features

* Provides support for C# 10 (At least the features .NetFramework can support).
* Allows multiple RimWorld versions to be supported at the same time.
* Automatically references common RimWorld assemblies.
  * Provides an easy hook for referencing more assemblies.
* Easily reference other Steam mods for your mod.
  * Harmony and HugsLib have special support, to make them even easier to reference.
  * Other local mods for development can are automatically referenced in an msbuild `<ProjectReference>` is created.
* Automatically copies the mod to the RimWorld mods folder.
* Support for Rider + Visual Studio run buttons without additional configuration.
  * Debugging is not yet magically supported and does require additional manual configuration.
* Implicit `using`s for `RimWorld`, `Verse`, `System`, `System.Collections.Generic` and `System.Linq`.
* Fully automatically generated `About.xml` file.
* The `PublishedFileId.txt` is automatically copied back into the project when the mod is build if the mod has been published to Steam.

## Installation

A word of warning: I develop on Windows and have only tested this on Windows. It should be possible to tweak enough
settings to get it working on other platforms, but I have not done so. If you are using Linux or Mac, 
please let me know if you want to help get it working for those platforms. 

1. Make sure you have at least a DotNet 6 SDK installed: https://dotnet.microsoft.com/en-us/download
2. Open your IDE and create a new project, the specific project type does not matter, but I recommend a class library.
3. Open the .csproj file and replace the content with the following:
```xml
<Project Sdk="Zlepper.RimWorld.ModSdk/0.0.6">

    <PropertyGroup>
        <RimWorldModName>Your mod name</RimWorldModName>
        <RimWorldPackageId>Your.Mod.PackageId</RimWorldPackageId>
        <Authors>Your Name</Authors>
        <Description>
          This is a good description.
          
          It can even be multiline.
        </Description>
    </PropertyGroup>

</Project>
```
4. Create a new class and add the following code:
```csharp
namespace MyName.MyFancyMod;

[StaticConstructorOnStartup]
public static class MyMod
{
    static MyMod()
    {
        Log.Message("Hello World from MyMod!");
    }
}
```
5. If you get compile errors at this point, the SDK might not be able to locate your RimWorld installation, please see the "Optional configuration" section
     and try setting `<RimWorldPath>` to the path to your RimWorld installation.
6. Run the project and wait for RimWorld to load.
7. Go into mod settings and enable your mod.
8. Check the RimWorld dev tools for a message from your mod.
9. Now, go and implement your mod! (Tip: Read the Tips and Tricks section)


## Tip and tricks

* You don't need maintain your own structure for your mod, you can just add files to your project and 
they will be copied to the mod folder for your current RimWorld version. For example: Add a `Defs` folder
with a `ThingDefs` folder inside and add a `MyThingDef.xml` file to it. It will be copied to the mod folder.
  * This also works for all the other "standard" folders RimWorld has: `About`, `Defs`, `Languages`, 
  `Textures`, `Sounds` and `Patches`. The "Assemblies" folder is generated automatically your assemblies
  when you build your project.


## Configuration
The SDK contains several settings that can be configured in the .csproj file. Some of these are required to be
set, while others are optional.


### Required configuration:

```xml
<PropertyGroup>
  <!-- 
      The display name of this mod. This will also be used as the folder name in the 
      mods folder in the root of the repository. 
   -->
  <RimWorldModName>My Mod Name</RimWorldModName>
  <!-- 
      The package id of this mod. This is used to identify the mod in the RimWorld mod settings.
      It should be unique and follow the same rules as a C# namespace.
   -->
  <RimWorldPackageId>My.Mod</RimWorldPackageId>
  
  <!-- 
    The name of the authors of the mod. If there are multiple authors, separate their names with semicolon ';', 
    e.g. <Authors>Me;MyFriend</Authors>
   -->
  
  <Authors>My Name</Authors>

  <!--
    A description of the mod. Indentation and prefix/suffix of whitespace will be stripped before
    the description is included in the About.xml file. 
  -->

  <Description>
    This is a good description.

    It can even be multiline.
  </Description>
</PropertyGroup>
```

### Optional configuration:

This shows the default values each parameter have, so if you are happy with those values, no need
to tweak them.
```xml
<PropertyGroup>
  <!--  
      If C# nullable reference types should be enabled. Do note that _nothing_ in RimWorld is annotated
      with nullability annotations, so this is purely for use in your internal mod code. You should
      still check for null in RimWorld code.
  -->
  <Nullable>enable</Nullable>
  
  <!-- The language version of C# to use. -->
  
  <LangVersion>10</LangVersion>
  
  <!-- 
      If true, the SDK will automatically add a reference to HugsLib.
   -->
  <IncludeHugsLib>false</IncludeHugsLib>
  
  <!-- 
      If true, the SDK will automatically add a reference to Harmony.
   -->
  <IncludeHarmony>false</IncludeHarmony>
  
  <!--
    The folder that contains the build output of the mod. This is used to copy the mod to the RimWorld mods folder.
    This is also whats allows you to support multiple RimWorld versions at the same time. If RimWorld
    comes with a new version, the SDK will automatically pick up the RimWorld version you have installed
    and start putting your mod into a folder with that version number.
  -->
  <OutputFolder>../Mods</OutputFolder>
  
  <!--
    Will add `global using` for `RimWorld`, `Verse`, `System`, `System.Collections.Generic` and `System.Linq`.
  -->
  <RimWorldImplicitUsings>enable</RimWorldImplicitUsings>
  
  <!-- 
    The path to your RimWorld installation. If you don't have RimWorld installed in the default location
   you will have to tweak this for things to work. Though if the folder don't exist, you should get an
   error message when you try to build your project. The concrete default value depends on your operating system.
   -->
  
  <RimWorldPath>C:\Program Files (x86)\Steam\steamapps\common\RimWorld</RimWorldPath>
  
  <!-- 
    The version of RimWorld to pack the mod for. This is used to copy the correct assemblies to the mod folder.
    If you don't set this, the SDK will try to figure out the version of RimWorld you have installed and use that,
    and you should generally not have to set this.
   -->
  
  <RimWorldVersion>(infered based on your installation)</RimWorldVersion>
  
  <!--  
    A url to your mod. Doesn't have to be set. If provided it will be included in the About.xml file.
  -->
  <RimWorldModUrl></RimWorldModUrl>
  
</PropertyGroup>
```

### Adding a reference to a RimWorld assembly
By default the SDK references `Assembly-CSharp` and `UnityEngine.CoreModule` for you. If you need to reference
another assembly, you can do so by adding a reference to it in the project file. For example, if you want to
reference `UnityEngine.IMGUIModule`, you can add the following to your project file:
```xml
<ItemGroup>
  <RimWorldAssemblyReference Include="UnityEngine.IMGUIModule" />
</ItemGroup>
```

### Adding a reference to another mod
The sdk makes it very easy to reference and use other mods in your mod. For example, if you want to reference
the VanillaExpanded framework, you can do this:
```xml
<ItemGroup>
  <RimWorldSteamModDependency Include="OskarPotocki.VanillaFactionsExpanded.Core" />
</ItemGroup>
```
This will add a reference to the mod in the `About.xml` file, and also add a reference to the mod's assembly
in your project. You can then use the mod's API in your mod.

This does require that you actually subscribe to the mod in Steam, and it has been downloaded. 
If you don't have the mod installed, you will get a compile error.

### Controlling mod loading order
The following project item tags are available to control mod loading order if you don't have an explicit
dependency to the given mod:
```xml
<ItemGroup>
  <RimWorldLoadBefore Include="Some.Mod" />
  <RimWorldLoadAfter Include="Some.Mod" />
  <RimWorldIncompatibleWith Include="Some.Mod" />
</ItemGroup>
```

These tags works like their counterparts in the `About.xml` file, and will be added to the `About.xml` file
when you build your project.

If you already reference the mod using `<RimWorldSteamModDependency>`, then you do not need to add `<RimWorldLoadAfter>`,
that is done automatically.