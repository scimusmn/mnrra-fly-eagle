
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class MegaScatterObjectTexture : MegaScatterObject
{
	public float			scatterWidth = 1.0f;
	public float			scatterLength = 1.0f;
	public Texture2D		scatterTexture;
	public Texture2D		scaleTexture;
	//public Texture2D		colorTexture;

	public float			mintexscale = 0.0f;
	public float			maxtexscale = 1.0f;
	public float			minsizetoadd = 0.2f;
	Color texturecol;

	public override string GetURL() { return "/?page_id=4951"; }

	void Init()
	{
		RemoveObjects();
		CollectColliders();
	}

	bool FindPos(int m, int cc, ref Vector3 p, Vector3 scl, ref float val, float rad, ref float scale)
	{
		scale = 1.0f;
		float r = rad;

		Texture2D texture = scatterTexture;

		//int width = texture.width;
		//int height = texture.height;

		int maxindex = PosFailCount;	//ww(height * width);

		int fails = PosFailCount;

		Texture2D tex2d = null;
		Vector2 texscale = Vector2.one;
		Vector2 texoff = Vector2.zero;

		if ( texturecollider )
		{
			Renderer rend = texturecollider.GetComponent<Renderer>();
			if ( rend )
			{
				tex2d = (Texture2D)rend.sharedMaterial.mainTexture;
				texscale = rend.sharedMaterial.mainTextureScale;
				texoff = rend.sharedMaterial.mainTextureOffset;
			}
			texturecollider.enabled = true;
		}

		Color col = Color.white;
		Vector3 cp = Vector3.zero;

		while ( true )
		{
			fails--;
			if ( fails == 0 )
				break;

			Vector2 tp = Vector3.zero;

			int t = 0;

			for ( t = 0; t < maxindex; t++ )
			{
				tp.x = Random.value;
				tp.y = Random.value;

				if ( texturecollider )
				{
					cp = texturecollider.transform.position;
					cp.x = (tp.x * texturecollider.bounds.size.x) + texturecollider.bounds.min.x;
					cp.z = (tp.y * texturecollider.bounds.size.z) + texturecollider.bounds.min.z;

					cp.y += 100.0f;

					RaycastHit hit;
					Ray ray = new Ray();
					ray.origin = cp;
					ray.direction = Vector3.down;
					if ( texturecollider.Raycast(ray, out hit, raycastheight) )
					{
						Vector2 uv = hit.textureCoord;
						uv = Vector2.Scale(uv, texscale) + texoff;
						tp = uv;
						col = tex2d.GetPixelBilinear(uv.x, uv.y);
					}
				}
				else
					col = texture.GetPixelBilinear(tp.x, tp.y);

				if ( scaleTexture )
				{
					Color scol = scaleTexture.GetPixelBilinear(tp.x, tp.y);
					scale = scol.r;
				}

				if ( layers[m].colorTexture )
					texturecol = layers[m].colorTexture.GetPixelBilinear(tp.x, tp.y);

				r = rad * scale;

				bool match = false;

				Color col1 = layers[m].scattercols[cc].lowcol;
				Color col2 = layers[m].scattercols[cc].highcol;

				if ( col.r >= col1.r && col.r <= col2.r )
				{
					if ( col.g >= col1.g && col.g <= col2.g )
					{
						if ( col.b >= col1.b && col.b <= col2.b )
							match = true;
					}
				}

				if ( match )
					break;
			}

			if ( texturecollider )
			{
				p = transform.worldToLocalMatrix.MultiplyPoint3x4(cp);
				texturecollider.enabled = false;
			}
			else
			{
				tp.x = (tp.x - 0.5f) * 1.0f;
				tp.y = (tp.y - 0.5f) * 1.0f;
				p = new Vector3((tp.x * scatterWidth), 0.0f, (tp.y * scatterLength));
			}

			if ( t >= maxindex )
				return false;

			p = Snap(p, layers[m].snap);

			if ( layers[m].noOverlap )
			{
				if ( !Overlap(p, r) )
					return true;

				return false;
			}
			else
				return true;
		}

		if ( texturecollider )
			texturecollider.enabled = false;

		return false;
	}

	public override void Fill()
	{
		if ( scatterTexture == null && texturecollider == null )
			return;

		if ( layers.Count == 0 )
			return;

		if ( update )
		{
			objcount = 0;
			vertcount = 0;
			scattercount = 0;

			Init();
			update = false;
			float totalweight = 0.0f;

			for ( int i = 0; i < layers.Count; i++ )
			{
				if ( layers[i].Enabled )
					totalweight = layers[i].InitObj(totalweight);
			}

			instances.Clear();	// May not be right place

			totalarea = scatterLength * scatterWidth;
			int fullcount = (int)(totalarea * Density);
			if ( countmode == MegaScatterMode.Count )
				fullcount = forcecount;

			for ( int m = 0; m < layers.Count; m++ )
			{
				if ( layers[m].Enabled && scatterTexture )
				{
					int meshcount = (int)(fullcount * (layers[m].weight / totalweight));

					if ( !layers[m].perCurveCount )
					{
						if ( layers[m].forcecount != 0 )
							meshcount = layers[m].forcecount;

						if ( layers[m].maxcount != 0 && meshcount > layers[m].maxcount )
							meshcount = layers[m].maxcount;
					}

					float val = 0.0f;

					Quaternion prerot = Quaternion.Euler(layers[m].prerot);

					// Do this if want overlap only for each mesh
					if ( layers[m].clearOverlap )
						instances.Clear();	// May not be right place

					Random.seed = seed + layers[m].seed + m;

					for ( int s = 0; s < layers[m].scattercols.Count; s++ )
					{
						//Random.seed = layers[m].seed + m;

						int count = meshcount / layers[m].scattercols.Count;	// * (curves[s].area / totalarea));

						if ( layers[m].perCurveCount )
						{
							if ( layers[m].forcecount != 0 )
								count = layers[m].forcecount;

							if ( layers[m].maxcount != 0 && count > layers[m].maxcount )
								count = layers[m].maxcount;
						}

						Vector3 p = Vector3.zero;

						int failcount = FailCount;

						for ( int i = 0; i < count; i++ )
						{
							float r1 = Random.value;
							float r2 = Random.value;
							float r3 = Random.value;
							float r4 = Random.value;
							float r5 = Random.value;
							float r6 = Random.value;
							float r7 = Random.value;
							float r8 = Random.value;
							float r9 = Random.value;
							float rc = Random.value;

							float alpha = Mathf.Clamp01(layers[m].distCrv.Evaluate(r1));
							float alpha1 = 0.0f;
							float alpha2 = 0.0f;

							Vector3 scl = Vector3.one;

							if ( layers[m].uniformScaling )
							{
								Vector3 low = globalScale * layers[m].uniscaleLow;
								Vector3 high = globalScale * layers[m].uniscaleHigh;
								//scl = (low + (alpha * (high - low))) * 1.0f * (layers[m].scale);

								switch ( layers[m].uniscalemode )
								{
									case MegaScatterScaleMode.XYZ:
										scl = (low + (alpha * (high - low))) * 1.0f * (layers[m].scale);
										break;

									case MegaScatterScaleMode.XY:
										scl.y = scl.x = (low.x + (alpha * (high.x - low.x))) * 1.0f * (layers[m].scale);
										scl.z = (low.z + (alpha1 * (high.z - low.z))) * 1.0f * (layers[m].scale);
										break;

									case MegaScatterScaleMode.XZ:
										scl.z = scl.x = (low.x + (alpha * (high.x - low.x))) * 1.0f * (layers[m].scale);
										scl.y = (low.y + (alpha1 * (high.y - low.y))) * 1.0f * (layers[m].scale);
										break;

									case MegaScatterScaleMode.YZ:
										scl.y = scl.z = (low.x + (alpha * (high.x - low.x))) * 1.0f * (layers[m].scale);
										scl.x = (low.x + (alpha1 * (high.x - low.x))) * 1.0f * (layers[m].scale);
										break;
								}
							}
							else
							{
								alpha1 = Mathf.Clamp01(layers[m].distCrv.Evaluate(r2));
								alpha2 = Mathf.Clamp01(layers[m].distCrv.Evaluate(r3));

								Vector3 low = Vector3.Scale(globalScale, layers[m].scaleLow);
								Vector3 high = Vector3.Scale(globalScale, layers[m].scaleHigh);
								scl.x = (low.x + (alpha * (high.x - low.x))) * 1.0f * (layers[m].scale);
								scl.y = (low.y + (alpha1 * (high.y - low.y))) * 1.0f * (layers[m].scale);
								scl.z = (low.z + (alpha2 * (high.z - low.z))) * 1.0f * (layers[m].scale);
							}

							alpha = Mathf.Clamp01(layers[m].distCrv.Evaluate(r4));
							alpha1 = Mathf.Clamp01(layers[m].distCrv.Evaluate(r5));
							alpha2 = Mathf.Clamp01(layers[m].distCrv.Evaluate(r6));
							Vector3 rot = Vector3.zero;

							rot.x = (layers[m].rotLow.x + (alpha * (layers[m].rotHigh.x - layers[m].rotLow.x)));
							rot.y = (layers[m].rotLow.y + (alpha * (layers[m].rotHigh.y - layers[m].rotLow.y)));
							rot.z = (layers[m].rotLow.z + (alpha * (layers[m].rotHigh.z - layers[m].rotLow.z)));
							rot = Snap(rot, layers[m].snapRot);

							float distalpha = 0.0f;

							float maxscale = Mathf.Abs(scl.x);
							if ( Mathf.Abs(scl.z) > maxscale )
								maxscale = Mathf.Abs(scl.z);

							float rad = layers[m].radius * maxscale;
							float texscale = 0.0f;

							if ( layers[m].colorTexture != null )
								rc = -1.0f;

							if ( FindPos(m, s, ref p, scl, ref val, rad, ref texscale) )
							{
								bool addedmesh = false;

								alpha = Mathf.Clamp01(layers[m].distCrv.Evaluate(r7));
								alpha1 = Mathf.Clamp01(layers[m].distCrv.Evaluate(r8));
								alpha2 = Mathf.Clamp01(layers[m].distCrv.Evaluate(r9));

								p.x += (layers[m].offsetLow.x + (alpha * (layers[m].offsetHigh.x - layers[m].offsetLow.x)));
								p.y += (layers[m].offsetLow.y + (alpha1 * (layers[m].offsetHigh.y - layers[m].offsetLow.y)));
								p.z += (layers[m].offsetLow.z + (alpha2 * (layers[m].offsetHigh.z - layers[m].offsetLow.z)));

								Vector3 wp = transform.localToWorldMatrix.MultiplyPoint3x4(p);
								Vector3 lp = transform.worldToLocalMatrix.MultiplyPoint3x4(wp);

								float texscaleval = Mathf.Lerp(mintexscale, maxtexscale, texscale);

								if ( texscaleval > minsizetoadd )
								{
									scl *= layers[m].scaleOnDist.Evaluate(distalpha) * texscaleval;
									rad *= texscaleval;

									if ( raycast )
									{
										if ( MultiRayCast(wp, rad * layers[m].colradiusadj, layers[m].raycount) )
										{
											RaycastHit hit;
											Vector3 o = wp;
											o.y += collisionOffset;

											if ( Physics.Raycast(o, Vector3.down, out hit, raycastheight) )
											{
												float ang = Mathf.Acos(Vector3.Dot(hit.normal, Vector3.up)) * Mathf.Rad2Deg;

												p = hit.point;
												if ( !layers[m].useheight || (p.y < layers[m].maxheight && p.y > layers[m].minheight) )
												{
													if ( ang >= layers[m].minslope && ang <= layers[m].maxslope )
													{
														//p.y += layers[m].collisionOffset * scl.y;
														p = transform.worldToLocalMatrix.MultiplyPoint3x4(p);

														Vector3 hn = transform.worldToLocalMatrix.MultiplyVector(hit.normal);

														Vector3 newup = Vector3.Lerp(Vector3.up, hn, layers[m].align).normalized;
														p += newup * (layers[m].collisionOffset * scl.y);
														Quaternion align = Quaternion.FromToRotation(Vector3.up, newup);

														layers[m].AddObj(this, p, scl, rot, align, prerot, rc, texturecol);
														addedmesh = true;
													}
												}
											}
										}
										else
										{
											if ( !NeedsGround )
											{
												layers[m].AddObj(this, lp, scl, rot, Quaternion.identity, prerot, rc, texturecol);
												addedmesh = true;
											}
										}
									}
									else
									{
										layers[m].AddObj(this, lp, scl, rot, Quaternion.identity, prerot, rc, texturecol);
										addedmesh = true;
									}

									if ( !addedmesh )
									{
										i--;
										failcount--;
										if ( failcount < 0 )
											break;
									}
								}
							}
						}
					}
				}
			}
			RestoreColliders();

			if ( dostaticbatching )
				StaticBatchingUtility.Combine(gameObject);
		}
	}

	public override IEnumerator FillCo()
	{
		if ( scatterTexture && texturecollider && layers.Count > 0 && update )
		{
			objcount = 0;
			vertcount = 0;
			scattercount = 0;

			Init();
			update = false;
			float totalweight = 0.0f;

			for ( int i = 0; i < layers.Count; i++ )
			{
				if ( layers[i].Enabled )
					totalweight = layers[i].InitObj(totalweight);
			}

			instances.Clear();	// May not be right place

			totalarea = scatterLength * scatterWidth;
			int fullcount = (int)(totalarea * Density);
			if ( countmode == MegaScatterMode.Count )
				fullcount = forcecount;

			int placed = 0;
			if ( NumPerFrame < 1 )
				NumPerFrame = 1;

			for ( int m = 0; m < layers.Count; m++ )
			{
				if ( layers[m].Enabled && scatterTexture )
				{
					int meshcount = (int)(fullcount * (layers[m].weight / totalweight));

					if ( !layers[m].perCurveCount )
					{
						if ( layers[m].forcecount != 0 )
							meshcount = layers[m].forcecount;

						if ( layers[m].maxcount != 0 && meshcount > layers[m].maxcount )
							meshcount = layers[m].maxcount;
					}

					float val = 0.0f;

					Quaternion prerot = Quaternion.Euler(layers[m].prerot);

					// Do this if want overlap only for each mesh
					if ( layers[m].clearOverlap )
						instances.Clear();	// May not be right place

					Random.seed = seed + layers[m].seed + m;

					for ( int s = 0; s < layers[m].scattercols.Count; s++ )
					{
						//Random.seed = layers[m].seed + m;

						int count = meshcount / layers[m].scattercols.Count;	// * (curves[s].area / totalarea));

						if ( layers[m].perCurveCount )
						{
							if ( layers[m].forcecount != 0 )
								count = layers[m].forcecount;

							if ( layers[m].maxcount != 0 && count > layers[m].maxcount )
								count = layers[m].maxcount;
						}

						Vector3 p = Vector3.zero;

						int failcount = FailCount;

						for ( int i = 0; i < count; i++ )
						{
							float r1 = Random.value;
							float r2 = Random.value;
							float r3 = Random.value;
							float r4 = Random.value;
							float r5 = Random.value;
							float r6 = Random.value;
							float r7 = Random.value;
							float r8 = Random.value;
							float r9 = Random.value;
							float rc = Random.value;

							float alpha = Mathf.Clamp01(layers[m].distCrv.Evaluate(r1));
							float alpha1 = 0.0f;
							float alpha2 = 0.0f;

							Vector3 scl = Vector3.one;

							if ( layers[m].uniformScaling )
							{
								Vector3 low = globalScale * layers[m].uniscaleLow;
								Vector3 high = globalScale * layers[m].uniscaleHigh;
								//scl = (low + (alpha * (high - low))) * 1.0f * (layers[m].scale);

								switch ( layers[m].uniscalemode )
								{
									case MegaScatterScaleMode.XYZ:
										scl = (low + (alpha * (high - low))) * 1.0f * (layers[m].scale);
										break;

									case MegaScatterScaleMode.XY:
										scl.y = scl.x = (low.x + (alpha * (high.x - low.x))) * 1.0f * (layers[m].scale);
										scl.z = (low.z + (alpha1 * (high.z - low.z))) * 1.0f * (layers[m].scale);
										break;

									case MegaScatterScaleMode.XZ:
										scl.z = scl.x = (low.x + (alpha * (high.x - low.x))) * 1.0f * (layers[m].scale);
										scl.y = (low.y + (alpha1 * (high.y - low.y))) * 1.0f * (layers[m].scale);
										break;

									case MegaScatterScaleMode.YZ:
										scl.y = scl.z = (low.x + (alpha * (high.x - low.x))) * 1.0f * (layers[m].scale);
										scl.x = (low.x + (alpha1 * (high.x - low.x))) * 1.0f * (layers[m].scale);
										break;
								}
							}
							else
							{
								alpha1 = Mathf.Clamp01(layers[m].distCrv.Evaluate(r2));
								alpha2 = Mathf.Clamp01(layers[m].distCrv.Evaluate(r3));

								Vector3 low = Vector3.Scale(globalScale, layers[m].scaleLow);
								Vector3 high = Vector3.Scale(globalScale, layers[m].scaleHigh);
								scl.x = (low.x + (alpha * (high.x - low.x))) * 1.0f * (layers[m].scale);
								scl.y = (low.y + (alpha1 * (high.y - low.y))) * 1.0f * (layers[m].scale);
								scl.z = (low.z + (alpha2 * (high.z - low.z))) * 1.0f * (layers[m].scale);
							}

							alpha = Mathf.Clamp01(layers[m].distCrv.Evaluate(r4));
							alpha1 = Mathf.Clamp01(layers[m].distCrv.Evaluate(r5));
							alpha2 = Mathf.Clamp01(layers[m].distCrv.Evaluate(r6));
							Vector3 rot = Vector3.zero;

							rot.x = (layers[m].rotLow.x + (alpha * (layers[m].rotHigh.x - layers[m].rotLow.x)));
							rot.y = (layers[m].rotLow.y + (alpha * (layers[m].rotHigh.y - layers[m].rotLow.y)));
							rot.z = (layers[m].rotLow.z + (alpha * (layers[m].rotHigh.z - layers[m].rotLow.z)));
							rot = Snap(rot, layers[m].snapRot);

							float distalpha = 0.0f;

							float maxscale = Mathf.Abs(scl.x);
							if ( Mathf.Abs(scl.z) > maxscale )
								maxscale = Mathf.Abs(scl.z);

							float rad = layers[m].radius * maxscale;
							float texscale = 0.0f;

							if ( layers[m].colorTexture != null )
								rc = -1.0f;

							if ( FindPos(m, s, ref p, scl, ref val, rad, ref texscale) )
							{
								bool addedmesh = false;

								alpha = Mathf.Clamp01(layers[m].distCrv.Evaluate(r7));
								alpha1 = Mathf.Clamp01(layers[m].distCrv.Evaluate(r8));
								alpha2 = Mathf.Clamp01(layers[m].distCrv.Evaluate(r9));

								p.x += (layers[m].offsetLow.x + (alpha * (layers[m].offsetHigh.x - layers[m].offsetLow.x)));
								p.y += (layers[m].offsetLow.y + (alpha1 * (layers[m].offsetHigh.y - layers[m].offsetLow.y)));
								p.z += (layers[m].offsetLow.z + (alpha2 * (layers[m].offsetHigh.z - layers[m].offsetLow.z)));

								Vector3 wp = transform.localToWorldMatrix.MultiplyPoint3x4(p);
								Vector3 lp = transform.worldToLocalMatrix.MultiplyPoint3x4(wp);

								float texscaleval = Mathf.Lerp(mintexscale, maxtexscale, texscale);

								if ( texscaleval > minsizetoadd )
								{
									scl *= layers[m].scaleOnDist.Evaluate(distalpha) * texscaleval;
									rad *= texscaleval;

									if ( raycast )
									{
										if ( MultiRayCast(wp, rad * layers[m].colradiusadj, layers[m].raycount) )
										{
											RaycastHit hit;
											Vector3 o = wp;
											o.y += collisionOffset;

											if ( Physics.Raycast(o, Vector3.down, out hit, raycastheight) )
											{
												float ang = Mathf.Acos(Vector3.Dot(hit.normal, Vector3.up)) * Mathf.Rad2Deg;

												p = hit.point;
												if ( !layers[m].useheight || (p.y < layers[m].maxheight && p.y > layers[m].minheight) )
												{
													if ( ang >= layers[m].minslope && ang <= layers[m].maxslope )
													{
														//p.y += layers[m].collisionOffset * scl.y;
														p = transform.worldToLocalMatrix.MultiplyPoint3x4(p);

														Vector3 hn = transform.worldToLocalMatrix.MultiplyVector(hit.normal);

														Vector3 newup = Vector3.Lerp(Vector3.up, hn, layers[m].align).normalized;
														p += newup * (layers[m].collisionOffset * scl.y);
														Quaternion align = Quaternion.FromToRotation(Vector3.up, newup);

														layers[m].AddObj(this, p, scl, rot, align, prerot, rc, texturecol);
														addedmesh = true;
													}
												}
											}
										}
										else
										{
											if ( !NeedsGround )
											{
												layers[m].AddObj(this, lp, scl, rot, Quaternion.identity, prerot, rc, texturecol);
												addedmesh = true;
											}
										}
									}
									else
									{
										layers[m].AddObj(this, lp, scl, rot, Quaternion.identity, prerot, rc, texturecol);
										addedmesh = true;
									}

									if ( !addedmesh )
									{
										i--;
										failcount--;
										if ( failcount < 0 )
											break;
									}
									else
									{
										placed++;
										if ( placed > NumPerFrame )
										{
											placed = 0;
											yield return null;
										}
									}
								}
							}
						}
					}
				}
			}
			RestoreColliders();

			if ( dostaticbatching )
				StaticBatchingUtility.Combine(gameObject);
		}
	}

}