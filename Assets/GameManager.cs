using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public Button buttonSprite;
    public int ButtonSize = 40;  // the size of a single button
    public int MarginSize = 5;
    public int ShiftLeft = 100;
    public const int RootSize = 3;
    private const int Size = RootSize * RootSize;  // the grid will by Size x Size
    private const int SizeSquared = Size * Size;
    private const int InvaliIdx = -1;
    private int selectedTile = InvaliIdx;

    private Button[] selectionButtons;
    private Button[] buttons;
    private int[] inputValues;
    private List<int>[] inputConflicts;

    private SudokuSolver solver;
    private bool is_solving;

    private int getX(int idx) { return idx % Size; }
    private int getY(int idx) { return idx / Size; }
    private int getGameCoord(int cd) { return (int) (ButtonSize * ( cd - Size / 2) + MarginSize * (cd / RootSize - RootSize / 2)); }
    private Vector3 getVector3Position(int idx) { return new Vector3(getGameCoord(getX(idx)) - ShiftLeft, getGameCoord(getY(idx)), 0); }

    private void changeButtonText(int button_idx, string value) {
        buttons[button_idx].GetComponent<TileButton>().changeText(value);
    }
    private void changeButtonText(int button_idx, int value) {
        buttons[button_idx].GetComponent<TileButton>().changeText(value);
    }
    private void clearButtonText(int button_idx) {
        buttons[button_idx].GetComponent<TileButton>().clearText();
    }
    private void setInputColorButton(int button_idx) {
        buttons[button_idx].GetComponent<TileButton>().setInputColor();
    }
    private void setAlgoColorButton(int button_idx) {
        buttons[button_idx].GetComponent<TileButton>().setAlgoColor();
    }
    private void selectButton(int button_idx) {
        selectedTile = button_idx;
        buttons[button_idx].GetComponent<TileButton>().select();
    }
    private void deselectButton(int button_idx) {
        if (selectedTile == button_idx) {
            selectedTile = InvaliIdx;
        }
        buttons[button_idx].GetComponent<TileButton>().deselect(inputConflicts[button_idx].Count == 0);
    }

    // Start is called before the first frame update
    private void Start() {        
        is_solving = false;
        selectionButtons = new Button[Size + 1];
        for (int value = 0; value <= Size; ++value) {
            bool hasValue = value > 0;
            int pos_value = hasValue ? value - 1 : Size;
            int xPos = getGameCoord(Size + pos_value % RootSize) - ShiftLeft / 2 + (hasValue ? ButtonSize / 2 : ButtonSize * RootSize / 2);
            int yPos = getGameCoord(Size - 1 - pos_value / RootSize);
            selectionButtons[value] = Instantiate(buttonSprite, new Vector3(xPos, yPos, 0), Quaternion.identity) as Button;
            selectionButtons[value].transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, false);  // otherwise the object is invisible
            selectionButtons[value].GetComponent<TileButton>().setInputColor();
            selectionButtons[value].GetComponent<TileButton>().deselect();
            selectionButtons[value].GetComponentInChildren<Text>().text = hasValue ? value.ToString() : "" ;
            selectionButtons[value].GetComponent<TileButton>().idx = value;
            selectionButtons[value].GetComponent<TileButton>().is_selection_button = true;
        }

        inputValues = new int[SizeSquared];
        inputConflicts = new List<int>[SizeSquared];
        buttons = new Button[SizeSquared];
        for (int button_idx = 0; button_idx < SizeSquared; ++button_idx) {
            inputValues[button_idx] = InvaliIdx;
            inputConflicts[button_idx] = new List<int>();
            buttons[button_idx] = Instantiate(buttonSprite, getVector3Position(button_idx), Quaternion.identity) as Button;
            buttons[button_idx].transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, false);  // otherwise the object is invisible
            setInputColorButton(button_idx);
            deselectButton(button_idx);
            buttons[button_idx].GetComponent<TileButton>().idx = button_idx;
        }
    }

    // Update is called once per frame
    private void Update() {
        detectKeyProssed();
    }

    private void detectKeyProssed() {
        if (selectedTile < 0 || selectedTile >= SizeSquared) return;
        if (Input.GetKeyDown(KeyCode.Escape)) {
            deselectButton(selectedTile);
        }
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) {
            int newSelectedTile = (selectedTile + Size) % SizeSquared;
            deselectButton(selectedTile);
            selectButton(newSelectedTile);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) {
            int newSelectedTile = (SizeSquared + selectedTile - Size) % SizeSquared;
            deselectButton(selectedTile);
            selectButton(newSelectedTile);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) {
            int newSelectedTile = selectedTile - selectedTile % Size + (selectedTile + 1) % Size;
            deselectButton(selectedTile);
            selectButton(newSelectedTile);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) {
            int newSelectedTile = selectedTile - selectedTile % Size + (Size + selectedTile - 1) % Size;
            deselectButton(selectedTile);
            selectButton(newSelectedTile);
        }
        if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Delete)) writeInput(0);
        if (Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.Alpha1)) writeInput(1);
        if (Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.Alpha2)) writeInput(2);
        if (Input.GetKeyDown(KeyCode.Keypad3) || Input.GetKeyDown(KeyCode.Alpha3)) writeInput(3);
        if (Input.GetKeyDown(KeyCode.Keypad4) || Input.GetKeyDown(KeyCode.Alpha4)) writeInput(4);
        if (Input.GetKeyDown(KeyCode.Keypad5) || Input.GetKeyDown(KeyCode.Alpha5)) writeInput(5);
        if (Input.GetKeyDown(KeyCode.Keypad6) || Input.GetKeyDown(KeyCode.Alpha6)) writeInput(6);
        if (Input.GetKeyDown(KeyCode.Keypad7) || Input.GetKeyDown(KeyCode.Alpha7)) writeInput(7);
        if (Input.GetKeyDown(KeyCode.Keypad8) || Input.GetKeyDown(KeyCode.Alpha8)) writeInput(8);
        if (Input.GetKeyDown(KeyCode.Keypad9) || Input.GetKeyDown(KeyCode.Alpha9)) writeInput(9);
    }

    // solve the current Sudoku
    public void Solve() {
        if (hasConflicts() || is_solving) return;
        is_solving = true;
        for (int button_idx = 0; button_idx != SizeSquared; ++button_idx) {
            deselectButton(button_idx);
            if (inputValues[button_idx] == InvaliIdx) {
                setAlgoColorButton(button_idx);
            }
        }
        Debug.Log("You pressed the SOLVE button");  // TODO replace with real implementatiton
        solver = new SudokuSolver(inputValues);
        solver.Solve();
        is_solving = false;
    }

    public void changeSelectedButton(int idx) {
        if (is_solving) return;
        bool just_deselect = (idx == selectedTile);
        if (selectedTile != InvaliIdx) {
            deselectButton(selectedTile);
        }
        if (!just_deselect && idx >= 0 && idx < SizeSquared) {
            selectButton(idx);
        } else {
            selectedTile = InvaliIdx;
        }
    }

    public void writeInput(int value) {
        if (selectedTile == InvaliIdx || is_solving) return;
        removeInputConflicts(selectedTile);
        if (value > 0 && inputValues[selectedTile] != value - 1) {
            changeButtonText(selectedTile, value);
            inputValues[selectedTile] = value - 1;
            addInputConflicts(selectedTile);
        } else if (value == 0) {
            clearButtonText(selectedTile);
            inputValues[selectedTile] = InvaliIdx;
        }
    }

    private void addInputConflicts(int pivot) {
        if (pivot < 0 || pivot >= SizeSquared) return;
        int[] neighbours = getNeighbourIds(pivot);
        foreach (int neighbour_idx in neighbours) {
            if (inputValues[neighbour_idx] == inputValues[pivot]) {
                inputConflicts[pivot].Add(neighbour_idx);
                inputConflicts[neighbour_idx].Add(pivot);
                if (inputConflicts[neighbour_idx].Count == 1) {
                    deselectButton(neighbour_idx);
                }
            }
        }
    }

    private void removeInputConflicts(int pivot) {
        if (pivot < 0 || pivot >= SizeSquared) return;
        int[] neighbours = getNeighbourIds(pivot);
        foreach (int neighbour_idx in neighbours) {
            if (inputValues[neighbour_idx] == inputValues[pivot]) {
                inputConflicts[neighbour_idx].Remove(pivot);
                if (inputConflicts[neighbour_idx].Count == 0) {
                    deselectButton(neighbour_idx);
                }
            }
        }
        inputConflicts[pivot].Clear();
    }

    // get the indices of all the neighbours of pivot
    private int[] getNeighbourIds(int pivot) {
        if (pivot < 0 || pivot >= SizeSquared) return new int[0];
        int[] neighbours = new int[3 * Size - 2 *RootSize - 1];
        int neighbour_idx = 0;
        int pivotX = pivot % Size, pivotY = pivot / Size;
        for (int x = 0; x < Size; ++x) {
            if (x == pivotX) continue;
            neighbours[neighbour_idx] = Size * pivotY + x;
            neighbour_idx++;
        }
        for (int y = 0; y < Size; ++y) {
            if (y == pivotY) continue;
            neighbours[neighbour_idx] = Size * y + pivotX;
            neighbour_idx++;
        }
        for (int x = RootSize * (pivotX / RootSize); x < RootSize * (pivotX / RootSize + 1); ++x) {
            if (x == pivotX) continue;
            for (int y = RootSize * (pivotY / RootSize); y < RootSize * (pivotY / RootSize + 1); ++y) {
                if (y == pivotY) continue;
                neighbours[neighbour_idx] = Size * y + x;
                ++neighbour_idx;
            }
        }
        if (neighbour_idx != neighbours.Length) Debug.Log("Wrong number of neighbours generated");
        return neighbours;
    }

    private bool hasConflicts() {
        for (int idx = 0; idx != inputConflicts.Length; ++idx) {
            if (inputConflicts[idx].Count != 0) return true;
        }
        return false;
    }
}
