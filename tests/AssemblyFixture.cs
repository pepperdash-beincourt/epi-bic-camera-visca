using System.Reflection;

namespace PepperDash.Essentials.Plugins.Camera.Visca.Tests;

public static class AssemblyFixture
{
    private static readonly Lazy<MetadataLoadContext> LazyContext = new(CreateContext);
    private static readonly Lazy<Assembly> LazyAssembly = new(LoadPluginAssembly);

    private static string PluginDllPath =>
        Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..",
            "src", "4Series", "bin", "Debug", "net8.0",
            "epi-camera-visca.4Series.dll"));

    private static string PluginOutputDir => Path.GetDirectoryName(PluginDllPath)!;

    public static MetadataLoadContext Context => LazyContext.Value;
    public static Assembly PluginAssembly => LazyAssembly.Value;

    private static MetadataLoadContext CreateContext()
    {
        var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        // Use a dictionary keyed by filename to avoid loading the same assembly name from multiple paths
        var dllByName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Priority 1: Plugin output directory (has the correct versions)
        foreach (var dll in Directory.GetFiles(PluginOutputDir, "*.dll"))
            dllByName[Path.GetFileName(dll)] = dll;

        // Priority 2: .NET runtime directory
        foreach (var dll in Directory.GetFiles(runtimeDir, "*.dll"))
            dllByName.TryAdd(Path.GetFileName(dll), dll);

        // Priority 3: NuGet global packages cache (fallback for transitive deps)
        var nugetDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".nuget", "packages");
        if (Directory.Exists(nugetDir))
        {
            foreach (var dll in Directory.GetFiles(nugetDir, "*.dll", SearchOption.AllDirectories))
                dllByName.TryAdd(Path.GetFileName(dll), dll);
        }

        return new MetadataLoadContext(new PathAssemblyResolver(dllByName.Values));
    }

    private static Assembly LoadPluginAssembly()
    {
        if (!File.Exists(PluginDllPath))
            throw new FileNotFoundException(
                $"Plugin DLL not found at '{PluginDllPath}'. Build the plugin first with: dotnet build src/epi-camera-visca.4Series.csproj -c Debug");
        return Context.LoadFromAssemblyPath(PluginDllPath);
    }

    public static List<Type> FindFactoryTypes(string baseTypePrefix = "EssentialsPluginDeviceFactory")
    {
        return PluginAssembly.GetTypes()
            .Where(t => !t.IsAbstract
                && t.BaseType is { IsGenericType: true }
                && t.BaseType.GetGenericTypeDefinition().Name.StartsWith(baseTypePrefix))
            .ToList();
    }

    public static string SourceDirectory =>
        Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "src"));
}
