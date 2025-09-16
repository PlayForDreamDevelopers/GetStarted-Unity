using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace YVR.Core
{
    [StructLayout(LayoutKind.Sequential)]
    public struct YVRExtent2DInt
    {
        public int width;
        public int height;
    }
}