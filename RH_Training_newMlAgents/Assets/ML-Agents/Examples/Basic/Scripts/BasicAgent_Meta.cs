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
    //public List<ISensor> sensors;
    DemonstrationRecorderOverRide MM_Recorder;
    DemonstrationStoreOverRide demoStore2;

    public override void InitializeAgent()
    {
      cnt=1;
      m_Academy = FindObjectOfType(typeof(BasicAcademy)) as BasicAcademy;
      MM_Recorder = GetComponent<DemonstrationRecorderOverRide>();
      demoStore2 = new DemonstrationStoreOverRide(null);
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
        Debug.Log(cnt);
        if(cnt<5){TestStoreInitalize_2(demoStore2);Debug.Log("TestSotreInitialsed! 1 !");}
        if(cnt>4 && cnt <10 ){TestStoreInitalize_2(demoStore2);Debug.Log("TestSotreInitialsed! 2 !");}

    }

    public void TestStoreInitalize_2(DemonstrationStoreOverRide demoStore)
    {
      // var brainParameters = new BrainParameters
      // {
      //     vectorObservationSize = 20,
      //     numStackedVectorObservations = 2,
      //     vectorActionDescriptions = new[] { "TestActionA", "TestACTION" },
      //     vectorActionSize = new[] { 1 },
      //     vectorActionSpaceType = SpaceType.Discrete
      // };
      //
      // var agentInfo = new AgentInfo
      //   {
      //       reward = 20f,
      //       actionMasks = new[] {  true },
      //       done = true,
      //       id = 5,
      //       maxStepReached = true,
      //      storedVectorActions = new[] { 0f },
      //     };

      string demonstrationName = "ASD_A";
      string demonstrationName2 = "ASD_B";

        if(cnt==1){
          var behaviorParams = GetComponent<BehaviorParameters>();
          demonstrationName = demoStore.SanitizeName(demonstrationName, 16);
          demoStore.Initialize(demonstrationName, behaviorParams.brainParameters, behaviorParams.behaviorName);
        }
        if(cnt ==5){
          var behaviorParams = GetComponent<BehaviorParameters>();
          demonstrationName2 = demoStore.SanitizeName(demonstrationName2, 16);
          demoStore.Initialize(demonstrationName2, behaviorParams.brainParameters, behaviorParams.behaviorName);
        }

        if(cnt>1 ){
          demoStore.Record(Info);
          }
        if(cnt==4 || cnt==9){
          demoStore.Close();
          }

        cnt+=1;
    }


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
