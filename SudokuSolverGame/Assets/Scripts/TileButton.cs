using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TileButton : MonoBehaviour {
    private UTL.Coord2 _Coord;  // the "grid position" of the tile, aka a pair of coordinates in [0,_GridLength) * [0,_GridLength)
    private int _lastUnitButtonSize;  // keep memory of the last value recorded for the size of the button, so we recompute the size only when it changes (runtime improvement)

    // Called once, when the TileButton is created
    public void Awake() {
        _Coord = new UTL.Coord2();
        _lastUnitButtonSize = UTL.InvalidInt;
    }

    // Update is called once per frame
    private void Update() {
        AdjustSizeAndPosition();
    }

    // the actual initialization of the TileButton (serves the purpose that is usully done by the constructor)
    public void Initialize(int x_, int y_)
    {
        _Coord.x = x_;
        _Coord.y = y_;
        AdjustSizeAndPosition();
    }

    // adjust the size and position of the button in the world space. Does nothing if UnitButtonSize() hasn't change since the last call of AdjustSizeAndPosition
    private void AdjustSizeAndPosition() {
        if (_lastUnitButtonSize == GameManager.Instance.UnitButtonSize()) return;
        _lastUnitButtonSize = GameManager.Instance.UnitButtonSize();
        AdjustSize();
        AdjustPosition();
    }

    // adjustes the size of the button
    private void AdjustSize() {
        gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(_lastUnitButtonSize, _lastUnitButtonSize);
        GetComponentInChildren<Text>().fontSize = (_lastUnitButtonSize * 3) / 4;
    }

    // adjust the position of the button
    private void AdjustPosition()
    {
        gameObject.transform.localPosition = new Vector3((x() + 1) * _lastUnitButtonSize + (x() / GameManager.Instance.GridRootLength()) * GameManager.Instance.BorderSize() - Screen.width / 2,
            (y() + 1) * _lastUnitButtonSize + (y() / GameManager.Instance.GridRootLength()) * GameManager.Instance.BorderSize() - Screen.height / 2, 0f);
    }

    // some getters
    public UTL.Coord2 Coord() { return _Coord; }
    public int x() { return _Coord.x; }
    public int y() { return _Coord.y; }

    // change the text desplayed in the button
    public void changeText(string value) {
        GetComponentInChildren<Text>().text = value;
    }
    public void chageText(int value)
    {
        changeText(value.ToString());
    }
}
