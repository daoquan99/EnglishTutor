# Packages & build rules

Central Package Management (CPM) is enabled. All version numbers live in one file.

## Files

- `EnglishTutor.API/Directory.Build.props` — solution-wide MSBuild defaults (TargetFramework, Nullable, ImplicitUsings, LangVersion).
- `EnglishTutor.API/Directory.Packages.props` — every NuGet version. `ManagePackageVersionsCentrally = true`.
- `EnglishTutor.API/global.json` — pinned .NET SDK version.

## .csproj rules

Allowed:

```xml
<ItemGroup>
  <PackageReference Include="FluentValidation" />
  <PackageReference Include="MediatR" />
</ItemGroup>
```

Forbidden:

```xml
<PackageReference Include="FluentValidation" Version="12.1.0" />   <!-- no Version -->
```

```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>                       <!-- duplicates Directory.Build.props -->
  <Nullable>enable</Nullable>
</PropertyGroup>
```

A project may add its own `<PropertyGroup>` settings only when the override is genuinely necessary (e.g., `<OutputType>Exe</OutputType>` for a host) — explain why in the PR.

## Adding a package

1. Open `Directory.Packages.props`. Check if the package is already listed.
2. Add `<PackageVersion Include="PackageName" Version="x.y.z" />`. Prefer the latest stable compatible version.
3. In the consuming `.csproj`, add `<PackageReference Include="PackageName" />` (no Version).
4. Mention the addition in the task summary.

## Version policy

- Stable, actively maintained packages only.
- No preview / beta / rc / nightly / deprecated unless explicitly approved.
- One version of a package across the entire solution. No duplicates.
- Don't add a package for something the .NET BCL or an existing dependency already covers.

## When the latest stable doesn't work

If a compatibility issue forces an older version, use the newest compatible stable and explain the reason in the task summary.

## Forbidden

- `<PackageReference ... Version="..."/>` in any `.csproj`.
- Adding the same package as two different versions across projects.
- Duplicating `TargetFramework` / `Nullable` / `ImplicitUsings` / `LangVersion` per project.
- Preview / RC / nightly packages without explicit approval.
- Skipping `Directory.Packages.props` and pulling a package transitively via `<PackageReference>` with a Version attribute.
