using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// using System.Collections.Generic;

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

    private SudokuSolver solver;

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
    private void clearText(int button_idx) {
        buttons[button_idx].GetComponent<TileButton>().clearText();
    }
    private void setInputColorButton(int button_idx) {
        buttons[button_idx].GetComponent<TileButton>().setInputColor();
    }
    private void setAlgoColorButton(int button_idx) {
        buttons[button_idx].GetComponent<TileButton>().setAlgoColor();
    }
    private void selectButton(int button_idx) {
        buttons[button_idx].GetComponent<TileButton>().select();
    }
    private void deselectButton(int button_idx) {
        buttons[button_idx].GetComponent<TileButton>().deselect();
    }

    // Start is called before the first frame update
    void Start()
    {
        selectionButtons = new Button[Size + 1];
        for (int value = 0; value <= Size; ++value) {
            bool hasValue = value < Size;
            int xPos = getGameCoord(Size + value % RootSize) - ShiftLeft / 2 + (hasValue ? ButtonSize / 2 : ButtonSize * RootSize / 2);
            int yPos = getGameCoord(Size - 1 - value / RootSize);
            selectionButtons[value] = Instantiate(buttonSprite, new Vector3(xPos, yPos, 0), Quaternion.identity) as Button;
            selectionButtons[value].transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, false);  // otherwise the object is invisible
            selectionButtons[value].GetComponent<TileButton>().setInputColor();
            selectionButtons[value].GetComponent<TileButton>().deselect();
            selectionButtons[value].GetComponentInChildren<Text>().text = hasValue ? (value+1).ToString() : "" ;
            selectionButtons[value].GetComponent<TileButton>().idx = value;
            selectionButtons[value].GetComponent<TileButton>().is_selection_button = true;
        }

        inputValues = new int[SizeSquared];
        buttons = new Button[SizeSquared];
        for (int button_idx = 0; button_idx < SizeSquared; ++button_idx) {
            inputValues[button_idx] = InvaliIdx;
            buttons[button_idx] = Instantiate(buttonSprite, getVector3Position(button_idx), Quaternion.identity) as Button;
            buttons[button_idx].transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform, false);  // otherwise the object is invisible
            setInputColorButton(button_idx);
            deselectButton(button_idx);
            buttons[button_idx].GetComponent<TileButton>().idx = button_idx;
        }
    }

    // solve the current Sudoku
    public void Solve() {
        for (int button_idx = 0; button_idx != SizeSquared; ++button_idx) {
            deselectButton(button_idx);
            if (inputValues[button_idx] == InvaliIdx) {
                setAlgoColorButton(button_idx);
            }
        }
        Debug.Log("You pressed the SOLVE button");  // TODO replace with real implementatiton
        solver = new SudokuSolver(inputValues);
        solver.Solve();
    }

    public void changeSelectedButton(int idx) {
        bool just_deselect = (idx == selectedTile);
        if (selectedTile != InvaliIdx) {
            deselectButton(selectedTile);
        }
        if (!just_deselect && idx >= 0 && idx < SizeSquared) {
            selectedTile = idx;
            selectButton(selectedTile);
        } else {
            selectedTile = InvaliIdx;
        }
    }

    public void writeInput(int value) {
        if (selectedTile == InvaliIdx) return;
        if (value < Size) {
            changeButtonText(selectedTile, value);
            inputValues[selectedTile] = value;
        } else if (value == Size) {
            clearText(selectedTile);
            inputValues[selectedTile] = InvaliIdx;
        }
    }
}
