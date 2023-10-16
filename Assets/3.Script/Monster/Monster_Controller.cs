using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Node {
    public Node parent;
    public Vector2 enter_vector;
    public Vector2 center;
    public float H;
    public float G = -1;
    public float F { get { return H + G; } set { F = value; } }

    public void enter(int x, int y)
    {
        enter_vector = new Vector2(x, y);
    }
    public void cal_H(Vector2 player_position) {
       // H= (player_position - center).magnitude;
        Vector2 v = (player_position - center);
        H = Mathf.Abs(v.x) + Mathf.Abs(v.y);
    }
    public bool try_set_G(float value) {
        if (G == -1 || G > value) {
            G = value;
            return true;
        }
        return false;
    }
}

public class Monster_Controller : Action_Object
{
    
    [SerializeField] protected WaitForSeconds action_delay = new WaitForSeconds(2);
    [SerializeField] protected WaitForSeconds path_detection_delay = new WaitForSeconds(0.2f);
    [SerializeField] protected float guard_time = 2f;
    [SerializeField] protected float move_speed = 2f;
    [SerializeField] protected float run_speed = 2f;
    [SerializeField] protected float run_speed_coeff = 1.5f;
    [SerializeField] protected float player_detection_range = 9f;
    protected float temp_dir = 1f;
    protected float rage;
    protected float distance_x = 100f;
    protected float distance_y = 100f;

    protected GameObject player;
    protected Player_Controller player_ao;
    protected float direction2player;
    protected float jump_height= 1f;
    protected float jump_time = 1f;

    //debug path finding
    protected Vector2 origin = Vector3.zero;
    protected Vector2 size = Vector2.zero;
    protected Vector2 origin1 = Vector3.zero;
    protected Vector2 size1 = Vector2.zero;
    protected Vector2 origin2 = Vector3.zero;
    protected Vector2 size2 = Vector2.zero;
    protected Vector2 origin3 = Vector3.zero;
    protected Vector2 size3 = Vector2.zero;
    protected Vector2 origin4 = Vector2.zero;
    protected Vector2 size4 = Vector2.zero;
    protected Vector2 next_center = Vector2.zero;
    protected float move2fall = 0;
    protected Node next_node;
    protected Node last_node;
    // Start is called before the first frame update
    protected virtual void Start()
    {
        jump_height = basic_jump_force * basic_jump_force / (-Gravity) / 2f;
        jump_time = 0;
        rage = 0f;
        temp_dir = (Random.Range(-1, 1) * 2 + 1);
        player = GameObject.FindGameObjectWithTag("Player");
        player.TryGetComponent(out player_ao);
        direction2player = (player.transform.position - transform.position).normalized.x;
        StartCoroutine(auto());
    }

    protected override void Update()
    {
        base.Update();       
        Debug.DrawCircle(transform.position, player_detection_range + this.collider2D.bounds.size.x / 2, 32, Color.blue);
        Debug.DrawBox(origin, size, Color.red);
        Debug.DrawBox(origin1, size1, Color.blue);
        Debug.DrawBox(origin2, size2, Color.green);
        Debug.DrawBox(origin3, size3, Color.yellow);
        //DrawPath(next_node);
        DrawPath(last_node, Color.white);

        if (distance_x > player_detection_range)
        { // 플레이어가 많이 멀면
            lose_rage();
        }
        else
        {
            become_rage();
        }
    }


    public virtual void do_attack(int n) {
            attack(n);
    }

    public virtual IEnumerator do_guard()
    {
        float elapsed_time = 0;
        while (elapsed_time < guard_time) {
            if (player != null) {
                direction2player = (player.transform.position - transform.position).normalized.x;
                turn(direction2player / 2f);
            }
            guard();
            yield return null;
            elapsed_time += Time.deltaTime;
        }
        guard_cancel();
    }

    protected void become_rage() {
        if (rage == 100f)
        {
            return;
        }
        if (rage <= 0)
        {
            character_info.on_rage();
            rage = 50f;
        }
        else{
            rage += Time.deltaTime * 10f;
            if (rage > 100f) {
                rage = 100f;
            }
        }
    }
    protected void lose_rage()
    {
        if (rage == 0f)
        {
            return;
        }
        rage -= Time.deltaTime * 5f;
        if (rage < 0f)
        {
            character_info.off_rage();
            rage = 0f;
        }
    }

    public Vector3 find_player_dir() {
        if (player != null)
        {
            return (player.transform.position - transform.position);
        }
        return Vector3.zero;
    }
    public IEnumerator auto()
    {
        int action_num;
        float speed_coeff = 1f;
        int move_probablitiy_coeff = 1;
        Vector3 direction2player_vec;
        float elapsed_time = 0;
        bool dash_attack_flag = true;
        while (true)
        {
            if (elapsed_time > 3f) {
                temp_dir *= -1;
                elapsed_time %= 3f;
                dash_attack_flag = !dash_attack_flag;
            }
            elapsed_time += Time.deltaTime;
            /*if (!can_move(attack_allow: false))
            {
                yield return null;
                continue;
            }*/
            speed_coeff = 0;
            action_num = -1;
            if (player != null)
            {
                //Debug.Log(rage);
                direction2player_vec = (player.transform.position - transform.position);
                distance_x = Mathf.Abs(direction2player_vec.x) - this.collider2D.bounds.size.x / 2;
                distance_y = Mathf.Max(Mathf.Abs(direction2player_vec.y) - this.collider2D.bounds.size.y / 2, 0);
                direction2player = direction2player_vec.normalized.x;
                if (rage > 0f) { //화가 난 상태면
                    //Debug.Log("distance: "+distance_x);                    
                    if (distance_x > 1.7f  || distance_y > 0f) //플레이어에게 다가오는 경우
                    {
                        //Debug.Log(dash_attack_flag);
                        if (dash_attack_flag && distance_y <0.2f && Raycast_Horizontal_Long(direction2player, Mathf.Abs(distance_x)))
                        {
                            move(direction2player, 0f, 0f);
                            action_num = 4;
                        }
                        else {
                            determine_move(1f);
                            elapsed_time += 0.2f;
                            yield return path_detection_delay;
                        }                       
                       
                    }
                    else if (distance_x <= 1.7f) // 플레이어 대상으로 액션하는 경우
                    {
                        //Debug.Log("action");
                        move(direction2player, 0f, 0f);
                         if (player_ao.action_state == Action_State.stun)
                        {
                            yield return StartCoroutine(do_critical_attack(0, "Critical_Layer_Player"));
                        }
                        else if(player_ao.action_state == Action_State.attack)
                        {
                            action_num = attack_number;
                        }                        
                        else {
                            action_num = Random.Range(0, attack_number + 1 + move_probablitiy_coeff);
                        }
                    }
                } else //화가 안난 상태면
                {
                   // 자유 이동 하는 경우
                    speed_coeff = 1f;
                    move(temp_dir, 0f, move_speed * speed_coeff);                  
                }
            }
            else
            { // 자유 이동
                speed_coeff = 1f;
                move(temp_dir, 0f, move_speed * speed_coeff);
            }

            //Debug.Log(action_state);
            //Debug.Log(action_num);
            //Debug.Log(rage);

            update_idle(false);
            if (action_num >= 0 && action_state != Action_State.attack && action_state != Action_State.guard) {
                if (action_num < attack_number)
                {                    
                    do_attack(action_num);
                    yield return null;
                }
                else if (action_num < attack_number + 1)
                {
                    yield return StartCoroutine(do_guard());
                }
            }      
            yield return null;
        }
    }

    protected override float get_relative_adjustment(float distance)
    {
        if (distance < find_player_dir().magnitude) {
            return 1f;
        }
        return find_player_dir().magnitude/ distance ;
    }

    #region Path Finding
    /*--------------------------A star Path Finding----------------------------*/
    protected void determine_move(float speed_coeff) {
        if (!collider_checker.Down) {
            return;
        }
        List<Node> frontier = new List<Node>();
        List<Node> visited = new List<Node>();
        
        Vector2 temp_origin = new Vector2(collider2D.transform.position.x + collider2D.offset.x * collider2D.transform.localScale.x, collider2D.transform.position.y + collider2D.offset.y * collider2D.transform.localScale.y);
        Node root_node = new Node();
        root_node.center = temp_origin;
        root_node.enter(0,0);
        root_node.cal_H(player.transform.position);
        root_node.G = 0;

        frontier.Add(root_node);


        int[] dx_arr = {  1, -1 };
        int[] dy_arr = {  0, 1 ,-1};
        int dx, dy;
        Node result_node = null;
        float x = collider2D.bounds.size.x;
        float y = collider2D.bounds.size.y;
        float diagonal_cost = Mathf.Sqrt(x*x+y*y);

        float max_detection_range = 50f;

        Node now_node;
        int cnt = 0;
        int max_cnt = 2000;
        while (frontier.Count > 0 && cnt < max_cnt) {                    
            cnt++;
            int ind = 0;
            now_node = frontier[0];
            for (int i = 1; i < frontier.Count;i++ ) {
                if (now_node.F > frontier[i].F) {
                    now_node = frontier[i];
                    ind = i;
                }
            }            
            frontier.RemoveAt(ind);
            Vector2 node2player = (now_node.center - (Vector2)player.transform.position);
            if ((Mathf.Abs(node2player.x) <= x && Mathf.Abs(node2player.y) <= y / 2f) || cnt >= max_cnt) {
                result_node = now_node;
                break;
            }

            /*if (now_node.G > max_detection_range) {
                Debug.Log("too far");
                break;
            }*/

            for (int i = 0; i < dx_arr.Length; i++)
            {
                dx = dx_arr[i];
                for (int j = 0; j < dy_arr.Length; j++)
                {
                    dy = dy_arr[j];
                    if (can_go(dx, dy, now_node.center)) {
                        float fall_adjust_cost = (dy == -1? move2fall:0);
                        next_center = (dy > 0 ? origin1 : (dy < 0 ? origin4:origin3));
                        next_node = get_node(next_center, visited, x, y);
                        if (next_node != null) { // 이미 방문한 노드이면
                            continue;
                        }
                        next_node = get_node(next_center, frontier,x ,y);
                        if (next_node == null) // 새로운 노드이면
                        {
                            next_node = new Node();
                            next_node.center = next_center;
                            next_node.G = now_node.G + (next_center - now_node.center).magnitude;
                            next_node.cal_H(player.transform.position);                            
                            next_node.enter(dx, dy);                            
                            next_node.parent = now_node;                            
                            frontier.Add(next_node);
                        }
                        // 프론티어 노드이면
                        else if(next_node.try_set_G(now_node.G + (next_center - (now_node.center + new Vector2(dx*fall_adjust_cost,0))).magnitude + fall_adjust_cost) ) {// x + Mathf.Abs(dy)* diagonal_cost)){
                            next_node.center = next_center;
                            next_node.cal_H(player.transform.position);
                            next_node.enter(dx, dy);
                            next_node.parent = now_node;
                        }                
                        
                    }
                }
            }
            visited.Add(now_node);
        }

        // 최적 경로를 구한 후 1스텝 실행
        if (result_node == null)
        {
            result_node = visited[0];
            for (int i = 1; i < visited.Count; i++)
            {
                if (result_node.F > visited[i].F)
                {
                    result_node = visited[i];
                }
            }
        }
        last_node = result_node;

        while (result_node != null && result_node.parent != root_node) {
            //Debug.DrawLine(result_node.center, result_node.parent.center, Color.red);
            result_node = result_node.parent;            
        }
        if (result_node == null)
        {
            //Debug.Log("no result");
            if (can_go(direction2player, 0, temp_origin))
            {
                move(direction2player, 0f, move_speed * speed_coeff);
            }
            else if (can_go(direction2player, 1, temp_origin))
            {
                move(direction2player, 0f, move_speed * speed_coeff);
                move_up();
            }
            else
            {
                move(direction2player, 0f, move_speed * speed_coeff);
            }
        }
        else {

            if (result_node.enter_vector.y > 0)
            {
                move(result_node.enter_vector.x, 0f, move_speed * speed_coeff * run_speed_coeff);
                move_up();
            }
            else {
                move(result_node.enter_vector.x, 0f, move_speed * speed_coeff);
            }
        }        
    }
    protected void DrawPath(Node node, Color c) {
        Node result_node = node;
        while (node != null && result_node.parent != null)
        { 
            Debug.DrawLine(result_node.center, result_node.parent.center, c);
            result_node = result_node.parent;
        }
    }
    protected Node get_node(Vector2 position, List<Node> list, float x, float y) {
        Node now;
        for (int i =0; i < list.Count;i++) {
            now = list[i];
            if ((now.center - position).magnitude < (x+y)/5f) {
                return now;
            }
        }
        return null;
        //return visited.Contains(node);
    }

    
    protected bool can_go(float dx_, float dy_, Vector2 temp_origin) {
        
        float x = collider2D.bounds.size.x;
        float y = collider2D.bounds.size.y;
        float time_move = x / move_speed;
        float move_while_jump = 0.4f + Skin_Width;//-Gravity * time_move * time_move / 2f;
       // Debug.Log(move_while_jump);
        float foot_width = 0.05f;

        int dx, dy;
        if (dx_ > 0)
        {
            dx = 1;
        }
        else if (dx_ < 0)
        {
            dx = -1;
        }
        else {
            dx = 0;        
        }
        dy = (int)dy_;
        Vector3 dest_pos;
        size.x = x;
        float jump_distance = 0.5f*x;
        if (dy > 0)
        {

            bool result = false;
            //옆쪽에 올라탈 수 있는 지면이 있는지 체크
            origin = temp_origin + new Vector2((x+ jump_distance) * dx, jump_height / 2f - y / 2f);
            size.x = x+ jump_distance;
            size.y = jump_height;
            RaycastHit2D[] hits = Physics2D.BoxCastAll(origin, size, 0f, -Vector2.one, 0f, LayerMask.GetMask("Collision_Layer"));

            for (int i = 0; i < hits.Length; i++)
            {
                //Debug.Log("Hit0");
                result = true;
                RaycastHit2D hit = hits[i];
                dest_pos = hit.collider.bounds.max;
                 if (dest_pos.y >  (temp_origin.y - y/2f) + jump_height) { // 점프할 블럭 바닥의 높이가 점프 높이보다 높으면 못감
                     //Debug.Log("Too High: "+dest_pos);
                     continue;
                 }

                origin1 = new Vector2(origin.x, dest_pos.y + (y + move_while_jump) / 2f);
                size1.x = size.x - foot_width;
                size1.y = y + move_while_jump - foot_width;
                RaycastHit2D[] hits1 = Physics2D.BoxCastAll(origin1, size1, 0f, Vector2.zero, 0f, LayerMask.GetMask("Collision_Layer"));
                
                if (hits1.Length > 0)
                {
                   // Debug.Log("hit1");
                    continue;
                }

                //점프할 수 있는 지 검사 (머리 위)
                size2.y = ( origin1.y+ (move_while_jump - foot_width)/2f - temp_origin.y);//jump_height;//size1.y;//
                origin2 = temp_origin + new Vector2(0, size2.y/2f + y/2f);// + new Vector2(0, jump_height / 2f + y / 2f);//new Vector2(temp_origin.x,origin1.y);//
                size2.x = x + Skin_Width + foot_width;
                
                RaycastHit2D[] hits2 = Physics2D.BoxCastAll(origin2, size2, 0f, Vector2.zero, 0f, LayerMask.GetMask("Collision_Layer"));
                if (hits2.Length > 0)
                {
                    //Debug.Log("hit2");
                    continue;
                }

                origin1.y += Skin_Width - move_while_jump / 2f;
                if (result)
                {
                    return true;
                }
            }
        }
        else if(dy < 0)
        {
            float fall_check_range = y / 8f;
            float dest_check_range = 10f * y;
            //밟고 있는 바닥 검사
            float floor_y = -float.MinValue;
            int ind = 0;
            size4.x = x;
            size4.y = fall_check_range;
            RaycastHit2D[] hits = Physics2D.BoxCastAll(temp_origin + new Vector2(0, -y/2f), size4, 0f, Vector2.zero, 0f, LayerMask.GetMask("Collision_Layer"));
            if (hits.Length > 0)
            {
                for (int i = 0; i < hits.Length; i++)
                {
                    if (floor_y < hits[i].collider.bounds.max.y)
                    {
                        floor_y = hits[i].collider.bounds.max.y;
                        ind = i;
                    }
                }
                float floor_x = (dx > 0 ? hits[ind].collider.bounds.max.x : hits[ind].collider.bounds.min.x);
                if (dx * (floor_x - temp_origin.x) > x)
                {
                    move2fall = x;                   
                    return false;
                }
                else {                    
                    move2fall = dx * (floor_x - temp_origin.x) + x / 2f + foot_width;
                    //Debug.Log(move2fall + ",   " + x);
                }                
            }
            else {
                move2fall = x;
            }
            
            
            // 떨어질 수 있는지 검사            
            origin4 = temp_origin + new Vector2(dx* move2fall, -fall_check_range);
            size4 = new Vector2();
            size4.x = x;
            size4.y = y*1.1f;
            hits = Physics2D.BoxCastAll(origin4, size4, 0f, Vector2.zero, 0f, LayerMask.GetMask("Collision_Layer"));

            if (hits.Length == 0)
            {                
                // 착지 지점 검사
                size4.x = x;
                size4.y = dest_check_range;
                origin4.y += fall_check_range - dest_check_range/2f;

                hits = Physics2D.BoxCastAll(origin4, size4, 0f, Vector2.zero, 0f, LayerMask.GetMask("Collision_Layer"));
                origin4.y -= dest_check_range / 2f;
                for (int i =0; i < hits.Length; i++) {
                    if (origin4.y < hits[i].collider.bounds.max.y) {
                        origin4.y = hits[i].collider.bounds.max.y;
                    }
                }
                origin4.y += y / 2f + foot_width;
                if (hits.Length > 0)
                { 
                    return true;
                }                    
            }
        }
        else {
            float walk_distance = x/4f;
            origin3 = temp_origin + new Vector2(walk_distance * dx, 0f);
            size3.x = x;
            size3.y = y;
            RaycastHit2D[] hits = Physics2D.BoxCastAll(origin3, size3, 0f, Vector2.zero, 0f, LayerMask.GetMask("Collision_Layer"));

            if (hits.Length == 0) {
                hits = Physics2D.BoxCastAll(temp_origin + new Vector2(walk_distance * dx, -y / 2f), size3, 0f, Vector2.zero, 0f, LayerMask.GetMask("Collision_Layer"));
                if (hits.Length > 0) {
                   // Debug.Log("ground check");
                    return true;
                }
            }
        }
        return false;
    }
    #endregion
}
