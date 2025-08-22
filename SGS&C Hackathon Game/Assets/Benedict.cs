using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class Benedict : MonoBehaviour
{
    [Header("Prefab Settings")]
 
    public Camera mainCamera;
    public LayerMask placementLayer;
    public LayerMask burnerLayer;
    public LayerMask hlayer;
    public LayerMask BlackBoard;
    public LayerMask Benedictt;
    public LayerMask Sugar;
    int i = 0;
    public static bool yes = false;
    [Header("Temperature Settings")]
    public TextMeshProUGUI temperatureText;
    public float temperatureC = 30f;
    public float temperatureIncreaseStep = 5f;
    public TextMeshProUGUI feedback;
    private GameObject currentChemical;
    private bool isPlacing = false;
    private bool started = false;
    public GameObject g1;
    public GameObject g2;

    public Slider slider;
    public GameObject g4;
    public GameObject g5;
    bool spawnAtMouse;
    public GameObject chemicalPrefabB;
    public GameObject chemicalPrefabA;
    public GameObject pl1;
    public GameObject pl3;
    public GameObject Methodology;
    public AudioClip myClip;
    public AudioClip MyClip;
    GameObject temp;

    public void SpawnChemicalA()
    {
        SpawnChemical(chemicalPrefabA, true);

    }

    public void SpawnChemicalB()
    {
        SpawnChemical(chemicalPrefabB, true);


    }

    // ---- Core Spawn Method ----
    private void SpawnChemical(GameObject prefab, bool mouseSpawn)
    {
        
        if (!isPlacing && prefab != null)
        {
            spawnAtMouse = mouseSpawn;
            Vector3 spawnPos =GetMouseWorldPosition();
            Quaternion spawnRot = Quaternion.Euler(-90f, 0f, 0f);
            currentChemical = Instantiate(prefab, spawnPos, spawnRot);

            isPlacing = true;
        }
    }
    void Start()
    {
        slider.maxValue = 120f;
        slider.minValue = 0f;

    }
    public void OnPlaceChemical(InputAction.CallbackContext context)
    {

        if (context.performed)
        {
            Debug.Log("yes");
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (i == 0)
            {
                if (Physics.Raycast(ray, out RaycastHit j, 100f, Benedictt))
                {
                    AudioSource.PlayClipAtPoint(myClip, transform.position);
                    SpawnChemicalA();
                    pl3.SetActive(true);
                    Destroy(j.collider.gameObject);
                }
                if (Physics.Raycast(ray, out RaycastHit hit, 100f, burnerLayer) && isPlacing && currentChemical != null)
                {
                    // snap to fixed burner point
                    AudioSource.PlayClipAtPoint(myClip, transform.position);
                    currentChemical.transform.position = new Vector3(-3.43f, 4.77f, -2.29f);
                    i = 1;
                    pl1.SetActive(true);
                    pl3.SetActive(false);
                    currentChemical = null;
                    isPlacing = false;
                }
            }
            else
            {
                if (Physics.Raycast(ray, out RaycastHit j, 100f, Sugar))
                {
                    AudioSource.PlayClipAtPoint(myClip, transform.position);
                    SpawnChemicalB();
                    pl3.SetActive(true);
                    Destroy(j.collider.gameObject);
                }
                if (Physics.Raycast(ray, out RaycastHit hit, 100f, hlayer) && isPlacing && currentChemical != null)
                {
                    // snap to fixed burner point
                    AudioSource.PlayClipAtPoint(myClip, transform.position);
                    currentChemical.transform.position = new Vector3(-3.43f, 5f, -2.29f);
                    currentChemical.AddComponent<Rigidbody>();
                    StartCoroutine(Owarida());
                    pl3.SetActive(true);
                    isPlacing = false;
                    g5.SetActive(true);
                    g4.SetActive(true);
                }
            }
            if (Physics.Raycast(ray, out RaycastHit k, 100f, BlackBoard))
            {
                AudioSource.PlayClipAtPoint(myClip, transform.position);
                Methodology.SetActive(true);
            }
            if (Physics.Raycast(ray, out RaycastHit l, 100f, burnerLayer) && i==1)
            {
                AudioSource.PlayClipAtPoint(myClip, transform.position);
                IncreaseTemperature();
            }

        }
    }


    public void IncreaseTemperature()
    {
        if (temperatureC >= 85) return;
        temperatureC += temperatureIncreaseStep;
        float tempF = temperatureC * 9f / 5f + 32f;
        temperatureText.text = $"{temperatureC:F1}°C / {tempF:F1}°F";
    }

    

    void Update()
    {
        slider.value = temperatureC;
        if (isPlacing && currentChemical != null)
        {
            currentChemical.transform.position = GetMouseWorldPosition();
        }
        if (temperatureC >= 60 && !started)
        {
            StartCoroutine(Owarida());
        }
        
    }

    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit y, 100f, burnerLayer)) Debug.Log("Maybe");
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, placementLayer | burnerLayer))
        {
            return hit.point;
        }
        return Vector3.zero;
    }
    IEnumerator Owarida()
    {
        started = true;

        yield return new WaitForSeconds(2f);
        currentChemical.GetComponent<sugar>().yes();
        yield return new WaitForSeconds(10f);
        g5.SetActive(false);
        yield return new WaitForSeconds(3f);
        g1.SetActive(false);
        g2.SetActive(true);
        Destroy(temp);
        if (temperatureC > 75f)
        {
            feedback.text = "Feedback: Try to maintain the temperature closer to 65C";
        }
        else
        {
            feedback.text = "Feedback: Experiment performed accurately";
        }
        if (!yes)
        {
            yes = true;
            MainMenui.reactions += 1;
        }
    }

    public void Load()
    {
        
        SceneManager.LoadScene("MainMenu");
    }
}
