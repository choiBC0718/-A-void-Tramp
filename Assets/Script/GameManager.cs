using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public PlayerController player;

    public GameObject gameoverText;
    public GameObject gameclearText;
    public Text stage;
    public int stageIndex;
    public int hp;
    private bool isGameover;

    public GameObject[] Stages;
    public Image[] healthIMG;

    void Start()
    {
        isGameover=false;
        stageIndex=0;
        hp=3;
    }

    
    void Update()
    {
        if (isGameover && Input.GetKeyDown(KeyCode.R)){
            ResetGame();
        }
    }
    //hp모두 소진 시 텍스트 나타내기
    public void EndGame(){
        isGameover=true;
        gameoverText.SetActive(true);
    }
    //포탈에 닿았을 때 스테이지 인덱스 증가
    public void NextStage(){
        if (stageIndex<Stages.Length-1){
            Stages[stageIndex].SetActive(false);
            stageIndex++;
            Stages[stageIndex].SetActive(true);
        }
        else{
            isGameover=true;
            gameclearText.SetActive(true);
        }
        stage.text="STAGE "+(stageIndex+1);
    }
    //hp관리
    public void hpDown(){
        if (hp>0){
            hp--;
            healthIMG[hp].color = new Color(1,1,1,0.2f);
        }
    }
    private void ResetGame(){
        isGameover=false;
        Stages[stageIndex].SetActive(false);
        gameoverText.SetActive(false);
        gameclearText.SetActive(false);

        player.transform.position=new Vector3(0,-1,-10);
        player.gameObject.layer=10;
        player.Resetplayer();

        hp=3;
        healthIMG[0].color = new Color(1,1,1,1);
        healthIMG[1].color = new Color(1,1,1,1);
        healthIMG[2].color = new Color(1,1,1,1);

        stageIndex=0;
        Stages[stageIndex].SetActive(true);
        ActivateAllChildren(Stages[stageIndex]);
        stage.text="STAGE "+(stageIndex+1);
    }
    private void ActivateAllChildren(GameObject stage){
        foreach (Transform child in stage.GetComponentsInChildren<Transform>(true)){
            if (child.CompareTag("Enemy")){
                child.gameObject.SetActive(true);
                EnemyMove enemyMove = child.GetComponent<EnemyMove>();
                if (enemyMove != null){
                    enemyMove.ResetEnemy();
                }
            }
        }
    }
}
