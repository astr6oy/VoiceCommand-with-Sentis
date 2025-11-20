using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SampleTester : MonoBehaviour {
    [Header("UI References")]
    [SerializeField] private List<Button> buttons;

    [Header("Audio Clips")]
    [SerializeField] private List<AudioClip> audioClips;

    [Header("Component References")]
    [SerializeField] private SimpleWaveformVisualizer waveformVisualizer;
    [SerializeField] private SpeechRecognitionController speechController;

    private void Start() {
        for (int i = 0; i < buttons.Count; i++)
        {
            int index = i;
            buttons[i].onClick.AddListener(() => OnButtonClick(index));
        }
    }

    /// <summary>
    /// 버튼 클릭 시 호출되는 메소드
    /// </summary>
    /// <param name="index">클릭된 버튼의 인덱스</param>
    private void OnButtonClick(int index)
    {
        // 인덱스 범위 체크
        if (index >= audioClips.Count) {
            Debug.LogWarning($"Audio clip at index {index} does not exist!");
            return;
        }

        AudioClip clip = audioClips[index];

        if (clip == null) {
            Debug.LogWarning($"Audio clip at index {index} is null!");
            return;
        }

        // SimpleWaveformVisualizer의 AudioSource에 클립 연결 및 재생
        if (waveformVisualizer != null && waveformVisualizer.audioSource != null) {
            waveformVisualizer.audioSource.clip = clip;
            waveformVisualizer.audioSource.Play();
        } else {
            Debug.LogError("WaveformVisualizer or its AudioSource is not set!");
        }

        // SpeechRecognitionController로 오디오 클립 전달하여 음성인식 수행
        if (speechController != null) {
            speechController.SendAudioClip(clip);
        } else {
            Debug.LogError("SpeechRecognitionController is not set!");
        }
    }

    private void OnDestroy() {
        // 버튼 이벤트 구독 해제
        for (int i = 0; i < buttons.Count; i++) {
            int index = i;
            buttons[i].onClick.RemoveListener(() => OnButtonClick(index));
        }
    }
}
