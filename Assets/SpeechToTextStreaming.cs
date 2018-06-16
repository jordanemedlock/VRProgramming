
using UnityEngine;
using System.Collections;
using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Services.SpeechToText.v1;
using IBM.Watson.DeveloperCloud.Utilities;
using IBM.Watson.DeveloperCloud.DataTypes;
using System.Collections.Generic;
using UnityEngine.UI;



public class SpeechToTextStreaming : MonoBehaviour 
{

    public string serviceURL;
    public string username;
    public string password;
    public GameObject speechConsumer;

    private SpeechToText service;

    private int recordingRoutine = 0;
    private string microphoneID = null;
    private AudioClip recording = null;
    private int recordingBufferSize = 1;
    private int recordingHZ = 22050;

	// Use this for initialization
	void Start () 
    {
        LogSystem.InstallDefaultReactors(); // TODO: not sure what this does
        Runnable.Run(CreateService());
	}
	
    private IEnumerator CreateService() 
    {
        Credentials creds = null;
        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password)) 
        {
            creds = new Credentials(username, password, serviceURL);
        } 
        else 
        {
            throw new WatsonException("Please provide a username and password");
        }

        service = new SpeechToText(creds);
        service.StreamMultipart = true;

        Active = true;
        StartRecording();

        yield return null;
    }

    public bool Active 
    {
        get 
        {
            return service.IsListening;
        }
        set
        {
            if (value && !service.IsListening) // start listening
            {
                service.DetectSilence = true;
                service.EnableWordConfidence = true;
                service.EnableTimestamps = true;
                service.SilenceThreshold = 0.01f;
                service.MaxAlternatives = 0;
                service.EnableInterimResults = true;
                service.OnError = OnError;
                service.InactivityTimeout = -1;
                service.ProfanityFilter = false;
                service.SmartFormatting = true;
                service.SpeakerLabels = false;
                service.WordAlternativesThreshold = null;
                service.StartListening(OnRecognize, OnRecognizeSpeaker);
                
            }
            else if (!value && service.IsListening) // stop listening
            {
                service.StopListening();
            }
        }
    }

    private void StartRecording() 
    {
        if (recordingRoutine == 0) 
        {
            UnityObjectUtil.StartDestroyQueue();
            recordingRoutine = Runnable.Run(RecordingHandler());
        }
    }

    private void StopRecording() 
    {
        if (recordingRoutine != 0) 
        {
            Microphone.End(microphoneID);
            Runnable.Stop(recordingRoutine);
            recordingRoutine = 0;
        }
    }

    private void OnError(string error) 
    {
        Active = false;
        Log.Debug("SpeechToTextStreaming.OnError()", "Error! {0}", error);
    }

    private IEnumerator RecordingHandler() 
    {
        Log.Debug("SpeechToTextStreaming.RecordingHandler()", "devices: {0}", Microphone.devices);
        recording = Microphone.Start(microphoneID, true, recordingBufferSize, recordingHZ);
        yield return null;

        if (recording == null) 
        {
            StopRecording();
            yield break;
        }

        bool firstBlock = true;
        int midPoint = recording.samples / 2;
        float[] samples = null;

        while (recordingRoutine != 0 && recording != null) 
        {
            int writePos = Microphone.GetPosition(microphoneID);
            if (writePos > recording.samples || !Microphone.IsRecording(microphoneID))
            {
                Log.Error("SpeechToTextStreaming.RecordingHandler()", "Microphone disconnected.");

                StopRecording();
                yield break;
            }

            if ((firstBlock && writePos >= midPoint)
                || (!firstBlock && writePos < midPoint))
            {
                samples = new float[midPoint];
                recording.GetData(samples, firstBlock ? 0 : midPoint);

                AudioData record = new AudioData();
                record.MaxLevel = Mathf.Max(Mathf.Abs(Mathf.Min(samples)), Mathf.Max(samples));
                record.Clip = AudioClip.Create("Recording", midPoint, recording.channels, recordingHZ, false);
                record.Clip.SetData(samples, 0);

                service.OnListen(record);

                firstBlock = !firstBlock;
            }
            else
            {
                int remaining = firstBlock ? (midPoint - writePos) : (recording.samples - writePos);
                float timeRemaining = (float)remaining / (float)recordingHZ;

                yield return new WaitForSeconds(timeRemaining);
            }
        }

        yield break;
    }

    private void OnRecognize(SpeechRecognitionEvent result, Dictionary<string, object> customData) 
    {
        if (result != null && result.results.Length > 0) 
        {
            foreach (var res in result.results) 
            {
                speechConsumer.SendMessage("SpeechRecognized", res);

            }
        }
    }

    private void OnRecognizeSpeaker(SpeakerRecognitionEvent result, Dictionary<string, object> customData) 
    {
        if (result != null)
        {
            foreach (SpeakerLabelsResult labelResult in result.speaker_labels) 
            {
                Log.Debug("SpeechToTextStreaming.OnRecognize()", string.Format("speaker result: {0} | confidence: {3} | from: {1} | to: {2}", labelResult.speaker, labelResult.from, labelResult.to, labelResult.confidence));
            }
        }
    }
}
