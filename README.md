# RADIUS Dictionaries Library for .Net Core

Working with RADIUS attributes requires closely following the RFCs as well as vendor-defined rules for encoding the content of AAA packets.  Typically, software is written to use the so-called "RADIUS Dictionaries" as generic dictionary data structures.  This library provides a different approach, with each attribute being a type that can be easily used with the pattern matching capabilities of C# 8 (switch expressions, for example).  Perhaps not for everyone, but perhaps prefered by some.

## Usage

Firstly, this package is distributed as a Nuget packages hosted here at github. I may clone it to nuget.org as well at a later date, but for now follow [github's instructions](https://help.github.com/en/github/managing-packages-with-github-package-registry/configuring-nuget-for-use-with-github-package-registry) to setup a new source.

Now, just add the package:
```
dotnet add package RadiusDictionariesLib
```

Next, some code.  Let's say you are a vendor and want to make sure that your code only makes use of your own VSA's.  Well, you can do that:

```csharp
public void DoTheThing(IArubaAttribute attr) {
    // attr is strongly-typed to be one of the attributes defined in Aruba's dictionary.
    // ...
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
        UserPassword => "Shhh.",
    };
}
```
Also, where attribute values are defined in a dictionary this library will include them as an attribute-specific enumeration that makes it more difficult to "do the wrong thing" by assigning invalid values to or misinterpreting them. For example:
```csharp
void MakeSureToBeSafe(Hp.ManagementProtocol attr) {
    // is "HTTP" represented by a 5 or 6? I can never remember...
    if (attr.Value == Hp.ManagementProtocolValue.Http) throw new SecurityException("Nope.");
}
```

## Versions and Continuous Builds
This library is auto-generated from the FreeRADIUS project's dictionaries (see their repo) text files.  Once CI/CD is fully figured-out new commits to the `master` branch over there will cause this project to automatically build, test and publish a new "preview" package version. 

The versioning of preview packages will make use of the FreeRADIUS commit ID as part of the suffix, so the two can be correllated. When a new release of FreeRADIUS is tagged, a "production" version of the RadiusDictionariesLib nuget will be produced with a matching version. If this versioning strategy causes you problems, please file an issue so we can work it out.

Enjoy!
