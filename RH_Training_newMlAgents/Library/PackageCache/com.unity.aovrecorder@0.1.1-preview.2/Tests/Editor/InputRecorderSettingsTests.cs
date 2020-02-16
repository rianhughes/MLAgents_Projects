using UnityEngine;
using NUnit.Framework;
using UnityEditor.Recorder.Input;
using UnityObject = UnityEngine.Object;

namespace UnityEditor.Recorder.Tests
{
	class AOVInputRecorderSettingsTests
	{
		[Test]
		public void AOVCameraInputSettings_ShouldHaveProperPublicAPI()
		{
			var input = new AOVCameraInputSettings
			{
				Source = ImageSource.MainCamera,
				OutputWidth = 640,
				OutputHeight = 480,
				CameraTag = "AAA",
				CaptureUI = true,
				FlipFinalOutput = false
			};

			Assert.AreEqual(640, input.OutputWidth);
			Assert.AreEqual(480, input.OutputHeight);
			
			Assert.NotNull(input);
		}
	}
}
