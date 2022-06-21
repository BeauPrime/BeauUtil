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