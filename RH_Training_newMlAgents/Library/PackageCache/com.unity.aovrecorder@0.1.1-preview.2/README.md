# AOV (Arbitrary Output Variables) Recorder

The AOV Recorder is an extension of the [Unity Recorder](https://github.cds.internal.unity3d.com/unity/com.unity.recorder).
It allows to recorder one or several Arbitrary Output Variables passes (render passes) as OpenEXR sequences.

The following AOV passes are currently available:
* Albedo,
* Normal,
* Smoothness,
* AmbientOcclusion,
* Metal,
* Specular,
* Alpha,
* Depth

See package documentation for more details.

Requirements
------------

The AOV Recorder depends on the High Definition Scriptable Render Pipeline [HDRP](https://github.com/Unity-Technologies/ScriptableRenderPipeline).

For Unity version 2019.1:

 	HDRP 5.11 or greater 

For Unity version  2019.2:

	HDRP 6.6 or greater
