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
[![Clarius Org](https://avatars.githubusercontent.com/u/71888636?v=4&s=39 "Clarius Org")](https://github.com/clarius)
[![MFB Technologies, Inc.](https://avatars.githubusercontent.com/u/87181630?v=4&s=39 "MFB Technologies, Inc.")](https://github.com/MFB-Technologies-Inc)
[![SandRock](https://avatars.githubusercontent.com/u/321868?u=99e50a714276c43ae820632f1da88cb71632ec97&v=4&s=39 "SandRock")](https://github.com/sandrock)
[![DRIVE.NET, Inc.](https://avatars.githubusercontent.com/u/15047123?v=4&s=39 "DRIVE.NET, Inc.")](https://github.com/drivenet)
[![Keith Pickford](https://avatars.githubusercontent.com/u/16598898?u=64416b80caf7092a885f60bb31612270bffc9598&v=4&s=39 "Keith Pickford")](https://github.com/Keflon)
[![Thomas Bolon](https://avatars.githubusercontent.com/u/127185?u=7f50babfc888675e37feb80851a4e9708f573386&v=4&s=39 "Thomas Bolon")](https://github.com/tbolon)
[![Kori Francis](https://avatars.githubusercontent.com/u/67574?u=3991fb983e1c399edf39aebc00a9f9cd425703bd&v=4&s=39 "Kori Francis")](https://github.com/kfrancis)
[![Uno Platform](https://avatars.githubusercontent.com/u/52228309?v=4&s=39 "Uno Platform")](https://github.com/unoplatform)
[![Reuben Swartz](https://avatars.githubusercontent.com/u/724704?u=2076fe336f9f6ad678009f1595cbea434b0c5a41&v=4&s=39 "Reuben Swartz")](https://github.com/rbnswartz)
[![Jacob Foshee](https://avatars.githubusercontent.com/u/480334?v=4&s=39 "Jacob Foshee")](https://github.com/jfoshee)
[![](https://avatars.githubusercontent.com/u/33566379?u=bf62e2b46435a267fa246a64537870fd2449410f&v=4&s=39 "")](https://github.com/Mrxx99)
[![Eric Johnson](https://avatars.githubusercontent.com/u/26369281?u=41b560c2bc493149b32d384b960e0948c78767ab&v=4&s=39 "Eric Johnson")](https://github.com/eajhnsn1)
[![David JENNI](https://avatars.githubusercontent.com/u/3200210?v=4&s=39 "David JENNI")](https://github.com/davidjenni)
[![Jonathan ](https://avatars.githubusercontent.com/u/5510103?u=98dcfbef3f32de629d30f1f418a095bf09e14891&v=4&s=39 "Jonathan ")](https://github.com/Jonathan-Hickey)
[![Ken Bonny](https://avatars.githubusercontent.com/u/6417376?u=569af445b6f387917029ffb5129e9cf9f6f68421&v=4&s=39 "Ken Bonny")](https://github.com/KenBonny)
[![Simon Cropp](https://avatars.githubusercontent.com/u/122666?v=4&s=39 "Simon Cropp")](https://github.com/SimonCropp)
[![agileworks-eu](https://avatars.githubusercontent.com/u/5989304?v=4&s=39 "agileworks-eu")](https://github.com/agileworks-eu)
[![Zheyu Shen](https://avatars.githubusercontent.com/u/4067473?v=4&s=39 "Zheyu Shen")](https://github.com/arsdragonfly)
[![Vezel](https://avatars.githubusercontent.com/u/87844133?v=4&s=39 "Vezel")](https://github.com/vezel-dev)
[![ChilliCream](https://avatars.githubusercontent.com/u/16239022?v=4&s=39 "ChilliCream")](https://github.com/ChilliCream)
[![4OTC](https://avatars.githubusercontent.com/u/68428092?v=4&s=39 "4OTC")](https://github.com/4OTC)
[![Vincent Limo](https://avatars.githubusercontent.com/devlooped-user?s=39 "Vincent Limo")](https://github.com/v-limo)
[![domischell](https://avatars.githubusercontent.com/u/66068846?u=0a5c5e2e7d90f15ea657bc660f175605935c5bea&v=4&s=39 "domischell")](https://github.com/DominicSchell)
[![Adrian Alonso](https://avatars.githubusercontent.com/u/2027083?u=129cf516d99f5cb2fd0f4a0787a069f3446b7522&v=4&s=39 "Adrian Alonso")](https://github.com/adalon)
[![Michael Hagedorn](https://avatars.githubusercontent.com/u/61711586?u=8f653dfcb641e8c18cc5f78692ebc6bb3a0c92be&v=4&s=39 "Michael Hagedorn")](https://github.com/Eule02)
[![](https://avatars.githubusercontent.com/devlooped-user?s=39 "")](https://github.com/henkmartijn)
[![torutek](https://avatars.githubusercontent.com/u/33917059?v=4&s=39 "torutek")](https://github.com/torutek)
[![mccaffers](https://avatars.githubusercontent.com/u/16667079?u=739e110e62a75870c981640447efa5eb2cb3bc8f&v=4&s=39 "mccaffers")](https://github.com/mccaffers)
[![Cleosia](https://avatars.githubusercontent.com/u/85127128?u=3c889baa39bbe3607998c931490bd67c6ed854f2&v=4&s=39 "Cleosia")](https://github.com/cleosia)


<!-- sponsors.md -->
[![Sponsor this project](https://avatars.githubusercontent.com/devlooped-sponsor?s=118 "Sponsor this project")](https://github.com/sponsors/devlooped)

[Learn more about GitHub Sponsors](https://github.com/sponsors)

<!-- https://github.com/devlooped/sponsors/raw/main/footer.md -->
