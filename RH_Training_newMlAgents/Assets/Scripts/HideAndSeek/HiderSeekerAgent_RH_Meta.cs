using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using Barracuda;

public class HiderSeekerAgent_RH_Meta : Agent{
    Rigidbody rb; 
    float timeLeft = 10.0f;
    public List<float> VectorActs = new List<float> {0,0,0}; // Actions output by NN 
    public bool Seeker = true; // Determine whether agent is the seeker 
    bool Seeking = false; // Whether game is in seeking mode 
    float Speed = 10; // Current speed 
    bool hiderLearning = true; 
    bool seekerLearning = true;
    HideAndSeekAcademy m_Academy;
    public HiderSeekerAgent_RH_Meta Enemy; // Refernce to your enemy 
    float seekerFloat;
    bool sawEnemy = false;
    float waitPeriod = 1;
    string behaviorName;
    float distanceToSeenEnemey = 0.5f;
    float NumVectObs = 34; // Number of Vector Observations

    float Area_1_x 	=  -17;
    float Area_1_z 	=  8;
    float Area_length 	=  16;
    float Area_width 	=  8;
    // Depending on this value, the agent will select a different Brain
    int meta_policy_id;
    // Brain to use when agents wants to move to top-right room
    public NNModel Brain_TR;
    // Brain to use when agents wants to move to bottom-left room
    public NNModel Brain_BL;

    // Start is called before the first frame update
    void Start(){

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
        meta_policy_id = Random.Range(0, 1);
            ConfigureAgent(meta_policy_id);
    }

    public override void CollectObservations() {

        if (!Seeking&& GetStepCount()<waitPeriod) { 
            AddVectorObs((waitPeriod - GetStepCount()) / waitPeriod);
        } else {
            AddVectorObs(0); 
        }
        AddVectorObs(rb.velocity.x / 10f); // Transform variables -- CAN REMOVE THESE [Requires Retraining]
        AddVectorObs(rb.velocity.z / 10f); // Transform variables -- CAN REMOVE THESE [Requires Retraining]
        AddVectorObs((transform.eulerAngles.y > 180 ? transform.eulerAngles.y - 360f : transform.eulerAngles.y)/180f);
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
            Done();
	}

	// Choose a Brain for the agent
	if (timeLeft<-10){
        ConfigureAgent(0);
	timeLeft=10f;
	}
	else if (timeLeft<0) {
        ConfigureAgent(Random.Range(0, 2));
	}

    }

    /// NB: Currently both agents use the same set of Brains.
    /// If 0 : Choose policy top-right
    /// If 1 : Choose policy bottom-left
    void ConfigureAgent(int config)
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

    private void FixedUpdate() {
     timeLeft -= Time.deltaTime;

        if (Seeker && !Seeking) // Do nothing when not seeking as the seeker 
            return;
	
	/// When do we want  to change the brain??


        // Convert NN output to action 
        // Lerp is to interpolate between the current velocity (rb.velocity)
        // and the target velocity
        // in a time Time.deltaTime*10
        // we use that because we take a decision every 10 frames.
        // The actions have interpretation of (speed, angle of velocity in x-z plane, and angle)
        // rb.velocity = Vector3.Lerp(rb.velocity,Speed*VectorActs[0]*new Vector3(Mathf.Sin(VectorActs[1]*Mathf.PI),0, Mathf.Cos(VectorActs[1] * Mathf.PI)),Time.deltaTime*10);
//        Vector3 new_velociy = new Vector3(VectorActs[0],0, VectorActs[1]);
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
