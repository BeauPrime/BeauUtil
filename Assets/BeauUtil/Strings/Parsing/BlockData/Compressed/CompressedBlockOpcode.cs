/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    10 Nov 2020
 * 
 * File:    CompressedBlockOpcode.cs
 * Purpose: Opcode for compressed block data.
 */

using BeauUtil.Tags;

namespace BeauUtil.Blocks
{
    internal enum CompressedBlockOpcode : byte
    {
        SkipSingleLine      = 0, // skips 1 line
        SkipLine8           = 1, // skips 1-256 lines (byte + 1)
        SkipLine16          = 2, // skips 1-65536 lines (ushort + 1)

        PackageCommand      = 8, // package command (ushort id, ushort arg)
        
        BeginBlock          = 16, // begin block (ushort id)
        BeginBlockExt       = 17, // begin block (ushort id, ushort arg)

        BlockMeta           = 18, // block meta (ushort id, ushort arg)
        EndBlockHeader      = 19, // end block header
        EndBlockHeaderExt   = 20, // end block header (ushort id, ushort arg)
        
        BlockContent        = 21, // block content (ushort content)
        
        BlockEnd            = 22, // block end
        BlockEndExt         = 23 // block end (ushort id, ushort arg)
    }
}