using System.Collections;
using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    public GameManager gameManager;

    private Rigidbody2D rigid;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    public int moveto;
    public int previousMoveto;

    public Transform player;
    public float detectRange = 3.5f;
    public float normalSpeed = 1f;
    public float runSpeed = 3f;

    private bool isDead=false;

    private bool HasTarget{
        get{ //탐지범위 내에 플레이어 있다면 HasTarget = true
            if (Vector2.Distance(transform.position, player.position) < detectRange && player.gameObject.layer==10){
                return true;
            }
            else{
                return false;
            }
        }
    }
    private bool isGround(){ //앞에 땅이 있는지 확인 - run / idle 상태에서 확인
        Vector2 frontRay = new Vector2(rigid.position.x + moveto * 0.4f, rigid.position.y);
        RaycastHit2D hit = Physics2D.Raycast(frontRay, Vector3.down, 1, LayerMask.GetMask("platform"));
        Debug.DrawRay(frontRay, Vector3.down);
        return hit.collider != null;
    }

    void Awake(){
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        moveto = 1;
        StartCoroutine(UpdatePath());
    }

    void Update(){
        if (HasTarget && gameObject.layer==9 && gameManager.hp!=0){
            RunState();
        }
        else if (!HasTarget && gameObject.layer==9){
            IdleState();
        }
        // 시선 방향 설정
        if (moveto != 0){
            spriteRenderer.flipX = moveto < 0;
        }
        anim.SetBool("isWalk", moveto!=0);
        anim.SetBool("hasTarget",HasTarget);
    }

    private void RunState(){ //HasTarget==true 인 경우에 연결
        if (transform.position.x < player.position.x){
            moveto = 1;
            previousMoveto=-1;
        }  
        else{
            moveto = -1;
            previousMoveto=1;
        }
        if (!isGround()){
            moveto=0;
        }
        rigid.velocity = new Vector2(moveto * runSpeed, rigid.velocity.y);
    }

    private void IdleState(){ //HasTarget==false 인 경우에 연결
        rigid.velocity = new Vector2(moveto * normalSpeed, rigid.velocity.y);
        if (moveto==1){
            previousMoveto=1;
        }
        else if (moveto==-1){
            previousMoveto=-1;
        }
        if (!isGround()){
            moveto *= -1;
        }
    }

    private IEnumerator UpdatePath(){
        while (!isDead){
            if (!HasTarget){
                previousMoveto = moveto; //이전에 이동한 방향 기억하고
                yield return new WaitForSeconds(10f); //10초동안 이동 후
                moveto = 0;  // 움직이지 마
                yield return new WaitForSeconds(5.1f); // 5.1초 동안
                moveto = previousMoveto; // 기억한 방향으로 다시 이동해
            }
            yield return new WaitForSeconds(1f);
        }
    }
    public void OnDamage(){
        isDead=true;
        gameObject.layer=12;
        anim.SetTrigger("dead");
    }

    public void ResetEnemy(){
        isDead=false;
        gameObject.layer=9;
        anim.Rebind();
        anim.Update(0);
    }
}
