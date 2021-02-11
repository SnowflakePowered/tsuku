# 付く tsuku

[![Nuget](https://img.shields.io/nuget/v/tsuku)](https://www.nuget.org/packages/tsuku)
[![GitHub Workflow Status](https://img.shields.io/github/workflow/status/SnowflakePowered/tsuku/.NET)](https://github.com/SnowflakePowered/tsuku/actions?query=workflow%3A.NET)
[![Codecov](https://img.shields.io/codecov/c/github/SnowflakePowered/tsuku)](https://codecov.io/gh/SnowflakePowered/tsuku/branch/master)

tsuku is a C# library that makes cross-platform extended attribute access easy. 

On Linux and MacOS, this is handled via `getxattr` and `setxattr`. On Windows, alternate data streams are used to imitate this feature.

Note that this is not a full ADS library. The max size of attributes is forcibly limited to 512 bytes. Attribute names are also limited to 112 characters max, and are prefixed with '`tsuku`' across all platforms. tsuku only handles file attributes created by tsuku, for safety reasons.

tsuku's API is very simple and easy to understand.

```csharp
public struct TsukuAttributeInfo 
{
    public string Name { get; }
    public int Size { get; }
}

public static void SetAttribute(this FileInfo @this, string name, ReadOnlySpan<byte> data);
public static void GetAttribute(this FileInfo @this, string name, ref Span<byte> data);
public static byte[] GetAttribute(this FileInfo @this, string name);
public static void DeleteAttribute(this FileInfo @this, string name);
public static IEnumerable<TsukuAttributeInfo>GetAttributeInfos(this FileInfo @this);
```

There are also helper extensions in `Tsuku.Extensions` for easy setting of `string`, `Guid`, and `bool` attributes.

Supported filesystems are NTFS on Windows, Ext4 and Btrfs on Linux, and APFS on macOS, with HFS+ possibly supported but untested. Attributes are preserved across move and copy if the underlying filesystem driver supports attribute preservation.
