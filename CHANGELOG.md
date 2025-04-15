## Version 0.10.17
**15 April 2025**

Improvements to `Assert` functionality

### Improvements
* Added `Assert.NotNullOrDestroyed` to assert that an object is neither null nor destroyed
* `Assert` failure no longer throws an exception if already asserting from an uncaught exception
* `UnityHelper.ComponentCount` now uses `GameObject.GetComponentCount` if available

## Version 0.10.16
**31 March 2025**

Improvements to `Assert` functionality
Fixed `DynamicMeshFilter` mesh destruction exception

### Improvements
* `Assert` fills in stack trace information more reliably
* `Assert` now displays an alert box in WebGL builds
* Added `Unsafe.AsRef` to convert from pointer to reference
* Added `UnsafeSpan<>.ToArray`

### Fixes
* Removed problematic finalizer from `DynamicMeshFilter`
* `SceneHelper.AllBuildScenes` correctly filters out disabled scenes in editor

## Version 0.10.14
**26 Feb 2025**

Hotfix for memory corruption in wasm builds related to `NonBoxedValue`

### Fixes
* `MethodCache.TryStaticInvoke` will no longer crash in wasm builds after garbage collection
  * Removing explicit `StructLayout.Size` from `NonBoxedValue.PackedValues` fixed instances of memory corruption
  * The underlying mechanism for this corruption still needs to be investigated - suspected compiler bug surrounding explicitly-sized structs

### Improvements
* Reduced `MethodCache` memory usage

## Version 0.10.13
**23 Jan 2025**

`PointerListener` is now aware of any `Selectable` components' interactable state

### Improvements
* `PointerListener` now checks if an attached `Selectable` is interactable before dispatching click event
*  Added `MathUtils.Wrap` to handle wrapping values within a range

## Version 0.10.12
**22 Nov 2024**

Hotfix for occasional `UniqueIdAllocator` `Alloc` and `Reserve` exception

### Features
* `DMMenuUI` can now accept navigational input

### Improvements
* `SerializedAttributeSet.Read` can filter by member type
* `Bits.Set(Enum)` made branchless
* Added `StringHashing.DumpReverseLookupTables` for dumping string hash tables to stream
* Added `Unsafe.Quicksort(delegate*)`

### Fixes
* `UniqueIdAllocator.Reserve` no longer crashes after expanding capacity

## Version 0.10.11
**25 September 2024**

Hotfix for pointer truncation in `Unsafe.Align[...](void*)` on Android
Hotfix for Mono build crashes when using TinyIL

### Features
* Added `STuple` readonly struct as replacement for `System.Tuple` 

### Improvements
* Added `Equals`, `GetHashCode`, and `Compare` shortcuts to `CompareUtils`

### Fixes
* Made word size detection in `Unsafe` more robust
* Fixed pointer truncation in `Unsafe.Align[...](void*)` on Android
* Builds targeting Mono will no longer crash upon calling internal call shortcuts when compiled with TinyIL

## Version 0.10.10
**30 August 2024**

Hotfix for `MapUtils.EnsureCapacity` and `SetUtils.EnsureCapacity` in Unity 2019

### Features
* `Variant` can now store an instance id as a reference to a unity object
  * Note that if BeauData is included, this value type cannot be serialized 

### Fixes
* `MapUtils.EnsureCapacity` and `SetUtils.EnsureCapacity` now work in Unity 2019 for collections constructed without an initial capacity

## Version 0.10.9
**29 August 2024**

Hotfix for `Enums.IsZero` and `Enums.IsNotZero`, Unity 2019 compatibility

### Improvements
* `UniqueIdAllocator`, `UniqueIdMap` and `LLTable` can now be set to a fixed capacity (non-flexible)
* `UnityHelper.Find` and similar use the new `Resources.InstanceIdToObject` and similar API
* `LruCache` eviction now uses a linked list instead of an access counter for O(1) perf instead of O(N)
* Implemented `Unsafe.NullRef`, `Unsafe.IsNullRef`, `Unsafe.AsPointer`, and `Unsafe.AsTypedPointer` as alternatives to unavailable .NET `Unsafe` api

### Fixes
* `Enums.IsZero` and `Enums.IsNotZero` no longer emit improper IL when using TinyIL
* `SceneReference` constructor made compatible with Unity 2019 editor again

## Version 0.10.8
**25 July 2024**

Hotfix for `CameraHelper` global camera ordering

## Version 0.10.7
**25 July 2024**

Hotfix for `BitSet256` bitwise operators

### Breaking Changes
* Default `TypeIndex` capacity lowered from 512 to 128
* `QueryParams` always preserves parameter string value, even if it can be parsed to a `Variant`

### Improvements
* Added optional ordering parameter to `CameraHelper` callback registration
* Improved performance of `ToEnum(long)` and `ToEnum(ulong`
* Added `MinWidth` property to all `DMElementInfo` types
* Added generic comparison and bitwise operators to `Enums` utility
* Added `Unsafe.FastCast` to unchecked cast class types
* Made `BitSet32` and `BitSet64` serializable via proxy when BeauData package is present
* Condensed default debug console layout

### Fixes
* `BitSet256` bitwise operators corrected

## Version 0.10.6
**7 June 2024**

New `SceneReference` constructors, Performance improvements

### Improvements
* Added `SceneReference(string path)` and `SceneReference(int buildIndex)` constructors
* `SerializedAttributeSet` now reuses `AttributeBinding<TAttr, MemberInfo>` instead of its own identical type
* Added support for registering function pointers to `CastableArgument`
* Made `CastableArgument.Cast` cost more consistent 

## Version 0.10.5
**5 June 2024**

Hotfix for TinyIL integration build process revisions

## Version 0.10.4
**21 May 2024**

Hotfix for `Grid2D.IsValid` failing on `x == 0` or `y == 0`

### Features
* Added `UnsafeBitSet`, an `IBitSet` backed by unsafe memory

### Improvements
* Added `Log.Trace` and `Log.Debug` for bypassing Unity's console log
* Added `DMInfo.SortByLabel` for sorting menu elements alphabetically
* Added `DMInfo.FindSubmenu` and `DMInfo.FindOrCreateSubmenu` for locating submenus
* Added `DMInfo.MergeSubmenu` for merging submenus into a parent menu.
* Reduced branching in `Bits.Set` (eliminated entirely with `TinyIL` package)
* Adjusted `PerformanceTracker.TryGetMemoryUsage` output when profiler is active

### Fixes
* Fixed `Grid2D.IsValid` not considering `x == 0` or `y == 0` valid coordinates

## Version 0.10.3
**21 March 2024**

Hotfix for `MeshData.Upload` not uploading modified vertex counts when reusing a `MeshDataTarget`

## Version 0.10.2
**20 March 2024**

Fixed out-of-bounds memory access in `Unsafe.Hash64`
Fixed incorrect bounds checking for `MeshData.Vertex` ref accessors

### Features
* Added `NonIndexedAttribute` for marking types as non-traversable with `TypeIndex`
* Added `UnmanagedMeshData16` for reusable mesh data backed by unmanaged memory
* `MeshData.Upload` now takes in an optional set of flags for reducing the amount of data uploaded

### Improvements
* Added implicit conversions from `StringSlice` to `ReadOnlySpan<char>` and `ReadOnlyMemory<char>`
* `MeshData.Upload` tries to avoid uploading unchanged data to meshes by default
* Improved performance of `Unsafe.Hash32` and `UnsafeHash64`
* Added `Ref.Replace` overloads for primitives
* Added `DownloadHandlerUnsafeBuffer.Data` for retrieving an `UnsafeSpan<byte>`
* Added `CanvasHelper.GetSelectionState(Selectable)` for retrieving selection state of a `Selectable`
* Added `Geom.MinRect` for calculating the minimum bounding rectangle for a set of points

### Fixes
* Fixed out-of-bounds memory access in `Unsafe.Hash64`
* Restored Unity 2019 compatibility
* Fixed `AssetDatabase.SaveAssets cannot be called during import` error in CustomTextAsset importer
* Fixed `MeshData.Vertex` bounds checking against index count rather than vertex count

## Version 0.10.1
**21 Feb 2024**

Fixed exception in `AssetDBUtils.Find` methods when encountering assets with missing types
Added `BEAUUTIL_USE_LEGACY_UNITYEVENTS` define for reverting `PointerListener`, `TriggerListener[2D]`, and `CollisionListener[2D]` events to serializable UnityEvents

## Version 0.10.0
**9 Feb 2024**

Performance improvements
Pointer alignment fixes
`TypeIndex` can retrieve inheritance information
Disabling experimental MethodInvocationHelper pointer specialization

### Features
* Added `UnsafeEnumerator` for iterating over unsafe memory
* Added IntrinsicIL definitions for several operations
  * Requires `TinyIL.Mono` package
* Added `BitEnumerator32`, `BitEnumerator64`, `EnumMaskEnumerator` and `EnumBitEnumerator` structs for enumerating over set bits/mask values
* Added `TypeIndex.GetAll` for retrieving indices up an inheritance chain

### Improvements
* Added conversion from `UnsafeSpan` to `Span` and `ReadOnlySpan`
* Added conversion between `UnsafeSpan<T>` and `UnsafeSpan<U>`
* Added `Unsafe.IsAligned` methods for checking value/pointer alignment
* Added pointer alignment checks to critical aligned operations
* Switch pointer-to-integral conversions to 32-bit on 32-bit machines
* Added `CastableAction` and `CastableEvent` variants for 2, 3, and 4 parameters
* Added `RingBuffer.Quicksort` shortcut for unmanaged element types
* Added `Profiling.AvgTime` for averaging the duration for a set number of samples
* Exposed `Unsafe.AllocAligned(Arena)`
* Added `MeshData.Vertex` and `MeshData.Index` calls for modifying vertex and index values directly
* Added `Geom.SetTranslation` for setting matrix translation
* Added `Geom.Forward/Up/Right` shortcuts for faster quaternion-to-vector calculations

### Fixes
* Disabled MethodInvocationHelper pointer specialization optimizations
* `ConsolePerformance` no longer starts the profiler automatically
* `Profiling.Time` is more accurate and can be run on non-main threads
* `RingBuffer.Compress` no longer allocates memory in the case of split segments
* `AssetDBUtils.Find` calls now return the most accurate result, not just the first one
* Fixed accidental recursion in `StringUtils.ToUpperInvariant`
* Fixed missed cases of using `Comparer<T>.Default` instead of `CompareUtils.DefaultSort<T>()`

### Breaking Changes
* `RingBuffer.Compress` renamed to `RingBuffer.Linearize` for consistency
* Events no longer exposed to inspector for the following components:
  * `CollisionListener`/`CollisionListener2D`
  * `TriggerListener`/`TriggerListener2D`
  * `PointerListener`
  * `RectTransformPinned`

## Version 0.9.9
**1 Dec 2023**

Hotfix for MethodInvocationHelper on IL2CPP platforms

### Improvements
* Added rect-aware clamping mode to `RectTransformPinned`
* Added experimental `TRS.TryCreateFromMatrix`

### Fixes
* Fixed specialized method invocation from `MethodInvocationHelper` on IL2CPP platforms
* Fixed potential overflow issue when calling `Bits.IndexOf(Enum)` with a 64-bit enum
* Fixed erroneous inclusion of context argument in `CastableEvent` and `ActionEvent` unsafe delegate registration methods

## Version 0.9.8
**27 Nov 2023**

Hotfix for out-of-range execution order values.

## Version 0.9.7
**27 Nov 2023**

Hotfix for `AttributeCache.Get`

### Improvements
* `Reflect.FindAllAssemblies` can now cache all loaded assemblies on IL2CPP builds

### Fixes
* `AttributeCache.Get` now works correctly for non-MemberInfo inputs
* `SerializedAttributeSet.Read` correctly skips over assemblies it could not find
* Fixed `Persist` default execution order

### Breaking Changes
* `SerializedAttributeSet.Read` now requires a list of assemblies as input

## Version 0.9.6
**16 Nov 2023**

Hotfix for `StringHashing` compilation error in non-debug builds

### Features
* Added `RegularPolyGraphic` for rendering regular polygons
* Support for function pointers in 2021.2+ in function wrappers and `MethodInvocationHelper`

### Improvements
* `Unsafe.Hash` functions now use Murmur2 instead of FNV-1a
* Added various `Reserve` extension methods for `Dictionary`, `List`, `HashSet`, and `StringBuilder`
* Added tick rate conversion methods to `Profiling`
* Added extension method polyfills for several `Dictionary` and `HashSet` methods present in .NET standard but not earlier versions
* Added `CameraHelper.GetStateHash` and `TransformHelper.GetStateHash` for quickly checking camera and transform changes

### Fixes
* Fixed `StringHashing.DumpReverseLookupStats` compilation error in non-debug builds
* Fixed `Unsafe.PinString(StringBuilder)` to properly catch instances of chained StringBuilders

## Version 0.9.5
**14 Nov 2023**

### Features
* Added `UnityHelper.IsPersistent` for checking if an asset is a persistent asset
* Added `SceneHelper.GetLoadingState` and `SceneHelper.GetGUID` for returning info about scenes
* Added `SerializedAttributeSet` class for serializing reflection information
* Added `ReloadableRef<T>` struct for references to assets with custom importers that may be reloaded during the editor
* Added `AssetOnlyAttribute` property attribute for restricting an object reference to persistent objects only

### Improvements
* Added optional `Unity.Mathematics` integration for several bit calls
* Inlined some `Reflect.Find` enumerable calls for better performance
* Some `Reflect.Find` calls can now make use of the editor type cache in the editor
* Added `StringHashing.DumpReverseLookupStats` for writing reverse lookup table stats to console
* Added `StringHashReverseCacheInitialCapacityAttribute` attribute for setting the initial capacity of the reverse lookup table
* Added `Unsafe.FormatBytes` for formatting byte counts

### Fixes
* Fixed several incompatibilities with Unity 2019

### Breaking Changes
* `UnsafePtr.Ref` renamed to `UnsafePtr.AsRef`

## Version 0.9.4
**1 Nov 2023**

New time-bound execution utilities
Fix for `SceneReference` serialization
New `Unsafe` features

### Features
* Added `WorkSlider` utility for time-bound execution of work queues
* Added `AttributeCache` classes for caching attribute information
* Added `Unsafe.Clear` for clearing memory to defaults

### Improvements
* All versions of `OffsetLength` structs are now serializable
* Added `OffsetLength.Contains` method to OffsetLength types for checking if value is within range
* Improved `RingBuffer` and `UnsafeSpan` debugger views
* Added `BitSet.IsEmpty` method
* Avoid adding multiple dividers in a row to `DMInfo`
* Added a few more common vertex formats to `VertexUtility` file
* Added `Unsafe.AllocSpan` variant of `Unsafe.AllocArray` to directly return an `UnsafeSpan`
* Added `AllocArray` and `AllocSpan` methods to `Unsafe.ArenaHandle` API
* Added `Unsafe.AllocatorFlags.ZeroOnAllocate` for zeroing out allocated memory

### Fixes
* Fixed potential data format inconsistencies when using `SceneReference` in build

### Breaking Changes
* `Unsafe.Reinterpret(TFrom*)` renamed in favor of `Unsafe.FastReinterpret(TFrom*)` to clarity reinterpretation method

## Version 0.9.3
**13 Oct 2023**

Hotfix for incorrect `Unsafe.Read` behavior when arguments passed as `ref`

## Version 0.9.2
**28 July 2023**

Hotfix for `UniqueIdAllocator` exception when exceeding initial capacity

### Improvements
* `DownloadHandlerUnsafeBuffer` can be passed allocation and free delegates instead of a pre-allocated buffer
* New `AnimatorStateSnapshot` class for storing and restoring Animator layer and parameter information
* All versions of `BitSet` are now serializable

### Fixes
* `UniqueIdAllocator` no longer throws an exception when reserving more identifier slots than initially allocated

### Breaking Changes
* All variants of `Unsafe.Read/Write(string)` renamed to `Unsafe.ReadUTF8/WriteUTF8` for consistency

## Version 0.9.1
**29 June 2023**

Hotfix for `TypeIndex` custom capacity

## Version 0.9.0
**29 June 2023**

New collection types
`MeshData` improvements

### Features
* New `BitSet` structs for easier access to bit sets
* New `LruCache` fixed-size cache type
* New `LLTable` collection, for array-backed linked lists
* New `UniqueId`, `UniqueIdAllocator` and `UniqueIdMap` classes for generating unique validatable handles for array-backed storage
* `CompareUtils.Default[...]` can now use types marked with `DefaultEqualityComparer` and `DefaultSorter` attributes for their respective types
* New `Vectors` utility class for sequentially transforming vectors and vectors embedded in structs
* New `OffsetLength` struct and utilities for representing ranges of data in 1-dimensional arrays/buffers
* New `TypeIndex<T>` utility for statically mapping types to indices (for faster array lookups)

### Improvements
* `Bits` utilities now work on 64-bit integrals
* Improved `MeshData.Upload` performance
* Added `MeshData.Topology`, specified in constructor
* `MeshData` can now transform ranges of vertices
* `Profiling.Time` can now specify time units (milliseconds, microseconds, ticks)
* Added callbacks to `DMInfo` for menu open and close
* `CameraHelper` can now register global pre-cull, pre-render, and post-render handlers
* `VertexUtility.GenerateLayout` also now returns size, attribute, and offset information

### Fixes
* `MeshData` correctly calculates maximum vertices based on index format and vertex size

### Breaking Changes
* `RingBuffer<T>` is now a sealed class
* `CompareUtils.DefaultComparer<T>` renamed to `CompareUtils.DefaultEquals<T>` to avoid conceptual naming conflicts with `CompareUtils.DefaultSort<T>`
* `MeshData.Add[Shape]` returns an `OffsetLength` instead of the `MeshData` instance

## Version 0.8.6
**19 June 2023**

New `Mesh` construction utilities
Support for `Slider` control type in `DMMenu`

### Features
* New `MeshData16<Vertex>` class for interleaved mesh construction
* Added `Slider` control support to `DMMenu`
* Added `Push`/`Pop` methods to arena allocator for more fine-grained freeing of memory
* Added `ArrayUtils.EnsureCapacity` to match `ListUtils.EnsureCapacity`

### Improvements
* Assert arguments are passed in using generics to avoid boxing
* Added Quantization methods to `MathUtils`
* Added endian swap methods to `Unsafe`
* Added `RingBuffer.MoveFrontToBack` and similar methods for moving elements between head and tail

### Fixes
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

## Version 0.7.12
**4 Sept 2022**

Hotfix for incorrect `SerializedHash32` hashes being masked in editor
Hotfix for BlockParser block error state not propagating correctly
Added support for `IGNORE_UNITY_EDITOR` define to disable debug checks in editor

## Version 0.7.11
**29 Aug 2022**

Hotfix for `RectTransformPinned` to handle objects behind camera
Hotfix for `CameraHelper.HeightForDistanceAndFOV` calculation
Memory "dump to string" utilities
Reduced memory usage of debug font

## Version 0.7.10
**19 Aug 2022**

Hotfix for `CameraRenderScale.PixelHeight` setter
Improved consistency `CameraFOVPlane.SetTargetPreserveFOV`

## Version 0.7.9
**19 July 2022**

Hotfix for `StringUtils.Args.Splitter` state corruption

## Version 0.7.8
**19 July 2022**

Hotfix for `Bits.Count` hang

## Version 0.7.7
**18 July 2022**

StringSlice.Split improvements for thread safety and better configurability

### Features
* Added `StringSlice.Split` configuration option to limit number of slices
* New `Enums` utility class to convert generic enums to/from integral types
* Added methods in `Bits` utility class to operate on generic flag enums
* New `Batch` utility class to group items together by batch id and dispatch callbacks on sets

### Breaking Changes
* `Bits.ContainsMask` renamed to `Bits.ContainsAny`
* `CharStreamParams` factory methods contain an additional `Owner` argument, to ensure garbage collection doesn't sweep up source of pinned buffers

### Fixes
* Using `Colors.Hex` in a static initializer will no longer crash

### Improvements
* Improved thread safety of `StringSlice.Split` operations involving `ISplitter`
* `StringUtils.ArgsList.Splitter` can now specify if quotes should be stripped from arguments

## Version 0.7.6
**12 July 2022**

Fixed BlockParser leftover buffer corruption issues.
Added additional StringBuilder argument to IBlockGenerator callbacks

### Fixes
* Fixed `CharStream.Insert` occasional buffer corruption

### Improvements
* Added `StringBuilder` argument to `IBlockGenerator.TryEvaluatePackage` and `IBlockGenerator.TryEvaluateMeta` as additional context

## Version 0.7.5
**10 July 2022**

Updating for 2020.1+ compatibility.
Adjusted CameraFOVPlane math for stability when altering clipping planes.

### Breaking Changes
* `CameraHelper.TryGetDistance` methods now return distance from the camera transform to the target plane, not the distance from the near clip plane to the target plane.

### Fixes
* `BlockParser` no longer erroneously reports errors when setting content via a `BlockContent` attribute

### Improvements
* Updated several classes to avoid compiler warnings/errors in Unity 2020.1+.

## Version 0.7.4
**21 June 2022**

Critical bug fix in BlockParser for a stream insertion error.

### Fixes
* `IBlockParserUtil.InsertStream` will properly defer processing on any unprocessed characters from the current stream

### Improvements
* Implemented `CharStream.InsertBytes` and `CharStream.InsertChars`, making buffer types a proper deque

## Version 0.7.3
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