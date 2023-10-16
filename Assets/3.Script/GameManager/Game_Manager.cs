using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Game_Manager : MonoBehaviour
{
    [SerializeField] List<GameObject> monster_prefabs;
    private List<GameObject> monsters;
    [SerializeField] private GameObject player_prefab;
    private GameObject player;
    [SerializeField] private GameObject retry_button;
    [SerializeField] private GameObject go_main_button;

    // Start is called before the first frame update
    void Awake()
    {
        Time.timeScale = 1;
        retry_button.SetActive(false);
        go_main_button.SetActive(false);
        player = Instantiate(player_prefab, new Vector3(-2f, 5f, 0f), Quaternion.identity);
        monsters = new List<GameObject>();
        for (int i =0; i < monster_prefabs.Count; i++) {
            GameObject now_monster = Instantiate(monster_prefabs[i], new Vector3(10f +i*40f, 5f, 0f), Quaternion.identity);
            monsters.Add(now_monster);
        }
    }

    // Update is called once per frame
    void Update()
    {
        check_clear();
    }

    private void check_clear() {
        if (player.activeSelf == false) {
            retry_button.SetActive(true);
            Time.timeScale = 0;
            return;
            // retry
        }
        for (int i = 0; i < monsters.Count; i++)
        {
            if (monsters[i] != null && monsters[i].activeSelf == true)
            {

                return;
            }
        }
        go_main_button.SetActive(true);
        Time.timeScale = 0;
        // clear
    }

    public void click_retry()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Scene1");
    }
    public void click_go_home()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Intro");
    }
}
