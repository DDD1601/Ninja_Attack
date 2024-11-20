using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] private Animator anim;
    private float hp;

    public bool isDead => hp <= 0;
    private string currentAnimName;

    private void Start()
    {
        OnInit();
        
    }

    public virtual void OnInit()
    {
        hp = 100;

    }

    public virtual void OnDespawn()
    {

    }

    protected virtual void OnDeath()
    {
        ChangeAnim("die");
        Invoke(nameof(OnDespawn), 2f);

    }

    protected void ChangeAnim(string animName)
    {
        if (currentAnimName != animName)
        {
            anim.ResetTrigger(animName);
            currentAnimName = animName;
            anim.SetTrigger(currentAnimName);
            if(this is Player)
            {
                Debug.Log(animName);
            }
        }
    }

    public void OnMit(float damage)
    {
        if (!isDead)
        {
            hp -= damage;

            if (isDead)
            {
                OnDeath();
            }
        }
    }


}
