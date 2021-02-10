# 付く tsuku

tsuku is a C# library that makes cross-platform extended attribute access easy. 

On Linux and MacOS, this is handled via `getxattr` and `setxattr`. On Windows, alternate data streams are used to imitate this feature.

Note that this is not a full ADS library. The max size of attributes is forcibly limited to 4 kilobytes. Attribute names are also limited to 192 characters max, and are prefixed with '`tsuku`' across all platforms. tsuku only handles file attributes created by tsuku, for safety reasons.

tsuku's API is very simple.

```csharp
public class TsukuAttributeInfo 
{
    string Name { get; }
    int Size { get; }
}

public static void SetTsukuAttribute(this FileInfo @this, string name, ReadOnlySpan<byte> data);

public static byte[] GetTsukuAttribute(this FileInfo @this, string name);
public static bool TryGetTsukuAttribute(this FileInfo @this, string name, ref Span<byte> data);

public static IEnumerable<TsukuAttributeInfo>GetTsukuAttributeInfos(this FileInfo @this);
```

Supported filesystems are NTFS, Ext4, Btrfs. Possibly supported but untested are APFS and HFS+ attributes. Attributes are preserved across move and copy if the underlying filesystem driver supports attribute preservation.