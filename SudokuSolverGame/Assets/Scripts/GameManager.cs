using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// The main class, responsible for taking the input, and asking the Solver to solve the Sudoku-puzzle
public class GameManager : MonoBehaviour {
    public static GameManager Instance;  // the GameManager is a singleton. To invoke its members, call GameManager.Instance

    private const int _GridRootLength = 3;  // will usually be 3. This is the square root of the length of the puzzle
    private const int _GridLength = _GridRootLength * _GridRootLength;  // the Sudoku puzzle will be _GridLength * _GridLength
    private const int _GridArea = _GridLength * _GridLength;  // the number of tiles in the puzzle
    private int _BorderSize;  // the size of the border between two adjacent squadres of size _GridRootLength * _GridRootLength
    private int _UnitButtonSize;  // the size of a tile
    private const int _BorderSizeUnitButtonSizeRatio = 8;  // the ratio _UnitButtonSize / _BorderSize
    private const int _NumVerticalSpaces = (_GridLength + 1) * _BorderSizeUnitButtonSizeRatio + _GridRootLength - 1;  // the vertical space is devided into _NumVerticalSpaces intervals, of length _BorderSize each
    private const int _NumHorizontalSpaces = (_GridLength + _GridRootLength + 2) * _BorderSizeUnitButtonSizeRatio + _GridRootLength - 1;  // the horizontal space is devided into _NumHorizontalSpaces intervals, of length _BorderSize each

    public Button tileButtonPrefab;  // a prefab for the tiles (where to write the numbers of the puzzle)
    private Button[] tile_buttons;  // the Buttons representing the tiles, where to write the numbers
    public Button inputNumberButtonPrefab;  // a prefab for the input digits (where to write the numbers of the puzzle)
    private Button[] input_number_buttons;  // the Buttons to insert input
    public Button clearButtonPrefab;  // the prefab for the Clear button
    public Button solveButtonPrefab;  // the prefab for the Solve button
    private Button clear_button, solve_button;  // the actual buttons

    // This is to make GameManager a singleton
    private void MakeSingleton() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    // Awake is called when the singleton is instantiated
    private void Awake() {
        MakeSingleton();
        UpdateGraphicSizes();
        CreateTileButtons();
        CreateInputNumberButtons();
        CreateColoredButton();
    }

    // Update is called once per frame
    private void Update() {
        UpdateGraphicSizes();
    }

    // update the quantities representing the space-division in the game
    private void UpdateGraphicSizes() {
        UpdateBorederSize();
        UpdateUnitButtonSize();
    }

    private void UpdateBorederSize() {
        _BorderSize = UTL.Math.Min(Screen.height / _NumVerticalSpaces, Screen.width / _NumHorizontalSpaces);
    }

    private void UpdateUnitButtonSize() {
        _UnitButtonSize = UTL.Math.Min((Screen.height * _BorderSizeUnitButtonSizeRatio) / _NumVerticalSpaces, (Screen.width *_BorderSizeUnitButtonSizeRatio) / _NumHorizontalSpaces);
    }

    // create and initialize tile_buttons
    private void CreateTileButtons() {
        tile_buttons = new Button[_GridArea];
        for (int tile_idx = 0; tile_idx < _GridArea; tile_idx++) {
            tile_buttons[tile_idx] = Instantiate(tileButtonPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity) as Button;
            tile_buttons[tile_idx].transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, false);
            tile_buttons[tile_idx].GetComponent<TileButton>().Initialize(UTL.getx(tile_idx), UTL.gety(tile_idx));
        }
    }

    // create and initialize input_number_buttons
    private void CreateInputNumberButtons()
    {
        input_number_buttons = new Button[_GridLength + 1];
        int x = 0, y = -1;
        for (int number = 0; number <= _GridLength; number++)
        {
            input_number_buttons[number] = Instantiate(inputNumberButtonPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity) as Button;
            input_number_buttons[number].transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, false);
            if (number > 0) {
                x = (number - 1) % _GridRootLength;
                y = (number - 1) / _GridRootLength;
            }
            input_number_buttons[number].GetComponent<InputNumberButton>().Initialize(x, y, number);
        }
    }

    // create the clear and solve buttons
    private void CreateColoredButton()
    {
        clear_button = Instantiate(clearButtonPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity) as Button;
        clear_button.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, false);
        clear_button.GetComponent<ColoredButton>().Initialize(_GridRootLength, (_GridRootLength * 2) / 3, 2 + _GridRootLength * 2, 40);
        solve_button = Instantiate(solveButtonPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity) as Button;
        solve_button.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, false);
        solve_button.GetComponent<ColoredButton>().Initialize(_GridRootLength, (_GridRootLength * 2) / 3, 1 + (_GridRootLength * 2) / 3, 40);
    }

    // a list of getters
    public int GridRootLength() { return _GridRootLength; }
    public int GridLength() { return _GridLength; }
    public int GridArea() { return _GridArea; }
    public int BorderSize() { return _BorderSize; }
    public int ReverseBorderFraction() { return _BorderSizeUnitButtonSizeRatio; }
    public int UnitButtonSize() { return _UnitButtonSize; }
}
