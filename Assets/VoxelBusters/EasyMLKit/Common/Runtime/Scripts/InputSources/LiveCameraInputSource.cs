using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoxelBusters.EasyMLKit
{
    /// <summary>
    /// This creates live camera as input source
    /// </summary>
    public class LiveCameraInputSource : IImageInputSource
    {
        /// <summary>
        /// Enable this to switch on the flash
        /// </summary>
        public bool EnableFlash { get; set; }


        /// <summary>
        /// Set this to on for enabling front facing camera
        /// </summary>
        public bool IsFrontFacing { get; set; } 

        /// <summary>
        /// Max resolution that can be considered for camera preview. First we will see if this resolution is available. If not pick the next lowest resolution
        /// </summary>
        public Vector2 MaxResolution { get; set; } = new Vector2(-1f, -1f);

        /// <summary>
        /// Set this for viewport of the camera preview
        /// </summary>
        public Rect Viewport { get; set;} = new Rect(0, 0, 1, 1);


        public LiveCameraInputSource()
        {
        }


        public void Close()
        {
        }

        public float GetWidth()
        {
            return Screen.width;
        }

        public float GetHeight()
        {
            return Screen.height;
        }
    }
}
