using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Owned_Object : MonoBehaviour
{
    protected Action_Object ao;
    protected virtual void Start()
    {
        transform.parent.parent.gameObject.TryGetComponent(out ao);
    }
    public bool compare_owner_tag(string tag)
    {
        return ao.CompareTag(tag);
    }
}
