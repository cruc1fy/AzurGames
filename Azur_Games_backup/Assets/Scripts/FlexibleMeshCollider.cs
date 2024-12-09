using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
public class FlexibleMeshCollider : MonoBehaviour
{
	public float waveAmplitude = 0.1f;
	public float waveFrequency = 1.0f;
	public float timeScale = 1.0f;

	private Mesh originalMesh;
	private Mesh deformedMesh;
	private MeshCollider meshCollider;

	void Start()
	{
		// Получаем оригинальный меш
		originalMesh = GetComponent<MeshFilter>().mesh;
		deformedMesh = Instantiate(originalMesh); // Создаём копию меша
		GetComponent<MeshFilter>().mesh = deformedMesh;

		// Получаем MeshCollider
		meshCollider = GetComponent<MeshCollider>();
	}

	void Update()
	{
		Vector3[] vertices = originalMesh.vertices;

		for (int i = 0; i < vertices.Length; i++)
		{
			// Применяем синусоидальную волну к вершинам
			vertices[i].y += Mathf.Sin(vertices[i].x * waveFrequency + Time.time * timeScale) * waveAmplitude;
		}

		// Обновляем меш и коллайдер
		deformedMesh.vertices = vertices;
		deformedMesh.RecalculateNormals();
		deformedMesh.RecalculateBounds();

		meshCollider.sharedMesh = null; // Сбрасываем текущий меш
		meshCollider.sharedMesh = deformedMesh; // Устанавливаем обновлённый меш
	}
}
