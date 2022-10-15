# RimWorld ModSdk for unit testing

This is a variant of the `Zlepper.RimWorld.ModSdk` package, which is used for unit testing.
It is not intended to be used for actual mod development. I would recommend reading the
documentation for that package first, and then come back here.

This package enables using testing through NUnit. If you want to use XUnit or equivalent, you will have to
do some work yourself. I have not tested this with other testing frameworks, but it should be possible to
get it working with some tweaking.

## Installation

Like for the normal ModSdk you have to create a new project, but instead you use this configuration in the .csproj file:

```xml
<Project Sdk="Zlepper.RimWorld.ModSdk.Testing/0.0.4">
    <ItemGroup>
        <ProjectReference Include="..\MyMod\MyMod.csproj"/>
    </ItemGroup>
</Project>
```

This assumes that you have a project called `MyMod` in the same solution, which is the project you want to test.

## Configuration

There a fewer configuration than for the normal `ModSdk` since you don't need to run something,
so here is just an overview of the settings:

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
      Will add `global using` for `RimWorld`, `Verse`, `System`, `System.Collections.Generic`, `System.Linq` and `NUnit.Framework`.
    -->
    <RimWorldImplicitUsings>enable</RimWorldImplicitUsings>

    <!-- 
      The path to your RimWorld installation. If you don't have RimWorld installed in the default location
     you will have to tweak this for things to work. Though if the folder don't exist, you should get an
     error message when you try to build your project. The concrete default value depends on your operating system.
     -->

    <RimWorldPath>C:\Program Files (x86)\Steam\steamapps\common\RimWorld</RimWorldPath>

</PropertyGroup>
```

### Adding a reference to a RimWorld assembly

By default the SDK references `Assembly-CSharp` and `UnityEngine.CoreModule` for you. If you need to reference
another assembly, you can do so by adding a reference to it in the project file. For example, if you want to
reference `UnityEngine.IMGUIModule`, you can add the following to your project file. Sadly these kind of references
are not transitively inherited from the mod project itself, so you have to repeat them here again.

```xml
<ItemGroup>
    <RimWorldAssemblyReference Include="UnityEngine.IMGUIModule"/>
</ItemGroup>
```
