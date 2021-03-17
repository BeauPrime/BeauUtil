using System;
using BeauUtil;
using BeauUtil.Graph;
using UnityEngine;
using UnityEngine.EventSystems;

public class PathNode : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] public PathNode[] ConnectedNodes;
    public ushort Id;
    
    [NonSerialized] public Action<ushort, bool> OnEnableOrDisable;
    [NonSerialized] public Action<ushort> OnClick;

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick?.Invoke(Id);
    }

    public void OnEnable()
    {
        OnEnableOrDisable?.Invoke(Id, true);
    }

    public void OnDisable()
    {
        OnEnableOrDisable?.Invoke(Id, false);
    }
}