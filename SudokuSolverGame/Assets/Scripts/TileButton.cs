using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TileButton : ButtonIF {
    private UTL.Coord2 _Coord;  // the "grid position" of the tile, aka a pair of coordinates in [0,_GridLength) * [0,_GridLength)
    private int _written_number;

    // Called once, when the TileButton is created
    public override void Awake() {
        base.Awake();
        _Coord = new UTL.Coord2();
        _written_number = 0;
    }

    // the actual initialization of the TileButton (serves the purpose that is usully done by the constructor)
    public void Initialize(int x_, int y_)
    {
        _Coord.x = x_;
        _Coord.y = y_;
        AdjustSizeAndPosition();
    }

    // adjust the position of the button
    protected override void AdjustPosition()
    {
        gameObject.transform.localPosition = new Vector3((x() + 1) * _lastUnitButtonSize + (x() / GameManager.Instance.GridRootLength()) * GameManager.Instance.BorderSize() - Screen.width / 2,
            (y() + 1) * _lastUnitButtonSize + (y() / GameManager.Instance.GridRootLength()) * GameManager.Instance.BorderSize() - Screen.height / 2, 0f);
    }

    // some getters
    public UTL.Coord2 Coord() { return _Coord; }
    public int x() { return _Coord.x; }
    public int y() { return _Coord.y; }

    public void writeNumber(int n) {
        _written_number = n;
        writeButtonTextNumber(n);
    }
}
