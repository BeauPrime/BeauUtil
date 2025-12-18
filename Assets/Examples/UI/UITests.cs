using System.Collections;
using System.Collections.Generic;
using BeauUtil.UI;
using UnityEngine;

public class UITests : MonoBehaviour
{
    public PointerListener LeftMouseOnly;
    public PointerListener AnyMouseButton;

    // Start is called before the first frame update
    void Start()
    {
        LeftMouseOnly.onPointerDown.Register((d) =>
        {
            Debug.Log("LeftOnly Down " + d.ButtonMask);
        });
        LeftMouseOnly.onClick.Register((d) =>
        {
            Debug.Log("LeftOnly Click " + d.ButtonMask);
        });

        AnyMouseButton.onPointerDown.Register((d) =>
        {
            Debug.Log("AnyMouse Down " + d.ButtonMask);
        });
        AnyMouseButton.onPointerUp.Register((d) =>
        {
            Debug.Log("AnyMouse Up " + d.ButtonMask);
        });
        AnyMouseButton.onClick.Register((d) =>
        {
            Debug.Log("AnyMouse Click " + d.ButtonMask);
        });
    }
}
