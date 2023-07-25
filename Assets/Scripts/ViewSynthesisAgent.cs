using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class ViewSynthesisAgent : Agent
{
    // Start is called before the first frame update
    void Start()
    {
        GetTexture("Answer_tex");
    }

    public override void OnEpisodeBegin()
    {
        this.GetChild(3).localPosition = new Vector3(Random.value * 8 - 4, 0.5f, Random.value * 8 - 4);
        this.GetChild(3).localRotation = Quaternion.Euler(0, Random.value * 360, 0);
    }

    public override void CollectObservations(VectorSensor sensor){
        sensor.AddObservation(this.GetChild(3).localPosition);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers){
        Vector3 positioncontrol = Vector3.zero;
        Vector3 rotationcontrol = Vector3.zero;
        positioncontrol.x = actionBuffers.ContinuousActions[0];
        positioncontrol.z = actionBuffers.ContinuousActions[2];
        positioncontrol.y = actionBuffers.ContinuousActions[1];
        rotationcontrol.x = actionBuffers.ContinuousActions[3];
        rotationcontrol.y = actionBuffers.ContinuousActions[4];
        rotationcontrol.z = actionBuffers.ContinuousActions[5];

        this.GetChild(3).localPosition += positioncontrol;
        this.GetChild(3).localRotation *= Quaternion.Euler(rotationcontrol);

        float pixelLoss = 1;//픽셀로스 구현해야합니다.

        if(pixelLoss < 0.1f){
            SetReward(1.0f);
            EndEpisode();
        }
        if(this.GetChild(3).localPosition.y < 0 || this.GetChild(3).localPosition.y > 30 || this.GetChild(3).localPosition.x > 30 || this.GetChild(3).localPosition.x < -30 || this.GetChild(3).localPosition.z > 30 || this.GetChild(3).localPosition.z < -30 ){
            EndEpisode();
        }

    }

    private Texture2D GetTexture(string filename)
    {
        RenderTexture rt = (RenderTexture)Resources.Load("Textures/" + filename);
        Texture2D tex = new Texture2D(512, 512, TextureFormat.RGB24, false);

        // ReadPixels looks at the active RenderTexture.
        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();
        Debug.Log(tex.GetPixel(0, 0));

        return tex;
    }
}
