using FluentAssertions;
using Xunit;

namespace PepperDash.Essentials.Plugins.Camera.Visca.Tests;

public class ConfigDeserializationTests
{
    [Theory]
    [InlineData("ViscaCameraConfig")]
    [InlineData("ViscaCameraPresetsConfig")]
    public void Config_Class_Exists(string className)
    {
        var type = AssemblyFixture.PluginAssembly.GetTypes()
            .FirstOrDefault(t => t.Name == className);
        type.Should().NotBeNull($"config class '{className}' should exist in the assembly");
    }

    [Theory]
    [InlineData("ViscaCameraConfig")]
    [InlineData("ViscaCameraPresetsConfig")]
    public void Config_Has_Parameterless_Constructor(string className)
    {
        var type = AssemblyFixture.PluginAssembly.GetTypes()
            .First(t => t.Name == className);
        var ctor = type.GetConstructor(Type.EmptyTypes);
        ctor.Should().NotBeNull($"config class '{className}' must have a parameterless constructor for JSON deserialization");
    }

    [Theory]
    [InlineData("ViscaCameraConfig.cs", "control")]
    [InlineData("ViscaCameraConfig.cs", "address")]
    [InlineData("ViscaCameraConfig.cs", "panSpeed")]
    [InlineData("ViscaCameraConfig.cs", "tiltSpeed")]
    [InlineData("ViscaCameraConfig.cs", "ZoomSpeed")]
    [InlineData("ViscaCameraConfig.cs", "FocusSpeed")]
    [InlineData("ViscaCameraConfig.cs", "PrivacyOnPreset")]
    [InlineData("ViscaCameraConfig.cs", "PrivacyOffPreset")]
    [InlineData("ViscaCameraConfig.cs", "pollTimeMs")]
    [InlineData("ViscaCameraConfig.cs", "presets")]
    public void Config_Has_JsonProperty(string fileName, string propertyName)
    {
        var filePath = Path.Combine(AssemblyFixture.SourceDirectory, fileName);
        File.Exists(filePath).Should().BeTrue($"source file '{fileName}' should exist");

        var content = File.ReadAllText(filePath);
        content.Should().Contain($"JsonProperty(\"{propertyName}\")",
            $"config should have [JsonProperty(\"{propertyName}\")] attribute");
    }
}
