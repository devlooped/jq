# ./jq for .net

[![Version](https://img.shields.io/nuget/v/Devlooped.JQ.svg?color=royalblue)](https://www.nuget.org/packages/Devlooped.JQ) 
[![Downloads](https://img.shields.io/nuget/dt/Devlooped.JQ.svg?color=green)](https://www.nuget.org/packages/Devlooped.JQ) 
[![License](https://img.shields.io/github/license/devlooped/jq.svg?color=blue)](https://github.com/devlooped/jq/blob/main/license.txt) 
[![Build](https://img.shields.io/github/actions/workflow/status/devlooped/jq/build.yml?branch=main)](https://github.com/devlooped/jq/actions)

<!-- #content -->
Packs the [jq](https://jqlang.github.io/jq/) binaries for easy execution 
from dotnet applications running on Linux (AMD64 and ARM64), macOS (AMD64 and ARM64) 
and Windows (AMD64 and i386).

When JsonPath falls short, `jq` is the obvious next step in flexibility 
and power for JSON manipulation.

> jq is like sed for JSON data - you can use it to slice and filter and map 
> and transform structured data with the same ease that sed, awk, grep and 
> friends let you play with text.

Learn more about `jq` at [https://jqlang.github.io/jq/](https://jqlang.github.io/jq/).

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

The `JQ.Path` static property provides the full path to the jq binary that's appropriate 
for the current OS and architecture so you can execute it directly if needed.

## Examples

The following is a real-world scenario where [WhatsApp Cloud API messages](https://developers.facebook.com/docs/whatsapp/cloud-api/webhooks/payload-examples) 
are converted into clean polymorphic JSON for nice OO deserialization via System.Text.Json.

Rather than navigating deep into the JSON structure, we can use `jq` to transform the payload 
into what we expect for deserialization of a text message:

```json
{
  "id": "wamid.HBgNMTIwM==",
  "timestamp": 1678902345,
  "to": {
    "id": "792401583610927",
    "number": "12025550123"
  },
  "from": {
    "name": "Mlx",
    "number": "12029874563"
  },
  "content": {
    "$type": "text",
    "text": "😊"
  }
}
```

The original JSON looks like the following: 

```json
{
  "object": "whatsapp_business_account",
  "entry": [
    {
      "id": "813920475102346",
      "changes": [
        {
          "value": {
            "messaging_product": "whatsapp",
            "metadata": {
              "display_phone_number": "12025550123",
              "phone_number_id": "792401583610927"
            },
            "contacts": [
              {
                "profile": { "name": "Mlx" },
                "wa_id": "12029874563"
              }
            ],
            "messages": [
              {
                "from": "12029874563",
                "id": "wamid.HBgNMTIwM==",
                "timestamp": "1678902345",
                "text": { "body": "\ud83d\ude0a" },
                "type": "text"
              }
            ]
          },
          "field": "messages"
        }
      ]
    }
  ]
}
```

The following JQ query turns the latter info the former:

```jq
.entry[].changes[].value.metadata as $phone |
.entry[].changes[].value.contacts[] as $user |
.entry[].changes[].value.messages[] | 
{
    id: .id,
    timestamp: .timestamp | tonumber,
    to: {
        id: $phone.phone_number_id,
        number: $phone.display_phone_number
    },
    from: {
        name: $user.profile.name,
        number: $user.wa_id
    },
    content:  {
        "$type": "text",
        text: .text.body
    }
}
```

This allows you to focus your C# code into the actual object model you want 
to work with, rather than the one imposed by the JSON format of external APIs.

See this code in action at [Devlooped.WhatsApp](https://github.com/devlooped/WhatsApp/blob/main/src/WhatsApp/Message.cs).


<!-- include https://github.com/devlooped/sponsors/raw/main/footer.md -->
# Sponsors 

<!-- sponsors.md -->
[![Clarius Org](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/clarius.png "Clarius Org")](https://github.com/clarius)
[![MFB Technologies, Inc.](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/MFB-Technologies-Inc.png "MFB Technologies, Inc.")](https://github.com/MFB-Technologies-Inc)
[![Torutek](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/torutek-gh.png "Torutek")](https://github.com/torutek-gh)
[![DRIVE.NET, Inc.](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/drivenet.png "DRIVE.NET, Inc.")](https://github.com/drivenet)
[![Keith Pickford](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/Keflon.png "Keith Pickford")](https://github.com/Keflon)
[![Thomas Bolon](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/tbolon.png "Thomas Bolon")](https://github.com/tbolon)
[![Kori Francis](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/kfrancis.png "Kori Francis")](https://github.com/kfrancis)
[![Toni Wenzel](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/twenzel.png "Toni Wenzel")](https://github.com/twenzel)
[![Uno Platform](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/unoplatform.png "Uno Platform")](https://github.com/unoplatform)
[![Reuben Swartz](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/rbnswartz.png "Reuben Swartz")](https://github.com/rbnswartz)
[![Jacob Foshee](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/jfoshee.png "Jacob Foshee")](https://github.com/jfoshee)
[![](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/Mrxx99.png "")](https://github.com/Mrxx99)
[![Eric Johnson](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/eajhnsn1.png "Eric Johnson")](https://github.com/eajhnsn1)
[![David JENNI](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/davidjenni.png "David JENNI")](https://github.com/davidjenni)
[![Jonathan ](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/Jonathan-Hickey.png "Jonathan ")](https://github.com/Jonathan-Hickey)
[![Charley Wu](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/akunzai.png "Charley Wu")](https://github.com/akunzai)
[![Ken Bonny](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/KenBonny.png "Ken Bonny")](https://github.com/KenBonny)
[![Simon Cropp](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/SimonCropp.png "Simon Cropp")](https://github.com/SimonCropp)
[![agileworks-eu](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/agileworks-eu.png "agileworks-eu")](https://github.com/agileworks-eu)
[![Zheyu Shen](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/arsdragonfly.png "Zheyu Shen")](https://github.com/arsdragonfly)
[![Vezel](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/vezel-dev.png "Vezel")](https://github.com/vezel-dev)
[![ChilliCream](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/ChilliCream.png "ChilliCream")](https://github.com/ChilliCream)
[![4OTC](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/4OTC.png "4OTC")](https://github.com/4OTC)
[![Vincent Limo](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/v-limo.png "Vincent Limo")](https://github.com/v-limo)
[![Jordan S. Jones](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/jordansjones.png "Jordan S. Jones")](https://github.com/jordansjones)
[![domischell](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/DominicSchell.png "domischell")](https://github.com/DominicSchell)
[![Justin Wendlandt](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/jwendl.png "Justin Wendlandt")](https://github.com/jwendl)
[![Adrian Alonso](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/adalon.png "Adrian Alonso")](https://github.com/adalon)
[![Michael Hagedorn](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/Eule02.png "Michael Hagedorn")](https://github.com/Eule02)
[![](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/henkmartijn.png "")](https://github.com/henkmartijn)
[![Sebastien Lebreton](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/sailro.png "Sebastien Lebreton")](https://github.com/sailro)
[![torutek](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/torutek.png "torutek")](https://github.com/torutek)


<!-- sponsors.md -->

[![Sponsor this project](https://raw.githubusercontent.com/devlooped/sponsors/main/sponsor.png "Sponsor this project")](https://github.com/sponsors/devlooped)
&nbsp;

[Learn more about GitHub Sponsors](https://github.com/sponsors)

<!-- https://github.com/devlooped/sponsors/raw/main/footer.md -->
