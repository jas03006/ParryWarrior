using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Controller : Action_Object
{
    [SerializeField] private float run_speed = 2f;
    [SerializeField] private float player_speed = 2f;
    [SerializeField] private float run_speed_coeff = 1.5f;
    [SerializeField] private float jump_force = 8f;
    [SerializeField] private float camera_speed = 1f;
    [SerializeField] private float max_camera_distance = 0.7f;
    protected List<Item> items;
    protected bool is_shake = false;
    // Start is called before the first frame update
    void Start()
    {
        items = new List<Item>();
        StartCoroutine(get_input());        
    }
    protected override void Update() {
        base.Update();
        move_camera();
             
    }

    protected void move_camera() {
        Vector2 temp_dir = transform.position - cam.transform.position;
        temp_dir.y += 3f;
        if (Input.GetKey(KeyCode.DownArrow)) {
            if (Mathf.Abs(temp_dir.y) < max_camera_distance * 2f)
            {
                cam.transform.Translate(Vector3.down * camera_speed * Time.deltaTime * 2f);
            }
        } else if (Input.GetKey(KeyCode.UpArrow) ) {
            if (Mathf.Abs(temp_dir.y) < max_camera_distance * 2f) {
                cam.transform.Translate(Vector3.up * camera_speed * Time.deltaTime * 2f);
            }    
        }
        else {            
            if (Mathf.Abs(temp_dir.y) > max_camera_distance)
            {
                cam.transform.Translate(Vector3.up * temp_dir.y * camera_speed * Time.deltaTime * temp_dir.magnitude / max_camera_distance);
            }
            if (!is_shake) {
                cam.transform.Translate(Vector3.right * temp_dir.x * camera_speed * Time.deltaTime);
                //cam.transform.position = new Vector3(transform.position.x, cam.transform.position.y, cam.transform.position.z);
            }            
        }
    }
    protected IEnumerator shake_camera() {
        
        Vector2 dir = new Vector2((Random.Range(-1, 1) * 2f + 1f) * 4f,0);
        float elapsed_time = 0f;
        int cnt = 0;
        int cnt_;
        while (elapsed_time < 0.5f) {
            is_shake = true;
            cnt_ = (int)(elapsed_time / 0.07f);
            if (cnt != cnt_)
            {
                cnt = cnt_;
                dir.x *= -1;
                //dir.y = -1;
            }
            cam.transform.Translate(dir * camera_speed * Time.deltaTime );
            yield return null;
            elapsed_time += Time.deltaTime;                        
        }
        is_shake = false;
    }

    public override void attacked() {
        base.attacked();
        StartCoroutine(shake_camera());
    }

    public override void critical_attack(int n)
    {
        base.critical_attack(n);
        StartCoroutine(shake_camera());
    }
    public override void parried()
    {
        base.parried();
        StartCoroutine(shake_camera());
    }    

    public IEnumerator get_input() {
        float dir;
        float speed_coeff = 1f;
        bool is_run = false;
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                
                attack(0);
            }else if (Input.GetKeyDown(KeyCode.A))
            {
                
                yield return do_critical_attack(0, "Critical_Layer_Monster");
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                guard();
            }
            else if (Input.GetKeyUp(KeyCode.X))
            {
                guard_cancel();
            } else if (Input.GetKeyDown(KeyCode.C)) {
                dash();
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                move_up();
            }
           // else {
                dir = Input.GetAxisRaw("Horizontal");
                is_run = false;
                if (dir == 0)
                {                    
                    speed_coeff = 0;
                }
                else if (Input.GetKey(KeyCode.LeftShift)) {
                    is_run = true;
                    speed_coeff = run_speed_coeff;
                }
                else {
                    speed_coeff = 1f;                    
                }                
                move(dir, Input.GetAxisRaw("Vertical"), player_speed * speed_coeff);
                update_idle(is_run);
           // }
            yield return null;
        }
    }
      


}
