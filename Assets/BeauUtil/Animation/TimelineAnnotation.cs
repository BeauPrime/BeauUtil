using System;
using BeauUtil.Variants;

namespace BeauUtil
{
    [Serializable]
    public struct TimelineAnnotation
    {
        public TimelineFrameAnnotation[] Frames;
    }

    [Serializable]
    public struct TimelineFrameAnnotation
    {
        public BitSet64 Flags;
        public SerializedVariant Custom0;
        public SerializedVariant Custom1;
    }
}