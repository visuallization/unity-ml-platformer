using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using UnityEngine;
using System.Collections.Generic;

public class SimpleCharacterAgent: Agent
{
    [Tooltip("The platform, which will be instantiated & moved around on every reset")]
    public GameObject platformPrefab;
    public GameObject startPlatform;
    public float platformSpawnDistance = 3f;

    private Vector3 startPosition;
    private SimpleCharacterController characterController;
    new private Rigidbody rigidbody;

    private Queue<GameObject> platforms;

    /// <summary>
    /// Called once when the agent is first initialized
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();
        startPosition = transform.position;
        characterController = GetComponent<SimpleCharacterController>();
        rigidbody = GetComponent<Rigidbody>();
        platforms = new Queue<GameObject>();
    }

    /// <summary>
    /// Called every time an episode begins. This is where we reset the challenge.
    /// </summary>
    public override void OnEpisodeBegin()
    {
        startPlatform.SetActive(true);

        transform.position = startPosition;
        transform.rotation = Quaternion.Euler(Vector3.up * Random.Range(0f, 360f));
        rigidbody.velocity = Vector3.zero;
        // Reset platform position (5 meters away from the agent in a random direction)

        foreach (GameObject p in platforms)
        {
            Destroy(p);
        }
        platforms.Clear();

        GameObject platform = Instantiate(
            platformPrefab,
            new Vector3(transform.position.x, platformPrefab.transform.position.y, transform.position.z) + Quaternion.Euler(Vector3.up * Random.Range(0f, 360f)) * Vector3.forward * platformSpawnDistance,
            Quaternion.identity,
            transform.parent
        );

        platforms.Enqueue(platform);
    }

    /// <summary>
    /// Controls the agent with human input
    /// </summary>
    /// <param name="actionsOut">The actions parsed from keyboard input</param>
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Read input values and round them. GetAxisRaw works better in this case
        // because of the DecisionRequester, which only gets new decisions periodically.
        int vertical = Mathf.RoundToInt(Input.GetAxisRaw("Vertical"));
        int horizontal = Mathf.RoundToInt(Input.GetAxisRaw("Horizontal"));
        bool jump = Input.GetKey(KeyCode.Space);

        // Convert the actions to discrete choices (0, 1, 2)
        ActionSegment<int> actions = actionsOut.DiscreteActions;
        actions[0] = vertical >= 0 ? vertical : 2;
        actions[1] = horizontal >= 0 ? horizontal : 2;
        actions[2] = jump ? 1 : 0;
    }

    /// <summary>
    /// React to actions coming from either the neural net or human input
    /// </summary>
    /// <param name="actions">The actions received</param>
    public override void OnActionReceived(ActionBuffers actions)
    {
        // Convert actions from Discrete (0, 1, 2) to expected input values (-1, 0, +1)
        // of the character controller
        float vertical = actions.DiscreteActions[0] <= 1 ? actions.DiscreteActions[0] : -1;
        float horizontal = actions.DiscreteActions[1] <= 1 ? actions.DiscreteActions[1] : -1;
        bool jump = actions.DiscreteActions[2] > 0;

        characterController.ForwardInput = vertical;
        characterController.TurnInput = horizontal;
        characterController.JumpInput = jump;
    }

    /// <summary>
    /// Respond to entering a trigger collider
    /// </summary>
    /// <param name="other">The object (with trigger collider) that was touched</param>
    private void OnTriggerEnter(Collider other)
    {
        // If the other object is a collectible, reward and end episode
        if (other.tag == "collectible")
        {
            AddReward(1f);

            if (startPlatform.activeSelf)
            {
                startPlatform.SetActive(false);
            }

            // Destroy platform & create a new one
            GameObject platform;
            if (platforms.Count >= 2)
            {
                platform = platforms.Dequeue();
                Destroy(platform);
            }

            platform = Instantiate(
                platformPrefab,
                new Vector3(transform.position.x, platformPrefab.transform.position.y, transform.position.z) + Quaternion.Euler(Vector3.up * Random.Range(0f, 360f)) * Vector3.forward * platformSpawnDistance,
                Quaternion.identity,
                transform.parent
            );
            platforms.Enqueue(platform);
        }

        if (other.tag == "gameover")
        {

            //AddReward(-1f);
            EndEpisode();
        }
    }
}