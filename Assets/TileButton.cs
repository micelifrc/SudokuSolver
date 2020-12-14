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
        changeText(value.ToString());
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
    public void deselect(bool is_legal = true) {
        setColorBackground(is_legal ? Color.white : Color.red);
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
