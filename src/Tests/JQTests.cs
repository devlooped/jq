using System.Text;
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

    [Fact]
    public async Task SupportsUTF8()
    {
        var json = await File.ReadAllTextAsync("sample.json", Encoding.UTF8);
        var query = await JQ.ExecuteAsync(json, "[.itemListElement[].item]");

        Assert.NotEmpty(query);
    }
}