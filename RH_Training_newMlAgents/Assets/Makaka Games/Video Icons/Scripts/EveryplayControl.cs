using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class EveryplayControl : MonoBehaviour 
{
	private bool isEveryplayPanelShow = false;
	public bool isDebugLogOn = true;
	
	public CanvasGroup everyplayPanelCanvasGroup;

	public Toggle ToogleRecord;
	public Button ButtonWatchReplays;
	public Button ButtonFaceCam;
	public Button ButtonShare;
	

	void DebugLog(string s)
	{
		if (isDebugLogOn) 
		{
			Debug.Log(s);
		}
	}

	void Start()
	{
		DebugLog("Everplay Control Start()!");
	}

	void Destroy()
	{
		DebugLog("Everyplay Destroy!");
	}

	public void ShowEveryplayPanel()
	{
		DebugLog("Show Everyplay Panel!");

		isEveryplayPanelShow = !isEveryplayPanelShow;

		everyplayPanelCanvasGroup.interactable = isEveryplayPanelShow;
		everyplayPanelCanvasGroup.blocksRaycasts = isEveryplayPanelShow;
		everyplayPanelCanvasGroup.alpha = isEveryplayPanelShow ? 1 : 0;
	}
	
	private void ReadyForRecording(bool enabled)
	{
		DebugLog("ReadyForRecording Event");

		ToogleRecord.interactable = enabled;
	}

	private void RecordingStarted()
	{
		DebugLog("RecordingStarted Event");

		if (ButtonShare != null) 
		{
			ButtonShare.interactable = false;
		}
		
		if (ButtonFaceCam != null) 
		{
			ButtonFaceCam.interactable = false;
		}
	}
	
	private void RecordingStopped()
	{
		DebugLog("RecordingStopped Event");

		if (ButtonShare != null) 
		{
			ButtonShare.interactable = true;
		}

		if (ButtonFaceCam != null) 
		{
			ButtonFaceCam.interactable = true;
		}
	}

	public void RecordToggle()
	{
		DebugLog ("Record Toggle!");

		if (ToogleRecord.isOn)
		{
			StartRecording();
		}
		else
		{
			StopRecording();
			ShareVideo();
		}
	}

	public void StartRecording()
	{
		DebugLog("StartRecording!");
	}

	public void StopRecording()
	{
		DebugLog("StopRecording!");
	}
	private void FaceCamRecordingPermission(bool granted)
	{
		DebugLog ("FaceCamRecordingPermission Event");
	}
	
	public void FaceCamToggle()
	{
		DebugLog ("Face Cam Toggle!");
	}
	
	public void OpenEveryplay()
	{
		DebugLog ("Everplay Show!");
	}
	
	public void ShareVideo()
	{
		DebugLog ("Share Video!");
	}

}