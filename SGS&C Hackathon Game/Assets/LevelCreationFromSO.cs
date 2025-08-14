using System.Collections.Generic;
using System.Xml.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

public class LevelCreationFromSO : CircuitCreation
{
    public LevelSaveSO LevelSaveSO;
    public GateTypesSO GateTypesSO;
    public GameObject cellParent;
    public GameObject image;
    public GameObject wireGameobject;
    public TMP_Text textPrefab;
    public GameObject gatePrefab;
    public GameObject inputContainer;

    [Header("Level Dependent")]
    public List<List<Cell>> gridCells;
    public List<GateOption> gateOptions = new();
    public float cellSize;
    public List<Button> gateOptioonButtons = new();
    public List<Image> gateOptionButtonImages = new();


    [Header("Level Save System")]
    public List<Cell> inputs = new();


    private void Start()
    {
        // Calculate cellSize using the parent RectTransform
        RectTransform parentRect = cellParent.GetComponent<RectTransform>();
        float parentWidth = parentRect.rect.width;
        float parentHeight = parentRect.rect.height;

        float cellWidth = parentWidth / LevelSaveSO.gridCells[0].Count;
        float cellHeight = parentHeight / LevelSaveSO.gridCells.Count;

        // Use the smaller one to ensure it fits both directions
        cellSize = Mathf.Min(cellWidth, cellHeight);

        gridCells = new List<List<Cell>>();
        for (int i = 0; i < LevelSaveSO.gridCells.Count; i++)
        {
            gridCells.Add(new List<Cell>());
            for (int j = 0; j < LevelSaveSO.gridCells[0].Count; j++)
            {
                gridCells[i].Add(new Cell(0));
            }
        }
        for (int i = 0; i < LevelSaveSO.gateOptions.Count; i++)
        {
            gateOptions.Add(new GateOption());
            gateOptions[i].gateType = LevelSaveSO.gateOptions[i].gateType;
            gateOptions[i].amount = LevelSaveSO.gateOptions[i].amount;
        }

        InitializeGates();

        ReferenceEachCell();
        InstantiateCells();
        InitializeInputCells();
    }

    private void InitializeInputCells()
    {
        for (int i = 0; i < gridCells.Count; i++)
        {

            GameObject cellImage = Instantiate(image, GetInputPos(i), Quaternion.identity, inputContainer.transform);
            inputs.Add(new Cell(0));
            Cell cell = inputs[i];
            cell.image = cellImage;
            cell.button = cellImage.AddComponent<Button>();
            cell.isSource = true;

            //intantiate text
            cell.text = cell.image.GetComponentInChildren<TMP_Text>();
            cell.text.text = "0";

            // Set the size of the RectTransform
            RectTransform rect = cell.image.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(cellSize - cellSize / 5, cellSize - cellSize / 5);


            // Attach CellBehaviour
            var behaviour = cellImage.AddComponent<CellBehaviour>();
            behaviour.Initialize(cell, this);
        }
    }

    private Vector3 GetInputPos(int i)
    {
        RectTransform parentRect = inputContainer.GetComponent<RectTransform>();
        Vector3 parentCenter = inputContainer.transform.position;

        float totalHeight = CircuitGeneratorSO.rows * cellSize;

        float startY = totalHeight / 2 - cellSize / 2;

        float y = startY - i * cellSize;

        return parentCenter + new Vector3(0f, y, 0f);
    }

    private void InitializeGates()
    {
        for (int i = 0; i < gateOptions.Count; i++)
        {
            GameObject gate = Instantiate(gatePrefab, gatePrefab.transform.parent.transform);
            gate.SetActive(true);
            gateOptioonButtons.Add(gate.AddComponent<Button>());
            gateOptionButtonImages.Add(gate.GetComponent<Image>());
            gates gateType = gateOptions[i].gateType;
            int a = i;
            gateOptioonButtons[i].onClick.AddListener(() => { GateOptionButtonClicked(gateType, a); });
        }
    }

    private void GateOptionButtonClicked(gates gateType, int i)
    {
        for (int j = 0; j < gateOptioonButtons.Count; j++)
        {
            gateOptionButtonImages[j].color = normalcolor;
        }
        if (gateMode && selectedGateType != gateType)
        {
            selectedGateType = gateType;
            selectedGateIndex = i;
            gateMode = true;
            gateOptioonButtons[i].GetComponent<Image>().color = clickedcolor;
            return;
        }

        if (gateMode && selectedGateType == gateType)
        {
            gateMode = false;
            gateOptioonButtons[i].GetComponent<Image>().color = normalcolor;
            return;
        }

        if (!gateMode)
        {
            selectedGateType = gateType;
            gateMode = true;
            gateOptioonButtons[i].GetComponent<Image>().color = clickedcolor;
            return;
        }
    }

    private void InstantiateCells()
    {
        for (int i = 0; i < gridCells.Count; i++)
        {
            for (int j = 0; j < gridCells[i].Count; j++)
            {
                GameObject cellImage = Instantiate(image, GetPosition(cellParent, i, j), Quaternion.identity, cellParent.transform);
                Cell cell = gridCells[i][j];
                cell.image = cellImage;
                cell.button = cellImage.AddComponent<Button>();

                //intantiate text
                cell.text = cell.image.GetComponentInChildren<TMP_Text>();
                cell.text.text = "0";

                // Set the size of the RectTransform
                RectTransform rect = cell.image.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(cellSize - cellSize / 5, cellSize - cellSize / 5);


                // Attach CellBehaviour
                var behaviour = cellImage.AddComponent<CellBehaviour>();
                behaviour.Initialize(cell, this);
            }
        }

        //gridCells[0][0].isSource = true;
    }
    private void ReferenceEachCell()
    {
        for (int i = 0; i < gridCells.Count; i++)
        {
            for (int j = 0; j < gridCells[i].Count; j++)
            {
                Cell cell = gridCells[i][j];
                if (cell != null)
                {
                    // Right (adjcell[0])
                    if (j + 1 < gridCells[i].Count && gridCells[i][j + 1] != null)
                    {
                        cell.adjcell[0] = gridCells[i][j + 1];
                    }

                    // Up (adjcell[1])
                    if (i - 1 >= 0 && gridCells[i - 1][j] != null)
                    {
                        cell.adjcell[1] = gridCells[i - 1][j];
                    }

                    // Left (adjcell[2])
                    if (j - 1 >= 0 && gridCells[i][j - 1] != null)
                    {
                        cell.adjcell[2] = gridCells[i][j - 1];
                    }

                    // Down (adjcell[3])
                    if (i + 1 < gridCells.Count && gridCells[i + 1][j] != null)
                    {
                        cell.adjcell[3] = gridCells[i + 1][j];
                    }
                }
            }
        }
    }
    private Vector3 GetPosition(GameObject cellParent, int i, int j)
    {
        RectTransform parentRect = cellParent.GetComponent<RectTransform>();
        Vector3 parentCenter = cellParent.transform.position;

        float totalWidth = CircuitGeneratorSO.columns * cellSize;
        float totalHeight = CircuitGeneratorSO.rows * cellSize;

        float startX = -totalWidth / 2 + cellSize / 2;
        float startY = totalHeight / 2 - cellSize / 2;

        float x = startX + j * cellSize;
        float y = startY - i * cellSize;

        return parentCenter + new Vector3(x, y, 0f);
    }

}
