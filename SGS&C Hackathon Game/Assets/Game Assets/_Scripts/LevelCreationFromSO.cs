using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Windows;
using static UnityEngine.EventSystems.EventTrigger;

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
    public GameObject outputContainer;
    public GameObject simulateButton;
    public GameObject WhatToDoObj;


    public Sprite unActivatedCell;
    public Sprite activatedCell;
    public bool hardMode;


    [Header("Properties")]
    public bool simulate;
    public int frameCounter = 0;
    private GameObject currentWire;

    [Header("Level Dependent")]
    public List<List<Cell>> gridCells = new();
    public List<GateOption> gateOptions = new();
    public float cellSize;
    public List<Button> gateOptioonButtons = new();
    public List<Image> gateOptionButtonImages = new();


    [Header("Click Behaviour Making Gates")]
    //public bool gateMode;
    public gates selectedGateType;
    public int selectedGateIndex;
    public Color clickedcolor;
    public Color normalcolor;

    [Header("Level Save System")]
    public List<Cell> inputs = new();


    [Header("Check For Correct Output")]
    public float recheckTime = 3f;
    public float recheckTimer = 0f;
    public bool levelDone;
    public TMP_Text TimerVisual;


    [Header("Gate Explanation")]
    public TMP_Text TitleText;
    public TMP_Text DescText;
    public GameObject explanationWindow;
    public GateExplanationSO GateExplanationSO;

    private void Start()
    {
        GetValuesFromSO();

        ReferenceEachCell();

        InitializeGates();

        InstantiateCells();
        InitializeInputCells();
        InitializeOutputCells();
        InitializeWires();

        if (GameData.Instance.GameType == 1)
        {
            simulateButton.SetActive(true);

            StartCoroutine(SimulateFor2Sec());
        }

        GateExplanation();

        if (GameData.Instance.isFirst)
        {
            WhatToDoObj.SetActive(true);
            GameData.Instance.isFirst = false;
        }
    }

    private void GateExplanation()
    {
        for (int i = 0; i < gateOptions.Count; i++)
        {
            for (int j = 0; j < GameData.Instance.gatesToExplain.Count; j++)
            {
                GateExplainEntry gateToExplain = GameData.Instance.gatesToExplain[j];
                if (gateToExplain.gateType == gateOptions[i].gateType && !gateToExplain.explained)
                {
                    //Explain
                    explanationWindow.SetActive(true);
                    TitleText.text = $"{gateToExplain.gateType}";
                    for (global::System.Int32 k = 0; k < GateExplanationSO.gateExplanations.Count; k++)
                    {
                        if (GateExplanationSO.gateExplanations[k].gateType == gateToExplain.gateType)
                        {
                            DescText.text = GateExplanationSO.gateExplanations[k].desc + $"\n\nIt Takes {GetGateBehaviour(gateToExplain.gateType).noOfInputs} Input wire / wires and Output 1 Wire to the right, You can rotate the Output Direction";
                        }
                    }
                    Debug.Log($"Marking {gateToExplain.gateType} as explained");
                    GameData.Instance.gatesToExplain[j].explained = true;
                    Debug.Log($"Now explained? {GameData.Instance.gatesToExplain[j].explained}");
                }
            }
        }
    }

    IEnumerator SimulateFor2Sec()
    {
        simulate = true;
        yield return new WaitForSeconds(recheckTime - 0.2f);

        hardMode = true;
        simulate = false;
    }

    public void Simulate()
    {
        simulate = true;
    }

    private void GetValuesFromSO()
    {
        for (int i = 0; i < LevelSaveSO.rows; i++)
        {
            gridCells.Add(new List<Cell>());
            for (int j = 0; j < LevelSaveSO.cols; j++)
            {
                Cell cell = new Cell(0);
                cell.value = LevelSaveSO.cellData[i * LevelSaveSO.cols + j].value;
                cell.gate = LevelSaveSO.cellData[i * LevelSaveSO.cols + j].gate;
                cell.outputDir = LevelSaveSO.cellData[i * LevelSaveSO.cols + j].outputDir;
                cell.isGate = LevelSaveSO.cellData[i * LevelSaveSO.cols + j].isGate;
                cell.sourceID = LevelSaveSO.cellData[i * LevelSaveSO.cols + j].sourceID;
                cell.connection = LevelSaveSO.cellData[i * LevelSaveSO.cols + j].connections;

                gridCells[i].Add(cell);
            }
        }
        
        for (int i = 0; i < LevelSaveSO.rows; i++)
        {
            Cell cell = new Cell(0);
            
            cell.value = LevelSaveSO.inputData[i].value;
            cell.isSource = LevelSaveSO.inputData[i].isSource;
            cell.sourceID = LevelSaveSO.inputData[i].sourceID;
            cell.connection = LevelSaveSO.inputData[i].connections;

            inputs.Add(cell);
            
        }

        cellSize = LevelSaveSO.cellSize;

        //gateOptions = new List<GateOption>(LevelSaveSO.gateOptions);

        InitializeGateOptions();
    }

    private void InitializeGateOptions()
    {
        for (int i = 0; i < gridCells.Count; i++)
        {
            for (int j = 0; j < LevelSaveSO.cols; j++)
            {
                Cell cell = gridCells[i][j];
                if (cell.isGate)
                {
                    if (IsGateAvailableToAdd(cell.gate, out GateOption gateOption))
                    {
                        gateOption.amount++;
                        cell.isGate = false;
                    }else
                    {
                        GateOption newGateOption = new GateOption();
                        newGateOption.gateType = cell.gate;
                        newGateOption.amount++;
                        gateOptions.Add(newGateOption);
                        cell.isGate = false;
                    }
                }
            }
        }

        bool IsGateAvailableToAdd(gates gateType, out GateOption gateoption)
        {
            foreach (var gateOption in gateOptions)
            {
                if (gateOption.gateType == gateType)
                {
                    gateoption = gateOption;
                    return true;
                }
            }
            gateoption = null;
            return false;
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
    private void InitializeInputCells()
    {
        for (int i = 0; i < gridCells.Count; i++)
        {

            GameObject cellImage = Instantiate(image, GetInputPos(i), Quaternion.identity, inputContainer.transform);
            Cell cell = inputs[i];
            cell.image = cellImage;
            cell.button = cellImage.AddComponent<Button>();
            cell.isSource = true;

            //intantiate text
            cell.text = cell.image.GetComponentInChildren<TMP_Text>();
            cell.text.text = "0";

            // Set the size of the RectTransform
            RectTransform rect = cell.image.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(cellSize , cellSize );


            // Attach CellBehaviour
            var behaviour = cellImage.AddComponent<CellBehaviour>();
            behaviour.Initialize(cell, this);
        }
    }


    private void InitializeOutputCells()
    {
        for (int i = 0; i < gridCells.Count; i++)
        {

            GameObject cellImage = Instantiate(image, GetOutputPos(i), Quaternion.identity, outputContainer.transform);

            RectTransform rect = cellImage.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(cellSize , cellSize );

            cellImage.GetComponent<Image>().sprite = LevelSaveSO.outputData[i] ? activatedCell : unActivatedCell;
        }
    }


    private void InitializeWires()
    {
        for (int i = 0; i < gridCells.Count; i++)
        {
            for (int j = 0; j < gridCells[i].Count; j+=2)
            {
                Cell cell = gridCells[i][j + (i % 2)];
                if (cell == null) continue;


                for (int k = 0; k < 4; k++)
                {
                    if (cell.connection[k])  //not for input wires
                    {
                        if (cell == null || wireGameobject == null || cellParent == null)
                        {
                            return;
                        }

                        currentWire = Instantiate(wireGameobject, cellParent.transform);
                        cell.currentWires.Add(currentWire);

                        RectTransform rect = currentWire.GetComponent<RectTransform>();

                        Vector2 startPos = cell.image.transform.position;
                        Vector2 endPos = Vector2.zero; ;
                        if (cell.adjcell[k] != null)
                        {
                            endPos = cell.adjcell[k].image.transform.position;
                            cell.adjcell[k].currentWires.Add(currentWire);
                        }
                        else
                        {
                            endPos = inputs[i].image.transform.position;
                        }

                            Vector2 direction = endPos - startPos;
                        float distance = direction.magnitude;

                        rect.sizeDelta = new Vector2(distance, 24f);
                        rect.pivot = new Vector2(0, 0.5f);
                        rect.position = startPos;

                        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                        rect.rotation = Quaternion.Euler(0, 0, angle);
                    }
                }
            }
        }
    }

    private Vector3 GetOutputPos(int i)
    {
        RectTransform parentRect = outputContainer.GetComponent<RectTransform>();
        Vector3 parentCenter = outputContainer.transform.position;

        float totalHeight = gridCells.Count * cellSize;

        float startY = totalHeight / 2 - cellSize / 2;

        float y = startY - i * cellSize;

        return parentCenter + new Vector3(0f, y, 0f);
    }

    private Vector3 GetInputPos(int i)
    {
        RectTransform parentRect = inputContainer.GetComponent<RectTransform>();
        Vector3 parentCenter = inputContainer.transform.position;

        float totalHeight = gridCells.Count * cellSize;

        float startY = totalHeight / 2 - cellSize / 2;

        float y = startY - i * cellSize;

        return parentCenter + new Vector3(0f, y, 0f);
    }

    private void InitializeGates()
    {
        for (int i = 0; i < gateOptions.Count; i++)
        {
            GameObject gate = Instantiate(GetGateBehaviour(gateOptions[i].gateType).prefab, gatePrefab.transform.parent.transform);
            gate.SetActive(true);


            TMP_Text amount = gate.GetComponentInChildren<TMP_Text>();
            amount.text = $"{gateOptions[i].amount}";
            gateOptions[i].text = amount;
            if (GameData.Instance.GameType == 2 || SceneManager.GetActiveScene().name == "Level Builder")
            {
                gateOptions[i].text.gameObject.SetActive(false);
            }
            
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
            AudioManager.Instance.PlayGateSelected();
            selectedGateType = gateType;
            selectedGateIndex = i;
            gateMode = true;
            gateOptioonButtons[i].GetComponent<Image>().color = clickedcolor;
            return;
        }

        if (gateMode && selectedGateType == gateType)
        {
            AudioManager.Instance.PlayGateDeSelected();
            gateMode = false;
            gateOptioonButtons[i].GetComponent<Image>().color = normalcolor;
            return;
        }

        if (!gateMode)
        {
            AudioManager.Instance.PlayGateSelected();
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
                rect.sizeDelta = new Vector2(cellSize , cellSize );


                // Attach CellBehaviour
                var behaviour = cellImage.AddComponent<CellBehaviour>();
                behaviour.Initialize(cell, this);
            }
        }

        //gridCells[0][0].isSource = true;
    }
    private Vector3 GetPosition(GameObject cellParent, int i, int j)
    {
        RectTransform parentRect = cellParent.GetComponent<RectTransform>();
        Vector3 parentCenter = cellParent.transform.position;

        float totalWidth = gridCells[0].Count * cellSize;
        float totalHeight = gridCells.Count * cellSize;

        float startX = -totalWidth / 2 + cellSize / 2;
        float startY = totalHeight / 2 - cellSize / 2;

        float x = startX + j * cellSize;
        float y = startY - i * cellSize;

        return parentCenter + new Vector3(x, y, 0f);
    }


    void Update()
    {
        // Only run simulation every 3rd frame
        frameCounter++;
        if (frameCounter % 3 == 0)
        {
            if (simulate)
            {
                HandleOnOffStateOfEachCell();
            }
            HandleVisual();

            if (!hardMode)
                CheckOutput();

            if (hardMode && simulate)
                CheckOutput();
        }
    }

    Coroutine reset;
    private void CheckOutput()
    {
        TimerVisual.gameObject.SetActive(false);


        for (int i = 0; i < gridCells.Count; i++)
        {
            bool val = gridCells[i][gridCells[0].Count - 1].value == 0 ? false : true;

            if (LevelSaveSO.outputData[i] != val)
            {
                recheckTimer = 0;
                if (hardMode && reset == null)
                {
                    reset = StartCoroutine(Reset());
                }
                Debug.Log("Output Not Maching!");
                return;
            }
        }
        for (int i = 0; i < gateOptions.Count; i++)
        {
            if (gateOptions[i].amount > 0)
            {
                recheckTimer = 0;
                if (hardMode && reset == null)
                {
                    reset = StartCoroutine(Reset());
                }
                Debug.Log("Gates Still Present!");
                return;
            }
        }
        TimerVisual.gameObject.SetActive(true);
        recheckTimer += Time.deltaTime * 3;
        TimerVisual.text = $"{(recheckTime - recheckTimer).ToString("F2")}";


        if (recheckTimer > recheckTime)
        {
            AudioManager.Instance.PlayWon();
            levelDone = true;
            GameData.Instance.LevelClicked++;
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
        }
    }

    IEnumerator Reset()
    {
        yield return new WaitForSeconds(recheckTime + 1f);
        if (!levelDone)
        {
            AudioManager.Instance.PlayLose();
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
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
                        //cell.value = 100;
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

                if (cell.isSource)
                {
                    continue;
                }

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
                    if (cell.connection[k] && cell.adjcell[k] == null && cell.sourceID != -1) //either input or output adj cells
                    {
                        if (inputs[cell.sourceID].value != 0)
                        {
                            cell.value += inputs[cell.sourceID].value - 1;
                        }
                        break;
                    }


                    if (cell.connection[k] 
                        && cell.adjcell[k].value != 0 
                        && cell.adjcell[k].value > cell.value 
                        && cell.powerDir[k] != 1 
                        && !cell.adjcell[k].isGate)
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

    private void HandleGateProperties(Cell cell)
    {
        if (!cell.isGate) return;

        switch (cell.gate)
        {
            case gates.not:
                NotGateFunction(cell);
                break;

            case gates.and:
                AndGateFunction(cell);
                break;

            case gates.or:
                OrGateFunction(cell);
                break;

            case gates.nand:
                NandGateFunction(cell);
                break;

            case gates.nor:
                NorGateFunction(cell);
                break;

            case gates.xor:
                XorGateFunction(cell);
                break;
        }

        // === Gate Implementations ===
        void NotGateFunction(Cell cell)
        {
            bool hasInput = false;
            for (int i = 0; i < 4; i++)
            {
                if (cell.connection[i] && i != cell.outputDir && cell.adjcell[i].value < 1)
                    hasInput = true;
                //else if (cell.connection[i] && i != cell.outputDir && cell.adjcell[i] == null)
                //    hasInput = true;
            }
            cell.value = hasInput ? 50 : 0;
        }

        void AndGateFunction(Cell cell)
        {
            int activeInputs = 0;
            for (int i = 0; i < 4; i++)
            {
                if (cell.connection[i] && i != cell.outputDir && cell.adjcell[i].value > 0)
                    activeInputs++;
            }
            cell.value = (activeInputs >= 2) ? 50 : 0;
        }

        void OrGateFunction(Cell cell)
        {
            bool hasActiveInput = false;
            for (int i = 0; i < 4; i++)
            {
                if (cell.connection[i] && i != cell.outputDir && cell.adjcell[i].value > 0)
                    hasActiveInput = true;
            }
            cell.value = hasActiveInput ? 50 : 0;
        }

        void NandGateFunction(Cell cell)
        {
            // NAND = NOT(AND)
            int activeInputs = 0;
            for (int i = 0; i < 4; i++)
            {
                if (cell.connection[i] && i != cell.outputDir && cell.adjcell[i].value > 0)
                    activeInputs++;
            }
            cell.value = (activeInputs >= 2) ? 0 : 50;
        }

        void NorGateFunction(Cell cell)
        {
            // NOR = NOT(OR)
            bool hasActiveInput = false;
            for (int i = 0; i < 4; i++)
            {
                if (cell.connection[i] && i != cell.outputDir && cell.adjcell[i].value > 0)
                    hasActiveInput = true;
            }
            cell.value = hasActiveInput ? 0 : 50;
        }

        void XorGateFunction(Cell cell)
        {
            int activeInputs = 0;
            for (int i = 0; i < 4; i++)
            {
                if (cell.connection[i] && i != cell.outputDir && cell.adjcell[i].value > 0)
                    activeInputs++;
            }
            // XOR outputs ON only when exactly 1 input is ON
            cell.value = (activeInputs == 1) ? 50 : 0;
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
                    //cell.image.GetComponent<Image>().color = Color.red;
                    cell.image.GetComponent<Image>().sprite = activatedCell;
                }
                else
                {
                    //cell.image.GetComponent<Image>().color = Color.grey;
                    cell.image.GetComponent<Image>().sprite = unActivatedCell;
                }
                cell.text.text = $"{cell.value}";
            }
        }

        for (int i = 0; i < inputs.Count; i++)
        {
            Cell cell = inputs[i];

            if (cell.value != 0)
            {
                //cell.image.GetComponent<Image>().color = Color.red;
                cell.image.GetComponent<Image>().sprite = activatedCell;
            }
            else
            {
                //cell.image.GetComponent<Image>().color = Color.grey;
                cell.image.GetComponent<Image>().sprite = unActivatedCell;
            }
            cell.text.text = $"{cell.value}";
        }
    }

    public override void MakeGate(Cell cell)
    {
        if (cell.isSource) return;
        if (cell.isGate)
        {
            cell.outputDir++;
            cell.outputDir %= 4;
            cell.gateGameobject.transform.eulerAngles = cell.gateGameobject.transform.eulerAngles + new Vector3(0, 0, 90);
            //CheckIfConnectionsAreGood(cell);
            AudioManager.Instance.PlayGateRotated();
            return;
        }
        if (gateOptions[selectedGateIndex].amount <= 0 || gateOptions[selectedGateIndex].gateType != selectedGateType) return;
        if (!cell.isGate && IsCellConnected(cell))
        {
            cell.isGate = true;
            cell.gate = selectedGateType;
            cell.outputDir = 0;
            cell.noOfInputs = GetGateBehaviour(selectedGateType).noOfInputs;
            gateOptions[selectedGateIndex].amount--;

            RefreshAllGateText();


            AudioManager.Instance.PlayGatePlaced();

            cell.gateGameobject = Instantiate(GetGateBehaviour(selectedGateType).prefab, cell.image.transform);
            cell.gateGameobject.GetComponentInChildren<TMP_Text>().gameObject.SetActive(false);
            //CheckIfConnectionsAreGood(cell);
        }
    }

    private bool IsCellConnected(Cell cell)
    {
        for (int i = 0; i < 4; i++)
        {
            if (cell.connection[i])
                { return true; }
        }
        return false;
    }

    private void RefreshAllGateText()
    {
        for (int i = 0; i < gateOptions.Count; i++)
        {
            gateOptions[i].text.text = $"{gateOptions[i].amount}";
        }
    }

    public override void RemoveGate(Cell cell)
    {
        if (!cell.isGate) return;
        cell.isGate = false;
        cell.noOfInputs = 4;
        Destroy(cell.gateGameobject);
        for (int i = 0; i < gateOptions.Count; i++)
        {
            if (gateOptions[i].gateType == cell.gate)
            {
                AudioManager.Instance.PlayWireDisconnected();
                gateOptions[i].amount++;
                RefreshAllGateText();
                gateOptions[selectedGateIndex].text.text = $"{gateOptions[selectedGateIndex].amount}";
                break;
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


    public void PLayClick()
    {
        if (Random.Range(0, 2) == 0)
        {
            AudioManager.Instance.PlayClick1();
        }else
        {
            AudioManager.Instance.PlayClick0();
        }
    }
}


