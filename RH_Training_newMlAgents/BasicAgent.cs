using UnityEngine;
using MLAgents;
using MLAgents.Sensor;
using System.Collections.Generic;

public class BasicAgent : Agent
{
    [Header("Specific to Basic")]
    public float timeBetweenDecisionsAtInference;
    float m_TimeSinceDecision;
    int m_Position;
    int m_SmallGoalPosition;
    int m_LargeGoalPosition;
    public GameObject largeGoal;
    public GameObject smallGoal;
    int m_MinPosition;
    int m_MaxPosition;
    //Camera cam = GameObject.Find("BasicAgent").GetComponent<Camera>();
    //[FormerlySerializedAs("m_Sensors")]
    public List<ISensor> sensors;

    //AgentInfo m_Info;

    public override void CollectObservations()
    {
        AddVectorObs(m_Position, 20);
    }

    public override void AgentAction(float[] vectorAction)
    {
        var movement = (int)vectorAction[0];

        var direction = 0;

        switch (movement)
        {
            case 1:
                direction = -1;
                break;
            case 2:
                direction = 1;
                break;
        }

        m_Position += direction;
        if (m_Position < m_MinPosition) { m_Position = m_MinPosition; }
        if (m_Position > m_MaxPosition) { m_Position = m_MaxPosition; }

        gameObject.transform.position = new Vector3(m_Position - 10f, 0f, 0f);

        AddReward(-0.01f);

        if (m_Position == m_SmallGoalPosition)
        {
            Done();
            AddReward(0.1f);
        }

        if (m_Position == m_LargeGoalPosition)
        {
            Done();
            AddReward(1f);
        }
    }

    public override void AgentReset()
    {
        m_Position = 10;
        m_MinPosition = 0;
        m_MaxPosition = 20;
        m_SmallGoalPosition = 7;
        m_LargeGoalPosition = 17;
        smallGoal.transform.position = new Vector3(m_SmallGoalPosition - 10f, 0f, 0f);
        largeGoal.transform.position = new Vector3(m_LargeGoalPosition - 10f, 0f, 0f);
    }

    public override float[] Heuristic()
    {
        if (Input.GetKey(KeyCode.D))
        {
            return new float[] { 2 };
        }
        if (Input.GetKey(KeyCode.A))
        {
            return new float[] { 1 };
        }
        return new float[] { 0 };
    }

    public void FixedUpdate()
    {
        WaitTimeInference();
        string demo_name = "Test";
        if(BoolWriteOutput()){Debug.Log("saving demonstration");TestStoreInitalize_2(string demo_name);}
    }


    public void BoolWriteOutput(){
    bool willIWrite;

    return willIWrite;
    }

    public void TestStoreInitalize_2()
    {
      const string k_DemoDirecory = "Assets/Demonstrations/";
      const string k_ExtensionType = ".demo";
      string k_DemoName = demo_name; //"TestStill" + Time.fixedTime;
        var demoStore = new DemonstrationStore(null);

        //Assert.IsFalse(fileSystem.Directory.Exists(k_DemoDirecory));

        var brainParameters = new BrainParameters
        {
            vectorObservationSize = 3,
            numStackedVectorObservations = 2,
            vectorActionDescriptions = new[] { "TestActionA", "TestActionB" },
            vectorActionSize = new[] { 2, 2 },
            vectorActionSpaceType = SpaceType.Discrete
        };

        demoStore.Initialize(k_DemoName, brainParameters, "TestBrain");

        //Assert.IsTrue(fileSystem.Directory.Exists(k_DemoDirecory));
        //Assert.IsTrue(fileSystem.FileExists(k_DemoDirecory + k_DemoName + k_ExtensionType));

        var agentInfo = new AgentInfo
        {
            reward = 1f,
            actionMasks = new[] { false, true },
            done = true,
            id = 5,
            maxStepReached = true,
            storedVectorActions = new[] { 0f, 1f },
        };

        demoStore.Record(agentInfo, sensors );//new System.Collections.Generic.List<MLAgents.Sensor.ISensor>());
        demoStore.Close();
    }

    void WaitTimeInference()
    {
        if (!Academy.Instance.IsCommunicatorOn)
        {
            RequestDecision();
        }
        else
        {
            if (m_TimeSinceDecision >= timeBetweenDecisionsAtInference)
            {
                m_TimeSinceDecision = 0f;
                RequestDecision();
            }
            else
            {
                m_TimeSinceDecision += Time.fixedDeltaTime;
            }
        }
    }
}
