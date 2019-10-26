# RADIUS Dictionaries Library for .Net Core

Working with RADIUS attributes requires closing following open specifications as well as vendor-defined rules for encoding the content of authentication packets.  Typically, software is written to used the so-called "RADIUS Dictionaries" as generic dictionary data structures.  This library provides a different approach, with each attribute being a type that can be easily used with the pattern matching capabilities of C# 8 (switch expressions, for example).  Perhaps not for everyone, but perhaps prefered by some.

## Usage

Firstly, this package is distributed as a Nuget packages hosted here at github. I may clone it to nuget.org as well at a later date, but for now follow github's instructions to setup a new source.

Now, just add the package:
```
dotnet add package RadiusDictionariesLib
```

Next, some code.  Let's say you are a vendor and want to make sure that your code only makes use of your own VSA's.  Well, you can do that:

```csharp
class MyClass {
    public do DoTheThing(IArubaAttribute attr) {
        // attr is strongly-typed to be one of the attributes defined in Aruba's dictionary.
        // ...
    }
}
```
Or, if you want to vary control-flow based on attribute types with pattern matching:
```csharp
string GetTheThing(IRadiusAttribute attr) {
    return attr switch {
        IVendorSpecificAttribute vsa => vsa switch {
            IArubaAttribute => "Aruba calling...",
            {} => "I'm a VSA"
        },
        UserName u => $"It's {u.Value}",
        UserPassword pw => "Shhh.",
    };
}
```

## Versions and Continuous Builds
This library is auto-generated from the FreeRADIUS project's dictionaries (see their repo).  As new commits hit the `master` branch over there, this project will automatically build, test and publish a new "preview" package version. The versioning makes use of the FreeRADIUS commit ID as part of the suffix, so the two can be correllated. When a new release of FreeRADIUS is tagged, a "production" version of the RadiusDictionariesLIb nuget will be produced with a matching version. If this CI strategy causes you problems, pelase file an issue so we can work it out.

Enjoy!
