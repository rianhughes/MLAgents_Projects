using UnityEngine;
using MLAgents;
using MLAgents.Sensor;
using System.Collections.Generic;
using MLAgents.CommunicatorObjects;

public class BasicAgent_Meta : Agent
{
    [Header("Specific to Basic")]
    BasicAcademy m_Academy;
    public float timeBetweenDecisionsAtInference;
    float m_TimeSinceDecision;
    int m_Position;
    int m_SmallGoalPosition;
    int m_LargeGoalPosition;
    public GameObject largeGoal;
    public GameObject smallGoal;
    int m_MinPosition;
    int m_MaxPosition;
    int cnt;
    public bool record;
    //Camera cam = GameObject.Find("BasicAgent").GetComponent<Camera>();
    //[FormerlySerializedAs("m_Sensors")]
    public List<ISensor> sensors;
    DemonstrationRecorderOverRide MM_Recorder;

    //AgentInfo m_Info;

    public override void InitializeAgent()
    {
      cnt=1;
      m_Academy = FindObjectOfType(typeof(BasicAcademy)) as BasicAcademy;
      MM_Recorder = GetComponent<DemonstrationRecorderOverRide>();
    }

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
        // The .demo file is of the following format:
        // (meta?), [agent_info {...} action_info {...} , agent_info {...} action_info{..}, numberExperiences ]
        if(cnt<10){TestStoreInitalize_2();Debug.Log("TestSotreInitialsed!");}
        //if(cnt==2){TestAgentWrite_2();Debug.Log("TestSotreWrote!");}
    }

    public void TestStoreInitalize_2()
    {
      var brainParameters = new BrainParameters
      {
          vectorObservationSize = 20,
          numStackedVectorObservations = 2,
          vectorActionDescriptions = new[] { "TestActionA", "TestACTION" },
          vectorActionSize = new[] { 1 },
          vectorActionSpaceType = SpaceType.Discrete
      };

      var agentInfo = new AgentInfo
        {
            reward = 20f,
            actionMasks = new[] {  true },
            done = true,
            id = 5,
            maxStepReached = true,
           storedVectorActions = new[] { 0f },
          };

      Debug.Log("#####");
      Debug.Log(cnt);
      Debug.Log("#####");
      const string k_ExtensionType = ".demo";
      string brainName="dummy";
      string demonstrationName = "ZXC";// + Time.fixedTime;


      var demoStore = new DemonstrationStoreOverRide(null);

      //MM_Recorder = GetComponent<DemonstrationRecorderOverRide>();

        if(true){
          Debug.Log("Initialization of Demo!!!");
          var behaviorParams = GetComponent<BehaviorParameters>();
          demonstrationName = demoStore.SanitizeName(demonstrationName, 16);
          demoStore.Initialize(demonstrationName, behaviorParams.brainParameters, behaviorParams.behaviorName);
        }

        //demoStore.WriteBrainParameters(brainName, brainParameters);
        demoStore.Record(agentInfo);
        demoStore.Record(agentInfo);
        //demoStore.WriteBrainParameters(brainName, brainParameters);
        demoStore.Record(agentInfo);
        demoStore.Record(agentInfo);
        demoStore.Record(agentInfo);
        //demoStore.WriteBrainParameters(brainName, brainParameters);

        Debug.Log("CLOOOSING Demo");
        demoStore.Close();

        cnt+=1;
    }

    // public void TestAgentWrite_2()
    // {
    //     var agentGo1 = new GameObject("BasicAgent2");
    //     var bpA = agentGo1.AddComponent<BehaviorParameters>();
    //     bpA.brainParameters.vectorObservationSize = 3;
    //     bpA.brainParameters.numStackedVectorObservations = 1;
    //     bpA.brainParameters.vectorActionDescriptions = new[] { "TestActionA", "TestActionB" };
    //     bpA.brainParameters.vectorActionSize = new[] { 2, 2 };
    //     bpA.brainParameters.vectorActionSpaceType = SpaceType.Discrete;
    //
    //     agentGo1.AddComponent<ObservationAgent>();
    //     var agent1 = agentGo1.GetComponent<ObservationAgent>();
    //
    //     agentGo1.AddComponent<DemonstrationRecorderOverRide>();
    //     var demoRecorder = agentGo1.GetComponent<DemonstrationRecorderOverRide>();
    //     var fileSystem = new MockFileSystem();
    //     demoRecorder.demonstrationName = "TestBrain__RH";
    //     demoRecorder.record = true;
    //     demoRecorder.InitializeDemoStore(fileSystem);
    //
    //     var acaGo = new GameObject("TestAcademy");
    //     acaGo.AddComponent<TestAcademy>();
    //     var aca = acaGo.GetComponent<TestAcademy>();
    //
    //     agentEnableMethod?.Invoke(agent1, new object[] { });
    //     academyInitializeMethod?.Invoke(aca, new object[] { });
    //
    //     // Step the agent
    //     agent1.RequestDecision();
    //     agentSendInfo?.Invoke(agent1, new object[] { });
    //
    //     demoRecorder.Close();
    //
    //     // Read back the demo file and make sure observations were written
    //     var reader = fileSystem.File.OpenRead("Assets/Demonstrations/TestBrain.demo");
    //     reader.Seek(DemonstrationStore.MetaDataBytes + 1, 0);
    //     BrainParametersProto.Parser.ParseDelimitedFrom(reader);
    //
    //     var agentInfoProto = AgentInfoActionPairProto.Parser.ParseDelimitedFrom(reader).AgentInfo;
    //     var obs = agentInfoProto.Observations[2]; // skip dummy sensors
    //     {
    //         var vecObs = obs.FloatData.Data;
    //         Assert.AreEqual(bpA.brainParameters.vectorObservationSize, vecObs.Count);
    //         for (var i = 0; i < vecObs.Count; i++)
    //         {
    //             Assert.AreEqual((float)i + 1, vecObs[i]);
    //         }
    //     }
    //
    //
    // }


    void WaitTimeInference()
    {
        if (!m_Academy.IsCommunicatorOn)
        //if (!Academy.Instance.IsCommunicatorOn)
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
