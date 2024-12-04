using System.Collections;
using UnityEngine;

public class BloodOverlayController : MonoBehaviour
{
    public Material bloodMaterial; // Blood overlay material
    public float fadeDuration = 1.0f; // Duration for fade-out effect

    // Initialize blood overlay (fully transparent at the start)
    private void Start()
    {
        if (bloodMaterial != null)
        {
            Color color = bloodMaterial.color;
            color.a = 0;
            bloodMaterial.color = color;
        }
    }

    // Trigger the blood effect and start fading out
    public void ShowBloodEffect()
    {
        if (bloodMaterial != null)
        {
            StopAllCoroutines(); // Cancel any ongoing fades
            StartCoroutine(FadeBloodEffect());
        }
    }

    // Coroutine to handle fading the blood overlay
    private IEnumerator FadeBloodEffect()
    {
        Color color = bloodMaterial.color;
        color.a = 1; // Fully visible at the start
        bloodMaterial.color = color;

        float elapsedTime = 0;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(1, 0, elapsedTime / fadeDuration); // Gradual fade
            bloodMaterial.color = color;
            yield return null;
        }

        color.a = 0; // Fully transparent at the end
        bloodMaterial.color = color;
    }
}
