### Version 0.7.6
**12 July 2022**

Fixed BlockParser leftover buffer corruption issues.
Added additional StringBuilder argument to IBlockGenerator callbacks

#### Fixes
* Fixed `CharStream.Insert` occasional buffer corruption

#### Improvements
* Added `StringBuilder` argument to `IBlockGenerator.TryEvaluatePackage` and `IBlockGenerator.TryEvaluateMeta` as additional context

### Version 0.7.5
**10 July 2022**

Updating for 2020.1+ compatibility.
Adjusted CameraFOVPlane math for stability when altering clipping planes.

#### Breaking Changes
* `CameraHelper.TryGetDistance` methods now return distance from the camera transform to the target plane, not the distance from the near clip plane to the target plane.

#### Fixes
* `BlockParser` no longer erroneously reports errors when setting content via a `BlockContent` attribute

#### Improvements
* Updated several classes to avoid compiler warnings/errors in Unity 2020.1+.

### Version 0.7.4
**21 June 2022**

Critical bug fix in BlockParser for a stream insertion error.

#### Fixes
* `IBlockParserUtil.InsertStream` will properly defer processing on any unprocessed characters from the current stream

#### Improvements
* Implemented `CharStream.InsertBytes` and `CharStream.InsertChars`, making buffer types a proper deque

### Version 0.7.3
**20 June 2022**

Unified character/byte stream interface. Implemented `BlockParser` streaming behavior.

### Features
* Added `CharStream` to handle reading bytes/chars from streams, strings, arrays, and unmanaged memory
* Overhaul of `BlockParser` parsing behavior for performance and memory usage

### Improvements
* Added unsafe Quicksort implementation to `Unsafe`
* Added unsafe methods for retrieving `NativeArray` instance pointers
* Added `StringBuilderSlice` for slicing StringBuilder instances. Use with caution!
* `CustomTextAsset` no longer caches the source string by default