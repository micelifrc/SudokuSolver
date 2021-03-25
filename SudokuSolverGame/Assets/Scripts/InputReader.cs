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
        if (Instance == null)
        {
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
        _tile_buttons[_selectedTile.getIdx()].GetComponent<Image>().color = Color.yellow;
    }

    // Update is called once per frame
    private void Update() {
        detectKeyProssed();
    }

    // update _selectedTile to new_pos
    public bool selectTile(UTL.Coord2 new_pos) {
        if (!new_pos.is_in_grid()) return false;
        _tile_buttons[_selectedTile.getIdx()].GetComponent<Image>().color = Color.white;
        _selectedTile = new_pos;
        _tile_buttons[_selectedTile.getIdx()].GetComponent<Image>().color = Color.yellow;
        return true;
    }

    // move _selectedTile by a movement vector
    private bool moveSelectedTile(UTL.Coord2 movement) {
        return selectTile(_selectedTile + movement);
    }

    // move _selectedTile by a (x,y) vector
    private bool moveSelectedTile(int x, int y) {
        return moveSelectedTile(new UTL.Coord2(x, y));
    }

    // reset _selectedTile
    private void deselectButton() {
        _tile_buttons[_selectedTile.getIdx()].GetComponent<Image>().color = Color.white;
        _selectedTile = null;
    }

    // write the value n in the tile with position coord
    private void writeInput(int n, UTL.Coord2 coord) {
        if (!is_valid_tile(coord)) return;
        _tile_buttons[coord.getIdx()].GetComponent<TileButton>().writeNumber(n);
        _input_numbers[coord.getIdx()] = n - 1;
    }

    // write the value n in the selected tile
    public void writeInput(int n) { writeInput(n, _selectedTile); }

    // actions if a key is pressed
    private void detectKeyProssed() {
        if (!is_valid_selectedTile() || !_can_input_data) return;
        if (Input.GetKeyDown(KeyCode.Escape)) deselectButton();
        int shiftMultiplier = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? GameManager.Instance.GridRootLength() : 1;
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) moveSelectedTile(0, shiftMultiplier);
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) moveSelectedTile(-shiftMultiplier, 0);
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) moveSelectedTile(0, -shiftMultiplier);
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) moveSelectedTile(shiftMultiplier, 0);
        // input can be deleted using the backspace or the delete key
        if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Delete)) writeInput(0);  // input 0 means to delete
        if (Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.Alpha1)) writeInput(1);
        if (Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.Alpha2)) writeInput(2);
        if (Input.GetKeyDown(KeyCode.Keypad3) || Input.GetKeyDown(KeyCode.Alpha3)) writeInput(3);
        if (Input.GetKeyDown(KeyCode.Keypad4) || Input.GetKeyDown(KeyCode.Alpha4)) writeInput(4);
        if (Input.GetKeyDown(KeyCode.Keypad5) || Input.GetKeyDown(KeyCode.Alpha5)) writeInput(5);
        if (Input.GetKeyDown(KeyCode.Keypad6) || Input.GetKeyDown(KeyCode.Alpha6)) writeInput(6);
        if (Input.GetKeyDown(KeyCode.Keypad7) || Input.GetKeyDown(KeyCode.Alpha7)) writeInput(7);
        if (Input.GetKeyDown(KeyCode.Keypad8) || Input.GetKeyDown(KeyCode.Alpha8)) writeInput(8);
        if (Input.GetKeyDown(KeyCode.Keypad9) || Input.GetKeyDown(KeyCode.Alpha9)) writeInput(9);
        // the return key is equivalent to the Start button
        if (Input.GetKeyDown(KeyCode.Return)) Solve();
    }

    private bool is_valid_tile(UTL.Coord2 coord) { return coord != null && coord.is_in_grid(); }  // true if coord belongs to the 2D grid
    private bool is_valid_selectedTile() { return is_valid_tile(_selectedTile); }  // true if _selectedTile belongs to the 2D grid

    // Solve the instance
    public void Solve() {
        _can_input_data = false;
        if (!GameManager.Instance.Solve()) deselectButton();
        _can_input_data = true;
    }

    // Clear the input, by deleting all the written numbers
    public void Clear() {
        UTL.Coord2 coord = new UTL.Coord2(0, 0);
        for (coord.y = 0; coord.y < _gridLength; ++coord.y) {
            for (coord.x = 0; coord.x < _gridLength; ++coord.x) {
                writeInput(0, coord);
                getTileButton(coord).chageTextColor(Color.black);
            }
        }
    }

    // change the color of tile-texts from black to blue for all the tiles that are not written from input
    public void changeTextColorForAlgo()
    {
        UTL.Coord2 coord = new UTL.Coord2(0, 0);
        for (coord.y = 0; coord.y < _gridLength; ++coord.y) {
            for (coord.x = 0; coord.x < _gridLength; ++coord.x) {
                if(getTileButton(coord).isZero()) getTileButton(coord).chageTextColor(Color.blue);
            }
        }
    }

    // get the TileButton in the required coord
    private TileButton getTileButton(UTL.Coord2 coord) {
        return _tile_buttons[coord.getIdx()].GetComponent<TileButton>();
    }
}
