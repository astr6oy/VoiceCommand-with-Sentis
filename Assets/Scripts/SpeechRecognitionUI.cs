using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpeechRecognitionUI : MonoBehaviour {
    [Header("Button Sprites")]
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite recordingSprite;

    [Header("UI References")]
    [SerializeField] private Button recordButton;
    [SerializeField] private Image buttonImage;
    [SerializeField] private TMP_Text resultText;

    [Header("Controller Reference")]
    [SerializeField] private SpeechRecognitionController speechController;

    private bool isRecording = false;

    private void Start() {
        // 텍스트 초기화
        resultText.text = "";

        // 버튼 이벤트 등록
        recordButton.onClick.AddListener(OnButtonClick);

        // SpeechRecognitionController의 onResponse 구독
        speechController.onResponse.AddListener(OnTranscriptionResult);
    }

    /// <summary>
    /// 버튼 클릭 시 호출
    /// </summary>
    private void OnButtonClick() {
        // SpeechRecognitionController의 Click 메소드 호출
        speechController.Click();

        // 녹음 상태 토글
        isRecording = !isRecording;

        // 버튼 이미지 변경
        buttonImage.sprite = isRecording ? recordingSprite : idleSprite;
    }

    /// <summary>
    /// 음성인식 결과를 받아서 텍스트에 표시
    /// </summary>
    /// <param name="text">음성인식 결과 텍스트</param>
    private void OnTranscriptionResult(string text) {
        // 결과 텍스트 갱신
        resultText.text = text;

        // 녹음 종료 후 결과가 오면 버튼 이미지를 idle로 복원
        isRecording = false;
        buttonImage.sprite = idleSprite;
    }

    private void OnDestroy() {
        // 이벤트 구독 해제
        if (recordButton != null) {
            recordButton.onClick.RemoveListener(OnButtonClick);
        }

        if (speechController != null) {
            speechController.onResponse.RemoveListener(OnTranscriptionResult);
        }
    }
}
