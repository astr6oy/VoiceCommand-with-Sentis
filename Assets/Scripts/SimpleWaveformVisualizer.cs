using UnityEngine;
using UnityEngine.UI;

public class SimpleWaveformVisualizer : MonoBehaviour
{
    public AudioSource audioSource;
    public RawImage waveformImage;
    
    private Texture2D texture;
    private float[] samples = new float[1024];
    
    void Start()
    {
        texture = new Texture2D(1024, 200);
        waveformImage.texture = texture;
    }
    
    void Update()
    {
        if (audioSource.isPlaying)
        {
            audioSource.GetOutputData(samples, 0);
            DrawWaveform();
        }
    }
    
    void DrawWaveform()
    {
        // 텍스처 초기화 (배경)
        Color[] colors = new Color[texture.width * texture.height];
        for (int i = 0; i < colors.Length; i++)
            colors[i] = new Color(0.2f, 0.2f, 0.2f);
        
        // 파형 그리기
        for (int x = 0; x < samples.Length; x++)
        {
            int y = Mathf.RoundToInt((samples[x] + 1f) * 0.5f * texture.height);
            y = Mathf.Clamp(y, 0, texture.height - 1);
            colors[y * texture.width + x] = Color.yellow;
        }
        
        texture.SetPixels(colors);
        texture.Apply();
    }
}