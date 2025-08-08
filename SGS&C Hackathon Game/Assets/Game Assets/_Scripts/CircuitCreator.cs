using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CircuitCreator : MonoBehaviour
{
    public CircuitGeneratorSO CircuitGeneratorSO;
    public List<List<Cell>> gridCells;
    public GameObject cellParent;
    public GameObject image;
    public GameObject wireGameobject;
    public TMP_Text textPrefab;
    public float cellSize;

    [Header("Click &  Drag Behaviour")]
    public bool isDragging = false;
    public Cell currentStartCell;
    public Cell currentEndCell;
    private GameObject currentWire; 
    public RectTransform canvasTransform;
    public Vector2 startPos;
    public Vector2 endPos;
    public int currentDir;

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

        // Continue as before
        gridCells = new List<List<Cell>>();
        for (int i = 0; i < CircuitGeneratorSO.rows; i++)
        {
            gridCells.Add(new List<Cell>());
            for (int j = 0; j < CircuitGeneratorSO.columns_MakeItOdd; j++)
            {
                gridCells[i].Add(new Cell(0));
            }
        }

        ReferenceEachCell();
        InstantiateCells();
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
        timer += Time.deltaTime;

        if (timer < updateInterval)
        {
            return;
        }
        timer = 0f;

        if (isDragging && currentWire != null && currentStartCell != null)
        {
            StartLineAndUpdation();

            HandleLineEndAndNewLine();
        }

        CheckEachCellForNullSaves();

        HandleOnOffStateOfEachCell();
        HandleVisual();
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
                }else
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
                        cell.powerDir[k] = 0;                      // then remove

                        Cell neighbor = cell.adjcell[k];
                        if (neighbor != null)
                        {
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
                    }else if (cell.powerDir[k] == -1 && cell.adjcell[k].value != 0)
                    {
                        cell.powerDir[k] = 0;
                        cell.adjcell[k].powerDir[(k + 2) % 4] = 0;
                    }
                }

                if (!receivingPower)
                {
                    cell.value = 0;
                }else
                {
                    cell.value = value;
                }

                for (int k = 0; k < 4; k++)  //now check if new connection is made to give power
                {
                    if (cell.connection[k] && cell.adjcell[k].value != 0 && cell.powerDir[k] != 1)
                    {
                        cell.powerDir[k] = -1;
                        cell.adjcell[k].powerDir[(k + 2) % 4] = 1;
                        cell.value = cell.adjcell[k].value - 1;
                    }
                }
            }
        }
    }


    //private void CheckIfCellIsGettingPower(Cell cell)
    //{
    //    for (int i = 0; i < 4; i++)
    //    {
    //        Cell adjCell = cell.adjcell[i];

    //        if (cell.powerDir[i] != -1) continue;
    //        if (adjCell.value)
    //        {

    //        }else
    //        {
    //            cell.powerDir[i] = 0;
    //            adjCell.powerDir[(i + 2) % 4] = 0;
    //        }
    //    }
    //}

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
            currentEndCell = currentStartCell.adjcell[ delta.x > 0 ? 0 : 2 ];
            currentDir = delta.x > 0 ? 0 : 2;
        }
        else
        {
            // Vertical line
            endPos = new Vector2(startPos.x, endPos.y);
            currentEndCell = currentStartCell.adjcell[ delta.y > 0 ? 1 : 3 ];
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

}

public class Cell
{
    //Values
    public int value;
    public bool isSource;
    public List<bool> connection; // 0-noConnection, 1-connection
    public List<int> powerDir; // 0-noConnection, 1-fromThisCell, -1-toThisCell
    public List<GameObject> currentWires;
    
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
            circuitCreator.isDragging = true;
            circuitCreator.StartWire(cell);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            circuitCreator.HandleRightClick(cell);
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
