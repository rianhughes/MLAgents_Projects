using System;
using System.Collections.Generic;
using UnityEditor.Recorder.Input;
using UnityEngine;


namespace UnityEditor.Recorder
{
    /// <summary>
    /// Available options for the output image format used by AOV Image Sequence Recorder.
    /// </summary>
    public enum AOVRecorderOutputFormat
    {
        /// <summary>
        /// Output the recording in PNG format.
        /// </summary>
        PNG,
        /// <summary>
        /// Output the recording in JPEG format.
        /// </summary>
        JPEG,
        /// <summary>
        /// Output the recording in EXR format.
        /// </summary>
        EXR     
    }

    // First entries (up to Alpha) of AOVGType must be kept in sync with
    // HDRP Runtime/Debug/MaterialDebug.cs MaterialShaderProperty
    
    /// <summary>
    /// Available options AOV Types.
    /// </summary>    
    public enum AOVGType
    {
        /// <summary>
        /// Select the Beauty AOV.
        /// </summary>
        Beauty,
        /// <summary>
        /// Select the Albedo AOV.
        /// </summary>
        Albedo,
        /// <summary>
        /// Select the Normal AOV.
        /// </summary>
        Normal,
        /// <summary>
        /// Select the Smootness AOV.
        /// </summary>
        Smoothness,
        /// <summary>
        /// Select the Ambient Occlusion AOV.
        /// </summary>
        AmbientOcclusion,
        /// <summary>
        /// Select the Metal AOV.
        /// </summary>
        Metal,
        /// <summary>
        /// Select the Specular AOV.
        /// </summary>
        Specular,
        /// <summary>
        /// Select the Alpha AOV.
        /// </summary>
        Alpha,
        // Additional option for the AOV Recorder
        /// <summary>
        /// Select the Depth AOV.
        /// </summary>
        Depth

    }

    /// <summary>
    /// Futur compession type support.
    /// </summary>       
    public enum AOVCompressionType
    {
     
        /// <summary>
        /// Futur compession type support.
        /// </summary>       
        none,
        /// <summary>
        /// Futur compession type support.
        /// </summary>       
        rle,
        /// <summary>
        /// Futur compession type support.
        /// </summary>       
        zip,
        /// <summary>
        /// Futur compession type support.
        /// </summary>       
        zips,
        /// <summary>
        /// Futur compession type support.
        /// </summary>       
        piz,
        /// <summary>
        /// Futur compession type support.
        /// </summary>       
        pxr24,
        /// <summary>
        /// Futur compession type support.
        /// </summary>       
        b44,
        /// <summary>
        /// Futur compession type support.
        /// </summary>       
        b44a,
        /// <summary>
        /// Futur compession type support.
        /// </summary>       
        dwaa,
        /// <summary>
        /// Futur compession type support.
        /// </summary>       
        dwab
    }
     
    [RecorderSettings(typeof(AOVRecorder), "AOV Image Sequence", "aovimagesequence_16")]
    class AOVRecorderSettings : RecorderSettings
    {
        [SerializeField] internal AOVRecorderOutputFormat outputFormat = AOVRecorderOutputFormat.EXR;
        [SerializeField] internal AOVGType AOVGSelection = AOVGType.Beauty;
        [SerializeField] internal AOVCompressionType AOVCompression = AOVCompressionType.dwaa;
        [SerializeField] internal AOVImageInputSelector m_AOVImageInputSelector = new AOVImageInputSelector();
 
        public AOVRecorderSettings()
        {
            FileNameGenerator.FileName = "aov_image_" + DefaultWildcard.Frame;
        }

        protected override string Extension
        {
            get
            {
                switch (outputFormat)
                {
                    case AOVRecorderOutputFormat.PNG:
                        return "png";
                    case AOVRecorderOutputFormat.JPEG:
                        return "jpg";
                    case AOVRecorderOutputFormat.EXR:
                        return "exr";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public ImageInputSettings imageInputSettings
        {
            get { return m_AOVImageInputSelector.imageInputSettings; }
            set { m_AOVImageInputSelector.imageInputSettings = value; }
        }

        protected override bool ValidityCheck(List<string> errors)
        {
            var ok = base.ValidityCheck(errors);

            if(string.IsNullOrEmpty(FileNameGenerator.FileName))
            {
                ok = false;
                errors.Add("missing file name");
            }

            #if !HDRP_AVAILABLE
            ok = false;
            errors.Add("HDRP package not available");
            #endif

            return ok;
        }

        protected override  bool HasErrors()
        {
            #if HDRP_AVAILABLE
            
            return false;
            #else
            #if UNITY_2019_2_OR_NEWER
            var hdrpMinVersion = 6.6;
            #else
            var hdrpMinVersion = 5.11;
            #endif

            Enabled = false;
            Debug.LogError("AOV Recorder requires the HDRP package version " + hdrpMinVersion + " or greater to be installed");
            return true;
            #endif
        }

        public override IEnumerable<RecorderInputSettings> InputsSettings
        {
            get { yield return m_AOVImageInputSelector.Selected; }
        } 
    }
}
