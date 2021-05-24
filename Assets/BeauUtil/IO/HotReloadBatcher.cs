/*
 * Copyright (C) 2017-2020. Autumn Beauchesne. All rights reserved.
 * Author:  Autumn Beauchesne
 * Date:    30 Sept 2020
 * 
 * File:    HotReloadBatcher.cs
 * Purpose: Reload batcher.
 */

using System;
using System.Collections.Generic;

namespace BeauUtil.IO
{
    /// <summary>
    /// Reload batcher.
    /// </summary>
    public class HotReloadBatcher : IDisposable
    {
        private HashSet<IHotReloadable> m_AllReloadables = new HashSet<IHotReloadable>();
        private Dictionary<StringHash32, HashSet<IHotReloadable>> m_ReloadablesByTag = new Dictionary<StringHash32, HashSet<IHotReloadable>>();

        /// <summary>
        /// Adds an IHotReloadable.
        /// </summary>
        public bool Add(IHotReloadable inReloadable)
        {
            if (inReloadable == null)
                return false;

            if (!m_AllReloadables.Add(inReloadable))
                return false;

            HashSet<IHotReloadable> byTag;
            if (!m_ReloadablesByTag.TryGetValue(inReloadable.Tag, out byTag))
            {
                m_ReloadablesByTag[inReloadable.Tag] = byTag = new HashSet<IHotReloadable>();
            }
            byTag.Add(inReloadable);
            return true;
        }

        /// <summary>
        /// Removes an IHotReloadable.
        /// </summary>
        public bool Remove(IHotReloadable inReloadable)
        {
            if (inReloadable == null)
                return false;
            
            if (!m_AllReloadables.Remove(inReloadable))
                return false;

            HashSet<IHotReloadable> byTag;
            if (m_ReloadablesByTag.TryGetValue(inReloadable.Tag, out byTag))
                byTag.Remove(inReloadable);

            return true;
        }

        /// <summary>
        /// Attempts to reload assets of a specific tag.
        /// </summary>
        public int TryReloadTag(StringHash32 inTag, ICollection<HotReloadResult> outResults = null, bool inbForce = false)
        {
            HashSet<IHotReloadable> byTag;
            if (!m_ReloadablesByTag.TryGetValue(inTag, out byTag) || byTag.Count == 0)
                return 0;

            int reloadCount = 0;
            HashSet<IHotReloadable> deletedAssets = new HashSet<IHotReloadable>();
            foreach(var asset in byTag)
            {
                HotReloadOperation op = TryReload(asset, outResults, inbForce);
                switch(op)
                {
                    case HotReloadOperation.Deleted:
                        deletedAssets.Add(asset);
                        ++reloadCount;
                        break;

                    case HotReloadOperation.Modified:
                        ++reloadCount;
                        break;
                }
            }

            foreach(var deleted in deletedAssets)
            {
                byTag.Remove(deleted);
                m_AllReloadables.Remove(deleted);
            }

            return reloadCount;
        }

        /// <summary>
        /// Attempts to reload all assets.
        /// </summary>
        public int TryReloadAll(ICollection<HotReloadResult> outResults = null, bool inbForce = false)
        {
            int reloadCount = 0;
            HashSet<IHotReloadable> deletedAssets = new HashSet<IHotReloadable>();
            foreach(var asset in m_AllReloadables)
            {
                HotReloadOperation op = TryReload(asset, outResults, inbForce);
                switch(op)
                {
                    case HotReloadOperation.Deleted:
                        deletedAssets.Add(asset);
                        ++reloadCount;
                        break;

                    case HotReloadOperation.Modified:
                        ++reloadCount;
                        break;
                }
            }

            foreach(var deleted in deletedAssets)
            {
                Remove(deleted);
            }

            return reloadCount;
        }

        private HotReloadOperation TryReload(IHotReloadable inAsset, ICollection<HotReloadResult> outResults, bool inbForce)
        {
            HotReloadOperation operation = inAsset.NeedsReload();
            if (inbForce && operation == HotReloadOperation.Unaffected)
                operation = HotReloadOperation.Modified;
            
            if (operation != HotReloadOperation.Unaffected)
            {
                if (outResults != null)
                {
                    HotReloadResult result = new HotReloadResult(inAsset, operation);
                    outResults.Add(result);
                }

                inAsset.HotReload(operation);
            }

            return operation;
        }
    
        /// <summary>
        /// Disposes of all hot reloadables.
        /// </summary>
        public void Dispose()
        {
            m_ReloadablesByTag.Clear();
            foreach(var reloadable in m_AllReloadables)
            {
                IHotReloadable rel = reloadable;
                Ref.TryDispose(ref rel);
            }
            m_AllReloadables.Clear();
        }
    }

    /// <summary>
    /// Result of a hot reload.
    /// </summary>
    public struct HotReloadResult : IDebugString
    {
        public readonly StringHash32 ObjectId;
        public readonly StringHash32 ObjectTag;
        public readonly HotReloadOperation Operation;

        public HotReloadResult(StringHash32 inObjectId, StringHash32 inObjectTag, HotReloadOperation inOperation)
        {
            ObjectId = inObjectId;
            ObjectTag = inObjectTag;
            Operation = inOperation;
        }

        public HotReloadResult(IHotReloadable inReloadable, HotReloadOperation inOperation)
        {
            if (inReloadable != null)
            {
                ObjectId = inReloadable.Id;
                ObjectTag = inReloadable.Tag;
            }
            else
            {
                ObjectId = StringHash32.Null;
                ObjectTag = StringHash32.Null;
            }
            Operation = inOperation;
        }

        public override int GetHashCode()
        {
            int hash = ObjectId.GetHashCode();
            hash = (hash << 3) ^ ObjectTag.GetHashCode();
            hash = (hash << 2) ^ Operation.GetHashCode();
            return hash;
        }

        public override string ToString()
        {
            return string.Format("{0} {1} (tag {2})", Operation, ObjectId.ToString(), ObjectTag.ToString());
        }

        public string ToDebugString()
        {
            return string.Format("{0} {1} (tag {2})", Operation, ObjectId.ToDebugString(), ObjectTag.ToDebugString());
        }
    }
}