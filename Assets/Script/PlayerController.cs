using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public GameManager gameManager;

    private Rigidbody2D rigid;
    private SpriteRenderer spriterenderer;
    private Animator anim;
    private AudioSource playerAudio;
    //오디오 관련
    public AudioClip audioJump;
    public AudioClip audioDie;
    public AudioClip audioClear;
    public AudioClip audioDamaged;
    public AudioClip audioAttack;

    private float moveSpeed=510f;
    private bool isDead;
    //점프 관련
    public Slider gaugeSlider;
    private float jumpForce=440f;
    private bool grounded=false;
    private int jumpCount=0;
    private bool isJumping=false;
    private bool isCrouching=false;
    private float jumpChargeTime=0f;
    private float maxCharge=1.5f;
    // 스태미나 관련
    public Slider staminaSlider;
    private float currentStamina=1f;
    private float decreaseStamina=0.25f;
    private float recoverStamina=0.2f;
    private bool isRunning=false;

    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriterenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        playerAudio = GetComponent<AudioSource>();

        if (gaugeSlider!=null){
            gaugeSlider.value=0;
        }
        if (staminaSlider!=null){
            staminaSlider.value=1;
        }
        isDead=false;
    }

    void Update()
    {      
            //점프
            if (Input.GetKeyDown(KeyCode.Space) && jumpCount<1 && !isDead){
                isJumping=true;
                isCrouching=true;
                jumpChargeTime=0f;
                rigid.velocity = Vector2.zero;
                gaugeSlider.gameObject.SetActive(true);
            }
            //점프 힘 모으기 (꾹 눌러)
            if (isJumping && Input.GetKey(KeyCode.Space) && !isDead){
                jumpChargeTime += Time.deltaTime;
                if (jumpChargeTime> maxCharge){
                    jumpChargeTime=maxCharge;
                }
                if (gaugeSlider!=null){
                    gaugeSlider.value=jumpChargeTime/maxCharge;
                }
            }
            //점프 키에서 손 놓음 (실행)
            if (Input.GetKeyUp(KeyCode.Space) && isJumping && !isDead){
                isJumping=false;
                isCrouching=false;
                jumpCount++;
                float realjumpForce=jumpForce*(jumpChargeTime/maxCharge)+jumpForce;
                rigid.velocity=Vector2.zero;
                rigid.AddForce(new Vector2(0,realjumpForce));
                playerAudio.clip=audioJump;
                playerAudio.Play();
                if (gaugeSlider!=null){
                    gaugeSlider.value=0;
                    gaugeSlider.gameObject.SetActive(false);
                }
            }
            anim.SetBool("isCrouch",isCrouching);
            anim.SetBool("isJump",!grounded);
            //달리기
            isRunning = Input.GetKey(KeyCode.LeftShift) && currentStamina>0;

            if (!isJumping && !isDead){ // 점프 중이 아닐 때 이동가능
                float xInput=Input.GetAxis("Horizontal");
                float xSpeed=xInput*moveSpeed*Time.deltaTime;

                if (isRunning){
                    staminaSlider.gameObject.SetActive(true);
                    xSpeed*=2f;
                    currentStamina-=decreaseStamina * Time.deltaTime;
                    if (currentStamina<0){
                        currentStamina=0;
                    }
                }
                else{
                    currentStamina+=recoverStamina * Time.deltaTime;
                    if (currentStamina>1f){
                        currentStamina=1f;
                        staminaSlider.gameObject.SetActive(false);
                    }
                }
                if (staminaSlider != null){
                    staminaSlider.value=currentStamina/1f;
                }

                Vector3 newVelocity = new Vector3(xSpeed,rigid.velocity.y,0f);
                rigid.velocity = newVelocity;

                //시선 방향 변경
                if (Input.GetKey(KeyCode.LeftArrow) && !isDead){
                    spriterenderer.flipX=true;
                }
                else if (Input.GetKey(KeyCode.RightArrow) && !isDead){
                    spriterenderer.flipX=false;
                }

                //애니메이션
                if (Input.GetAxis("Horizontal")!=0 && !isDead){
                    anim.SetBool("isWalk",true);
                }
                else{
                    anim.SetBool("isWalk",false);
                }
                anim.SetBool("isRun",isRunning);
            
        } 
    }

    //적과 부딪힘
    private void OnDamage(Vector2 targetPos){
        playerAudio.clip=audioDamaged;
        playerAudio.Play();
        gameManager.hpDown();
        gameObject.layer=11;
        spriterenderer.color= new Color(1,1,1,0.4f);
        rigid.AddForce(new Vector2(0,7), ForceMode2D.Impulse);
        if (gameManager.hp>0){
            Invoke("OffDamage",1.5f);
        }
        else {
            gameObject.layer=9;
            spriterenderer.color=new Color(1,1,1,1);
        }
    }
    //무적시간
    private void OffDamage(){
        gameObject.layer=10;
        spriterenderer.color=new Color(1,1,1,1);
    }
    //적을 밟음
    private void OnAttack(Transform enemy){
        playerAudio.clip=audioAttack;
        playerAudio.Play();
        rigid.AddForce(Vector2.up * 7, ForceMode2D.Impulse);
        EnemyMove enemymove = enemy.GetComponent<EnemyMove>();
        enemymove.OnDamage();
    }
    //is Trigger와 부딪힘 (포탈 / 데드존)
    void OnTriggerEnter2D(Collider2D other){
        if (other.tag=="Finish"){
            playerAudio.clip=audioClear;
            playerAudio.Play();
            gameManager.NextStage();
            transform.position = new Vector3(0,-1,-10);
        }
        if (other.tag=="Dead"){
            gameManager.hpDown();
            if (gameManager.hp>0){
                playerAudio.clip=audioDamaged;
                playerAudio.Play();
            }
            else{
                Die();
            }
            
            transform.position = new Vector3(0,-1,-10);
        }
    }
    //콜라이더와 부딪힘 (땅 / 적)
    private void OnCollisionEnter2D(Collision2D collision){
        if (collision.contacts[0].normal.y>0.7f){
            grounded=true;
            jumpCount=0;
        }
        if (collision.gameObject.tag=="Enemy"){
            if (rigid.velocity.y<0 && transform.position.y>collision.transform.position.y){
                OnAttack(collision.transform);
            }
            else{
                OnDamage(collision.transform.position);
                if (gameManager.hp<=0){
                    Die();
                }
            }
        }
    }
    private void OnCollisionExit2D(Collision2D collision){
        grounded=false;
    }
    //hp모두 소진
    public void Die(){
        isDead=true;
        playerAudio.clip=audioDie;
        playerAudio.Play();
        anim.SetTrigger("dead");
        gameManager.EndGame();
    }
    public void Resetplayer(){
        isDead=false;
        anim.Rebind();
        anim.Update(0);
    }
}
