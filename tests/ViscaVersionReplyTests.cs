using FluentAssertions;
using ViscaCameraPlugin;
using Xunit;

namespace PepperDash.Essentials.Plugins.Camera.Visca.Tests;

/// <summary>
/// The version reply and the power reply both start y0 50; the device reads the version only from a
/// reply long enough to be one, so a power answer is never taken for a firmware version.
/// </summary>
public class ViscaVersionReplyTests
{
    [Fact]
    public void Asks_the_configured_camera()
    {
        ViscaVersionReply.Inquiry(0x81).Should().Equal(0x81, 0x09, 0x00, 0x02, 0xFF);
        ViscaVersionReply.Inquiry(0x83).Should().Equal(0x83, 0x09, 0x00, 0x02, 0xFF);
    }

    [Theory]
    [InlineData(new byte[] { 0x90, 0x50, 0x00, 0x01, 0x05, 0x19, 0x01, 0x70, 0x02, 0xFF })]
    [InlineData(new byte[] { 0x90, 0x50, 0x00, 0x01, 0x05, 0x19, 0x01, 0x70, 0x02 })]
    public void Reads_vendor_model_and_rom_with_or_without_the_terminator(byte[] reply)
    {
        ViscaVersionReply.TryParse(reply, out var vendor, out var model, out var rom).Should().BeTrue();

        vendor.Should().Be(0x0001);
        model.Should().Be(0x0519);
        rom.Should().Be(0x0170);
    }

    [Fact]
    public void Answers_from_any_camera_address()
    {
        // camera 3 replies with B0
        ViscaVersionReply.TryParse(new byte[] { 0xB0, 0x50, 0x00, 0x01, 0x05, 0x19, 0x01, 0x70, 0x02, 0xFF },
            out _, out _, out _).Should().BeTrue();
    }

    [Theory]
    [InlineData(new byte[] { 0x90, 0x50, 0x02, 0xFF })]                                     // power on
    [InlineData(new byte[] { 0x90, 0x50, 0x03 })]                                           // power off, terminator gathered off
    [InlineData(new byte[] { 0x90, 0x41, 0xFF })]                                           // ack
    [InlineData(new byte[] { 0x90, 0x60, 0x02, 0xFF })]                                     // syntax error
    [InlineData(new byte[] { 0x90, 0x51, 0x00, 0x01, 0x05, 0x19, 0x01, 0x70, 0x02, 0xFF })] // completion, not a reply
    [InlineData(new byte[] { 0x81, 0x50, 0x00, 0x01, 0x05, 0x19, 0x01, 0x70, 0x02, 0xFF })] // not a reply address
    [InlineData(new byte[0])]
    public void Anything_else_is_not_a_version(byte[] reply)
    {
        ViscaVersionReply.TryParse(reply, out _, out _, out _).Should().BeFalse();
    }

    [Fact]
    public void A_missing_reply_is_not_a_version()
    {
        ViscaVersionReply.TryParse(null!, out _, out _, out _).Should().BeFalse();
    }

    [Fact]
    public void Describes_the_rom_revision_first()
    {
        ViscaVersionReply.Describe(0x0001, 0x0519, 0x0170).Should().Be("ROM 0170 (vendor 0001, model 0519)");
    }
}
