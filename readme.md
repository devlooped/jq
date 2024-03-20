# ./jq for dotnet

[![Version](https://img.shields.io/nuget/v/Devlooped.JQ.svg?color=royalblue)](https://www.nuget.org/packages/Devlooped.JQ) [![Downloads](https://img.shields.io/nuget/dt/Devlooped.JQ.svg?color=green)](https://www.nuget.org/packages/Devlooped.JQ) [![License](https://img.shields.io/github/license/devlooped/jq.svg?color=blue)](https://github.com/devlooped/jq/blob/main/license.txt) [![Build](https://github.com/devlooped/jq/workflows/build/badge.svg?branch=main)](https://github.com/devlooped/jq/actions)

<!-- #content -->
Packs the [jq](https://jqlang.github.io/jq/) binaries for easy execution 
from dotnet applications.

## Usage

```csharp
var name = await JQ.ExecuteAsync(
    """
    {
        "name": "John",
        "age": 30
    }
    """,
    ".name"));
```

The `JQ.Path` provides the full path to the jq binary that's appropriate 
for the current OS and architecture.

<!-- include https://github.com/devlooped/sponsors/raw/main/footer.md -->