# VoiceCommand-with-Sentis


## 음성 인식 모델 비교

### inference-engine-whisper-tiny (Unity 공식)
- 용량: 425.6MB
- 한글 미지원
- 영어도 정확한 발음 필요해서 실용성 낮음
- ONNX 포맷 범용성은 있으나 설정이 복잡함

![scene01.mp4](docs/scene01.mp4)
*(inference-engine-whisper-tiny sample test)*

### whisper.unity (whisper.cpp 기반)
- ggml-tiny (77.7MB): 공식 모델보다 가볍고 성능 나음
- 한글 지원
- 발음 인식률은 여전히 아쉬움
- ggml-large-v3-turbo-q8_0 (874.2MB): 인식률 개선되나 용량 부담

![docs/scene02-01.mp4](docs/scene02-01.mp4)
*(whisper.unity  ggml-tiny sample test)*

![docs/scene02-02.mp4](docs/scene02-02.mp4)
*(whisper.unity  ggml-large-v3-turbo-q8_0 sample test)*

![docs/scene03.mp4](docs/scene03.mp4)
*(whisper.unity  ggml-large-v3-turbo-q8_0 command demo)*

**결론**: whisper.unity + ggml-tiny 조합 사용

## 명령어 매칭

FuzzySharp로 텍스트-명령어 유사도 검사

**문제점**: 기본 전처리기가 한글을 전부 제거함
- 정규식 `[^ a-zA-Z0-9]`가 한글을 공백으로 치환
- 한글 유니코드 범위 포함 전처리기 구현으로 해결

```csharp
var pattern = @"[^\uAC00-\uD7AF\u1100-\u11FF a-zA-Z0-9]";
```

## 환경

- Unity 6.2
- URP

## 패키지

- com.whisper.unity
- com.niftyhat.fuzzysharp
- com.unity.ai.inference (Unity 6.2에서 com.unity.sentis → 이름 변경됨)
- com.boxqkrtm.ide.cursor

## 구조

```
SpeechRecognitionController
  ↓ onResponse
CommandParser (FuzzySharp 유사도 검사)
  ↓ 명령어 매칭
TestController (Rigidbody 기반 이동)
```

## 참고

- https://huggingface.co/unity/inference-engine-whisper-tiny
- https://github.com/Macoron/whisper.unity
- https://github.com/JakeBayer/FuzzySharp
- https://huggingface.co/ggerganov/whisper.cpp
