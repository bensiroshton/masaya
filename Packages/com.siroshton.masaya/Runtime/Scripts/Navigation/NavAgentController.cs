using UnityEngine;
using UnityEngine.AI;

namespace Siroshton.Masaya.Navigation
{
    public class NavAgentController : MonoBehaviour
    {
        static public void EnableAgent(GameObject obj)
        {
            EnableAgent(obj.GetComponent<NavMeshAgent>());
        }

        static public void EnableAgent(NavMeshAgent agent)
        {
            if (agent != null) agent.enabled = true;
        }

        static public void DisableAgent(GameObject obj)
        {
            DisableAgent(obj.GetComponent<NavMeshAgent>());
        }

        static public void DisableAgent(NavMeshAgent agent)
        {
            if (agent != null) agent.enabled = false;
        }

        static public void TargetPlayer(GameObject obj)
        {
            TargetPlayer(obj.GetComponent<NavAgentGameObjectTarget>());
        }

        static public void TargetPlayer(NavAgentGameObjectTarget agent)
        {
            if (agent != null) agent.target = Player.Player.instance.gameObject;
        }

        static public void EnableUpdateRotation(NavMeshAgent agent)
        {
            if (agent != null) agent.updateRotation = true;
        }

        static public void DisableUpdateRotation(NavMeshAgent agent)
        {
            if (agent != null) agent.updateRotation = false;
        }

    }
}