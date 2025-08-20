using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class ChemicalSpawner : MonoBehaviour
{
    [Header("Prefab Settings")]
    public GameObject chemicalPrefab;
    public Camera mainCamera;
    public LayerMask placementLayer;
    public LayerMask burnerLayer;
    public LayerMask Water;
    public LayerMask BlackBoard;
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
    public GameObject Methodology;
    [Header("Steam Settings")]
    public ParticleSystem steamParticles;
    public float baseEmissionRate = 5f;     // gentle steam at low heat
    public float maxEmissionRate = 50f;     // heavy steam at boiling
    public float boilPointC = 100f;         // start maxing here
    public GameObject pl3;
 
    void UpdateSteamEffect()
    {
        var emission = steamParticles.emission;

        // Clamp temperature so it doesn't go above boiling effect
        float t = Mathf.Clamp01(temperatureC / boilPointC);

        // Lerp from base rate to max rate
        emission.rateOverTime = Mathf.Lerp(baseEmissionRate, maxEmissionRate, t);
    }
        void Start()
    {
        slider.maxValue = 200f;
        slider.minValue = 0f;
        
    }
    public void SpawnChemical()
    {
        if (!isPlacing)
        {
            Vector3 spawnPos = GetMouseWorldPosition();
            Quaternion spawnRot = Quaternion.Euler(-90f, 0f, 0f);
            currentChemical = Instantiate(chemicalPrefab, spawnPos, spawnRot);
            isPlacing = true;
        }

        IncreaseTemperature(); 
    }

 
    public void IncreaseTemperature()
    {
        temperatureC += temperatureIncreaseStep;
        float tempF = temperatureC * 9f / 5f + 32f;
        temperatureText.text = $"{temperatureC:F1}°C / {tempF:F1}°F";
    }

    public void OnPlaceChemical(InputAction.CallbackContext context) { 
    if (context.performed)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            
                if (Physics.Raycast(ray, out RaycastHit j, 100f, Water))
                {
                SpawnChemical();
    pl3.SetActive(true);
                    Destroy(j.collider.gameObject);
}
if (Physics.Raycast(ray, out RaycastHit hit, 100f, burnerLayer) && isPlacing && currentChemical != null)
{
                // snap to fixed burner point

                currentChemical.transform.position = new Vector3(-3.43f, 4.77f, -2.29f);
                g4.SetActive(true);
                g5.SetActive(true);
                currentChemical = null;
                isPlacing = false;
            }
         

if (Physics.Raycast(ray, out RaycastHit k, 100f, BlackBoard))
{

    Methodology.SetActive(true);
}
            if(Physics.Raycast(ray, out RaycastHit l, 100f, burnerLayer) && currentChemical == null){
                IncreaseTemperature();
            }
        }
    }

    void Update()
    {
        slider.value = temperatureC;
        if (isPlacing && currentChemical != null)
        {
            currentChemical.transform.position = GetMouseWorldPosition();
        }
        if (temperatureC >= 100 && !started)
        {
            StartCoroutine(Owarida());
        }
        if (!isPlacing)
        {
            UpdateSteamEffect();
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
        g3 = GameObject.FindGameObjectWithTag("Sphere");
        yield return new WaitForSeconds(5f);
        g3.SetActive(false);
        yield return new WaitForSeconds(5f);
        g1.SetActive(false);
        g2.SetActive(true);

    }
    public void Load()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
