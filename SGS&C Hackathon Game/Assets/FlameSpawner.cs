using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class FlameSpawner : MonoBehaviour
{
    [Header("Prefab Settings")]
    public GameObject chemicalPrefab;
    public Camera mainCamera;
    public LayerMask placementLayer;
    public LayerMask burnerLayer;
    public LayerMask BlackBoard;
    public LayerMask Tongs;

    private GameObject currentChemical;
    private bool isPlacing = false;
    private bool started = false;
    public GameObject g1;
    public GameObject g2;
    public GameObject g3;

    public GameObject g4;
    public GameObject g5;
    public GameObject Methodology;
    int click = 0;
    public GameObject pl1;
    public GameObject pl2;
    public Transform tr;
    public AudioClip myClip;
    public AudioClip MyClip;

    public TextMeshProUGUI feedback;
    bool ok;
    float timer=0f;
    void Start()
    {

    }
    public void SpawnChemical()
    {
        if (!isPlacing)
        {
            Vector3 spawnPos = GetMouseWorldPosition();
            Quaternion spawnRot = Quaternion.Euler(-90f, 0f, 0f);
            currentChemical = Instantiate(chemicalPrefab, spawnPos, spawnRot);
            isPlacing = true;
            pl1.SetActive(true);
        }


    }


    public void Heat()
    {
        Destroy(g3);
        g5.SetActive(true);
        StartCoroutine(Owarida());
        pl2.SetActive(false);
        

    }

    public void OnPlaceChemical(InputAction.CallbackContext context)
    {

        if (context.performed)
        {
            
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, 10f, burnerLayer) && isPlacing && currentChemical != null)
            {

                currentChemical.transform.position = new Vector3(-3.43f, 4.47f, -2.29f);


                pl1.SetActive(false);
                pl2.SetActive(true);
                isPlacing = false;
                AudioSource.PlayClipAtPoint(myClip, tr.position);
                



            }
            else if (Physics.Raycast(ray, out RaycastHit hitt, 10f, burnerLayer) && !isPlacing && currentChemical != null && click==0)
            {
                Heat();
                click += 1;
                AudioSource.PlayClipAtPoint(myClip, tr.position);
                ok = false;

            }
            if (Physics.Raycast(ray, out RaycastHit j, 10f, BlackBoard))
            {
                AudioSource.PlayClipAtPoint(myClip, tr.position);

                Methodology.SetActive(true);


            }
            if (Physics.Raycast(ray, out RaycastHit h, 100f, Tongs))
            {
                Destroy(h.collider.gameObject);
                SpawnChemical();
                ok = true;
                AudioSource.PlayClipAtPoint(myClip, tr.position);

            }



        }
    }

    void Update()
    {

        if (isPlacing)
        {
            currentChemical.transform.position = GetMouseWorldPosition();
        }
        if (ok)
        {
            timer += Time.deltaTime;
        }

    }

    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit y, 10f, burnerLayer)) Debug.Log("Maybe");
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, placementLayer | burnerLayer))
        {
            return hit.point;
        }
        return Vector3.zero;
    }
    IEnumerator Owarida()
    {
        started = true;
        currentChemical.GetComponent<flame>().yes();
        GameObject temp = new GameObject("TempAudio");
        AudioSource src = temp.AddComponent<AudioSource>();
        src.clip = MyClip;
        src.Play();
        yield return new WaitForSeconds(5f);

        yield return new WaitForSeconds(5f);
        g2.SetActive(true);
        g1.SetActive(false);
        Destroy(currentChemical);
        Destroy(g5);
        Destroy(temp);
        if (timer > 5f)
        {
            feedback.text = "Feedback: Since these metals are highly reactive, try not to expose them in air for long time.";

        }
        else
        {
            feedback.text = "Feedback: Experiment performed accurately.";
        }
    }
    public void Mainmenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
