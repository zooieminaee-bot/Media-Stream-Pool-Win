using System.Text;
using MediaStreamPool.Infrastructure.Decoder;

namespace MediaStreamPool.Tests;

public sealed class DecoderTests
{
    [Fact]
    public async Task MissingConfigurationFailsWithoutAttemptingDecryption()
    {
        var decoder = new AesGzipDecoder(null, null);

        var result = await decoder.DecodeAsync(Encoding.UTF8.GetBytes("not-base64"));

        Assert.False(result.Success);
        Assert.Contains("not configured", result.Error, StringComparison.OrdinalIgnoreCase);
    }
}
