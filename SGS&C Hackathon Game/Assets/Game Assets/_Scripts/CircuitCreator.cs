using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CircuitCreator : MonoBehaviour
{
    public CircuitGeneratorSO CircuitGeneratorSO;
    public GateTypesSO GateTypesSO;
    public GameObject cellParent;
    public GameObject image;
    public GameObject wireGameobject;
    public TMP_Text textPrefab;
    public GameObject gatePrefab;

    [Header("Properties")]
    public bool simulate;
    public int frameCounter = 0;

    [Header("Level Dependent")]
    public List<List<Cell>> gridCells;
    public List<GateOption> gateOptions = new();
    public float cellSize;
    public List<Button> gateOptioonButtons = new();
    public List<Image> gateOptionButtonImages = new();

    [Header("Click &  Drag Behaviour Making WIRE")]
    public bool isDragging = false;
    public Cell currentStartCell;
    public Cell currentEndCell;
    private GameObject currentWire;
    public RectTransform canvasTransform;
    public Vector2 startPos;
    public Vector2 endPos;
    public int currentDir;

    [Header("Click Behaviour Making Gates")]
    public bool gateMode;
    public gates selectedGateType;
    public int selectedGateIndex;
    public Color clickedcolor;
    public Color normalcolor;

    [Header("For Slower Update")]
    private float updateInterval = 0f; // seconds
    private float timer = 0f;




    private void Start()
    {
        // Calculate cellSize using the parent RectTransform
        RectTransform parentRect = cellParent.GetComponent<RectTransform>();
        float parentWidth = parentRect.rect.width;
        float parentHeight = parentRect.rect.height;

        float cellWidth = parentWidth / CircuitGeneratorSO.columns_MakeItOdd;
        float cellHeight = parentHeight / CircuitGeneratorSO.rows;

        // Use the smaller one to ensure it fits both directions
        cellSize = Mathf.Min(cellWidth, cellHeight);

        gridCells = new List<List<Cell>>();
        for (int i = 0; i < CircuitGeneratorSO.rows; i++)
        {
            gridCells.Add(new List<Cell>());
            for (int j = 0; j < CircuitGeneratorSO.columns_MakeItOdd; j++)
            {
                gridCells[i].Add(new Cell(0));
            }
        }
        for (int i = 0; i < CircuitGeneratorSO.gateOptions.Count; i++)
        {
            gateOptions.Add(new GateOption());
            gateOptions[i].gateType = CircuitGeneratorSO.gateOptions[i].gateType;
            gateOptions[i].amount = CircuitGeneratorSO.gateOptions[i].amount;
        }

        InitializeGates();

        ReferenceEachCell();
        InstantiateCells();
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

        gridCells[0][0].isSource = true;
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

        float totalWidth = CircuitGeneratorSO.columns_MakeItOdd * cellSize;
        float totalHeight = CircuitGeneratorSO.rows * cellSize;

        float startX = -totalWidth / 2 + cellSize / 2;
        float startY = totalHeight / 2 - cellSize / 2;

        float x = startX + j * cellSize;
        float y = startY - i * cellSize;

        return parentCenter + new Vector3(x, y, 0f);
    }






    void Update()
    {
        if (isDragging && currentWire != null && currentStartCell != null)
        {
            StartLineAndUpdation();
            HandleLineEndAndNewLine();
        }

        CheckEachCellForNullSaves();

        // Only run simulation every 3rd frame
        frameCounter++;
        if (frameCounter % 3 == 0)
        {
            if (simulate)
            {
                HandleOnOffStateOfEachCell();
            }
            HandleVisual();
        }
    }


    private void HandleVisual()
    {
        for (int i = 0; i < gridCells.Count; i++)
        {
            for (int j = 0; j < gridCells[i].Count; j++)
            {
                Cell cell = gridCells[i][j];

                if (cell.value != 0)
                {
                    cell.image.GetComponent<Image>().color = Color.red;
                }
                else
                {
                    cell.image.GetComponent<Image>().color = Color.grey;
                }
                cell.text.text = $"{cell.value}";
            }
        }
    }

    private void StartLineAndUpdation()
    {
        startPos = currentStartCell.image.GetComponent<RectTransform>().position;

        // Convert screen point to world point for canvas
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasTransform,
            Input.mousePosition,
            null, // If you're using screen space - overlay
            out Vector2 localMousePos
        );

        endPos = canvasTransform.TransformPoint(localMousePos);

        UpdateLine();
    }
    private void HandleLineEndAndNewLine()
    {
        if ((endPos - startPos).magnitude >= cellSize * 0.8)
        {
            if (currentEndCell == null || currentStartCell.connection[currentDir] || currentEndCell.connection[(currentDir + 2) % 4])
            {
                Destroy(currentWire);
                return;
            }

            if (currentEndCell != null && !currentEndCell.currentWires.Contains(currentWire))
                currentEndCell.currentWires.Add(currentWire);

            // Save Wire in cells

            currentStartCell.connection[currentDir] = true;
            currentEndCell.connection[(currentDir + 2) % 4] = true;


            endPos = currentEndCell.image.GetComponent<RectTransform>().position;
            UpdateLine();


            CheckIfConnectionsAreGood(currentEndCell, (currentDir + 2) % 4);
            CheckIfConnectionsAreGood(currentStartCell, currentDir);

            StartWire(currentEndCell);
        }

    }
    private void CheckEachCellForNullSaves()
    {
        for (int i = 0; i < gridCells.Count; i++)
        {
            for (int j = 0; j < gridCells[i].Count; j++)
            {
                Cell cell = gridCells[i][j];

                for (int k = cell.currentWires.Count - 1; k >= 0; k--)
                {
                    if (cell.currentWires[k] == null)
                    {
                        cell.currentWires.RemoveAt(k);
                    }
                }
            }
        }
    }
    private void HandleOnOffStateOfEachCell()
    {
        for (int i = 0; i < gridCells.Count; i++)
        {
            for (int j = 0; j < gridCells[i].Count; j++)
            {
                Cell cell = gridCells[i][j];

                if (cell.value == 0)               //if cell is not on
                {
                    if (cell.isSource)
                    {
                        cell.value = 100;
                        continue;
                    }                         //not source
                    for (int k = 0; k < cell.powerDir.Count; k++)
                    {
                        if (cell.powerDir[k] == -1) continue;          // and also giving power not taking


                        Cell neighbor = cell.adjcell[k];
                        if (neighbor != null && !neighbor.isGate)
                        {
                            cell.powerDir[k] = 0;                      // then remove
                            neighbor.powerDir[(k + 2) % 4] = 0;
                        }
                    }
                }

                if (cell.isSource) continue;

                bool receivingPower = false;              // if cell is not source
                int value = 0;
                for (int k = 0; k < cell.powerDir.Count; k++)
                {
                    if (cell.powerDir[k] == -1 && cell.adjcell[k].value != 0 && cell.value < cell.adjcell[k].value)            // not getting power from a turned on cell higher than that
                    {
                        receivingPower = true;
                        value = cell.adjcell[k].value - 1;
                        break;
                    }
                    else if (cell.powerDir[k] == -1 && cell.adjcell[k].value != 0 && cell.value >= cell.adjcell[k].value)
                    {
                        cell.powerDir[k] = 0;
                        if (!cell.adjcell[k].isGate)
                            cell.adjcell[k].powerDir[(k + 2) % 4] = 0;
                    }
                }

                if (!receivingPower)
                {
                    cell.value = 0;
                }
                else
                {
                    cell.value = value;
                }

                for (int k = 0; k < 4; k++)  //now check if new connection is made to give power
                {
                    if (cell.connection[k] && cell.adjcell[k].value != 0 && cell.adjcell[k].value > cell.value && cell.powerDir[k] != 1 && !cell.adjcell[k].isGate)
                    {
                        cell.powerDir[k] = -1;
                        cell.adjcell[k].powerDir[(k + 2) % 4] = 1;
                        cell.value = cell.adjcell[k].value - 1;
                    }
                    if (cell.connection[k] && cell.adjcell[k].isGate && (cell.adjcell[k].outputDir != (k + 2) % 4))
                    {
                        cell.powerDir[k] = 1;
                        cell.adjcell[k].powerDir[(k + 2) % 4] = -1;
                    }
                    if (cell.connection[k] && cell.adjcell[k].isGate && (cell.adjcell[k].outputDir == (k + 2) % 4))
                    {
                        cell.powerDir[k] = -1;
                        cell.adjcell[k].powerDir[(k + 2) % 4] = 1;
                        cell.value = cell.adjcell[k].value;
                    }
                    if (cell.isGate)
                    {
                        HandleGateProperties(cell);
                    }
                }
            }
        }
    }

    public void UpdateLine()
    {
        if (currentWire == null) return;

        RectTransform rect = currentWire.GetComponent<RectTransform>();

        Vector2 delta = endPos - startPos;

        // ===== Snap
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            // Horizontal line
            endPos = new Vector2(endPos.x, startPos.y);
            currentEndCell = currentStartCell.adjcell[delta.x > 0 ? 0 : 2];
            currentDir = delta.x > 0 ? 0 : 2;
        }
        else
        {
            // Vertical line
            endPos = new Vector2(startPos.x, endPos.y);
            currentEndCell = currentStartCell.adjcell[delta.y > 0 ? 1 : 3];
            currentDir = delta.y > 0 ? 1 : 3;
        }

        Vector2 direction = endPos - startPos;
        float distance = direction.magnitude;

        rect.sizeDelta = new Vector2(distance, 15f);
        rect.pivot = new Vector2(0, 0.5f);
        rect.position = startPos;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rect.rotation = Quaternion.Euler(0, 0, angle);
    }




    //==============Handle Clicks=============//
    internal void StartWire(Cell cell)
    {
        if (cell == null || wireGameobject == null || cellParent == null)
        {
            return;
        }

        currentWire = Instantiate(wireGameobject, cellParent.transform);
        currentStartCell = cell;
        currentEndCell = null;
        endPos = startPos;


        if (!cell.currentWires.Contains(currentWire))
        {
            cell.currentWires.Add(currentWire);
        }
    }
    internal void EndWire()
    {
        if ((endPos - startPos).magnitude < cellSize)
        {
            Destroy(currentWire);
            currentStartCell = null;
            currentEndCell = null;
        }
    }
    internal void HandleRightClick(Cell cell)
    {
        Vector2 cellPos = cell.image.GetComponent<RectTransform>().position;

        // Convert mouse to world position relative to canvas
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasTransform,
            Input.mousePosition,
            null, // Assuming Screen Space - Overlay
            out Vector2 localMousePos
        );

        Vector2 worldMousePos = canvasTransform.TransformPoint(localMousePos);
        Vector2 delta = worldMousePos - (Vector2)cellPos;

        int dir;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            dir = delta.x > 0 ? 0 : 2;
        }
        else
        {
            dir = delta.y > 0 ? 1 : 3;
        }

        RemoveConnection(cell, dir);


    }
    private void RemoveConnection(Cell cell, int dir)
    {
        Cell adj = cell.adjcell[dir];
        if (adj != null && cell.connection[dir])
        {
            cell.connection[dir] = false;
            adj.connection[(dir + 2) % 4] = false;

            cell.powerDir[dir] = 0;
            adj.powerDir[(dir + 2) % 4] = 0;

            // Destroy wire between them
            foreach (var wire in cell.currentWires.ToArray())
            {
                if (adj.currentWires.Contains(wire))
                {
                    cell.currentWires.Remove(wire);
                    adj.currentWires.Remove(wire);
                    if (wire != null) Destroy(wire);
                }
            }
        }
    }
    internal void MakeGate(Cell cell)
    {
        if (cell.isGate)
        {
            cell.outputDir++;
            cell.outputDir %= 4;
            cell.gateGameobject.transform.eulerAngles = cell.gateGameobject.transform.eulerAngles + new Vector3(0, 0, 90);
            CheckIfConnectionsAreGood(cell);
            return;
        }
        if (gateOptions[selectedGateIndex].amount <= 0)
        {
            return;
        }
        if (!cell.isGate)
        {
            cell.isGate = true;
            cell.gate = selectedGateType;
            cell.outputDir = 0;
            cell.noOfInputs = GetGateBehaviour(selectedGateType).noOfInputs;
            gateOptions[selectedGateIndex].amount--;
            cell.gateGameobject = Instantiate(GetGateBehaviour(selectedGateType).prefab, cell.image.transform);
            CheckIfConnectionsAreGood(cell);
        }
    }
    internal void RemoveGate(Cell cell)
    {
        if (!cell.isGate) return;
        cell.isGate = false;
        cell.noOfInputs = 4;
        Destroy(cell.gateGameobject);
        for (int i = 0; i < gateOptions.Count; i++)
        {
            if (gateOptions[i].gateType == cell.gate)
            {
                gateOptions[i].amount++;
                break;
            }
        }
    }







    private void CheckIfConnectionsAreGood(Cell cell, int preferencedDir = 0)
    {
        if (cell == null) return;
        int totalInputs = 0;
        for (int i = preferencedDir; i < preferencedDir + 4; i++)
        {
            int index = i % 4;
            if (cell.connection[index] && cell.outputDir != index && totalInputs < cell.noOfInputs)
            {
                totalInputs++;
            }
            else if (cell.connection[index] && cell.outputDir != index && totalInputs >= cell.noOfInputs)
            {
                RemoveConnection(cell, index);
            }
        }

        HandleGateProperties(cell);
    }
    private void HandleGateProperties(Cell cell)
    {
        if (!cell.isGate) return;

        if (cell.gate == gates.not)
        {
            NotGateFuntion(cell);
        }
        
        if (cell.gate == gates.and)
        {
            AndGateFuntion(cell);
        }
        
    
        void NotGateFuntion(Cell cell)
        {
            int val = 0;
            for (int i = 0; i < 4; i++)
            {
                if (cell.connection[i] && i != cell.outputDir && cell.adjcell[i].value < 1)
                {
                    val = 50;
                }
            }

            if (val > 0)
            {
                cell.value = val;
            }else
            {
                cell.value = 0;
            }
        }

        void AndGateFuntion(Cell cell)
        {
            int val = 0;
            for (int i = 0; i < 4; i++)
            {
                if (cell.connection[i] && i != cell.outputDir && cell.adjcell[i].value > 0)
                {
                    val++;
                }
            }

            if (val >= 2)
            {
                cell.value = 50;
            }
            else
            {
                cell.value = 0;
            }
        }
    }


    public GateBehaviour GetGateBehaviour(gates gateType)
    {
        for (int i = 0; i < GateTypesSO.gates.Count; i++)
        {
            if (gateType == GateTypesSO.gates[i].gateType)
            {
                return GateTypesSO.gates[i];
            }
        }
        return null;
    }

}

public class Cell
{
    //Values
    public int value;
    public bool isSource;
    public List<bool> connection; // 0-noConnection, 1-connection
    public List<int> powerDir; // 0-noConnection, 1-fromThisCell, -1-toThisCell
    public List<GameObject> currentWires;

    //gates
    public gates gate;
    public bool isGate;
    public int noOfInputs = 4;
    public int outputDir;
    public GameObject gateGameobject;

    //References
    public GameObject image;
    public TMP_Text text;
    public List<Cell> adjcell; // 0-right, 1-up...
    public Button button;

    //Register Clicking
    public bool clickStarted = false;

    public Cell(int value)
    {
        this.value = value;
        adjcell = new List<Cell>();
        connection = new List<bool>();
        powerDir = new List<int>();
        currentWires = new();
        for (int i = 0; i < 4; i++)
        {
            adjcell.Add(null);
            connection.Add(false);
            powerDir.Add(0);
        }

    }

}


public class CellBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler
{
    public Cell cell;
    public CircuitCreator circuitCreator;

    public void Initialize(Cell cell, CircuitCreator cc)
    {
        this.cell = cell;
        circuitCreator = cc;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        cell.clickStarted = true;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (!circuitCreator.gateMode)
            {
                circuitCreator.isDragging = true;
                circuitCreator.StartWire(cell);
            }
            else
            {
                circuitCreator.MakeGate(cell);
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (!circuitCreator.gateMode)
            {
                circuitCreator.HandleRightClick(cell);
            }
            else
            {
                circuitCreator.RemoveGate(cell);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (circuitCreator.isDragging)
        {

        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (cell.clickStarted)
        {
            circuitCreator.EndWire();
        }

        cell.clickStarted = false;
        circuitCreator.isDragging = false;
    }
}


public enum gates
{
    not,
    and,
    or,
    xor,
    nand,
}

[System.Serializable]
public class GateBehaviour
{
    public gates gateType;
    public int noOfInputs;
    public GameObject prefab;
}