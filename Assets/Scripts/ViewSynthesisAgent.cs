using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class ViewSynthesisAgent : Agent
{
    public int _multiplier;
    public Transform AgentCamera;
    public Transform TargetCamera;
    public RenderTexture TargetRT;
    public RenderTexture AgentRT;
    public Transform TargetFocalPoint;
    public Transform AgentFocalPoint;


    private Texture2D TargetTex;
    private Texture2D AgentTex;
    private float loss;
    private float _threshold;
    private int iter;

    // Start is called before the first frame update
    void Start()
    {
        int texhight = TargetRT.height;
        int texwidth = TargetRT.width;
        TargetTex = new Texture2D(texhight, texwidth, TextureFormat.RGB24, false);
        AgentTex = new Texture2D(texhight, texwidth, TextureFormat.RGB24, false);
        loss = 0;
        _threshold = 0.9f;
        iter = 0;
    }

    public override void OnEpisodeBegin()
    {
        AgentCamera.localPosition = new Vector3(0, 0.5f, 0);
        AgentCamera.LookAt(AgentFocalPoint);

        TargetCamera.localPosition = new Vector3(Random.Range(-10.0f, 10.0f), Random.Range(0.5f, 10.0f), Random.Range(-10.0f, 10.0f));
        TargetCamera.LookAt(TargetFocalPoint);

        SetTexture(TargetTex, TargetRT);
        SetTexture(AgentTex, AgentRT);
        iter++;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        loss = GetPixelLoss(TargetTex, AgentTex);
        //Debug.Log("Loss = "+loss);
        sensor.AddObservation(loss);
        sensor.AddObservation(AgentCamera.localPosition);
        sensor.AddObservation(AgentCamera.rotation);
        
    }

    public override void OnActionReceived(ActionBuffers actionBuffers){
        //AddReward(-0.001f);

        Vector3 positioncontrol = Vector3.zero;
        positioncontrol.x = actionBuffers.ContinuousActions[0];
        positioncontrol.y = actionBuffers.ContinuousActions[1];
        positioncontrol.z = actionBuffers.ContinuousActions[2];

         AgentCamera.Translate(positioncontrol*_multiplier);
        //AgentCamera.localPosition = positioncontrol*_multiplier;
        AgentCamera.LookAt(AgentFocalPoint);
        SetTexture(AgentTex, AgentRT);

        if(AgentCamera.position.y < 0 
        || AgentCamera.position.y > 31 
        || AgentCamera.position.x > 31 
        || AgentCamera.position.x < -31 
        || AgentCamera.position.z > 31 
        || AgentCamera.position.z < -31 ){
            AddReward(-10.0f);
            EndEpisode();
        } 

        loss = GetPixelLoss(TargetTex, AgentTex);
        RewardTime(loss, TargetTex.height*TargetTex.width, _threshold);
    }

    // public override void Heuristic(in ActionBuffers actionsOut){
    //     ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
    //     continuousActions[0] = Input.GetAxisRaw("Horizontal");
    //     continuousActions[1] = Input.GetKey(KeyCode.Space) ? 1.0f : 0.0f;
    //     continuousActions[2] = Input.GetAxisRaw("Vertical");

    // }

    private void SetTexture(Texture2D tex, RenderTexture rt)
    {
        // ReadPixels looks at the active RenderTexture.
        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();
    }

    private float GetPixelLoss(Texture2D tex1, Texture2D tex2)
    {
        int height = tex1.height;
        int width = tex1.width;
        float onepixelloss = 0;
        float meanpixelloss = 0;

        Color[] pixels1 = tex1.GetPixels();
        Color[] pixels2 = tex2.GetPixels();

        for(int i = 0; i<pixels1.Length; i++){
            onepixelloss = (pixels1[i].r - pixels2[i].r) + (pixels1[i].g - pixels2[i].g) + (pixels1[i].b - pixels2[i].b);
            onepixelloss = onepixelloss*onepixelloss;
            meanpixelloss += (onepixelloss/3);
        }
        meanpixelloss /= pixels1.Length;
        return meanpixelloss;
    }

    private void RewardTime(float loss, int pixels_num, float threshold = 0.9f){
        if(loss < 0.1f) {
            SetReward(0.01f);
            //Debug.Log(iter + " : Not bad!");
        }
        if(loss < 0.05f) {
            SetReward(0.05f);
            //Debug.Log(iter + " : Good!");
        }
        if(loss < 0.01f){
            SetReward(0.1f);
            Debug.Log(iter + " : Almost!");
        }
        if(loss < 0.001f) {
            SetReward(10.0f);
            Debug.Log(iter + " : Correct!");
            //Thread.Sleep(5000); //학습결과 확인용
            EndEpisode();
        }
    }
}
