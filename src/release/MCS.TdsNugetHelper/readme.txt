
NOTE:
The intention of this project is to download the required dependencies for tds (SitecoreAssemblyPath) into a local folder.
With the introduction of packagereferences a local nuget folder is no longer possible.
Some time it was possible to point to the global nuget folder ($(UserProfile)\.nuget\packages),
but at the moment with the upcoming different formats of the same dll (.net core/standard) tds will chose the wrong versions...

To prepare the required assemblies for tds, this project needs to have all the relevant nuget packages configured (old way: packages.config),
with low dependencies to gather some more dependencies:
- Sitecore.Kernel 9.2
-- includes
--- Sitecore.Logging
--- Microsoft.Extensions.DependencyInjection.Abstractions
--- Microsoft.Extensions.DependencyInjection
- Sitecore.Update 9.2
-- includes
--- Sitecore.Zip

For each sitecore version update, please update also this readme and the nuget package versions!
For installation of nuget packages via packages.config it may be required to change your visual studio config temporarily:
Tools > Options> NuGet Package Manager > General> Default package management format is set to Packages.config
