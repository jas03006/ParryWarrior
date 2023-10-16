using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster02 : Monster_Controller
{
    // Start is called before the first frame update
    protected override void Start()
    {
        //attack_number = 3;
        guard_time = 0.7f;
        action_delay = new WaitForSeconds(0.5f);
        base.Start();
    }
}
