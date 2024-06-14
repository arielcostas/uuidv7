# uuidv7

![Build status](https://img.shields.io/github/actions/workflow/status/arielcostas/uuidv7/build.yml?branch=main&style=for-the-badge)
![Nuget version](https://img.shields.io/nuget/v/Costasdev.Uuidv7?style=for-the-badge)
![Licence](https://img.shields.io/github/license/arielcostas/uuidv7?style=for-the-badge)

C# implementation of UUID version 7 per [RFC 9562](https://www.rfc-editor.org/rfc/rfc9562#name-uuid-version-7).

## Setup

The `Uuidv7` library is available as a NuGet package on the NuGet Gallery. You can install it using the following command with the .NET CLI:

```bash
dotnet add package Costasdev.Uuidv7
```

Alternatively, you can install it using Visual Studio's NuGet Package Manager.

```pwsh
Install-Package Costasdev.Uuidv7
```

Or you can add it to your project's `.csproj` file:

```xml
<ItemGroup>
	<PackageReference Include="Costasdev.Uuidv7" Version="CHANGEME THE LATEST" />
</ItemGroup>
```

## Basic usage

The following examples demonstrate how to use the `Uuidv7` library.

### Instantiation

```csharp
using System;
using Costasdev.Uuidv7;

// Create a new UUID for the current time
var uuid1 = Uuidv7.NewUuid();

// Create a new UUID for a specific DateTime
var dateTime = new DateTime(2024, 06, 13, 10, 08, 15, DateTimeKind.Utc);
var uuid2 = Uuidv7.NewUuid(dateTime);

// Create a new UUID for a specific DateTimeOffset
var dateTimeOffset = new DateTimeOffset(2024, 06, 13, 10, 08, 15, TimeSpan.Zero);
var uuid3 = Uuidv7.NewUuid(dateTimeOffset);
```

### String representation

The `Uuidv7` class has the `ToString()` method, which returns the UUID in lowercase and with hyphens. The `AsString()` method allows you to customise the output of the UUID, specifying whether it should be in lowercase or uppercase, and whether it should have hyphens or not.

```csharp
using System;
using Costasdev.Uuidv7;

var uuid = Uuidv7.NewUuid();

// Default ToString() method returns the UUID in lowercase and with hyphens
Console.WriteLine(uuid);
// Output: 7f1b3b6e-7b1b-7f1b-3b6e-7b1b7f1b3b6e

// AsString() method allows you to specify whether the UUID should be in lowercase or uppercase, and whether it should have hyphens
// AsString(bool uppercase, bool hyphens)
Console.WriteLine(uuid2.AsString(true)); // Output: 7F1B3B6E-7B1B-7F1B-3B6E-7B1B7F1B3B6E
Console.WriteLine(uuid2.AsString(false, false)); // Output: 7f1b3b6e7b1b7f1b3b6e7b1b7f1b3b6e
```

### Parsing

Parsing supports both the standard UUID format (with hyphens) and the format without hyphens, as well as both lowercase and uppercase characters.

```csharp
using System;

const string uuidString = "7f1b3b6e-7b1b-7f1b-3b6e-7b1b7f1b3b6e";

// Parse a UUID from a string
try {
	var uuid = Uuidv7.Parse(uuidString);
} catch (FormatException e) {
	Console.WriteLine(e.Message);
}

// TryParse a UUID from a string, returns a boolean indicating success
var validUuid = Uuidv7.TryParse(uuidString, out var uuid);

if (validUuid) {
	Console.WriteLine(uuid);
} else {
	Console.WriteLine("Invalid UUID");
}
```

## Licence

This project is licenced under the BSD 3-Clause licence. See [LICENCE](LICENCE) for more information. Essentially, you can do whatever you want with this code, as long as:

1. You give credit to the original author
1. You DO NOT hold them liable for any damages
1. You provide the licence with any derivative works
1. You DO NOT use the author's name to promote any derivative works.