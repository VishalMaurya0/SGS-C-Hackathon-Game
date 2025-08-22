using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.SceneManagement;
public class ChemicalSpawner3 : MonoBehaviour
{
    [Header("Prefab Settings")]
    public GameObject chemicalPrefabA; // follows mouse
    public GameObject chemicalPrefabB; // fixed spawn point
    public Vector3 prefabBSpawnPoint;
    public Camera mainCamera;
    public LayerMask placementLayer;
    public LayerMask burnerLayer;
    public LayerMask hlayer;
    public LayerMask Salt;
    public LayerMask Metal;
    public LayerMask BlackBoard;
    private GameObject currentChemical;
    private bool isPlacing = false;
    private bool spawnAtMouse = true;
    int i=0;
    public GameObject g1;
    public GameObject g2;

    public Transform tr;
    public static bool yes1;
    public static bool yes2;
    public static bool yes3;
    public static bool yes4;

    public GameObject g5;
    public GameObject pl1;
    public GameObject Methodology;
    public AudioClip myClip;
    public GameObject pl3;
    public AudioClip MyClip;

    void Start()
    {
      
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
            Debug.Log("yes");
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (i == 0)
            {
                if (Physics.Raycast(ray, out RaycastHit j, 100f, Salt))
                {
                    SpawnChemicalA();
                    pl3.SetActive(true);
                    Destroy(j.collider.gameObject);
                    AudioSource.PlayClipAtPoint(myClip, transform.position);
                }
                if (Physics.Raycast(ray, out RaycastHit hit, 100f, burnerLayer) && isPlacing && currentChemical != null)
                {
                    // snap to fixed burner point

                    currentChemical.transform.position = new Vector3(-0.4098189f, 3.182409f, -2.385961f);
                    i = 1;
                    pl1.SetActive(true);
                    pl3.SetActive(false);
                    currentChemical = null;
                    isPlacing = false;
                    AudioSource.PlayClipAtPoint(myClip, transform.position);
                }
            }
            else
            {
                if (Physics.Raycast(ray, out RaycastHit j, 100f, Metal))
                {
                    SpawnChemicalB();
                    pl3.SetActive(true);
                    Destroy(j.collider.gameObject);
                    AudioSource.PlayClipAtPoint(myClip, transform.position);
                }
                if (Physics.Raycast(ray, out RaycastHit hit, 100f, hlayer) && isPlacing && currentChemical != null)
                {
                    // snap to fixed burner point
             
                    currentChemical.transform.position = new Vector3(-0.4098189f, 5.182409f, -2.385961f);
                    currentChemical.AddComponent<Rigidbody>();
                    StartCoroutine(Owarida());
                    pl3.SetActive(false);
                    isPlacing = false;
                    AudioSource.PlayClipAtPoint(myClip, transform.position);
                }
            }
            if (Physics.Raycast(ray, out RaycastHit k, 100f, BlackBoard))
            {

                Methodology.SetActive(true);
                AudioSource.PlayClipAtPoint(myClip, transform.position);
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
       
    }

    // ---- Helpers ----
    private Vector3 GetMouseWorldPosition()
    {
        if (Physics.Raycast(mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue()), out RaycastHit hit, 100f, placementLayer | burnerLayer | BlackBoard))
        {
            return hit.point;
        }
        return Vector3.zero;
    }

    IEnumerator Owarida()
    {

        yield return new WaitForSeconds(1f);

        AudioSource.PlayClipAtPoint(MyClip, transform.position);
        yield return new WaitForSeconds(9f);
        g1.SetActive(false);
        g2.SetActive(true);
    }
    public void Load()
    {
        if (SceneManager.GetActiveScene().name == "Displacement1" && !yes1)
        {
            yes1 = true;
            MainMenui.reactions += 1;
        }
        if (SceneManager.GetActiveScene().name == "Displacement2" && !yes2)
        {
            yes2 = true;
            MainMenui.reactions += 1;
        }
        if (SceneManager.GetActiveScene().name == "Displacement3" && !yes3)
        {
            yes3 = true;
            MainMenui.reactions += 1;
        }
        if (SceneManager.GetActiveScene().name == "Displacement4" && !yes4)
        {
            yes4 = true;
            MainMenui.reactions += 1;
        }
        
        SceneManager.LoadScene("MainMenu");
    }
}
