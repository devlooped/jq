using System.Text;
using System.Text.Json;
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

    [Fact]
    public async Task SupportsUTF8Output()
    {
        var json =
            """
            {
              "tier": "sponsor 💜"
            }
            """;

        var query = await JQ.ExecuteAsync(json, ".tier");

        Assert.NotEmpty(query);
        Assert.Equal("sponsor 💜", query);
    }

    [Fact]
    public async Task ReplacesLineEndingsOnQuery()
    {
        var json =
            """
            {
              "name": "John",
              "age": 30
            }
            """;

        var data = await JQ.ExecuteAsync(json, @". | 
{
    name: .name
}
");

        var person = JsonSerializer.Deserialize<Person>(data, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.NotNull(person);
        Assert.Equal("John", person.Name);
    }

    record Person(string Name);
}