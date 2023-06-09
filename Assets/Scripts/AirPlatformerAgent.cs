using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using System.Collections.Generic;

public class AirPlatformerAgent: Agent
{
    [Tooltip("The platform, which will be instantiated & moved around on every reset")]
    public GameObject platformPrefab;
    public GameObject startPlatform;
    public float platformSpawnDistance = 3f;

    private Vector3 startPosition;
    private SimpleCharacterController characterController;
    new private Rigidbody rigidbody;

    private Queue<GameObject> platforms;
    private GameObject goalPlatform;
    private float bestGoalDistance = 0;

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
        goalPlatform = null;
        ResetBestGoalDistance();
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
        goalPlatform = platform;
        ResetBestGoalDistance();
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
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;

        continuousActions[0] = vertical;
        continuousActions[1] = horizontal;
        discreteActions[0] = jump ? 1 : 0;
    }

    /// <summary>
    /// React to actions coming from either the neural net or human input
    /// </summary>
    /// <param name="actions">The actions received</param>
    public override void OnActionReceived(ActionBuffers actions)
    {
        // Convert actions from Discrete (0, 1, 2) to expected input values (-1, 0, +1)
        // of the character controller
        float vertical = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float horizontal = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
        bool jump = actions.DiscreteActions[0] > 0;

        characterController.ForwardInput = vertical;
        characterController.TurnInput = horizontal;
        characterController.JumpInput = jump;

        UpdateRewardPerStep();
    }

    /// <summary>
    /// Add more observations
    /// Unity ML Agents observes the values of RayPerceptionSensors automatically
    /// This method allows to add more observations
    /// </summary>
    /// <param name="sensor">Sensor to add observations to</param>
    public override void CollectObservations(VectorSensor sensor)
    {
        float goalDistance = 0.0f;
        Vector3 goalVector = Vector3.zero;
        goalDistance = Vector3.Distance(transform.position, goalPlatform.transform.position);
        goalDistance = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(goalPlatform.transform.position.x, goalPlatform.transform.position.z));
        goalDistance = Mathf.Clamp(goalDistance, 0, platformSpawnDistance) / platformSpawnDistance;
        goalVector = (goalPlatform.transform.position - transform.position).normalized;
        //Debug.Log(transform.position);
        //Debug.Log(goalPlatform.transform.position);
        //Debug.Log(goalDistance);

        sensor.AddObservation(goalDistance);
        sensor.AddObservation(goalVector);
        sensor.AddObservation(characterController.IsGrounded);
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
            goalPlatform = platform;
            ResetBestGoalDistance();
        }

        if (other.tag == "gameover")
        {

            //AddReward(-1f);
            EndEpisode();
        }
    }

    private void UpdateRewardPerStep() {
        AddReward(-0.01f);
        AddReward(ShapingReward());
    }

    private float ShapingReward() {
        float reward = 0f;
        float goalDistance = 0f;

        goalDistance = Vector3.Distance(transform.position, goalPlatform.transform.position);
        goalDistance = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(goalPlatform.transform.position.x, goalPlatform.transform.position.z));

        if (goalDistance < bestGoalDistance) {
            reward += bestGoalDistance - goalDistance;
            bestGoalDistance = goalDistance;
        }
        
        return reward;
    }

    private void ResetBestGoalDistance() {
        bestGoalDistance = platformSpawnDistance;
    }
}
