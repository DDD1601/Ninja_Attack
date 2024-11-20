using UnityEngine;

public class IdleState : IState
{
    float randomTime;
    float timer;
    public void OnEnter(Enemy enemy)
    {
        enemy.StopMoving();
        timer = 0;
        randomTime = Random.Range(2f, 4f);
    }

    public void OnExecute(Enemy enemy)
    {
        timer += Time.deltaTime;
        //Debug.Log("IdleState");
        if (timer > randomTime)
        {
            enemy.ChangeState(new PatrolState());
        }


    }

    public void OnExit(Enemy enemy)
    {

    }
}
