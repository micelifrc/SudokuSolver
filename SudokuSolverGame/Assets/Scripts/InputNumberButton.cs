using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InputNumberButton : ButtonIF
{
    private UTL.Coord2 _Coord;  // the "grid position" of the tile, aka a pair of coordinates in [0,_GridLength) * [0,_GridLength)
    private int _written_number;

    // Called once, when the TileButton is created
    public override void Awake()
    {
        base.Awake();
        _Coord = new UTL.Coord2();
        _written_number = 0;
    }

    // the actual initialization of the TileButton (serves the purpose that is usully done by the constructor)
    public void Initialize(int x_, int y_, int n_)
    {
        _Coord.x = x_;
        _Coord.y = y_;
        _written_number = n_;
        if (_written_number == 0) _basicSizeMultiplier.x = GameManager.Instance.GridRootLength();
        AdjustSizeAndPosition();
        if (_written_number > 0) changeButtonText(_written_number);
        else changeButtonText("del");
    }

    // adjust the position of the button
    protected override void AdjustPosition()
    {
        if (_written_number > 0)
        {
            gameObject.transform.localPosition = new Vector3((x() + GameManager.Instance.GridLength() + 2) * _lastUnitButtonSize + (x() / GameManager.Instance.GridRootLength()) * GameManager.Instance.BorderSize() - Screen.width / 2,
                (y() + 1 + GameManager.Instance.GridLength() - GameManager.Instance.GridRootLength()) * _lastUnitButtonSize + (GameManager.Instance.GridRootLength() - 1) * GameManager.Instance.BorderSize() - Screen.height / 2, 0f);
        }
        else
        {
            gameObject.transform.localPosition = new Vector3((GameManager.Instance.GridLength() + 2) * _lastUnitButtonSize + ((GameManager.Instance.GridRootLength() - 1) * _lastUnitButtonSize) / 2 - Screen.width / 2,
                (GameManager.Instance.GridLength() - GameManager.Instance.GridRootLength()) * _lastUnitButtonSize + (GameManager.Instance.GridRootLength() - 2) * GameManager.Instance.BorderSize() - Screen.height / 2, 0f);
        }
    }

    // some getters
    public UTL.Coord2 Coord() { return _Coord; }
    public int x() { return _Coord.x; }
    public int y() { return _Coord.y; }
}
