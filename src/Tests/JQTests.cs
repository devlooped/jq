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

        JqResult result = await JQ.ExecuteAsync(json, ".name", "");
        Assert.Equal(0, result.ExitCode);
        Assert.Equal("John", result.StandardOutput);

        result = await JQ.ExecuteAsync(json, ".age", "");
        Assert.Equal(0, result.ExitCode);
        Assert.Equal("30", result.StandardOutput);
    }

    [Fact]
    public async Task SupportsUTF8()
    {
        var json = await File.ReadAllTextAsync("sample.json", Encoding.UTF8);
        JqResult result = await JQ.ExecuteAsync(json, "[.itemListElement[].item]", "");

        Assert.Equal(0, result.ExitCode);
        Assert.NotEmpty(result.StandardOutput);
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

    [Fact]
    public async Task SupportsCommentsInQuery()
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
    # this should return the name
    name: .name
}
");

        var person = JsonSerializer.Deserialize<Person>(data, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.NotNull(person);
        Assert.Equal("John", person.Name);
    }

    [Fact]
    public async Task Errors()
    {
        string json = "{}";
        JqResult result = await JQ.ExecuteAsync(json, "bummer", null, null);

        Assert.NotEqual(0, result.ExitCode);
        Assert.Contains("bummer", result.StandardError);
    }

    [Fact]
    public async Task SupportsJsonArgs()
    {
        string json = "{}";
        JqResult result = await JQ.ExecuteAsync(json, ". + {\"a\": $a}", "", new Dictionary<string, string>() { { "a", "\"argument_value\"" } });

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("argument_value", result.StandardOutput);
    }

    record Person(string Name);
}