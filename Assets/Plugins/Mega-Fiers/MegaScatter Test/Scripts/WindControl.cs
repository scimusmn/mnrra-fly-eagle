
using UnityEngine;

[ExecuteInEditMode]
public class WindControl : MonoBehaviour
{
	public float Dist = 1.1f;
	public float Scale = 0.01f;
	public float Speed = 4.1f;

	public float minscale = 0.0f;
	public float maxscale = 0.03f;
	public float spd = 0.0f;
	public float changetime = 3.0f;
	float t = 0.0f;
	float targetscale = 0.01f;

	public GUISkin	skin;

	// Can vary the Scale
	void Update()
	{
		t += Time.deltaTime;
		if ( t > changetime )
		{
			t = 0.0f;

			targetscale = Mathf.Lerp(minscale, maxscale, Random.value);
		}

		Scale = Mathf.SmoothDamp(Scale, targetscale, ref spd, changetime);

		Shader.SetGlobalFloat("_speedadj", Speed);
		Shader.SetGlobalFloat("_distadj", Dist);
		Shader.SetGlobalFloat("_scaleadj", Scale);
	}

	public Vector2 pos = Vector2.zero;
	public Vector3 guisize = Vector2.one;
	public Color col = Color.white;

	void OnGUI()
	{
		GUI.skin = skin;
		GUILayout.BeginArea(new Rect(Screen.width - pos.x, Screen.height - pos.y, guisize.x, guisize.y));

		GUILayout.BeginHorizontal();
		GUILayout.Label("Dist");
		GUI.color = col;
		Dist = GUILayout.HorizontalSlider(Dist, 0.0f, 2.0f, GUILayout.Width(100), GUILayout.Height(30));
		Scale = GUILayout.HorizontalSlider(Scale, 0.0f, 0.1f, GUILayout.Width(100));
		Speed = GUILayout.HorizontalSlider(Speed, 0.0f, 8.0f, GUILayout.Width(100));
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}
}