// MoveTo.cs
    using UnityEngine;
    using UnityEngine.AI;
    
    public class moveTo : MonoBehaviour {
       
       private NavMeshAgent agent;
       public Transform goal;
       
       void Start () {
          agent = GetComponent<NavMeshAgent>();
          agent.destination = goal.position; 
       }

       void Update () {
           agent.destination = goal.position;
       }
    }