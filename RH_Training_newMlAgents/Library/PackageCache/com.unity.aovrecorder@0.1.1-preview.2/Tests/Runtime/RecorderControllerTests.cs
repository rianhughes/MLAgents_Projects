using System;
using NUnit.Framework;
using UnityEditor.Recorder;

namespace UnityEngine.Recorder.Tests
{
	class RecorderControllerTests
	{
#if HDRP_AVAILABLE
		[Test]
		public void StartAndStopRecording_WithValidSettings_ShouldStartThenStopRecording()
		{
			var settings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
			var imageRecorder = ScriptableObject.CreateInstance<AOVRecorderSettings>();
			
			settings.AddRecorderSettings(imageRecorder);
			var recorderController = new RecorderController(settings);

			Assert.IsTrue(recorderController.StartRecording());
			Assert.IsTrue(recorderController.IsRecording());
			
			recorderController.StopRecording();
			Assert.IsFalse(recorderController.IsRecording());
			
			Object.DestroyImmediate(imageRecorder);
			Object.DestroyImmediate(settings);
		}
#endif
	}
}
