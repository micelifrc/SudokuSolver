using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TileButton : MonoBehaviour
{
    public int idx;
    public bool is_selection_button = false;
    private GameManager _gameManager;

    // Start is called before the first frame update
    void Start() {
        _gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    public void changeText(string value) {
        GetComponentInChildren<Text>().text = value;
    }
    public void changeText(int value) {
        changeText((value+1).ToString());
    }
    public void clearText() {
        changeText("");
    }

    private void setColorText(Color color) {
        GetComponentInChildren<Text>().color = color;
    }
    public void setInputColor() {
        setColorText(Color.black);
    }
    public void setAlgoColor() {
        setColorText(Color.blue);
    }

    private void setColorBackground(Color color) {
        GetComponent<Image>().color = color;
    }
    public void select() {
        setColorBackground(Color.yellow);
    }
    public void deselect() {
        setColorBackground(Color.white);
    }

    public bool hasText() {
        return GetComponentInChildren<Text>().text.Length != 0;
    }
    public int getValue() {
        return hasText() ? (int) (GetComponentInChildren<Text>().text[0] - '0') : -1;
    }

    public void Click() {
        // TODO need implementation
        if (is_selection_button) {
            _gameManager.writeInput(idx);
        } else {
            _gameManager.changeSelectedButton(idx);
        }
    }
}
