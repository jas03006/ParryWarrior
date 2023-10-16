using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack_Effective : Owned_Object
{
    public float dmg { get; private set; } = 10f;
    [SerializeField] public List<float> move_time_list;
    [SerializeField] public List<float> move_x_list ;
    [SerializeField] public List<float> move_y_list ;
    [SerializeField] private float dmg_coeff = 1.1f;
    [SerializeField] public float strong_attack_time= 1f;
    [SerializeField] public float total_attack_time = 1f;
    [SerializeField] public bool is_relative = false;
    [SerializeField] public float absolute_distance = 5f;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        dmg = ao.cal_attack_damage(is_critical: false) * dmg_coeff;
    }

    public void attack_success()
    {
        Debug.Log($"Attack Success!!!!!!!!!!!!!!:  {transform.gameObject.activeSelf}");

        transform.gameObject.SetActive(false);
    }

}
