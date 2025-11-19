using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;
using Whisper;

public enum WhisperImplementation {
    RunWhisper,
    WhisperManager
}

public class SpeechRecognitionController : MonoBehaviour {
    [Header("Whisper Implementation")]
    [SerializeField] private WhisperImplementation implementation = WhisperImplementation.RunWhisper;

    [Header("Implementation References")]
    [SerializeField] private RunWhisper runWhisper;
    [SerializeField] private WhisperManager whisperManager;
    [Header("Sample Audio Clip")]
    [SerializeField] private bool useSampleAudioClipOnStart = false;
    [SerializeField] private AudioClip sampleAudioClip;

    [Header("UI Events")]
    [SerializeField] private UnityEvent onStartRecording;
    [SerializeField] private UnityEvent onSendRecording;
    [SerializeField] public UnityEvent<string> onResponse;

    private string m_deviceName;
    private AudioClip m_clip;
    private bool m_recording;

    private void Start() {
        m_deviceName = Microphone.devices[0];

        if (useSampleAudioClipOnStart) {
            m_clip = sampleAudioClip;
            SendRecording();
        }
    }

    /// <summary>
    /// This method is called when the user clicks the button
    /// </summary>
    public void Click() {
        if (!m_recording) {
            StartRecording();
        } else {
            StopRecording();
        }
    }

    /// <summary>
    /// Start recording the user's voice
    /// </summary>
    private void StartRecording() {
        m_clip = Microphone.Start(m_deviceName, false, 10, 16000);
        m_recording = true;
        onStartRecording.Invoke();
    }

    /// <summary>
    /// Stop recording the user's voice and send the audio to the Whisper Model
    /// </summary>
    private void StopRecording() {
        var position = Microphone.GetPosition(m_deviceName);
        Microphone.End(m_deviceName);
        m_recording = false;
        SendRecording();
    }

    /// <summary>
    /// Run the Whisper Model with the audio clip to transcribe the user's voice
    /// </summary>
    private async void SendRecording() {
        onSendRecording.Invoke();

        switch (implementation) {
            case WhisperImplementation.RunWhisper:
                if (runWhisper != null) {
                    runWhisper.Transcribe(m_clip);
                } else {
                    Debug.LogError("RunWhisper reference is not set!");
                }
                break;

            case WhisperImplementation.WhisperManager:
                if (whisperManager != null) {
                    await TranscribeWithWhisperManager();
                } else {
                    Debug.LogError("WhisperManager reference is not set!");
                }
                break;
        }
    }

    /// <summary>
    /// Transcribe audio using WhisperManager implementation
    /// </summary>
    private async Task TranscribeWithWhisperManager() {
        if (m_clip == null) {
            Debug.LogError("Audio clip is null!");
            return;
        }

        // Convert AudioClip to float array
        float[] samples = new float[m_clip.samples * m_clip.channels];
        m_clip.GetData(samples, 0);

        try {
            // Call WhisperManager's GetTextAsync method
            WhisperResult result = await whisperManager.GetTextAsync(
                samples,
                m_clip.frequency,
                m_clip.channels
            );

            // Invoke response event with the transcription result
            if (result != null && !string.IsNullOrEmpty(result.Result)) {
                onResponse.Invoke(result.Result);
                Debug.Log($"Transcription: {result.Result}");
                Debug.Log($"Language: {result.Language} (ID: {result.LanguageId})");
            } else {
                Debug.LogWarning("Transcription result is empty!");
            }
        } catch (System.Exception e) {
            Debug.LogError($"Error during transcription: {e.Message}");
        }
    }

    private void Update() {
        if (!m_recording) {
            return;
        }

        if (Microphone.GetPosition(m_deviceName) >= m_clip.samples) {
            StopRecording();
        }
    }
}
