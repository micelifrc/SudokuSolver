using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// The main class, responsible for taking the input, and asking the Solver to solve the Sudoku-puzzle
public class InputReader : MonoBehaviour {
    public static InputReader Instance;  // the InputReader is a singleton. To invoke its members, call InputReader.Instance

    private static bool _can_input_data;  // will be false while we are solving
    private static int _gridLength;  // the length of the grid. The same as GameManager._GridLength
    private static int[] _input_numbers;  // the input numbers from GameManager
    private static Button[] _tile_buttons;  // the tile buttons from the GameManager
    private static UTL.Coord2 _selectedTile;  // the coord of the currently selected tile on the grid

    // This is to make InputReader a singleton
    private void MakeSingleton() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void Start() {
        MakeSingleton();
        _can_input_data = true;
        _gridLength = GameManager.Instance.GridLength();
        _input_numbers = GameManager.Instance.GetInputNumbers();
        _tile_buttons = GameManager.Instance.GetTileButtons();
        _selectedTile = new UTL.Coord2(0, _gridLength - 1);
        _tile_buttons[_selectedTile.GetIdx()].GetComponent<Image>().color = Color.yellow;
    }

    // Update is called once per frame
    private void Update() {
        DetectKeyProssed();
    }

    // update _selectedTile to new_pos
    public bool SelectTile(UTL.Coord2 new_pos) {
        if (!new_pos.IsInGrid()) return false;
        _tile_buttons[_selectedTile.GetIdx()].GetComponent<Image>().color = Color.white;
        _selectedTile = new_pos;
        _tile_buttons[_selectedTile.GetIdx()].GetComponent<Image>().color = Color.yellow;
        return true;
    }

    // move _selectedTile by a movement vector
    private bool MoveSelectedTile(UTL.Coord2 movement) {
        return SelectTile(_selectedTile + movement);
    }

    // move _selectedTile by a (x,y) vector
    private bool MoveSelectedTile(int x, int y) {
        return MoveSelectedTile(new UTL.Coord2(x, y));
    }

    // reset _selectedTile
    private void DeselectButton() {
        _tile_buttons[_selectedTile.GetIdx()].GetComponent<Image>().color = Color.white;
        _selectedTile = null;
    }

    // write the value n in the tile with position coord
    private void WriteInput(int n, UTL.Coord2 coord) {
        if (!IsValidTile(coord)) return;
        _tile_buttons[coord.GetIdx()].GetComponent<TileButton>().writeNumber(n);
        _input_numbers[coord.GetIdx()] = n - 1;
    }

    // write the value n in the selected tile
    public void WriteInput(int n) { WriteInput(n, _selectedTile); }

    // actions if a key is pressed
    private void DetectKeyProssed() {
        if (!IsValidSelectedTile() || !_can_input_data) return;
        if (Input.GetKeyDown(KeyCode.Escape)) DeselectButton();
        int shiftMultiplier = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? GameManager.Instance.GridRootLength() : 1;
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) MoveSelectedTile(0, shiftMultiplier);
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) MoveSelectedTile(-shiftMultiplier, 0);
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) MoveSelectedTile(0, -shiftMultiplier);
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) MoveSelectedTile(shiftMultiplier, 0);
        // input can be deleted using the backspace or the delete key
        if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Delete)) WriteInput(0);  // input 0 means to delete
        if (Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.Alpha1)) WriteInput(1);
        if (Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.Alpha2)) WriteInput(2);
        if (Input.GetKeyDown(KeyCode.Keypad3) || Input.GetKeyDown(KeyCode.Alpha3)) WriteInput(3);
        if (Input.GetKeyDown(KeyCode.Keypad4) || Input.GetKeyDown(KeyCode.Alpha4)) WriteInput(4);
        if (Input.GetKeyDown(KeyCode.Keypad5) || Input.GetKeyDown(KeyCode.Alpha5)) WriteInput(5);
        if (Input.GetKeyDown(KeyCode.Keypad6) || Input.GetKeyDown(KeyCode.Alpha6)) WriteInput(6);
        if (Input.GetKeyDown(KeyCode.Keypad7) || Input.GetKeyDown(KeyCode.Alpha7)) WriteInput(7);
        if (Input.GetKeyDown(KeyCode.Keypad8) || Input.GetKeyDown(KeyCode.Alpha8)) WriteInput(8);
        if (Input.GetKeyDown(KeyCode.Keypad9) || Input.GetKeyDown(KeyCode.Alpha9)) WriteInput(9);
        // the return key is equivalent to the Start button
        if (Input.GetKeyDown(KeyCode.Return)) Solve();
        if (Input.GetKeyDown(KeyCode.C)) Clear();
    }

    private bool IsValidTile(UTL.Coord2 coord) { return coord != null && coord.IsInGrid(); }  // true if coord belongs to the 2D grid
    private bool IsValidSelectedTile() { return IsValidTile(_selectedTile); }  // true if _selectedTile belongs to the 2D grid

    // Solve the instance
    public void Solve() {
        _can_input_data = false;
        if (!GameManager.Instance.Solve()) DeselectButton();
        _can_input_data = true;
    }

    // Clear the input, by deleting all the written numbers
    public void Clear() {
        UTL.Coord2 coord = new UTL.Coord2(0, 0);
        for (coord.y = 0; coord.y < _gridLength; ++coord.y) {
            for (coord.x = 0; coord.x < _gridLength; ++coord.x) {
                WriteInput(0, coord);
                GetTileButton(coord).ChageTextColor(Color.black);
            }
        }
    }

    // change the color of tile-texts from black to blue for all the tiles that are not written from input
    public void ChangeTextColorForAlgo() {
        UTL.Coord2 coord = new UTL.Coord2(0, 0);
        for (coord.y = 0; coord.y < _gridLength; ++coord.y) {
            for (coord.x = 0; coord.x < _gridLength; ++coord.x) {
                if(GetTileButton(coord).IsZero()) GetTileButton(coord).ChageTextColor(Color.blue);
            }
        }
    }

    // get the TileButton in the required coord
    private TileButton GetTileButton(UTL.Coord2 coord) {
        return _tile_buttons[coord.GetIdx()].GetComponent<TileButton>();
    }
}
