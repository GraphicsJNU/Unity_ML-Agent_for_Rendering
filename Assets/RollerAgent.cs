using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class RollerAgent : Agent
{
    Rigidbody rBody;

    //inspector
    public Transform Target;
    public float forceMultiplier = 10;

    // Start is called before the first frame update
    void Start()
    {
        rBody = GetComponent<Rigidbody>();    
    }

    //Episode 설계
    public override void OnEpisodeBegin(){
        if(this.transform.localPosition.y < 0){
            this.rBody.angularVelocity = Vector3.zero;
            this.rBody.velocity = Vector3.zero;
            this.transform.localPosition = new Vector3(0, 0.5f, 0);
        }
        Target.localPosition = new Vector3(Random.value * 8 -4, 0.5f, Random.value * 8 -4);
    }

    //정보 수집
    public override void CollectObservations(VectorSensor sensor){
        //Target, Agent의 위치 정보 수집
        sensor.AddObservation(Target.localPosition);
        sensor.AddObservation(this.transform.localPosition);

        //Agent의 속도 정보 수집
        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.z);
    }

    //행동 결정
    public override void OnActionReceived(ActionBuffers actionBuffers){
        //Agent가 Target쪽으로 이동하기 위해 x, y축의 Force 정의
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.z = actionBuffers.ContinuousActions[1];
        rBody.AddForce(controlSignal * forceMultiplier);

        //Agent와 Target 사이 거리 측정(아마 Loss?)
        float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);

        //Episode 종료 조건
        if(distanceToTarget < 1.42f){
            SetReward(1.0f);
            EndEpisode();
        }
        if(this.transform.localPosition.y < 0){
            EndEpisode();
        }
    }

    //키보드 입력을 통한 행동 결정(테스트용)
    public override void Heuristic(in ActionBuffers actionsOut){
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
}
