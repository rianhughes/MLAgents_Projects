#if HDRP_AVAILABLE

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
#if OIIO_AVAILABLE
using Unity.OpenImageIO;
#endif

namespace UnityEditor.Recorder
{
    class AOVRecorder : BaseTextureRecorder<AOVRecorderSettings>
    {
        public bool m_asyncShaderCompileSetting;
        Queue<string> m_PathQueue = new Queue<string>();
#if OIIO_AVAILABLE
        ImageSpec m_imgSpec = null;
        ImageOutput m_imgOutput = null;
#endif       
        protected override TextureFormat ReadbackTextureFormat
        {
            get
            {
                return Settings.outputFormat != AOVRecorderOutputFormat.EXR
                    ? TextureFormat.RGBA32
                    : TextureFormat.RGBAFloat;
            }
        }

        protected override bool BeginRecording(RecordingSession session)
        {
            if (!base.BeginRecording(session))
            {
                return false;
            }

            // Save the async compile shader setting to restore it at the end of recording
            m_asyncShaderCompileSetting = EditorSettings.asyncShaderCompilation;
            // Disable async compile shader setting when recording
            EditorSettings.asyncShaderCompilation = false;

            Settings.FileNameGenerator.CreateDirectory(session);
#if OIIO_AVAILABLE
            m_imgOutput = ImageOutput.create("dummy."+m_Settings.extension);
#endif
            return true;
        }

        protected override void EndRecording(RecordingSession session)
        {
            // Restore the asyncShaderCompilation setting
            EditorSettings.asyncShaderCompilation = m_asyncShaderCompileSetting;
#if OIIO_AVAILABLE
            if (m_imgOutput != null)
            {
                ImageOutput.destroy(m_imgOutput);
            }
#endif
            base.EndRecording(session);
        }

        protected override void RecordFrame(RecordingSession session)
        {
            if (m_Inputs.Count != 1)
                throw new Exception("Unsupported number of sources");
            // Store path name for this frame into a queue, as WriteFrame may be called
            // asynchronously later on, when the current frame is no longer the same (thus creating
            // a file name that isn't in sync with the session's current frame).
            m_PathQueue.Enqueue(Settings.FileNameGenerator.BuildAbsolutePath(session));
            base.RecordFrame(session);
        }

        protected override void WriteFrame(Texture2D tex)
        {
            byte[] bytes;

            Profiler.BeginSample("AOVRecorder.EncodeImage");
            try
            {
                switch (Settings.outputFormat)
                {
                    case AOVRecorderOutputFormat.EXR:
                    {
                        #if OIIO_AVAILABLE
                        TypeDesc typedesc = new TypeDesc(TypeDesc.BASETYPE.HALF);
                        var width = tex.width;
                        var height = tex.height;
                        int nchanels = 4;
                        m_imgSpec = new ImageSpec(width, height, nchanels, typedesc);
                        string comp = m_Settings.AOVCompression.ToString();
                        m_imgSpec.attribute("compression", comp);
                        bytes = tex.GetRawTextureData();
                        string path = m_PathQueue.Dequeue();
                        m_imgOutput.open(path, m_imgSpec);
                        GCHandle bufferHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                        int scanwidth = width * nchanels * sizeof(float);
                        int scanheight = nchanels * sizeof(float);
                        int scanlinesize = (int) m_imgSpec.scanline_bytes();
                        TypeDesc odesc = new TypeDesc(TypeDesc.BASETYPE.FLOAT);
                        m_imgOutput.write_image(odesc,
                            new IntPtr(bufferHandle.AddrOfPinnedObject().ToInt64() + (height - 1) * scanwidth),
                            scanheight,
                            -scanwidth,
                            Globals.AutoStride);
                        bufferHandle.Free();
                        m_imgOutput.close();
                        #else
                        bytes = tex.EncodeToEXR();
                        WriteToFile(bytes);
                        #endif
                        break;
                    }
                    case AOVRecorderOutputFormat.PNG:
                        bytes = tex.EncodeToPNG();
                        WriteToFile(bytes);
                        break;
                    case AOVRecorderOutputFormat.JPEG:
                        bytes = tex.EncodeToJPG();
                        WriteToFile(bytes);
                        break;
                    default:
                        Profiler.EndSample();
                        throw new ArgumentOutOfRangeException();
                }
            }
            finally
            {
                Profiler.EndSample();
            }

            if(m_Inputs[0] is BaseRenderTextureInput || Settings.outputFormat != AOVRecorderOutputFormat.JPEG)
              UnityHelpers.Destroy(tex);           
        }

        private void WriteToFile(byte[] bytes)
        {
            Profiler.BeginSample("AOVRecorder.WriteToFile");
            File.WriteAllBytes(m_PathQueue.Dequeue(), bytes);
            Profiler.EndSample();
        }
    }
}
#else // HDRP_AVAILABLE
using UnityEngine;
namespace UnityEditor.Recorder
{
    class AOVRecorder : BaseTextureRecorder<AOVRecorderSettings>
    {
        protected override TextureFormat ReadbackTextureFormat
        {
            get
            {
                return  TextureFormat.RGBA32;
            }
        }

        protected override void WriteFrame(Texture2D tex)
        {
            throw new System.NotImplementedException();
        }
    }
}
#endif
