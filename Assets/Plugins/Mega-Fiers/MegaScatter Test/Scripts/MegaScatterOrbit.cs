
using UnityEngine;

[ExecuteInEditMode]
public class MegaScatterOrbit : MonoBehaviour
{
	public GameObject target;
	MeshRenderer render;
	SkinnedMeshRenderer srender;
	MeshFilter	filter;
	public float distance = 10.0f;
	public float xSpeed = 250.0f;
	public float ySpeed = 120.0f;
	public float zSpeed = 120.0f;
	public float yMinLimit = -20.0f;
	public float yMaxLimit = 80.0f;
	public float xMinLimit = -20.0f;
	public float xMaxLimit = 20.0f;
	private float x = 0.0f;
	private float y = 0.0f;
	private Vector3 center;
	public bool Dynamic = false;

	public Vector3	offset;
	public Vector3 EditTest;

	Vector3 tpos = new Vector3();	// target pos

	void Start()
	{
		NewTarget(target);

		if ( target )
		{
			tpos = target.transform.position;
			lastpos = tpos;
			Vector3 angles = Quaternion.LookRotation(tpos - transform.position).eulerAngles;
			x = angles.y;
			y = angles.x;
			distance = (tpos - transform.position).magnitude;
		}
		else
		{
			Vector3 angles = transform.eulerAngles;
			x = angles.y;
			y = angles.x;
		}

		nx = x;
		ny = y;
		nz = distance;
		vx = 0.0f;
		vy = 0.0f;
		// Make the rigid body not change rotation
		//if ( rigidbody )
			//rigidbody.freezeRotation = true;
	}

	private float easeInOutQuint(float start, float end, float value)
	{
		value /= .5f;
		end -= start;
		if ( value < 1 ) return end / 2 * value * value * value * value * value + start;
		value -= 2;
		return end / 2 * (value * value * value * value * value + 2) + start;
	}

	float easeInQuad(float start, float end, float value)
	{
		value /= 1.0f;
		end -= start;
		return end * value * value + start;
	}

	float easeInSine(float start, float end, float value)
	{
		end -= start;
		return -end * Mathf.Cos(value / 1.0f * (Mathf.PI / 2.0f)) + end + start;
	}

	private float easeInOutSine(float start, float end, float value)
	{
		end -= start;
		return -end / 2.0f * (Mathf.Cos(Mathf.PI * value / 1.0f) - 1.0f) + start;
	}

	float t = 0.0f;

	public void NewTarget(GameObject targ)
	{
		if ( target != targ )
		{
			target = targ;
			t = 0.0f;

			if ( target )
			{
				filter = (MeshFilter)target.GetComponent(typeof(MeshFilter));

				if ( filter != null )
				{
					//center = filter.mesh.bounds.center;
					center = filter.sharedMesh.bounds.center;
				}
				else
				{
					render = (MeshRenderer)target.GetComponent(typeof(MeshRenderer));

					if ( render != null )
						center = render.bounds.center;
					else
					{
						srender = (SkinnedMeshRenderer)target.GetComponent(typeof(SkinnedMeshRenderer));

						if ( srender != null )
							center = srender.bounds.center;
					}
				}
			}
		}
	}

	public float trantime = 4.0f;

	float vx = 0.0f;
	float vy = 0.0f;
	float vz = 0.0f;
	public float nx = 0.0f;
	public float ny = 0.0f;
	public float nz = 0.0f;

	public float delay = 0.2f;
	public float delayz = 0.2f;
	public float mindist = 1.0f;

	public GameObject target1;

	void LateUpdate()
	{
		Drag2();
	}

	void Update()
	{
		if ( fpstext )
		{
			fpstext.text = (1.0f / Time.smoothDeltaTime).ToString("0.00");
		}

		if ( target )
		{
			//Drag2();

			if ( Input.GetMouseButton(0) || Input.GetMouseButton(1) )
			{
				nx = x + Input.GetAxis("Mouse X") * xSpeed * 0.02f;
				ny = y - Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
			}

			if ( !Application.isPlaying )
			{
				x = nx;
				y = ny;
			}
			else
			{
				x = Mathf.SmoothDamp(x, nx, ref vx, delay);	//0.21f);	//, 100.0f, Time.deltaTime);
				y = Mathf.SmoothDamp(y, ny, ref vy, delay);	//0.21f);	//, 100.0f, Time.deltaTime);
			}

			// NOTE: If you get an exception for this line it means you dont have a scroll wheel input setup in
			// the input manager, go to Edit/Project Settings/Input and set the Mouse ScrollWheel setting to use 3rd mouse axis
			nz = nz - (Input.GetAxis("Mouse ScrollWheel") * zSpeed);

			y = ClampAngle(y, yMinLimit, yMaxLimit);
			//x = ClampAngle(x, xMinLimit, xMaxLimit);

			if ( !Application.isPlaying )
			{
				distance = nz;
			}
			else
				distance = Mathf.SmoothDamp(distance, nz, ref vz, delayz);
			//Vector3 crot = transform.localRotation.eulerAngles;

			if ( distance < mindist )
			{
				distance = mindist;
				nz = mindist;
			}

			Vector3 c;
			if ( Dynamic )
			{
				if ( filter != null )
				{
					c = target.transform.TransformPoint(filter.mesh.bounds.center + offset);
				}
				else
				{
					if ( render != null )
						c = target.transform.TransformPoint(render.bounds.center + offset);
					else
					{
						if ( srender != null )
							c = target.transform.TransformPoint(srender.bounds.center + offset);
						else
							c = target.transform.TransformPoint(center + offset);
					}
				}
			}
			else
				c = target.transform.TransformPoint(center + offset);

			// Want to ease to new target pos
			if ( t < trantime )
			{
				t = trantime;	//+= Time.deltaTime;

				tpos.x = easeInSine(tpos.x, c.x, t / trantime);
				tpos.y = easeInSine(tpos.y, c.y, t / trantime);
				tpos.z = easeInSine(tpos.z, c.z, t / trantime);
			}
			else
				tpos = c;

			Quaternion rotation = Quaternion.Euler(y, x, 0.0f);
			Vector3 position = rotation * new Vector3(0.0f, 0.0f, -distance) + tpos;	//c;

			Vector3 rp = position;
			rp.y -= 0.5f;
			RaycastHit hit;

			Vector3 dir = rp - tpos;
			float dist = dir.magnitude;
			//if ( WindControl.cameracollision )
			{
				if ( Physics.Raycast(tpos, dir.normalized, out hit, dist) )
				{
					//Debug.Log("hit " + hit.collider.name);
					Vector3 hp = hit.point;
					hp.y += 0.5f;

					//if ( hp.y > position.y )
					{
						position = hp;
					}
				}
			}

			if ( cpos == Vector3.zero )
			{
				cpos = position;
			}

			cpos = Vector3.SmoothDamp(cpos, position, ref posspd, 0.4f);

			transform.rotation = Quaternion.LookRotation((tpos - cpos).normalized, Vector3.up);	//rotation;
			transform.position = cpos;	//position;

		}
	}

	Vector3 posspd = Vector3.zero;
	Vector3 cpos;

	static float ClampAngle(float angle, float min, float max)
	{
		if ( angle < -360.0f )
			angle += 360.0f;

		if ( angle > 360.0f )
			angle -= 360.0f;

		return Mathf.Clamp(angle, min, max);
	}

	Vector3 lastpos;
	float rollang = 0.0f;
	Vector3 roll3 = Vector3.zero;

	void Drag2()
	{
		Vector3 p = target.transform.position;

		//Vector3 delta = p - lastpos;
		float dist = Vector3.Distance(p, lastpos);
		lastpos = p;

		Quaternion q = Camera.main.transform.rotation;
		Vector3 rot = q.eulerAngles;

		Vector3 move = Vector3.zero;

		float b = 0.0f;	//Input.GetMouseButton(0) ? 1.0f : 0.0f;

		//if ( options != null && options.showgui == true )
		//	b = 0.0f;

		float f1 = b;	//filters[0].Val(b);	//(float)Input.GetMouseButton(0));

		if ( Input.GetKey(KeyCode.W) || f1 != 0.0f || Input.GetKey(KeyCode.UpArrow) )	//Input.GetMouseButton(0) )
			move += Vector3.forward * 5.0f * Time.deltaTime;

		if ( Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) )
			move += Vector3.back * 5.0f * Time.deltaTime;

		move1 = Vector3.SmoothDamp(move1, move, ref spd, 0.4f);

		Quaternion qr = Quaternion.Euler(new Vector3(0.0f, rot.y, 0.0f));	// * Mathf.Deg2Rad);

		Quaternion qr1 = Quaternion.identity;
		rot.x = 0.0f;
		rot.z = 0.0f;
		qr1.eulerAngles = rot;	// * Mathf.Deg2Rad);

		Matrix4x4 mat = Matrix4x4.identity;
		mat.SetTRS(Vector3.zero, qr1, Vector3.one);
		Vector3 move2 = mat.MultiplyPoint3x4(move1);

		Vector3 newpos = p + move2;
		Vector3 rp = newpos;
		RaycastHit hit;
		if ( Physics.Raycast(rp, Vector3.down, out hit) )
		{
			newpos = hit.point;
			newpos.y += 0.5f;
		}

		target.transform.position = newpos;

		if ( target )
		{
			roll3 -= move1;	//delta;	// / (Mathf.PI * 2.0f);
			rollang += dist / (Mathf.PI * 2.0f);
			rot.x = -roll3.z * Mathf.Rad2Deg;	//0.0f;	//(rollang * Mathf.Rad2Deg);	//dist / (Mathf.PI * 2.0f);
			rot.z = -roll3.x * Mathf.Rad2Deg;	//0.0f;

			Quaternion qrx = Quaternion.Euler(new Vector3(rot.x, 0.0f, 0.0f));

			target1.transform.rotation = qr * qrx;	// * qrx;
		}
	}

	Vector3 spd = Vector3.zero;
	Vector3 move1 = Vector3.zero;

	public GUIText	fpstext;
}