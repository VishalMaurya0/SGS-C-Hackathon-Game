using UnityEngine;
using System.Collections;
public class nail : MonoBehaviour
{
    public GameObject g1;
    public Renderer targetRenderer;
    public Color startColor = Color.white;
    public Color endColor = Color.red;
    public float duration = 5f;

    private float timer = 0f;
    public GameObject g2;
    bool j = false;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Sphere"))
        {
            StartCoroutine(Yay());
            g2 = other.gameObject;
            targetRenderer = g2.GetComponent<Renderer>();

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

    IEnumerator Yay()
    {
        yield return new WaitForSeconds(3f);
        j = true;
        yield return new WaitForSeconds(4f);
        g1.SetActive(true);
    }
}
