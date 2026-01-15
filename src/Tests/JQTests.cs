using System.Collections.Generic;
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

    record Person(string Name);

    [Fact]
    public async Task JqResult_CanBeImplicitlyConvertedToString()
    {
        var json =
            """
            {
              "name": "John"
            }
            """;

        var result = await JQ.ExecuteAsync(new JqParams(json, ".name"));
        string output = result; // Implicit conversion

        Assert.Equal("John", output);
    }

    [Fact]
    public async Task JqResult_ContainsExitCode()
    {
        var json =
            """
            {
              "name": "John"
            }
            """;

        var result = await JQ.ExecuteAsync(new JqParams(json, ".name"));

        Assert.Equal(0, result.ExitCode);
    }

    [Fact]
    public async Task JqParams_SupportsArgs()
    {
        var json =
            """
            {
              "name": "John"
            }
            """;

        var result = await JQ.ExecuteAsync(new JqParams
        {
            Json = json,
            Query = "{name: .name, extra: $extra}",
            Args = new Dictionary<string, string> { { "extra", "value" } }
        });

        var data = JsonSerializer.Deserialize<Dictionary<string, string>>(result.StandardOutput, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.NotNull(data);
        Assert.Equal("John", data["name"]);
        Assert.Equal("value", data["extra"]);
    }

    [Fact]
    public async Task JqParams_SupportsArgsJson()
    {
        var json =
            """
            {
              "name": "John"
            }
            """;

        var result = await JQ.ExecuteAsync(new JqParams
        {
            Json = json,
            Query = "{name: .name, count: $count}",
            ArgsJson = new Dictionary<string, string> { { "count", "42" } }
        });

        var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(result.StandardOutput, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.NotNull(data);
        Assert.Equal("John", data["name"].GetString());
        Assert.Equal(42, data["count"].GetInt32());
    }

    [Fact]
    public async Task JqParams_CompactOutput()
    {
        var json =
            """
            {
              "name": "John",
              "age": 30
            }
            """;

        var result = await JQ.ExecuteAsync(new JqParams
        {
            Json = json,
            Query = ".",
            CompactOutput = true,
            RawOutput = false
        });

        // Compact output should not contain newlines
        Assert.DoesNotContain("\n", result.StandardOutput);
    }

    [Fact]
    public async Task JqParams_NullInput()
    {
        var result = await JQ.ExecuteAsync(new JqParams
        {
            Query = "{value: 123}",
            NullInput = true,
            RawOutput = false
        });

        var data = JsonSerializer.Deserialize<Dictionary<string, int>>(result.StandardOutput, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.NotNull(data);
        Assert.Equal(123, data["value"]);
    }

    [Fact]
    public async Task JqParams_SortKeys()
    {
        var json =
            """
            {
              "z": 1,
              "a": 2,
              "m": 3
            }
            """;

        var result = await JQ.ExecuteAsync(new JqParams
        {
            Json = json,
            Query = ".",
            SortKeys = true,
            CompactOutput = true,
            RawOutput = false
        });

        Assert.Equal("{\"a\":2,\"m\":3,\"z\":1}", result.StandardOutput);
    }

    [Fact]
    public async Task BackwardCompatibility_ExistingCodeStillWorks()
    {
        var json =
            """
            {
              "name": "John",
              "age": 30
            }
            """;

        // Test that the old API still works
        var name = await JQ.ExecuteAsync(json, ".name");
        var age = await JQ.ExecuteAsync(json, ".age");

        Assert.Equal("John", name);
        Assert.Equal("30", age);
    }
}