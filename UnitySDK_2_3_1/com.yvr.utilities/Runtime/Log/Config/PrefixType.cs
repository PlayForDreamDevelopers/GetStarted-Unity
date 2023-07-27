using System;

namespace YVR.Utilities
{
    [Flags, Serializable]
    public enum PrefixType
    {
        None = 0,
        Priority = 1 << 0,
        Context = 1 << 1,
    }
}