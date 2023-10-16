using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanBeParried : Owned_Object
{
   
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("CanParry") ) {
            // animator.SetTrigger("Parried Trigger");
            CanParry cp;
            collision.gameObject.TryGetComponent(out cp);
            if (!cp.compare_owner_tag(ao.tag)) {
                ao.parried();
                cp.parry_success();
            }            
        }
    }
}
