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

    // a list of getters
    public int GridRootLength() { return _GridRootLength; }
    public int GridLength() { return _GridLength; }
    public int GridArea() { return _GridArea; }
    public int BorderSize() { return _BorderSize; }
    public int ReverseBorderFraction() { return _BorderSizeUnitButtonSizeRatio; }
    public int UnitButtonSize() { return _UnitButtonSize; }
}
