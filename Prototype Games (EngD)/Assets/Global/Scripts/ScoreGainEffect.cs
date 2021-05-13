using UnityEngine;

public class ScoreGainEffect : MonoBehaviour
{
    public TextMesh Label;

    public float Lifetime = 1;

    public Vector3 AscensionRate = Vector3.up;

    private float spawnTime;

    public void SetText(string text) => Label.text = text;

    private void Awake() => spawnTime = Time.time;

    private void Update()
    {
        var progress = (Time.time - spawnTime) / Lifetime;
        if (progress < 1)
        {
            Label.transform.position += AscensionRate * Time.deltaTime;

            var labelColor = Label.color;
            labelColor.a = 1 - progress;
            Label.color = labelColor;
        }
        else
        {
            Destroy (gameObject);
        }
    }
}
