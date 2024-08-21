using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]//RigidBody必須
public class NPCBrain : MonoBehaviour
{
    [SerializeField] private float movespd = 10;
    private float InnerSphere = 10; //パーソナルスペースの半径
    private float S_power = 2;//分離力
    private float A_power = 2;//整列力
    private float C_power = 1;//結合力
    private Rigidbody rb;
    private List<NPCBrain> outerlist = new();//認識した個体のリスト
    private List<GameObject> objectList = new();
    private Vector3 movevec;//加速ベクトル
    public Vector3 GetVec { get { return rb.velocity; } }//現在の速度ベクトル

    private void Awake()
    {
        // this.transform.LookAt(Random.onUnitSphere, Vector3.up);//ランダムな方向を向く
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        movevec = transform.forward; //近くにエージェントがなければ前に進む
        if (outerlist.Count > 0)// 近くにエージェントがある場合
        {
            movevec = (Separation() * S_power + Align() * A_power + Cohesion() * C_power).normalized;
        }
        rb.AddForce(movevec * movespd - rb.velocity);//最大速度を制限
        this.transform.LookAt(transform.position + rb.velocity.normalized, Vector3.up);//移動方向を向く
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<NPCBrain>(out var b) && !outerlist.Contains(b))
        {//エージェントなら
            outerlist.Add(b);//登録
        }
        else
        {
            objectList.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent<NPCBrain>(out var b) && outerlist.Contains(b))
        {
            outerlist.Remove(b);
        }
    }

    private Vector3 Separation()
    {// 物体から距離をとる
        Vector3 targetVec = new();
        foreach (var v in outerlist)
        {
            var diff = (transform.position - v.transform.position);
            if (diff.magnitude < InnerSphere)
            {//特定距離より近いなら
                targetVec += (transform.position - v.transform.position).normalized / diff.magnitude;//近いほど影響大
            }
        }
        // 自身のコライダーを取得
        Collider myCollider = GetComponent<Collider>();
        foreach (var v in objectList)
        {
            var diff = (transform.position - v.transform.position).magnitude;
            // ターゲットオブジェクトのコライダーを取得
            Collider targetCollider = v.GetComponent<Collider>();
            if (myCollider != null && targetCollider != null)
            {
                // 自身の位置に最も近いターゲットのコライダーの表面上の点を取得
                Vector3 closestPointOnTarget = targetCollider.ClosestPoint(transform.position);
                // 自身の位置とターゲットの最も近い点との距離を計算
                diff = Vector3.Distance(transform.position, closestPointOnTarget);
                if (diff < InnerSphere)
                {//特定距離より近いなら
                    targetVec += (transform.position - closestPointOnTarget).normalized / diff;//近いほど影響大
                }
            }
        }
        targetVec /= outerlist.Count + objectList.Count;
        targetVec = targetVec.normalized;
        return targetVec;
    }

    private Vector3 Align()
    {//視界内のエージェントと向きを合わせる
        Vector3 TargetVec = new();
        foreach (var v in outerlist)
        {
            TargetVec += v.GetVec;
        }
        TargetVec.y = 0;
        TargetVec /= outerlist.Count;
        TargetVec = TargetVec.normalized;
        return TargetVec;
    }

    private Vector3 Cohesion()
    {//視界内のエージェントの中心座標を目指す
        Vector3 targetPos = new();
        int targetNum = 0;
        foreach (var v in outerlist)
        {
            var pos = v.gameObject.transform.position;
            if (Vector3.Distance(this.transform.position, pos) > InnerSphere)// 基準の範囲内の場合ターゲットとする
            {
                targetPos += pos;
                targetNum++;
            }
        }
        targetPos /= targetNum;
        var targetVec = (targetPos - this.transform.position).normalized;
        // targetVec.y = 0;
        targetVec = targetVec.normalized;
        return targetVec;
    }
}
