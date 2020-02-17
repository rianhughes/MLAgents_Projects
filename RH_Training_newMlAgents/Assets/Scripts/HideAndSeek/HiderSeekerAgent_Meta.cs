using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using Barracuda;

public class HiderSeekerAgent_Meta : Agent{
    Rigidbody rb;
    float timeLeft = 10.0f;
    public List<float> VectorActs = new List<float> {0,0,0}; // Actions output by NN
    public bool Seeker = true; // Determine whether agent is the seeker
    bool Seeking = false; // Whether game is in seeking mode
    float Speed = 10; // Current speed
    bool hiderLearning = true;
    bool seekerLearning = true;
    HideAndSeekAcademy m_Academy;
    public HiderSeekerAgent_Meta Enemy; // Refernce to your enemy
    float seekerFloat;
    bool sawEnemy = false;
    float waitPeriod = 1;
    string behaviorName;
    float distanceToSeenEnemey = 0.5f;
    float NumVectObs = 34; // Number of Vector Observations
    string startRoom;
    float Area_1_x 	=  -17;
    float Area_1_z 	=  8;
    float Area_length 	=  16;
    float Area_width 	=  8;
    // Bool to state if the agents attached to this script is recording a demo
    bool recording;
    // Depending on this value, the agent will select a different Brain
    int meta_policy_id;
    // Brain to use when agents wants to move to top-right room
    public NNModel Brain_TR;
    // Brain to use when agents wants to move to bottom-left room
    public NNModel Brain_BL;
    DemonstrationStore demoStore2;
    int cnt;
    bool closeDemoBool;
    string stringDummy;
    bool demoInitBool;

    // Start is called before the first frame update
    void Start(){

      cnt = 1;
      stringDummy = "R";
      demoInitBool=true;
      closeDemoBool=false;
        demoStore2 = new DemonstrationStore(null);
        recording = GetComponent<DemonstrationRecorder>().record;
        startRoom = roomID();

        if(startRoom==startRoom){Debug.Log("startRoom=startRoom");}
        Debug.Log("Start ROOOM:");
        Debug.Log(startRoom);

        behaviorName = GetComponent<BehaviorParameters>().behaviorName;
        m_Academy = FindObjectOfType<HideAndSeekAcademy>();

        if (Seeker) seekerFloat = 1f;
        else seekerFloat = 0f;
        seekerFloat = m_Academy.FloatProperties.GetPropertyWithDefault(behaviorName+"_seeking",seekerFloat);
        if (seekerFloat == 1f) Seeker = true;
        else if (seekerFloat == 0f) Seeker = false;
        if (Seeker) {// Seeker moves faster?
            Speed = 12;
        } else {
            Speed = 10;
        }
        rb = GetComponent<Rigidbody>();
	// Choose a Brain for the agent
        //meta_policy_id = Random.Range(0, 1);
        //    MetaPolicy_UpdateBrain(meta_policy_id);
    }




    public override void CollectObservations() {

        if (!Seeking&& GetStepCount()<waitPeriod) {
            AddVectorObs((waitPeriod - GetStepCount()) / waitPeriod);
        } else {
            AddVectorObs(0);
        }

        AddVectorObs((transform.eulerAngles.y > 180 ? transform.eulerAngles.y - 360f : transform.eulerAngles.y)/180f);
        AddVectorObs(rb.velocity.x / 10f); // Transform variables -- CAN REMOVE THESE [Requires Retraining]
        AddVectorObs(rb.velocity.z / 10f); // Transform variables -- CAN REMOVE THESE [Requires Retraining]
        // AddVectorObs(transform.localPosition.x / 50f);
        // AddVectorObs(transform.localPosition.z / 50f);

        for (int i = 0; i<5; i++) { // Ray cast for either wall or other agent
            float Wall = 0;
            float HiderOrSeeker = 0;
            float Distance = 1f;
            float seenThingAngle = 0;
            float seenThingVelocity_x = 0;
            float seenThingVelocity_y = 0;
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.TransformDirection(PolarToCartesian(142, 70 + i * 10)), out hit, 142f)) {
                seenThingAngle=(hit.transform.eulerAngles.y > 180 ? hit.transform.eulerAngles.y - 360f : hit.transform.eulerAngles.y)/180f;
                if (hit.rigidbody){
                    seenThingVelocity_x = hit.rigidbody.velocity.x/10f;
                    seenThingVelocity_y = hit.rigidbody.velocity.y/10f;
                }
                Distance = hit.distance / 142f;
                if (hit.transform.tag == "agent") {
                    HiderOrSeeker = 1;
                    sawEnemy = true;
                    distanceToSeenEnemey = Distance;
                } else {
                    Wall = 1;
                }
                if (Application.isEditor) {
                    Debug.DrawRay(transform.position,
                        transform.TransformDirection(
                        PolarToCartesian(hit.distance, 70 + i * 10)), Color.black, 0.01f, true);
                }
            } else {
                if (Application.isEditor) {
                    Debug.DrawRay(transform.position,
                        transform.TransformDirection(
                        PolarToCartesian(142, 70 + i * 10)), Color.black, 0.01f, true);
                }
            }
            AddVectorObs(Wall);  // If wall found
            AddVectorObs(HiderOrSeeker); // If hider or seeker found
            AddVectorObs(Distance); // Distance to object
        }
    }




    public override void AgentReset() { // End of episode
        sawEnemy = false;
        seekerFloat = m_Academy.FloatProperties.GetPropertyWithDefault(behaviorName+"_seeking",seekerFloat);
        print(seekerFloat);
        waitPeriod = m_Academy.FloatProperties.GetPropertyWithDefault("waitPeriod",waitPeriod);
        if (seekerFloat == 1f) Seeker = true;
        else if (seekerFloat == 0f) Seeker = false;
        distanceToSeenEnemey = 0.5f;

        Seeking = false;
        // Reset agent to its spawn point
        if (Seeker) {
            transform.eulerAngles = new Vector3(0, Random.Range(0f, 360f), 0);
            transform.position = transform.parent.gameObject.transform.position + new Vector3(Random.Range(Area_1_x-Area_width/2, Area_1_x+Area_width/2), 0, Random.Range(Area_1_z-Area_length/2, Area_1_z+Area_length/2));
        } else {
            transform.eulerAngles = new Vector3(0, Random.Range(0f, 360f), 0);
            transform.position = transform.parent.gameObject.transform.position + new Vector3(Random.Range(10f, 2f), 0, Random.Range(-10f, 10f));
        }
        // Reset action vector
        for (int i = 0; i<VectorActs.Count; i++) {
                VectorActs[i] = 0;
            }
    }



    public override void AgentAction(float[] vectorAction) {
        if (Seeking) { // In seeking mode
            if (Seeker && seekerLearning) {
                AddReward(-0.002f); // Negative survival
            } else if (hiderLearning) {
                AddReward(0.002f); // Positive survival
            }
        } else {
            if (GetStepCount() >= waitPeriod) {   // Wait until seeking mode
                Seeking = true;
            }
        }
        if (Seeker && !Seeking) // Do nothing when not seeking as the seeker
            return;
        for (int i = 0; i<vectorAction.Length; i++) {
            VectorActs[i] = Mathf.Clamp(vectorAction[i],-1.0f,1.0f);
        }

        if (Seeker && sawEnemy) {
            AddReward(Mathf.Clamp(1f-distanceToSeenEnemey,0,1));
            if (!Enemy.Seeker) Enemy.AddReward(Mathf.Clamp(-1f+distanceToSeenEnemey,-1,0));
            else Enemy.AddReward(Mathf.Clamp(1f-distanceToSeenEnemey,0,1));
            if (waitPeriod > 0) {
                Done();
                Enemy.Done();
            }
        }

        if (GetStepCount()>750) { // Stop game after 1500 steps
            if (Seeker && seekerLearning) AddReward(-1f);
            else if (hiderLearning) AddReward(1f);
            Done();}
      }




    private void FixedUpdate() {

      if(startRoom!=roomID()){
        if(startRoom==startRoom){Debug.Log("startRoom=startRoom");}
        Debug.Log("In New ROOOM!!" + Time.fixedTime);
        Debug.Log(startRoom  + Time.fixedTime);
        Debug.Log(roomID()   + Time.fixedTime);
        closeDemoBool=true;
        startRoom=roomID();
      }

      TestStoreInitalize_2(demoStore2, startRoom + stringDummy);


     timeLeft -= Time.deltaTime;
        if (Seeker && !Seeking) // Do nothing when not seeking as the seeker
            return;
        Vector3 new_velociy = new Vector3(0,0, VectorActs[1]);
        new_velociy = transform.TransformVector(new_velociy);
        rb.velocity = Vector3.Lerp(rb.velocity,Speed*new_velociy,Time.deltaTime*10);
        float new_angle = transform.eulerAngles.y -180f + VectorActs[2] * 180f;
        // Debug.Log(transform.eulerAngles.y);
        new_angle = (new_angle  > 180 ? new_angle - 360f : new_angle);
        new_angle = (new_angle  < -180 ? new_angle + 360f : new_angle);
        new_angle += 180f;
        transform.rotation = Quaternion.Lerp(transform.rotation,Quaternion.Euler(new Vector3(0, new_angle,0)),Time.deltaTime*5);
        // transform.rotation = Quaternion.Euler(new Vector3(0, new_angle,0));
    }





    public void TestStoreInitalize_2(DemonstrationStore demoStore, string demonstrationName)
    {

        if(demoInitBool == true){
          Debug.Log("Initialising the demo recording");
          var behaviorParams = GetComponent<BehaviorParameters>();
          demonstrationName = DemonstrationRecorder.SanitizeName(demonstrationName, 16);
          demoStore.Initialize(demonstrationName, behaviorParams.brainParameters, behaviorParams.behaviorName);
          demoInitBool=false;
        }

      demoStore.Record(Info);

      if(closeDemoBool==true){
        Debug.Log("Closing the demo recording");
        demoStore.Close();
        closeDemoBool=false;
        demoInitBool=true;
        }

    }












        void MetaPolicy_UpdateBrain(int config)
        {	Debug.Log(timeLeft);
    	Debug.Log(config);
            if (config == 0)
            {
                GiveModel("Brain_TR", Brain_TR);
    	    //Debug.Log("Choose TR" + behaviorName + config);
            }
            else if (config == 1)
            {
                GiveModel("Brain_BL", Brain_BL);
    	    //Debug.Log("Choose BL" + behaviorName + config);
            }
            else
            {
    	         Debug.Log("Something's wrong with brain selection..");
            }
        }




public void writeIfNewRoom(char startRoom, char crntRoom){
  if(startRoom!=crntRoom){
    char[] chars = {startRoom, crntRoom};
    string sss = new string(chars);
    WriteDemonstration(sss);
    Debug.Log("Writing Sub Demonstration out.");
  }
  else{
    Debug.Log("Something wrong with writing sub demonstration..");
  }
}

public bool inNewRoom(string startRoom){
  string testRoom = roomID();
  bool inNewRoomBool = false;
  bool sameRoom = startRoom==testRoom;
  bool hallRoom = "H"==testRoom;
  bool newRoom  = sameRoom==false && hallRoom==false;
    if(newRoom)
      {inNewRoomBool=true;}
  return inNewRoomBool;
}

public string roomID(){
  // Returns the ID of the room
  // Top-left = A, Top-right = B, Bottom-Right =C, Bottom-Left=D.
  string room_id;
  if(inRoomA())
    { room_id="A"; }
  else if(inRoomB())
    { room_id="B"; }
  else if(inRoomC())
    { room_id="C"; }
  else if(inRoomD())
    { room_id="D"; }
  else
    {room_id="H";}
  return room_id;
}

public bool inRoomA(){
  bool InRoomA;
  bool xrang = transform.position.x < Area_1_x;
  bool zrang = transform.position.z > Area_1_z;
  if( xrang && zrang )
    {InRoomA=true;}
  else
    {InRoomA=false;}
  return InRoomA;
}

public bool inRoomB(){
  bool InRoomA;
  bool xrang = transform.position.x > -Area_1_x;
  bool zrang = transform.position.z >  Area_1_z;
  if( xrang && zrang )
    {InRoomA=true;}
  else
    {InRoomA=false;}
  return InRoomA;
}

public bool inRoomC(){
  bool InRoomA;
  bool xrang = transform.position.x > -Area_1_x;
  bool zrang = transform.position.z <  Area_1_z;
  if( xrang && zrang )
    {InRoomA=true;}
  else
    {InRoomA=false;}
  return InRoomA;
}

public bool inRoomD(){
  bool InRoomA;
  bool xrang = transform.position.x < -Area_1_x;
  bool zrang = transform.position.z <  Area_1_z;
  if( xrang && zrang )
    {InRoomA=true;}
  else
    {InRoomA=false;}
  return InRoomA;
}



        public void WriteDemonstration(string demo_name)
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

            demoStore.Record(agentInfo);//new mlagents release records the agentInfo and brainParams seperately..
            //demoStore.WriteBrainParameters(behaviorName, brainParameters); // I overwrote the permissions...
            demoStore.Close();
        }



            public override float[] Heuristic() {
                if (Input.GetKey(KeyCode.S))
                {
                    return new float[] { 0, -1f, 0 };
                }
                if (Input.GetKey(KeyCode.D))
                {
                    return new float[] { 1f, 0, 0 };
                }
                if (Input.GetKey(KeyCode.W))
                {
                    return new float[] { 0, 1f, 0 };
                }
                if (Input.GetKey(KeyCode.A))
                {
                    return new float[] { -1f, 0, 0 };
                }
                if (Input.GetKey(KeyCode.Q))
                {
                    return new float[] { 0, 0, -0.1f };
                }
                if (Input.GetKey(KeyCode.E))
                {
                    return new float[] { 0, 0, 0.1f };
                }
                return new float[] { 0, 0, 0 };
                // return VectorActs.ToArray();
            }

            public static Vector3 PolarToCartesian(float radius, float angle) {
                var x = radius * Mathf.Cos(angle * Mathf.PI / 180f);
                var z = radius * Mathf.Sin(angle * Mathf.PI / 180f);
                return new Vector3(x, 0f, z);
            }

            private void OnCollisionEnter(Collision collision) { // Rewards for being caught
                if (Seeking&&collision.transform.CompareTag("agent")) {
                    if (!Seeker) AddReward(-10f);
                    else AddReward(10f);
                    Done();
                }


    }
}
