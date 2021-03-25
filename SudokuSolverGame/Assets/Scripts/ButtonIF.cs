using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ButtonIF : MonoBehaviour {
    protected int _lastUnitButtonSize;  // keep memory of the last value recorded for the size of the button, so we recompute the size only when it changes (runtime improvement)
    protected UTL.Coord2 _basicSizeMultiplier;  // a multiplier for the buttons
    protected int _basicTextMultiplier;  // a multiplier for the text size

    // Called once, when the TileButton is created
    public virtual void Awake() {
        _lastUnitButtonSize = UTL.InvalidInt;
        _basicSizeMultiplier = new UTL.Coord2(1, 1);
        _basicTextMultiplier = 75;
    }

    // Update is called once per frame
    private void Update() {
        AdjustSizeAndPosition();
    }

    // adjust the size and position of the button in the world space. Does nothing if UnitButtonSize() hasn't change since the last call of AdjustSizeAndPosition
    protected void AdjustSizeAndPosition() {
        if (_lastUnitButtonSize == GameManager.Instance.UnitButtonSize()) return;
        _lastUnitButtonSize = GameManager.Instance.UnitButtonSize();
        AdjustSize();
        AdjustPosition();
    }

    // adjustes the size of the button
    protected void AdjustSize() {
        gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(_basicSizeMultiplier.x * _lastUnitButtonSize, _basicSizeMultiplier.y * _lastUnitButtonSize);
        GetComponentInChildren<Text>().fontSize = (_basicSizeMultiplier.y * _lastUnitButtonSize * _basicTextMultiplier) / 100;
    }

    // adjust the position of the button
    protected virtual void AdjustPosition() { }

    // change the text desplayed in the button
    public void changeButtonText(string value) {
        GetComponentInChildren<Text>().text = value;
    }
    public void changeButtonText(int value) {
        changeButtonText(value.ToString());
    }
    public void clearText() {
        GetComponentInChildren<Text>().text = "";
    }
    public void writeButtonTextNumber(int number) {
        if (number > 0) changeButtonText(number);
        else clearText();
    }

    // change the color of the text
    public void chageTextColor(Color color) {
        GetComponentInChildren<Text>().color = color;
    }
}
