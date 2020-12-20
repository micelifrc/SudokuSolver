using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public Button buttonSprite;
    public int Button_Length = 40;  // the Length of a single button
    public int Margin_Length = 5;
    public int ShiftLeft = 100;
    public const int RootLength = 3;
    private const int Length = RootLength * RootLength;  // the grid will by Length x Length
    private const int Area = Length * Length;
    private const int Volume = Area * Length;
    private const int InvalidIdx = -1;

    private int selectedTile = InvalidIdx;
    private Button[] selectionButtons;
    private Button[] buttons;
    public Button solveButton, resetButton;
    private int[] inputValues;
    private List<int>[] inputConflicts;

    private SudokuSolver solver;
    private bool is_solving;

    static int getX(int idx) { return idx % Length; }
    static int getY(int idx) { return (idx / Length) % Length; }
    static int getCol(int idx) { return idx / Area; }
    private int getGameCoord(int cd) { return (int) (Button_Length * ( cd - Length / 2) + Margin_Length * (cd / RootLength - RootLength / 2)); }
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
            selectedTile = InvalidIdx;
        }
        buttons[button_idx].GetComponent<TileButton>().deselect(inputConflicts[button_idx].Count == 0);
    }

    // Start is called before the first frame update
    private void Start() {
        is_solving = false;
        selectionButtons = new Button[Length + 1];
        for (int value = 0; value <= Length; ++value) {
            bool hasValue = value > 0;
            int pos_value = hasValue ? value - 1 : Length;
            int xPos = getGameCoord(Length + pos_value % RootLength) - ShiftLeft / 2 + (hasValue ? Button_Length / 2 : Button_Length * RootLength / 2);
            int yPos = getGameCoord(Length - 1 - pos_value / RootLength);
            selectionButtons[value] = Instantiate(buttonSprite, new Vector3(xPos, yPos, 0), Quaternion.identity) as Button;
            selectionButtons[value].transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, false);  // otherwise the object is invisible
            selectionButtons[value].GetComponent<TileButton>().setInputColor();
            selectionButtons[value].GetComponent<TileButton>().deselect();
            selectionButtons[value].GetComponentInChildren<Text>().text = hasValue ? value.ToString() : "" ;
            selectionButtons[value].GetComponent<TileButton>().idx = value;
            selectionButtons[value].GetComponent<TileButton>().is_selection_button = true;
        }

        inputValues = new int[Area];
        inputConflicts = new List<int>[Area];
        buttons = new Button[Area];
        for (int button_idx = 0; button_idx < Area; ++button_idx) {
            inputValues[button_idx] = InvalidIdx;
            inputConflicts[button_idx] = new List<int>();
            buttons[button_idx] = Instantiate(buttonSprite, getVector3Position(button_idx), Quaternion.identity) as Button;
            buttons[button_idx].transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, false);  // otherwise the object is invisible
            setInputColorButton(button_idx);
            deselectButton(button_idx);
            buttons[button_idx].GetComponent<TileButton>().idx = button_idx;
        }
        solveButton.GetComponent<Button>().interactable = true;
        resetButton.GetComponent<Button>().interactable = false;
        changeSelectedButton(startingSelectedTile());
    }

    // Update is called once per frame
    private void Update() {
        detectKeyProssed();
    }

    private void detectKeyProssed() {
        if (selectedTile < 0 || selectedTile >= Area) return;
        if (Input.GetKeyDown(KeyCode.Escape)) {
            deselectButton(selectedTile);
        }
        int shiftMultiplier = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? RootLength : 1;
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) {
            int newSelectedTile = (selectedTile + shiftMultiplier * Length) % Area;
            deselectButton(selectedTile);
            selectButton(newSelectedTile);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) {
            int newSelectedTile = (Area + selectedTile - shiftMultiplier * Length) % Area;
            deselectButton(selectedTile);
            selectButton(newSelectedTile);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) {
            int newSelectedTile = selectedTile - selectedTile % Length + (selectedTile + shiftMultiplier) % Length;
            deselectButton(selectedTile);
            selectButton(newSelectedTile);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) {
            int newSelectedTile = selectedTile - selectedTile % Length + (Length + selectedTile - shiftMultiplier) % Length;
            deselectButton(selectedTile);
            selectButton(newSelectedTile);
        }
        // input can be deleted using the backspace or the delete key
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
        // the return key is equivalent to the Start button
        if (Input.GetKeyDown(KeyCode.Return)) Solve();
    }

    // solve the current Sudoku. Invoked by the start button, or (equivalently) the return key
    public void Solve() {
        if (hasConflicts() || is_solving) return;
        is_solving = true;
        for (int button_idx = 0; button_idx != Area; ++button_idx) {
            deselectButton(button_idx);
            if (inputValues[button_idx] == InvalidIdx) {
                setAlgoColorButton(button_idx);
            }
        }
        solver = new SudokuSolver(inputValues, buttons);
        solver.Solve();
        solveButton.GetComponent<Button>().interactable = false;
        resetButton.GetComponent<Button>().interactable = true;
        is_solving = false;
    }

    // Reset everything
    public void Reset() {
        is_solving = false;
        for (selectedTile = 0; selectedTile < buttons.Length; selectedTile++) {
            setInputColorButton(selectedTile);
            writeInput(0);
        }
        changeSelectedButton(startingSelectedTile());
        solveButton.GetComponent<Button>().interactable = true;
        resetButton.GetComponent<Button>().interactable = false;
    }

    private int startingSelectedTile() { return Area - Length; }

    public void changeSelectedButton(int idx) {
        if (is_solving) return;
        bool just_deselect = (idx == selectedTile);
        if (selectedTile >= 0 && selectedTile < Area) {
            deselectButton(selectedTile);
        }
        if (!just_deselect && idx >= 0 && idx < Area) {
            selectButton(idx);
        } else {
            selectedTile = InvalidIdx;
        }
    }

    public void writeInput(int value) {
        if (selectedTile == InvalidIdx || is_solving) return;
        removeInputConflicts(selectedTile);
        if (value > 0 && inputValues[selectedTile] != value - 1) {
            changeButtonText(selectedTile, value);
            inputValues[selectedTile] = value - 1;
            addInputConflicts(selectedTile);
        } else if (value == 0) {
            clearButtonText(selectedTile);
            inputValues[selectedTile] = InvalidIdx;
        }
    }

    private void addInputConflicts(int pivot) {
        if (pivot < 0 || pivot >= Area) return;
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
        if (pivot < 0 || pivot >= Area) return;
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
        if (pivot < 0 || pivot >= Area) return new int[0];
        int[] neighbours = new int[3 * Length - 2 *RootLength - 1];
        int neighbour_idx = 0;
        int pivotX = pivot % Length, pivotY = pivot / Length;
        for (int x = 0; x < Length; ++x) {
            if (x == pivotX) continue;
            neighbours[neighbour_idx] = Length * pivotY + x;
            ++neighbour_idx;
        }
        for (int y = 0; y < Length; ++y) {
            if (y == pivotY) continue;
            neighbours[neighbour_idx] = Length * y + pivotX;
            ++neighbour_idx;
        }
        for (int x = RootLength * (pivotX / RootLength); x < RootLength * (pivotX / RootLength + 1); ++x) {
            if (x == pivotX) continue;
            for (int y = RootLength * (pivotY / RootLength); y < RootLength * (pivotY / RootLength + 1); ++y) {
                if (y == pivotY) continue;
                neighbours[neighbour_idx] = Length * y + x;
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

    private void printSolution(ref int[] solution) {
        if (solution.Length != Area) {
            Debug.Log("Wrong number of values in output solution. Got " + solution.Length + " values");
            return;
        }
        for (int idx = 0; idx < solution.Length; ++idx) {
            if (solution[idx] < 0 || solution[idx] >= Length) {
                Debug.Log("Output solution for index " + idx + " is " + solution[idx] + ", which is out of range");
                return;
            }
            if (inputValues[idx] != InvalidIdx && inputValues[idx] != solution[idx]) {
                Debug.Log("Output solution does't match input in position (" + idx%Length+1 + "," + idx/Length+1 + "). Output value is: " + solution[idx]+1);
                return;
            }
            changeButtonText(idx, solution[idx]);
            inputValues[idx] = solution[idx] - 1;
        }
    }

    // close the application
    public void Quit() { 
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #elif UNITY_WEBPLAYER
        Application.OpenURL(webplayerQuitURL);
        #else
        Application.Quit();
        #endif
    }
}
