using UnityEngine;

public class sugar : MonoBehaviour
{
    public Animator anim;
    public Renderer targetRenderer;
    public Color startColor = Color.white;
    public Color endColor = Color.red;
    public float duration = 5f;

    private float timer = 0f;
 
    bool j = false;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Sphere"))
        {
            targetRenderer = other.GetComponent<Renderer>();

        }
    }
    void Update()
    {
        if (timer < duration && j)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            targetRenderer.material.color = Color.Lerp(startColor, endColor, t);
        }
    }
    public void yes()
    {
        j = true;
        anim.SetTrigger("caramel");
    }
}
