using System;
using System.Collections.Generic;
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
    public float cellSize;

    [Header("Click &  Drag Behaviour")]
    public bool isDragging = false;
    public Cell currentStartCell;
    public Cell currentEndCell;
    private GameObject currentWire; 
    public RectTransform canvasTransform;
    public Vector2 startPos;
    public Vector2 endPos;


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
                gridCells[i].Add(new Cell(false, i, j));
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
                
                // Set the size of the RectTransform
                RectTransform rect = cell.image.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(cellSize - cellSize / 5, cellSize - cellSize / 5);


                // Attach CellBehaviour
                var behaviour = cellImage.AddComponent<CellBehaviour>();
                behaviour.Initialize(cell, this);
            }
        }
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
    }

    private void CheckEachCellForNullSaves()
    {
        for (int i = 0; i < gridCells.Count; i++)
        {
            for (int j = 0; j < gridCells[i].Count; j++)
            {
                Cell cell = gridCells[i][j];

                foreach (var wire in cell.currentWires)
                {
                    if (wire == null)
                    {
                        cell.currentWires.Remove(wire);
                    }
                }
            }
        }
    }

    private void HandleLineEndAndNewLine()
    {
        if ((endPos - startPos).magnitude >= cellSize)
        {
            if (currentEndCell != null && !currentEndCell.currentWires.Contains(currentWire))
                currentEndCell.currentWires.Add(currentWire);

            StartWire(currentEndCell);
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

        UpdateLine(startPos, endPos);
    }

    public void UpdateLine(Vector2 startPos, Vector2 endPos)
    {
        if (currentWire == null) return;

        RectTransform rect = currentWire.GetComponent<RectTransform>();

        Vector2 delta = endPos - startPos;

        // Snap to the dominant direction (X or Y)
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            // Horizontal line
            endPos = new Vector2(endPos.x, startPos.y);
            currentEndCell = currentStartCell.adjcell[ delta.x > 0 ? 0 : 2 ];
        }
        else
        {
            // Vertical line
            endPos = new Vector2(startPos.x, endPos.y);
            currentEndCell = currentStartCell.adjcell[ delta.y > 0 ? 1 : 3 ];
        }

        Vector2 direction = endPos - startPos;
        float distance = direction.magnitude;

        rect.sizeDelta = new Vector2(distance, 15f); // Thickness
        rect.pivot = new Vector2(0, 0.5f);
        rect.position = startPos;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rect.rotation = Quaternion.Euler(0, 0, angle);
    }


    internal void StartWire(Cell cell)
    {
        currentWire = Instantiate(wireGameobject, cellParent.transform);
        currentStartCell = cell;
        currentEndCell = null;
        endPos = startPos;


        if (currentWire != null && !cell.currentWires.Contains(currentWire))
            cell.currentWires.Add(currentWire);
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
}

public class Cell
{
    public bool value;
    public int indexX;
    public int indexY;
    public GameObject image;
    public List<Cell> adjcell; // 0-right, 1-up...
    public List<bool> connection; // 0-noConnection, 1-fromThisCell, -1-toThisCell
    public Button button;
    public List<GameObject> currentWires;

    //Register Clicking
    public bool clickStarted = false;
    public bool clickEnded = false;
    public bool hovered = false;

    public Cell(bool value, int indexX, int indexY)
    {
        this.value = value;
        this.indexX = indexX;
        this.indexY = indexY;
        adjcell = new List<Cell>();
        connection = new List<bool>();
        currentWires = new();
        for (int i = 0; i < 4; i++)
        {
            adjcell.Add(null);
            connection.Add(false);
        }

    }

}


public class CellBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
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
        circuitCreator.isDragging = true;

        circuitCreator.StartWire(cell);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (circuitCreator.isDragging)
        {

        }
        cell.hovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        cell.hovered = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (cell.clickStarted)
        {
            cell.clickEnded = true;
            circuitCreator.EndWire();
        }

        cell.clickStarted = false;
        circuitCreator.isDragging = false;
    }
}
