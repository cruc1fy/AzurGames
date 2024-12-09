using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireworksSystem : MonoBehaviour
{
    public GameConfig gameConfig;
    public Fireworks prefab;

    List<Fireworks> fireworks = new List<Fireworks>();
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 50; i++)
        {
            var f = Instantiate(prefab, transform);
            f.image.sprite = gameConfig.fireworks[Random.Range(0, gameConfig.fireworks.Length)];

            f.transform.localPosition = (((i % 2) == 0 ) ? Vector3.left : Vector3.right) *Random.Range(512f, 800f) + Vector3.up * Random.Range(-500, 50);
            f.dir = -f.transform.localPosition;
            f.dir.y += Random.Range(0f, 500f);
            f.dir *= Random.Range(1f, 1.5f);
            fireworks.Add(f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach(var f in fireworks)
        {
            f.transform.position += f.dir * Time.deltaTime;
        }
    }
}
