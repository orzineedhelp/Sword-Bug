using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrisStartEnd : MonoBehaviour
{
    public GameObject spike;
    public TetrisGameLogic gameLogic;
    public GameObject phone;
    public Item brick;
    public bool isPlayerOut;//玩家是否离开俄罗斯方块


    private void Update()
    {
        if (gameLogic.isOver&&!isPlayerOut)
        {
            Character player = FindAnyObjectByType<Character>();
            if (player != null&&player.tag=="Player")
            {
                player.TakeDamge();
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            spike.SetActive(true);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            brick = collision.GetComponentInChildren<Item>(); ;
            gameLogic.DownSpeed = 0;
            phone.SetActive(true);
            Destroy(brick.gameObject);
            collision.gameObject.GetComponent<PlayerControl>().isTetromino = false;
            isPlayerOut = true;
        }
    }
}
