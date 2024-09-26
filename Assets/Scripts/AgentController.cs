using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class AgentController : MonoBehaviour
{
    private float moveSpeed = 3.1f;
    private readonly float innerRadius = 6.6f;// 分離領域半径
    private readonly float separatePower = 1.2f;// 分離力
    private readonly float alignPower = 3.3f;// 整列力
    private readonly float cohesionPower = 0.6f;// 結合力
    private readonly float destinationPower = 0.2f;// 目的地への重視度

    private Rigidbody rb;
    private List<AgentController> outerList = new();// 認識したエージェントのリスト
    private List<GameObject> objectList = new();// 認識したエージェント以外のオブジェクトxのリスト
    private Vector3 moveVector;
    public Vector3 GetVelocity { get { return rb.velocity; } }// 現在の速度ベクトル
    private Vector3 destination = new(200, 20, 200);
    private Renderer objectRenderer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // ランダムな方向を向く
        this.transform.LookAt(Random.onUnitSphere, Vector3.up);

        objectRenderer = GetComponent<Renderer>();
    }

    private void Update()
    {
        moveVector = ExecuteForwardMission();
        // moveVector = ExecuteTargetMission();
        // 最大速度を制限
        rb.AddForce(moveVector * moveSpeed - rb.velocity);

        // 移動方向を向く
        this.transform.LookAt(transform.position + rb.velocity.normalized, Vector3.up);
    }

    // センサー範囲内にColliderオブジェクトが入った時の処理メソッド．
    private void OnTriggerEnter(Collider other)
    {
        // エージェントとそれ以外のオブジェクトのリストをそれぞれ作成する．
        if (other.gameObject.TryGetComponent<AgentController>(out var agent) && !outerList.Contains(agent))
        {
            outerList.Add(agent);
        }
        else if (!outerList.Contains(agent))
        {
            objectList.Add(other.gameObject);
        }
    }

    // センサー範囲内からColliderオブジェクトが出た時の処理メソッド．
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent<AgentController>(out var agent) && outerList.Contains(agent))
        {
            outerList.Remove(agent);
        }
    }

    // 他のオブジェクトと衝突時の処理メソッド．
    private void OnCollisionEnter(Collision collision)
    {
        if (objectRenderer != null)
        {
            objectRenderer.material.color = Color.blue;
        }
    }

    // 他のエージェントから距離をとるメソッド．
    private Vector3 Separation()
    {
        Vector3 vector = new(0, 0, 0);
        int innerAgentNum = 0;

        foreach (var agent in outerList)
        {
            var distance = (transform.position - agent.transform.position).magnitude;

            if (distance <= innerRadius)
            {
                vector += (transform.position - agent.transform.position).normalized * innerRadius / distance;
                innerAgentNum++;
            }
        }

        if (innerAgentNum > 0) vector /= innerAgentNum;
        vector += Avoid();

        return vector;
    }

    // 視界内のエージェントと向きを合わせるメソッド．
    private Vector3 Align()
    {
        Vector3 vector = new();

        foreach (var agent in outerList)
        {
            vector += agent.GetVelocity;
        }

        // カメラ外に移動しないように上下方向を向かないようにする
        vector.y = 0;

        return vector.normalized;
    }

    // 視界内のエージェントの中心座標を目指すメソッド．
    private Vector3 Cohesion()
    {
        Vector3 totalPosition = new(0, 0, 0);

        foreach (var agent in outerList)
        {
            var targetPosition = agent.gameObject.transform.position;

            if (Vector3.Distance(transform.position, targetPosition) > innerRadius)
            {
                // 基準の範囲内の場合ターゲットとする
                totalPosition += targetPosition - transform.position;
            }
        }

        return totalPosition.normalized;
    }

      // エージェント以外のオブジェクトから距離をとるメソッド．
    private Vector3 Avoid()
    {
        Vector3 vector = new(0, 0, 0);
        Collider myCollider = GetComponent<Collider>();
        int innerAgentNum = 0;

        foreach (var obstacle in objectList)
        {
            Collider targetCollider = obstacle.GetComponent<Collider>();

            if (myCollider != null && targetCollider != null)
            {
                // 自身の位置に最も近いターゲットのコライダーの表面上の点を取得
                Vector3 closestPointOnTarget = targetCollider.ClosestPoint(transform.position);
                var distance = Vector3.Distance(transform.position, closestPointOnTarget);

                if (distance <= innerRadius)
                {
                    vector += (transform.position - closestPointOnTarget).normalized * innerRadius / distance;
                    innerAgentNum++;
                }
            }
        }

        if (innerAgentNum > 0) vector /= innerAgentNum;

        return vector;
    }

    // 前方へ進むミッションメソッド．
    public Vector3 ExecuteForwardMission()
    {
        Vector3 vector = transform.forward;

        if (outerList.Count > 0 || objectList.Count > 0)
        {
            vector = Separation() * separatePower + Align() * alignPower + Cohesion() * cohesionPower;
            vector += transform.forward * vector.magnitude * destinationPower;
        }

        return vector.normalized;
    }

    // 目的地へ進むミッションメソッド．
    public Vector3 ExecuteTargetMission()
    {
        Vector3 direction = (destination - transform.position).normalized;
        Vector3 vector = direction;

        // 目的地周辺にいる場合は結合と分離のみ
        if (Vector3.Distance(transform.position, destination) < 10)
        {
            vector = Separation() * separatePower + Cohesion() * cohesionPower;
        }
        else if (outerList.Count > 0 || objectList.Count > 0)
        {
            vector = Separation() * separatePower + Align() * alignPower + Cohesion() * cohesionPower;
            vector += direction * vector.magnitude * destinationPower;
        }

        return vector.normalized;
    }

}
