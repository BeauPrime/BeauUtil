## Version 0.9.2
**28 July 2023**

Hotfix for `UniqueIdAllocator` exception when exceeding initial capacity

## Improvements
* `DownloadHandlerUnsafeBuffer` can be passed allocation and free delegates instead of a pre-allocated buffer
* New `AnimatorStateSnapshot` class for storing and restoring Animator layer and parameter information
* All versions of `BitSet` are now serializable

## Fixes
* `UniqueIdAllocator` no longer throws an exception when reserving more identifier slots than initially allocated

## Breaking Changes
* All variants of `Unsafe.Read/Write(string)` renamed to `Unsafe.ReadUTF8/WriteUTF8` for consistency

## Version 0.9.1
**29 June 2023**

Hotfix for `TypeIndex` custom capacity

## Version 0.9.0
**29 June 2023**

New collection types
`MeshData` improvements

## Features
* New `BitSet` structs for easier access to bit sets
* New `LruCache` fixed-size cache type
* New `LLTable` collection, for array-backed linked lists
* New `UniqueId`, `UniqueIdAllocator` and `UniqueIdMap` classes for generating unique validatable handles for array-backed storage
* `CompareUtils.Default[...]` can now use types marked with `DefaultEqualityComparer` and `DefaultSorter` attributes for their respective types
* New `Vectors` utility class for sequentially transforming vectors and vectors embedded in structs
* New `OffsetLength` struct and utilities for representing ranges of data in 1-dimensional arrays/buffers
* New `TypeIndex<T>` utility for statically mapping types to indices (for faster array lookups)

## Improvements
* `Bits` utilities now work on 64-bit integrals
* Improved `MeshData.Upload` performance
* Added `MeshData.Topology`, specified in constructor
* `MeshData` can now transform ranges of vertices
* `Profiling.Time` can now specify time units (milliseconds, microseconds, ticks)
* Added callbacks to `DMInfo` for menu open and close
* `CameraHelper` can now register global pre-cull, pre-render, and post-render handlers
* `VertexUtility.GenerateLayout` also now returns size, attribute, and offset information

## Fixes
* `MeshData` correctly calculates maximum vertices based on index format and vertex size

### Breaking Changes
* `RingBuffer<T>` is now a sealed class
* `CompareUtils.DefaultComparer<T>` renamed to `CompareUtils.DefaultEquals<T>` to avoid conceptual naming conflicts with `CompareUtils.DefaultSort<T>`
* `MeshData.Add[Shape]` returns an `OffsetLength` instead of the `MeshData` instance

## Version 0.8.6
**19 June 2023**

New `Mesh` construction utilities
Support for `Slider` control type in `DMMenu`

## Features
* New `MeshData16<Vertex>` class for interleaved mesh construction
* Added `Slider` control support to `DMMenu`
* Added `Push`/`Pop` methods to arena allocator for more fine-grained freeing of memory
* Added `ArrayUtils.EnsureCapacity` to match `ListUtils.EnsureCapacity`

## Improvements
* Assert arguments are passed in using generics to avoid boxing
* Added Quantization methods to `MathUtils`
* Added endian swap methods to `Unsafe`
* Added `RingBuffer.MoveFrontToBack` and similar methods for moving elements between head and tail

## Fixes
* Fixed `RNG.Shuffle(RingBuffer)` call inference issue
* `UnityHelper.SafeDestroy` can now correctly destroy runtime assets in editor
* Important attributes like `BindContext`, `BlockMeta` and `BlockContent` now derive from `PreserveAttribute`

## Version 0.8.5
**14 March 2023**

Hotfix for redundant `ColorGroup` update registration and memory leak
Hotfix for `RectTransformPinned` update order

## Version 0.8.4
**6 March 2023**

Hotfix for inverted `RingBuffer.RemoveWhere` behavior

## Version 0.8.3
**5 Feb 2023**

Hotfix for CameraRenderScale rounding errors

## Version 0.8.2
**19 Dec 2022**

Hotfix for rare `RingBuffer` crash
New layout event listener
Initialization optimizations

### Features
* Added `LayoutListener` component for callbacks when layout building starts/stops
* Added `ActionEvent` for a parameter-less version of `CastableEvent`

### Improvements
* Added `StringSlice` and `StringBuilderSlice` versions of `TMP_Text.SetText` extensions

### Fixes
* Fixed rare `RingBuffer` crash when compressing a full buffer and then pushing

## Version 0.8.1
**18 Nov 2022**

Compilation error hotfix
`TextureRegion` no longer supports tightly packed sprites

## Version 0.8.0
**17 Nov 2022**

### Features
* Added `RoundedRectGraphic` and `EllipseGraphic`
* Shape graphics (Rect, RoundedRect, Ellipse, GradientRect) can now use a per-canvas "white pixel" sprite to improve batching
* Added `UnityHelper.Find`, `UnityHelper.IsAlive`, and `UnityHelper.Id` for locating Unity objects by integer id.
* Added `Unsafe.Hash32` and `Unsafe.Hash64` to perform FNV-1a hash on arbitrary buffers (and unmanaged structs)
* Added `NonBoxedValue` for returning an unknown type without boxing built-in numerics
* Added `CameraHelper.AddOnPreCull`, `CameraHelper.AddOnPreRender`, and `CameraHelper.AddOnPostRender` (along with corresponding `Remove` versions) for consistent camera callback mechanism between the default renderer and URP
* Added `CastableEvent` for a `CastableAction`-compatible alternative to C#'s `event` mechanism

### Improvements
* `CastableFunc` and `CastableAction` now support function signatures with the first parameter passed by reference.
* `StringUtils.ArgsList.Splitter` made immutable and thread-safe
* Added `StringSlice.Unpack` to extract string and region data
* Added `StringHash32.Fast` for a non-cached hash (does not record reverse-lookup information)
* `MatchRuleSet.FindMatch` now uses non-cached hash for lookups
* Added `Reflect.GetSignature` to return a `MethodSignature` object
* Added `Geom.Remap(Vector2, Rect, Rect)` and `Geom.Remap(Vector3, Bounds, Bounds)` for remapping points between regions
* `MethodCache` can now generate delegates for common method signatures instead of relying solely on `MethodInfo.Invoke`
* Added `RingBuffer.RemoveWhere` to remove elements passing a given predicate
* Added `RingBuffer.Find`, `RingBuffer.Exists`, and `RingBuffer.FindIndex` for locating elements passing a given predicate

### Fixes
* `StringParser` numeric parsing no longer allocates memory due to use of `decimal`
* Scale calculations for `CameraFOVPlane` when in `PixelHeight` mode now account for rounding errors

### Breaking Changes
* `Unsafe.FastReinterpret(TFrom*)` removed in favor of `Unsafe.Reinterpret(TFrom*)`
* `TextHelper` functions merged into `CanvasHelper`
* Added `IStringConverter.TryConvertToVariant` for fast variant conversions
* `IMethodCache.TryInvoke` now outputs a `NonBoxedValue` instead of `object`
* `IStringConverter.TryConvertTo` now outputs a `NonBoxedValue` instead of `object`
* `TiledRawImage`, `ScrollTiledRawImage`, `RectGraphic`, and `GradientRectGraphic` are now under the `BeauUtil.UI` namespace

## Version 0.7.14
**11 Oct 2022**

Hotfix for `ConsolePerformance` stats formatting
Improved `MatchRuleSet` performance and memory consumption
Fixed `Unsafe.Reinterpret` for cases of casting to a type of greater size
Implemented case-insensitive hash generation

## Version 0.7.13
**3 Oct 2022**

Hotfix for `SerializedHash32` incorrectly clearing hash when string has been stripped
`TagString` `TextEvent` now reports visible text and rich text offset+length pairs
Reduced garbage generated by string methods

### Breaking Changes
* `MathUtil` renamed to `MathUtils` for consistency

### Version 0.7.12
**4 Sept 2022**

Hotfix for incorrect `SerializedHash32` hashes being masked in editor
Hotfix for BlockParser block error state not propagating correctly
Added support for `IGNORE_UNITY_EDITOR` define to disable debug checks in editor

### Version 0.7.11
**29 Aug 2022**

Hotfix for `RectTransformPinned` to handle objects behind camera
Hotfix for `CameraHelper.HeightForDistanceAndFOV` calculation
Memory "dump to string" utilities
Reduced memory usage of debug font

### Version 0.7.10
**19 Aug 2022**

Hotfix for `CameraRenderScale.PixelHeight` setter
Improved consistency `CameraFOVPlane.SetTargetPreserveFOV`

### Version 0.7.9
**19 July 2022**

Hotfix for `StringUtils.Args.Splitter` state corruption

### Version 0.7.8
**19 July 2022**

Hotfix for `Bits.Count` hang

### Version 0.7.7
**18 July 2022**

StringSlice.Split improvements for thread safety and better configurability

#### Features
* Added `StringSlice.Split` configuration option to limit number of slices
* New `Enums` utility class to convert generic enums to/from integral types
* Added methods in `Bits` utility class to operate on generic flag enums
* New `Batch` utility class to group items together by batch id and dispatch callbacks on sets

#### Breaking Changes
* `Bits.ContainsMask` renamed to `Bits.ContainsAny`
* `CharStreamParams` factory methods contain an additional `Owner` argument, to ensure garbage collection doesn't sweep up source of pinned buffers

#### Fixes
* Using `Colors.Hex` in a static initializer will no longer crash

#### Improvements
* Improved thread safety of `StringSlice.Split` operations involving `ISplitter`
* `StringUtils.ArgsList.Splitter` can now specify if quotes should be stripped from arguments

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