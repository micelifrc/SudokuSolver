using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ColoredButton : ButtonIF {
    int _halfposy;

    // the actual initialization of the TileButton (serves the purpose that is usully done by the constructor)
    public void Initialize(int sizex, int sizey, int halfposy, int text_size_perc) {
        _basicSizeMultiplier.x = sizex;
        _basicSizeMultiplier.y = sizey;
        _halfposy = halfposy;
        _basicTextMultiplier = text_size_perc;
        AdjustSizeAndPosition();
    }

    // adjust the position of the button
    protected override void AdjustPosition() {
        gameObject.transform.localPosition = new Vector3((GameManager.Instance.GridLength() + 2) * GameManager.Instance.UnitButtonSize() + ((GameManager.Instance.GridRootLength() - 1) * _lastUnitButtonSize) / 2 - Screen.width / 2,
            (_halfposy * GameManager.Instance.UnitButtonSize() - Screen.height) / 2, 0f);
    }
}
