using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using Aoiti.Pathfinding;
using UnityEngine;
using UnityEngine.UI;

public class EnemyScript : MonoBehaviour
{
    public float MaxHealth;
    public int Money;
    public GameObject Coin;
    public float SpawnedCoinMean;
    public float SpawnedCoinStd;

    private Transform canvas;
    private Slider healthBar;
    private float health;
    
    [SerializeField] float gridSize = 10.0f; //increase patience or gridSize for larger maps
    [SerializeField] float speed = 1.0f; //increase for faster movement
    Pathfinder<Vector2> pathfinder;
     //the pathfinder object that stores the methods and patience
     
     [SerializeField] LayerMask obstacles;
     List<Vector2> pathLeftToGo= new List<Vector2>();
     List <Vector2> path;
     [SerializeField] bool searchShortcut = false;
     private GameObject finishLine = null;

    private void OnEnable()
    {
        canvas = transform.Find("Canvas");
        healthBar = canvas.Find("HealthBar").GetComponent<Slider>();
        canvas.gameObject.SetActive(false);

        health = MaxHealth;
        healthBar.maxValue = MaxHealth;
        healthBar.value = health;
        
        pathfinder = new Pathfinder<Vector2>(GetDistance,GetNeighbourNodes,1000); //increase patience or gridSize for larger maps

        finishLine = GameObject.Find("GroundFinishLine");
        var startLine = GameObject.Find("GroundStart");
        
        if (startLine != null)
        {
            transform.position = startLine.transform.position;
        }

        updatePathFinder();

    }

    public void updatePathFinder()
    {
        var finishLine = GameObject.Find("GroundFinishLine");
        if (finishLine != null)
        {
            GetMoveCommand(finishLine.transform.position);
        }
    }

    private void Update()
    {

        if (gameObject.name == "FakeSoldier")
        {
            return;
        }
        
        //canvas.rotation = Quaternion.identity;
        canvas.localScale = Vector3.one * 0.5f;
        
        if (pathLeftToGo.Count > 0) //if the target is not yet reached
        {
            Vector3 dir =  (Vector3)pathLeftToGo[0]-transform.position ;
            transform.position += dir.normalized * speed;
            if (((Vector2)transform.position - pathLeftToGo[0]).sqrMagnitude <speed*speed) 
            {
                transform.position = pathLeftToGo[0];
                pathLeftToGo.RemoveAt(0);
            }
        }
        
    }
    
    bool GetMoveCommand(Vector2 target)
    {
        Vector2 closestNode = GetClosestNode(transform.position);
        var result = pathfinder.GenerateAstarPath(closestNode, GetClosestNode(target), out path);
        if (result) //Generate path between two points on grid that are close to the transform position and the assigned target.
        {
            pathLeftToGo = new List<Vector2>(path);
            pathLeftToGo.Add(target);
        }
        else
        {
            // Pathfinding is not possible
        }

        return result;
    }

    private void SpawnCoins()
    {
        var num = (int)(MathHelpers.NextGaussianDouble() * SpawnedCoinStd + SpawnedCoinMean + 0.5f);

        for(int i = 0; i < num; i++)
        {
            var x = MathHelpers.NextGaussianDouble() * Mathf.Log(i + 1) * 4.0f;
            var y = MathHelpers.NextGaussianDouble() * Mathf.Log(i + 1) * 4.0f;

            var coin = Pool.Instance.ActivateObject(Coin.tag);
            coin.transform.position = transform.position + new Vector3(x, y, 0);
            coin.SetActive(true);
        }

        GameManager.Instance.AddMoney(Money);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!gameObject.activeSelf) return;

        if(collision.CompareTag("finish"))
        {
            GameManager.Instance.EnemyEscaped(gameObject);
        }

        else if((collision.CompareTag("bullet") && !CompareTag("plane")) || (collision.CompareTag("rocket") && !CompareTag("soldier")))
        {
            var flyingShot = collision.gameObject.GetComponent<FlyingShotScript>();
            var damage = flyingShot.Damage;
            health -= damage;
            healthBar.value = health;
            canvas.gameObject.SetActive(true);
            flyingShot.BlowUp();
            
            if(health <= 0)
            {
                if (CompareTag("plane") || CompareTag("tank"))
                {
                    Pool.Instance.ActivateObject("bigExplosionSoundEffect").SetActive(true);
                    var explosion = Pool.Instance.ActivateObject("explosionParticle");
                    explosion.transform.position = transform.position;
                    explosion.SetActive(true);
                }

                SpawnCoins();
                //GameManager.Instance.EnemyKilled(gameObject);
                Pool.Instance.DeactivateObject(gameObject);
                EnemyManagerScript.Instance.DeleteEnemy(gameObject);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "finish")
        {
            EnemyManagerScript.Instance.DeleteEnemy(gameObject);
            Pool.Instance.DeactivateObject(gameObject);
        }
    }
    
    /// <summary>
    /// A distance approximation. 
    /// </summary>
    /// <param name="A"></param>
    /// <param name="B"></param>
    /// <returns></returns>
    float GetDistance(Vector2 A, Vector2 B) 
    {
        return (A - B).sqrMagnitude; //Uses square magnitude to lessen the CPU time.
    }

    /// <summary>
    /// Finds possible conenctions and the distances to those connections on the grid.
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    Dictionary<Vector2,float> GetNeighbourNodes(Vector2 pos) 
    {
        Dictionary<Vector2, float> neighbours = new Dictionary<Vector2, float>();
        for (int i=-1;i<2;i++)
        {
            for (int j=-1;j<2;j++)
            {
                if (i == 0 && j == 0) continue;

                Vector2 dir = new Vector2(i, j)*gridSize;
                if (!Physics2D.Linecast(pos,pos+dir, obstacles))
                {
                    neighbours.Add(GetClosestNode( pos + dir), dir.magnitude);
                }
            }

        }
        return neighbours;
    }
    
    /// <summary>
    /// Finds closest point on the grid
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    Vector2 GetClosestNode(Vector2 target) 
    {
        return new Vector2(Mathf.Round(target.x/gridSize)*gridSize, Mathf.Round(target.y / gridSize) * gridSize);
    }

    public bool UpdatePathfinding()
    {
        return GetMoveCommand(finishLine.transform.position);
    } 
}
