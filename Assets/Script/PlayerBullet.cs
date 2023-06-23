using UnityEngine;

public class PlayerBullet : MonoBehaviour, IActorTemplate
{
    GameObject actor;
    int hitPower;
    int health;
    int travelSpeed;

    [SerializeField]
    SOActorModel bulletModel;

    void Awake()
    {
        ActorStats(bulletModel);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += new Vector3(travelSpeed,0,0)*Time.deltaTime;
    }

    public int SendDamage()
    {
        return hitPower;
    }

    public void TakeDamage(int incomingDamage)
    {
        health -= incomingDamage;
    }
    
    public void Die()
    {
        Destroy(this.gameObject);
    }

    public void ActorStats(SOActorModel actorModel)
    {
        hitPower = actorModel.hitPower;
        health = actorModel.health;
        travelSpeed = actorModel.speed;
        actor = actorModel.actor;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy")
        {
            if(other.GetComponent<IActorTemplate>() != null)
            {
                if (health >= 1)
                {
                    health -= other.
                    GetComponent<IActorTemplate>().SendDamage();
                }
                if (health <= 0)
                {
                    Die();
                }
            }
        }
    }

    void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}