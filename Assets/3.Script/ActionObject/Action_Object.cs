using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct ColliderCorner
{
    public Vector2 TopLeft;
    public Vector2 BottomLeft;
    public Vector2 BottomRight;

}

public struct ColliderChecker
{
    public bool Up;
    public bool Down;
    public bool Left;
    public bool Right;

    public void reset()
    {
        Up = false;
        Down = false;
        Left = false;
        Right = false;
    }
}

public enum Action_State { 
    idle =0,
    attack,
    guard,
    parried,
    hurt,
    stun,
    dash,
    critical_attack,
    critical_attacked,
    die
}

public class Action_Object : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] protected Audio_Data audio_data;
     protected AudioSource audio_source;


    [Header("Fight")]
    protected int attack_number = 1;
    [SerializeField] protected List<Attack_Effective> attack_list;
    [SerializeField] protected float damage = 10;
    [SerializeField] protected float defense = 1;
    [SerializeField] protected float guard_defense = 10;
    [SerializeField] protected float max_hp = 100;
    [SerializeField] protected float now_hp = 100;
    [SerializeField] protected float max_stamina = 100;
    [SerializeField] protected float now_stamina = 100;
    [SerializeField] protected float max_parry_gage = 100;
    [SerializeField] protected float now_parry_gage = 0;
    [SerializeField] protected float critical_range = 5f;
    [SerializeField] protected float critical_coeff = 7f;
    protected bool is_strong_attacking = false;
    protected bool is_critical_attacking = false;
    protected bool is_critical_attacked = false;
    protected bool is_attacked = false;

    [Header("Move")]
    protected Animator animator;
    protected float direction = 0;
    protected float old_direction = 0;

    protected float Gravity = -20f;
    protected Vector3 velocity;
    protected float basic_jump_force = 13f;

    [Header("Raycast Collision")]
    private LayerMask Collision_Layer;
    protected Collider2D collider2D;
    protected readonly float Skin_Width = 0.115f;
    private ColliderCorner collider_corner;
    protected ColliderChecker collider_checker;
    public Transform Hit_transform { get; private set; }

    [Header("Raycast Count")]
    [SerializeField] private int Horizontal_Count = 4;
    [SerializeField] private int Vertical_Count = 4;
    private float Horizontal_Spacing; // raycast interval distance along count
    private float Vertical_Spacing;

    [Header("UI")]
    protected Camera cam;
    protected GameObject canvas;
    [SerializeField] private GameObject character_info_ob_prefab;
    protected GameObject character_info_ob;
    protected Character_Info character_info;

    [Header("Transition")]
    protected float last_transition_time = 0f;
    public Action_State action_state { get; protected set; }

    // Start is called before the first frame update
    void Awake()
    {
        TryGetComponent(out audio_source);
        audio_source.playOnAwake = false;

        attack_number = attack_list.Count;
        Collision_Layer = LayerMask.GetMask("Collision_Layer");
        old_direction = transform.forward.z;

        gameObject.TryGetComponent(out animator);
        gameObject.TryGetComponent(out collider2D);

        GameObject.Find("Main Camera").TryGetComponent(out cam);
        canvas = GameObject.Find("Canvas");
        init_character_info();  // UI

        Calculate_Raycast_Spacing();
    }

    protected virtual void Update()
    {
        /*raycast*/
        Update_Collider_Corner();
        collider_checker.reset();

        /*stamina*/
        /*if (now_stamina <= 0) {
            stun();
            now_stamina = 100;
        }*/
        recover_stamina();


        /*move*/
        Update_Movement();


        /* ceiling || ground */
        if (collider_checker.Up || collider_checker.Down)
        {
            velocity.y = 0;
        }

        /* UI */
        update_character_info_pos();
        Debug.DrawCircle(transform.position, critical_range, 32, Color.red);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        check_attacked(collision);
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        //check_attacked(collision);
    }

    #region UI
    protected void init_character_info() {
        character_info_ob = Instantiate(character_info_ob_prefab, Vector3.zero, Quaternion.identity);
        character_info_ob.TryGetComponent(out character_info);
        character_info_ob.transform.SetParent(canvas.transform, false);
        character_info.update_hp(now_hp / max_hp);
        character_info.update_stamina(now_stamina / max_stamina);
        //character_info.init();
        //
    }

    protected void update_character_info_pos() {
        if (gameObject.CompareTag("Player"))
        {
            float bar_width = 800f;
            float bar_height = 60f;
            character_info.set_bar_width(bar_width);
            character_info.set_bar_height(bar_height);
            character_info.set_hp_bar_color(Color.green);
            character_info_ob.transform.position = new Vector3(bar_width / 2f + 30f + 300f, Screen.height - 30f - bar_height / 2f, 0);
            //character_info_ob.transform.position = new Vector3(cam.pixelWidth / 2f, 0f, 0);
        }
        else {
            Vector3 screenPos = cam.WorldToScreenPoint(transform.position);
            Vector3 pos_Min = cam.WorldToScreenPoint(collider2D.bounds.min);
            Vector3 pos_Max = cam.WorldToScreenPoint(collider2D.bounds.max);
            character_info_ob.transform.position = screenPos + Vector3.up * (pos_Max.y - pos_Min.y) * 1.6f / 2f;
            character_info.set_bar_width((pos_Max.x - pos_Min.x) * 1.1f);
        }
    }

    #endregion

    #region HP, Stamina manage
    /*---------------------------HP, Stamina Manage----------------------------*/

    public void heal_HP(float value)
    {
        float new_hp = now_hp + value;
        if (max_hp < new_hp)
        {
            now_hp = max_stamina;
        }
        else
        {
            now_hp = new_hp;
        }
        character_info.update_hp(now_hp / max_hp);
    }

    public void lose_HP(float value)
    {
        float new_hp = now_hp - value;
        if (0 <= new_hp)
        {
            now_hp = new_hp;
        }
        else
        {
            now_hp = 0;
            StartCoroutine(die());
        }
        character_info.update_hp(now_hp / max_hp);
    }

    public void heal_stamina(float value) {
        float new_stamina = now_stamina + value;
        if (max_stamina < new_stamina)
        {
            now_stamina = max_stamina;
        }
        else {
            now_stamina = new_stamina;
        }
        character_info.update_stamina(now_stamina / max_stamina);
    }
    public void recover_stamina()
    {
        float stamina_recover_coeff = 4f;
        if (now_stamina <= 0 ) {
            if ( animator.GetCurrentAnimatorStateInfo(0).IsName("Stun")) { 
                heal_stamina(70f);
            }
        }
        else if (now_stamina < max_stamina) {
            heal_stamina(Time.deltaTime * (0.5f + stamina_recover_coeff * now_stamina / max_stamina));
        }
    }

    public void lose_stamina(float value) {
        float new_stamina = now_stamina - value;
        if (0 <= new_stamina)
        {
            now_stamina = new_stamina;
        }
        else
        {
            now_stamina = 0;
            stun();
        }
        character_info.update_stamina(now_stamina / max_stamina);
    }

    public void cal_attacked_damage(float dmg, bool is_guarded) {
        float result = dmg;
        if (is_guarded) {
            result = dmg - guard_defense;
        }
        result -= defense;
        lose_HP(Mathf.Max(0f, result));
    }

    public float cal_attack_damage(bool is_critical)
    {
        if (is_critical) {
            return damage * critical_coeff;
        }
        return damage;
    }
    /*---------------------------------------------------------------------*/
    #endregion

    #region state transition
    /*--------------------------State Transition---------------------------*/
    public void update_last_time(Action_State state)
    {
        last_transition_time = Time.time;
        action_state = state;
    }
    public void update_idle(bool is_run = false)
    {
        if (velocity.y < 0)
        {
            fall();
        }
        else if (velocity.y > 0)
        {
            jump();
        } else if (velocity.x != 0) {
            if (is_run) {
                run();
            } else {
                walk();
            }
        }
        else
        {
            //update_last_time(Action_State.idle);
            animator.SetFloat("Idle Blend X", 0f);
            animator.SetFloat("Idle Blend Y", 0f);
        }
    }
    public IEnumerator die()
    {
        update_last_time(Action_State.die);
        animator.SetTrigger("Die Trigger");
        float elapsed_time = 0f;
        SpriteRenderer sr = gameObject.GetComponent<SpriteRenderer>();
        Color new_color = new Color(sr.color.r, sr.color.g, sr.color.b, 0f);
        while (elapsed_time < 1f) {
            yield return null;
            sr.color = Color.Lerp(sr.color, new_color, elapsed_time);
            elapsed_time += Time.deltaTime;
        }
        gameObject.SetActive(false);
        character_info_ob.SetActive(false);
       // Destroy(gameObject);
        //Destroy(character_info_ob);
    }
    public void idle()
    {
        //update_last_time(Action_State.idle);
        animator.SetFloat("Idle Blend X", 0f);
        animator.SetFloat("Idle Blend Y", 0f);
    }
    public void walk() {
        //update_last_time(Action_State.idle);
        animator.SetFloat("Idle Blend X", 0.5f);
        animator.SetFloat("Idle Blend Y", 0f);
    }

    public void run()
    {
       // update_last_time(Action_State.idle);
        animator.SetFloat("Idle Blend X", 1f);
        animator.SetFloat("Idle Blend Y", 0f);
    }

    public void jump()
    {
        //update_last_time(Action_State.idle);
        animator.SetFloat("Idle Blend X", 0f);
        animator.SetFloat("Idle Blend Y", 0.5f);
    }

    public void fall()
    {
        //update_last_time(Action_State.idle);
        animator.SetFloat("Idle Blend X", 0f);
        animator.SetFloat("Idle Blend Y", 1f);
    }



    public void attack(int n) {
        float value = n;
/*        if (attack_number == 1)
        {
            value = 0;
        }
        else
        {
            value = ((float)n) / (float)(attack_number - 1);
        }*/
        //Debug.Log("Attack: " + value);
        update_last_time(Action_State.attack);
        StartCoroutine(attack_move(n, last_transition_time));
        animator.SetFloat("Attack Blend X", value);
        animator.SetTrigger("Attack Trigger");

        audio_source.clip = audio_data.attack_base;
        audio_source.Play();
    }
    public virtual void attacked()
    {
        update_last_time(Action_State.hurt);
        animator.SetTrigger("Hurt Trigger");

        audio_source.clip = audio_data.attacked;
        audio_source.Play();
    }

    public virtual void critical_attack(int n)
    {
        update_last_time(Action_State.critical_attack);
        is_critical_attacking = true;
        animator.SetBool("Critical Bool", true);

        audio_source.clip = audio_data.attack_base;
        audio_source.Play();
    }
    public void critical_attack_cancel()
    {
        is_critical_attacking = false;
        animator.SetBool("Critical Bool", false);
    }
    public IEnumerator critical_attacked(float time, float dmg)
    {
        is_critical_attacked = true;
        float elapsed_time = 0f;
        while (elapsed_time < time * 2 / 3) {
            yield return null;
            elapsed_time += Time.deltaTime;
        }
        velocity.y += 8f;
        velocity.x += 4f;
        cal_attacked_damage(dmg, false);
        update_last_time(Action_State.critical_attacked);
        animator.SetTrigger("Critical Attacked Trigger");
        is_critical_attacked = false;


        audio_source.clip = audio_data.attacked;
        audio_source.Play();
    }


    public virtual void parried()
    {
        velocity.x = 0;
        lose_stamina(50f);
        if (now_stamina > 0) {
            update_last_time(Action_State.parried);
            animator.SetTrigger("Parried Trigger");
        }

        audio_source.clip = audio_data.parried;
        audio_source.Play();
    }
    public void guard()
    {
        if (!animator.GetBool("Guard Bool"))
        {
            update_last_time(Action_State.guard);
            animator.SetBool("Guard Bool", true);
        }
    }
    public void guard_cancel()
    {
        animator.SetBool("Guard Bool", false);
    }

    public void dash()
    {
        if (can_dash())
        {
            guard_cancel();
            update_last_time(Action_State.dash);
            animator.SetTrigger("Dash Trigger");
        }
    }
    public void stun() {
        velocity.x = 0;
        velocity.y = 0;
        update_last_time(Action_State.stun);
        animator.SetTrigger("Stun Trigger");
    }

    #endregion
    /*---------------------------------------------------------------------------*/

    public void check_attacked(Collider2D collision)
    { if (!is_attacked // 충돌해서 피격된 상태가 아니고
          && !is_critical_attacking // 크리티컬 공격(경직 대상 공격) 중이 아니고
          && !animator.GetCurrentAnimatorStateInfo(0).IsName("Hurt") // 피격 상태가 아니고
          && !is_critical_attacked // 크리티컬 어택을 맞기 직전이 아니고
          && !animator.GetCurrentAnimatorStateInfo(0).IsName("Critical_Attacked") // 크리티컬 피격 상태가 아니고
          && collision.gameObject.CompareTag("Attack") // 공격 콜라이더와 충돌했고
          )
        {

            Attack_Effective now_attack = collision.gameObject.GetComponent<Attack_Effective>();
            float dmg = now_attack.dmg;
            if (now_attack.compare_owner_tag(gameObject.tag)) { //공격의 주인과 내가 같은 팀이면 넘어가기
                return;
            }
            if (is_guarded(collision))
            {
                cal_attacked_damage(dmg, is_guarded: true);
                lose_stamina(15f);

                audio_source.clip = audio_data.guard;
                audio_source.Play();
            }
            else {// 막지 못했으면
                cal_attacked_damage(dmg, is_guarded: false);
                lose_stamina(5f);
                if (!is_strong_attacking) {
                    attacked();
                }                
            }
            StartCoroutine(on_is_attacked());
            //now_attack.attack_success();
            //Debug.Log($"attacked: {dmg} | {Time.time}");
        }
    }

    public IEnumerator on_is_attacked() {
        float invisible_time = 1;
        float elapsed_time = 0f;
        is_attacked = true;
        while (invisible_time > elapsed_time) {
            yield return null;
            elapsed_time += Time.deltaTime;
        }
        is_attacked = false;
    }

    public virtual IEnumerator do_critical_attack(int n, string Layer_Name) {
        float elapsed_time = 0f;
        float total_time = 0.3f;
        RaycastHit2D hit = Physics2D.CircleCast(transform.position,
            critical_range, Vector3.forward, 0f, LayerMask.GetMask(Layer_Name));

        if (hit) {
            Debug.Log(hit.transform.gameObject.tag);
            Action_Object ao;
            hit.transform.parent.parent.gameObject.TryGetComponent(out ao);

            Transform col_transform = hit.transform;
            Vector3 dir = (col_transform.position - transform.position) * 1.2f;
            turn(dir.x); // 대상 쪽으로 몸을 돌리고            
            critical_attack(n);

            StartCoroutine(ao.critical_attacked(total_time, cal_attack_damage(is_critical: true)));

            velocity.y = 0;
            velocity.x = 0;
            transform.Translate(Vector3.up * 1f); // 순간이동
            velocity.x = dir.x / total_time;
            velocity.y = dir.y / total_time;

            while (elapsed_time < total_time)
            {

                yield return null;
                elapsed_time += Time.deltaTime;
            }
            velocity.y = 0;
            velocity.x = 0;
            critical_attack_cancel();
        }
    }

    protected virtual IEnumerator attack_move(int n, float transition_time)
    {
        Attack_Effective now_attack = attack_list[n];
        this.is_strong_attacking = true;
        float elapsed_time = 0f;
        int ind = 0;
        float relative_adjustment = 1f;
        while (elapsed_time < now_attack.total_attack_time && transition_time==last_transition_time)
        {
            if (this.is_strong_attacking && elapsed_time > now_attack.strong_attack_time) {
                this.is_strong_attacking = false;
            }
            if (ind < now_attack.move_time_list.Count) {
                if (elapsed_time >= now_attack.move_time_list[ind]) {
                    if (now_attack.is_relative ) {
                        relative_adjustment = get_relative_adjustment(now_attack.absolute_distance);
                    }
                    velocity.x = (direction != 0 ? Mathf.Sign(direction) : direction) * now_attack.move_x_list[ind]* relative_adjustment;
                    velocity.y = now_attack.move_y_list[ind]* relative_adjustment;
                    ind++;
                }
            }
            yield return null;
            elapsed_time += Time.deltaTime;
        }
        velocity.x = 0;
        this.is_strong_attacking = false;
    }

    protected virtual float get_relative_adjustment(float distance) {
        return 1f;
    }

    private bool is_guarded(Collider2D collision)
    {
        Bounds bounds = collider2D.bounds;
        Vector3 col_dir = (collision.transform.position - transform.position).normalized; //오른쪽에서 박으면 x가 1 왼쪽은 -1
        // forward는 오른쪽이 z가 1 왼쪽이 -1
        if(animator.GetCurrentAnimatorStateInfo(0).IsName("Guard") // 가드 상태이고
            &&( (transform.forward.z>0 && bounds.max.x >= transform.position.x)
            || (transform.forward.z < 0 && bounds.min.x <= transform.position.x) )
            )//(transform.forward.z * col_dir.x >= 0)) // 박은 방향이 같은경우
        {
            return true;
        }
        return false;
    }

    public bool can_move(bool attack_allow = true)
    {
        AnimatorStateInfo asi = animator.GetCurrentAnimatorStateInfo(0);
        if (!is_critical_attacked  &&// 크리티컬 어택을 맞기 직전이 아니고
            (asi.IsName("Idle")
            || (attack_allow && asi.IsName("Attack"))
            || asi.IsName("Guard"))
            )
        {
            return true;
        }

        return false;
        /*if (asi.IsName("Dash")
            || asi.IsName("Attack")
           || asi.IsName("Parried")
           || asi.IsName("Critical_Attack")
           || asi.IsName("Critical_Attacked")
           || asi.IsName("Stun"))
        {
            return false;
        }

        return true;*/

    }

    public bool can_dash()
    {
        AnimatorStateInfo asi = animator.GetCurrentAnimatorStateInfo(0);
        if (!is_critical_attacked 
            &&(asi.IsName("Idle") 
            || asi.IsName("Guard"))
            )
        {
            return true;
        }

        return false;
    }


    virtual public void move(float new_direction_x, float new_direction_y, float move_speed, bool attack_allow = false)
    {
        if (turn(new_direction_x, attack_allow)) {
            velocity.x = (direction!=0? Mathf.Sign(direction): direction) *move_speed;
           // gameObject.transform.Translate(Mathf.Abs(new_direction_x) * Vector3.right * move_speed * Time.deltaTime);
            //gameObject.transform.Translate(Mathf.Abs(new_direction_y) * Vector3.up * move_speed * Time.deltaTime);
        }         
    }

    public bool turn(float new_direction_x, bool attack_allow = false) {
        if (!can_move(attack_allow))
        {
            return false;
        }

        old_direction = direction;
        direction = new_direction_x;
        if (old_direction * direction <= 0)
        {
            velocity.x = 0; //즉시 방향 전환
            if (direction < 0)
            {
                gameObject.transform.rotation = Quaternion.Euler(0f, -180f, 0f);
            }
            else if (direction > 0)
            {
                gameObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
        }
        return true;
    }

    public void move_up(float Jump_Force = 0)
    {        
        if (collider_checker.Down)
        {
            if (Jump_Force != 0)
            {
                velocity.y = Jump_Force;
                return;
            }
            velocity.y = this.basic_jump_force;
        }
    }

    
    /* useal move*/
    private void Update_Movement()
    {
        if (!is_critical_attacking) // 크리티컬 어택 중 일 때는 중력 미적용
        {
            velocity.y += Gravity * Time.deltaTime; 
        }        

        Vector3 current_velocity = velocity * Time.deltaTime;
        if (current_velocity.x != 0)
        {
            //RayCast
            Raycast_Horizontal(ref current_velocity);
        }
        if (current_velocity.y != 0)
        {
            Raycast_Vertical(ref current_velocity);
        }
        transform.position += current_velocity;
    }

    #region ray cast
    /*---------------------------------------- ray cast -------------------------------------*/
    private void Calculate_Raycast_Spacing()
    {
        Bounds bounds = collider2D.bounds;
        bounds.Expand(Skin_Width * -2f);
        Horizontal_Spacing = bounds.size.y / (Horizontal_Count - 1);
        Vertical_Spacing = bounds.size.x / (Vertical_Count - 1);
    }

    private void Update_Collider_Corner()
    {
        Bounds bounds = collider2D.bounds;
        //bounds.Expand(Skin_Width * -2f);
        collider_corner.TopLeft = new Vector2(bounds.min.x, bounds.max.y);
        collider_corner.BottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        collider_corner.BottomRight = new Vector2(bounds.max.x, bounds.min.y);

    }

    private void Raycast_Horizontal(ref Vector3 vel)
    {
        float direction = Mathf.Sign(vel.x); //이동 방향
        float distance = Mathf.Abs(vel.x) + Skin_Width; // length of ray
        Vector2 ray_position = Vector2.zero;
        RaycastHit2D hit;
        for (int i = 0; i < Horizontal_Count; i++)
        {
            ray_position = (direction == 1) ? collider_corner.BottomRight : collider_corner.BottomLeft;
            ray_position += Vector2.up * (Horizontal_Spacing * i);

            hit = Physics2D.Raycast(ray_position, Vector2.right * direction, distance, Collision_Layer);
            if (hit)
            {
                //x축 속력을 광선과 오브젝트 사이의 거리로 설정
                vel.x = (hit.distance - Skin_Width) * direction;

                distance = hit.distance;
                collider_checker.Left = (direction == -1);
                collider_checker.Right = (direction == 1);
            
            }
            Debug.DrawRay(ray_position, Vector2.right * direction * distance, Color.blue);
        }
    }

    protected bool Raycast_Horizontal_Long(float direction, float distance)
    {
        Vector2 ray_position = Vector2.zero;
        RaycastHit2D[] hit;
        for (int i = 0; i < Horizontal_Count; i++)
        {
            ray_position = (direction == 1) ? collider_corner.BottomRight : collider_corner.BottomLeft;
            ray_position += Vector2.up * (Horizontal_Spacing * i);

            hit = Physics2D.RaycastAll(ray_position, Vector2.right * direction, distance, Collision_Layer);

            //Debug.DrawRay(ray_position, Vector2.right * direction * distance, Color.blue);
            //Debug.Log(hit.Length);
            //Time.timeScale = 0;
            if (hit.Length == 0)
            {
                return true;
            }
            else if(hit.Length==1){
                if (hit[0].collider.gameObject.CompareTag("Player")) {
                    return true;
                }
            }            
        }
        return false;
    }

    private void Raycast_Vertical(ref Vector3 vel)
    {
        float direction_ = Mathf.Sign(vel.y); //이동 방향
        float distance = Mathf.Abs(vel.y) + Skin_Width; // length of ray
        Vector2 ray_position = Vector2.zero;
        RaycastHit2D hit;
        for (int i = 0; i < Vertical_Count; i++)
        {
            ray_position = (direction_ == 1) ? collider_corner.TopLeft : collider_corner.BottomLeft;
            ray_position += Vector2.right * (Vertical_Spacing * i + vel.x);

            hit = Physics2D.Raycast(ray_position, Vector2.up * direction_, distance,  Collision_Layer);
            if (hit)
            {
                //Debug.Log("hit");
                //속력을 광선과 오브젝트 사이의 거리로 설정
                vel.y = (hit.distance - Skin_Width) * direction_;

                distance = hit.distance;
                collider_checker.Up = (direction_ == 1);
                collider_checker.Down = (direction_ == -1);
                //Hit_transform = hit.transform;
            }
            Debug.DrawRay(ray_position, Vector2.up * direction_ * distance, Color.yellow);
        }
    }


    /*---------------------------------------------------------------------------------------*/
    #endregion
}
