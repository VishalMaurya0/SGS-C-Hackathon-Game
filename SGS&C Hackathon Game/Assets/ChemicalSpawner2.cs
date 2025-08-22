using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;
public class ChemicalSpawner2 : MonoBehaviour
{
    [Header("Prefab Settings")]
    public GameObject chemicalPrefabA; // follows mouse
    public GameObject chemicalPrefabB; // fixed spawn point
    public Vector3 prefabBSpawnPoint;
    public Camera mainCamera;
    public LayerMask placementLayer;
    public LayerMask burnerLayer;
    public LayerMask hlayer;
    public LayerMask Water;
    public LayerMask sodium;
    public LayerMask BlackBoard;
    private GameObject currentChemical;
    private bool isPlacing = false;
    private bool spawnAtMouse = true;
    int i = 0;
    public static bool yes = false;
    public TextMeshProUGUI feedback;
    public GameObject g1;
    public GameObject g2;
    public GameObject g3;
    public GameObject g4;
    public GameObject g5;
    public GameObject Methodology;
    public GameObject pl1;
    public AudioClip MyClip;
    public GameObject pl3;
    public float timer = 0f;
    bool ok = false;
    public AudioClip myClip;

    void Start()
    {
        // no temperature or slider setup needed
    }

    // ---- UI Button Hooks ----
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
            Vector3 spawnPos = mouseSpawn ? GetMouseWorldPosition() : prefabBSpawnPoint;
            Quaternion spawnRot = Quaternion.Euler(-90f, 0f, 0f);
            currentChemical = Instantiate(prefab, spawnPos, spawnRot);
           
            isPlacing = true;
        }
    }

    // ---- Placement ----
    public void OnPlaceChemical(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (i == 0)
            {
                if (Physics.Raycast(ray, out RaycastHit j, 100f, Water))
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
                    currentChemical.transform.position = new Vector3(-0.4098189f, 3.182409f, -2.385961f);
                    i = 1;
                    pl1.SetActive(true);
                    pl3.SetActive(false);
                    currentChemical = null;
                    isPlacing = false;
                }
            }
            else
            {
                if (Physics.Raycast(ray, out RaycastHit j, 100f, sodium))
                {
                    AudioSource.PlayClipAtPoint(myClip, transform.position);
                    SpawnChemicalB();
                    pl3.SetActive(true);
                    Destroy(j.collider.gameObject);
                    ok = true;
                }
                if (Physics.Raycast(ray, out RaycastHit hit, 100f, hlayer) && isPlacing && currentChemical != null)
                {
                    // snap to fixed burner point
                    AudioSource.PlayClipAtPoint(myClip, transform.position);
                    currentChemical.transform.position = new Vector3(-0.4098189f, 5.182409f, -2.385961f);
                    currentChemical.AddComponent<Rigidbody>();
                    StartCoroutine(Owarida());
                    pl3.SetActive(false);
                    isPlacing = false;
                    ok = false;
                }
            }
            if (Physics.Raycast(ray, out RaycastHit k, 100f, BlackBoard))
            {
                AudioSource.PlayClipAtPoint(myClip, transform.position);
                Methodology.SetActive(true);
            }
        }
    }

    void Update()
    {
        // follow mouse only if spawnAtMouse == true
        if (isPlacing && spawnAtMouse)
        {
            currentChemical.transform.position = GetMouseWorldPosition();
        }
        if (ok)
        {
            timer += Time.deltaTime;
        }
    }

    // ---- Helpers ----
    private Vector3 GetMouseWorldPosition()
    {
        if (Physics.Raycast(mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue()), out RaycastHit hit, 100f, placementLayer | burnerLayer))
        {
            return hit.point;
        }
        return Vector3.zero;
    }

    IEnumerator Owarida()
    {

        yield return new WaitForSeconds(1f);
        GameObject temp = new GameObject("TempAudio");
        AudioSource src = temp.AddComponent<AudioSource>();
        src.clip = MyClip;
        src.Play();
        yield return new WaitForSeconds(9f);
        if (timer > 5f)
        {
            feedback.text = "Feedback: Since sodium reacts with air, try not to expose sodium for long time.";

        }
        else
        {
            feedback.text = "Feedback: Experiment performed accurately.";
        }
            Destroy(currentChemical);
        g1.SetActive(false);
        g2.SetActive(true);
        Destroy(temp);
    }
    public void Load()
    {
        if (!yes)
        {
            yes = true;
            MainMenui.reactions += 1;
        }
        SceneManager.LoadScene("MainMenu");
    }

}
