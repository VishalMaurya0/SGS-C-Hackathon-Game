using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
public class Benedict : MonoBehaviour
{
    [Header("Prefab Settings")]
    public GameObject chemicalPrefab;
    public Camera mainCamera;
    public LayerMask placementLayer;
    public LayerMask burnerLayer;
    public LayerMask hlayer;
    int i = 0;
    [Header("Temperature Settings")]
    public TextMeshProUGUI temperatureText;
    public float temperatureC = 30f;
    public float temperatureIncreaseStep = 5f;

    private GameObject currentChemical;
    private bool isPlacing = false;
    private bool started = false;
    public GameObject g1;
    public GameObject g2;
    public GameObject g3;
    public Slider slider;
    public GameObject g4;
    public GameObject g5;
    bool spawnAtMouse;
    public GameObject chemicalPrefabB;
    public GameObject chemicalPrefabA;

    public void SpawnChemicalA()
    {
        SpawnChemical(chemicalPrefabA, true);
        i = 0;
    }

    public void SpawnChemicalB()
    {
        SpawnChemical(chemicalPrefabB, true);

        i = 1;
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
        if (context.performed && isPlacing && currentChemical != null)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (i == 0)
            {
                if (Physics.Raycast(ray, out RaycastHit hit, 100f, burnerLayer))
                {
                    // snap to fixed burner point

                    currentChemical.transform.position = new Vector3(-3.43f, 4.77f, -2.29f);
                    g4.SetActive(true);

                    currentChemical = null;
                    isPlacing = false;
                }
            }
            else
            {
                if (Physics.Raycast(ray, out RaycastHit hit, 100f, hlayer))
                {
                    // snap to fixed burner point

                    currentChemical.transform.position = new Vector3(-3.43f, 5.77f, -2.29f);
                    currentChemical.AddComponent<Rigidbody>();

                    g5.SetActive(true);
                    g3.SetActive(true);
                    isPlacing = false;
                }
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

    }
}
