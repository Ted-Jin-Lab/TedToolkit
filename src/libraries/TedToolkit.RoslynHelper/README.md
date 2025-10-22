# TedToolkit.RoslynHelper

The things that I love for roslyn.

## Usage

``` xml
	<ItemGroup>
		<PackageReference Include="TedToolkit.RoslynHelper" Version="0.9.6" />
	</ItemGroup>
	<PropertyGroup>
		<GetTargetPathDependsOn>$(GetTargetPathDependsOn);GetDependencyTargetPaths</GetTargetPathDependsOn>
	</PropertyGroup>

	<Target Name="GetDependencyTargetPaths" AfterTargets="ResolvePackageDependenciesForBuild">
		<ItemGroup>
			<TargetPathWithTargetPlatformMoniker Include="@(ResolvedCompileFileDefinitions)" IncludeRuntimeDependency="false" />
		</ItemGroup>
	</Target>
```
