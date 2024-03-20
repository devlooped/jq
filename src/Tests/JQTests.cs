using Devlooped;

namespace Tests;

public class JQTests
{
    [Fact]
    public async Task SanityCheckAsync()
    {
        var json =
            """
            {
              "name": "John",
              "age": 30
            }
            """;

        Assert.Equal("John", await JQ.ExecuteAsync(json, ".name"));
        Assert.Equal("30", await JQ.ExecuteAsync(json, ".age"));
    }
}