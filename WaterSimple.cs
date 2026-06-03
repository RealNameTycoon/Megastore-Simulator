using UnityEngine;

[ExecuteInEditMode]
public class WaterSimple : MonoBehaviour
{
	private void Update()
	{
		if ((bool)GetComponent<Renderer>())
		{
			Material sharedMaterial = GetComponent<Renderer>().sharedMaterial;
			if ((bool)sharedMaterial)
			{
				Vector4 vector = sharedMaterial.GetVector("WaveSpeed");
				float num = sharedMaterial.GetFloat("_WaveScale");
				float num2 = Time.time / 20f;
				Vector4 vector2 = vector * (num2 * num);
				Vector4 value = new Vector4(Mathf.Repeat(vector2.x, 1f), Mathf.Repeat(vector2.y, 1f), Mathf.Repeat(vector2.z, 1f), Mathf.Repeat(vector2.w, 1f));
				sharedMaterial.SetVector("_WaveOffset", value);
				Vector3 vector3 = new Vector3(1f / num, 1f / num, 1f);
				Matrix4x4 value2 = Matrix4x4.TRS(new Vector3(value.x, value.y, 0f), Quaternion.identity, vector3);
				sharedMaterial.SetMatrix("_WaveMatrix", value2);
				value2 = Matrix4x4.TRS(new Vector3(value.z, value.w, 0f), Quaternion.identity, vector3 * 0.45f);
				sharedMaterial.SetMatrix("_WaveMatrix2", value2);
			}
		}
	}
}
