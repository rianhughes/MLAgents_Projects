#if HDRP_AVAILABLE
using UnityEngine;
using UnityObject = UnityEngine.Object;
using UnityEngine.Rendering;
using System.Linq;

#if UNITY_2019_3_OR_NEWER
using UnityEngine.Rendering.HighDefinition.Attributes;
using UnityEngine.Rendering.HighDefinition;
#else
using UnityEngine.Experimental.Rendering.HDPipeline.Attributes;
using UnityEngine.Experimental.Rendering.HDPipeline;
using UnityEngine.Experimental.Rendering; 
#endif


namespace UnityEditor.Recorder.Input
{
#if HDRP_AOVREQUEST_API
    class AOVCameraAOVRequestAPIInput : CameraInput
    {
        RenderTexture m_TempRT;
#if UNITY_2019_3_OR_NEWER
        private RTHandle m_ColorRT;
#else   
        private  RTHandleSystem.RTHandle m_ColorRT;
#endif

        protected RenderTexture CreateFrameBuffer(RenderTextureFormat format, int width, int height, int depth = 0, bool sRGB = false)
        {
            return new RenderTexture(width, height, depth, format, sRGB ? RenderTextureReadWrite.sRGB: RenderTextureReadWrite.Default);
        }

        void EnableAOVCapture(RecordingSession session, Camera cam)
        {
            var aovRecorderSettings = session.settings as AOVRecorderSettings;

            if (aovRecorderSettings != null)
            {
                if ( aovRecorderSettings.AOVGSelection == AOVGType.Beauty )
                    return;

                var hdAdditionalCameraData = cam.GetComponent<HDAdditionalCameraData>();
                if (hdAdditionalCameraData != null )
                {
                    if (m_TempRT == null)
                    {
                        m_TempRT = CreateFrameBuffer(RenderTextureFormat.ARGBFloat, OutputWidth, OutputHeight, 0,
                            false);
                    }
                    var aovRequest = new AOVRequest(AOVRequest.@default);
                    var aovBuffer = AOVBuffers.Color;
                    if (aovRecorderSettings.AOVGSelection == AOVGType.Depth)
                    {
                        aovRequest.SetFullscreenOutput(DebugFullScreen.Depth);
                        aovBuffer = AOVBuffers.DepthStencil;
                    }
                    else
                    {
                        aovRequest.SetFullscreenOutput((MaterialSharedProperty) aovRecorderSettings.AOVGSelection);
                    }

                    var bufAlloc = m_ColorRT ?? (m_ColorRT = RTHandles.Alloc(OutputWidth, OutputHeight));

                    var aovRequestBuilder = new AOVRequestBuilder();
                    aovRequestBuilder.Add(aovRequest,
                        bufferId => bufAlloc,
                        null,
                        new[] {aovBuffer},
                        (cmd, textures, properties) =>
                        {
                            if (m_TempRT != null)
                            {
                                cmd.Blit(textures[0], m_TempRT);
                            }
                        });
                    var aovRequestDataCollection = aovRequestBuilder.Build();                    
                    var previousRequests = hdAdditionalCameraData.aovRequests;
                    if (previousRequests != null && previousRequests.Any())
                    {
                        var listOfRequests = previousRequests.ToList();
                        foreach (var p in aovRequestDataCollection)
                        {
                            listOfRequests.Add(p);
                        }
                        var allRequests = new AOVRequestDataCollection(listOfRequests);
                        hdAdditionalCameraData.SetAOVRequests(allRequests);        
                    
                    }
                    else
                    {
                        hdAdditionalCameraData.SetAOVRequests(aovRequestDataCollection);
                    }
                }
            }
        }

        void ReadbackAOVCapture(RecordingSession session)
        {
            var aovRecorderSettings = session.settings as AOVRecorderSettings;

            if (aovRecorderSettings != null)
            {
                if ( aovRecorderSettings.AOVGSelection == AOVGType.Beauty )
                    return;

                if (ReadbackTexture == null)
                {
                    ReadbackTexture = new Texture2D(OutputWidth, OutputHeight, TextureFormat.RGBAFloat, false);
                }
                RenderTexture.active = m_TempRT;
                ReadbackTexture.ReadPixels(new Rect(0, 0, OutputWidth, OutputHeight), 0, 0, false);
                ReadbackTexture.Apply();
                RenderTexture.active = null;
            }
        }

        void DisableAOVCapture(RecordingSession session)
        {
            var aovRecorderSettings = session.settings as AOVRecorderSettings;

            if (aovRecorderSettings != null)
            {
                if ( aovRecorderSettings.AOVGSelection == AOVGType.Beauty )
                    return;

                var add = TargetCamera.GetComponent<HDAdditionalCameraData>();
                if (add != null)
                {
                    add.SetAOVRequests(null);
                }
            }
        }

        protected override void NewFrameStarting(RecordingSession session)
        {
            base.NewFrameStarting(session);
            EnableAOVCapture(session, TargetCamera);
        }

        protected override void NewFrameReady(RecordingSession session)
        {
            ReadbackAOVCapture(session);
            base.NewFrameReady(session);
        }

        protected override void FrameDone(RecordingSession session)
        {
            base.FrameDone(session);
            DisableAOVCapture(session);
        }
    }
#else // HDRP_AOVREQUEST_API
    class AOVCameraDebugFrameworkInput : CameraInput
    {
        void PrepFrameRenderTexture()
        {
            if (OutputRenderTexture != null)
            {
                if (OutputRenderTexture.IsCreated() && OutputRenderTexture.width == OutputWidth && OutputRenderTexture.height == OutputHeight &&
                    OutputRenderTexture.format == RenderTextureFormat.ARGBFloat)
                    return;
                ReleaseBuffer();
            }
            OutputRenderTexture = new RenderTexture(OutputWidth, OutputHeight, 0, RenderTextureFormat.ARGBFloat)
            {
                wrapMode = TextureWrapMode.Repeat
            };
            OutputRenderTexture.Create();
        }

        void EnableAOVCapture(RecordingSession session)
        {
            var pipeline = RenderPipelineManager.currentPipeline as HDRenderPipeline;
            
            if (ReadbackTexture == null)
            {
                ReadbackTexture = new Texture2D(OutputWidth, OutputHeight, TextureFormat.RGBAFloat, false);
            }

            PrepFrameRenderTexture();

            var aovRecorderSettings = session.settings as AOVRecorderSettings;
            if (aovRecorderSettings != null)
            {
                if (aovRecorderSettings.AOVGSelection == AOVGType.Depth)
                {
                    pipeline.debugDisplaySettings.SetDebugViewGBuffer(10);
                }
                else
                {
                    pipeline.debugDisplaySettings.SetDebugViewGBuffer(0);
                    pipeline.debugDisplaySettings.SetDebugViewCommonMaterialProperty(
                        (MaterialSharedProperty) aovRecorderSettings.AOVGSelection);
                }
            }
        }

        void RenderAndReadbackAOVCapture(RecordingSession session)
        {
            var pipeline = RenderPipelineManager.currentPipeline as HDRenderPipeline;

            var aovRecorderSettings = session.settings as AOVRecorderSettings;
            if (aovRecorderSettings != null)
            {
                TargetCamera.Render();
                RenderTexture.active = OutputRenderTexture;
                ReadbackTexture.ReadPixels(new Rect(0, 0, OutputWidth, OutputHeight), 0, 0, false);
                ReadbackTexture.Apply();
                RenderTexture.active = null;
            }
        }

        void DisableAOVCapture(RecordingSession session)
        {
            var pipeline = RenderPipelineManager.currentPipeline as HDRenderPipeline;

            pipeline.debugDisplaySettings.SetDebugViewGBuffer(0);
            pipeline.debugDisplaySettings.SetDebugViewCommonMaterialProperty(MaterialSharedProperty.None); 
        }

        void CaptureAOV(RecordingSession session)
        {
            EnableAOVCapture(session);
            RenderAndReadbackAOVCapture(session);
            DisableAOVCapture(session);
        }

        protected override void NewFrameReady(RecordingSession session)
        {
            CaptureAOV(session);
            base.NewFrameReady(session);
        }
    }
 
#endif
}
#else // HDRP_AVAILABLE
namespace UnityEditor.Recorder.Input
{
    class AOVCameraDebugFrameworkInput : CameraInput
    {
        // nop No HDRP available
    }
}
#endif
