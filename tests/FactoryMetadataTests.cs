using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace PepperDash.Essentials.Plugins.Camera.Visca.Tests;

public class FactoryMetadataTests
{
    private static readonly string SrcDir = AssemblyFixture.SourceDirectory;

    [Fact]
    public void All_Factory_Sources_Set_MinimumEssentialsFrameworkVersion_To_3()
    {
        var factoryFile = Path.Combine(SrcDir, "ViscaCameraFactory.cs");
        File.Exists(factoryFile).Should().BeTrue($"factory source file should exist at {factoryFile}");

        var content = File.ReadAllText(factoryFile);
        content.Should().Contain("MinimumEssentialsFrameworkVersion",
            "factory must set MinimumEssentialsFrameworkVersion");

        var match = Regex.Match(content, @"MinimumEssentialsFrameworkVersion\s*=\s*""([^""]+)""");
        match.Success.Should().BeTrue("MinimumEssentialsFrameworkVersion should be assigned a string value");

        var version = Version.Parse(match.Groups[1].Value);
        version.Major.Should().BeGreaterThanOrEqualTo(3,
            "MinimumEssentialsFrameworkVersion should be >= 3.0.0 for v3 plugins");
    }

    [Fact]
    public void All_Factory_Sources_Set_TypeNames()
    {
        var factoryFile = Path.Combine(SrcDir, "ViscaCameraFactory.cs");
        var content = File.ReadAllText(factoryFile);

        content.Should().Contain("TypeNames",
            "factory must assign TypeNames in constructor");
    }

    [Theory]
    [InlineData("ViscaCameraFactory.cs", "visca")]
    [InlineData("ViscaCameraFactory.cs", "viscacamera")]
    public void Factory_Source_Contains_TypeName(string fileName, string typeName)
    {
        var factoryFile = Path.Combine(SrcDir, fileName);
        var content = File.ReadAllText(factoryFile);

        content.Should().Contain($"\"{typeName}\"",
            $"factory '{fileName}' should register type name '{typeName}'");
    }

    [Fact]
    public void No_Duplicate_TypeNames_Across_Factory_Sources()
    {
        var csFiles = Directory.GetFiles(SrcDir, "*.cs", SearchOption.AllDirectories);
        var allTypeNames = new List<string>();

        foreach (var file in csFiles)
        {
            var content = File.ReadAllText(file);
            if (!content.Contains("TypeNames")) continue;

            var matches = Regex.Matches(content, @"TypeNames\s*=\s*new\s+List<string>\s*\{([^}]+)\}");
            foreach (Match match in matches)
            {
                var names = Regex.Matches(match.Groups[1].Value, @"""([^""]+)""")
                    .Cast<Match>()
                    .Select(m => m.Groups[1].Value);
                allTypeNames.AddRange(names);
            }
        }

        allTypeNames.Should().OnlyHaveUniqueItems("TypeNames must not have duplicates across factories");
    }
}
