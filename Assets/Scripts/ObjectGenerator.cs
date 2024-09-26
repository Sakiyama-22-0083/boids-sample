using UnityEngine;

public class ObjectGenerator : MonoBehaviour
{
    public GameObject objectToSpawn;
    public int numberOfObjects = 6;
    public Vector3 range = new Vector3(10, 10, 10);// ランダムな座標範囲

    void Start()
    {
        SpawnObjects(objectToSpawn, numberOfObjects);
    }

    // オブジェクトを生成するメソッド
    void SpawnObjects(GameObject obj, int count)
    {
        for (int i = 0; i < count; i++)
        {
            // ランダムな座標を生成
            Vector3 randomPosition = new Vector3(
                Random.Range(-range.x, range.x),
                Random.Range(1, range.y),
                Random.Range(-range.z, range.z)
            );

            // オブジェクトを生成
            Instantiate(obj, randomPosition, Quaternion.identity);
        }
    }

}
