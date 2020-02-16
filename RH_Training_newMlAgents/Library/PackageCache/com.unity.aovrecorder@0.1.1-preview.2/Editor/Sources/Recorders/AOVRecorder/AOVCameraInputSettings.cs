using System;


namespace UnityEditor.Recorder.Input
{
    /// <summary>
    /// This class contains the information for an AOV.
    /// </summary>
    public class AOVCameraInputSettings : CameraInputSettings
    {
        /// <summary>
        /// Input type for an AOV.
        /// </summary>
        protected override Type InputType
        {
            #if HDRP_AOVREQUEST_API
            get { return typeof(AOVCameraAOVRequestAPIInput);}
            #else
            get { return typeof(AOVCameraDebugFrameworkInput); }
            #endif
        }
    }
}
