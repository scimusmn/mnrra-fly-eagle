using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MultiTerrain
{
	/// <summary>
	/// Terrain stitcher mini.
	/// </summary>
	public class TerrainStitcherMini : MonoBehaviour
	{

		/// <summary>
		/// The level smooth.
		/// </summary>
		public static float levelSmooth = 16;
	
		/// <summary>
		/// The length of the stitch check.
		/// </summary>
		public static int checkLength = 100;
	
		/// <summary>
		/// The power of average function.
		/// </summary>
		public static float power = 2.0f;


	
		public enum Side
		{
			Left,
			Right,
			Top,
			Bottom
		}

		/// <summary>
		/// Average the specified first and second value.
		/// </summary>
		/// <param name="first">First.</param>
		/// <param name="second">Second.</param>
		public static float average (float first, float second)
		{
		
			return Mathf.Pow ((Mathf.Pow (first, power) + Mathf.Pow (second, power)) / 2.0f, 1 / power);


		}

		/// <summary>
		/// Stitchs the terrains.
		/// </summary>
		/// <param name="terrain">Terrain.</param>
		/// <param name="second">Second.</param>
		/// <param name="side">Side.</param>
		/// <param name="xBase">X base.</param>
		/// <param name="yBase">Y base.</param>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		/// <param name="range">Range.</param>
		/// <param name="smooth">If set to <c>true</c> smooth.</param>
		public static void StitchTerrains (Terrain terrain, Terrain second, Side side, int xBase, int yBase, int width, int height, int range, bool smooth = false)
		{
	
		
		
			TerrainData terrainData = terrain.terrainData;
			TerrainData secondData = second.terrainData;
		
		
		
		

			//Debug.Log (xBase + " " + (terrainData.heightmapHeight - 1) + " " + (xBase + width) + " " + 1);
		
			if (side == Side.Right) {
				
				float[,] heights = terrainData.GetHeights (terrainData.heightmapWidth - width, yBase, width, height);
				float[,] secondHeights = secondData.GetHeights (0, yBase, range > 0 ? range : 1, height);

			
				//Debug.Log ("Right " + heights.GetLength (0) + " " + heights.GetLength (1));
				checkLength = width;

				int y = width - 1;
				int x = 0;
			
				int y2 = 0;
							
				for (x = 0; x < heights.GetLength (0); x++) {

					if (heights [x, y] == secondHeights [x, y2])
						continue;

					heights [x, y] = average (heights [x, y], secondHeights [x, y2]);
				
					if (smooth)
						heights [x, y] += Mathf.Abs (heights [x, y - 1] - secondHeights [x, y2 + 1]) / levelSmooth;
				
					secondHeights [x, y2] = heights [x, y];
				
					for (int i = 1; i < checkLength; i++) {
					
						heights [x, y - i] = (average (heights [x, y - i], heights [x, y - i + 1]) + Mathf.Abs (heights [x, y - i] - heights [x, y - i + 1]) / levelSmooth) * (checkLength - i) / checkLength + heights [x, y - i] * i / checkLength;

					}
					for (int i = 1; i < range; i++) {
						secondHeights [x, y2 + i] = (average (secondHeights [x, y2 + i], secondHeights [x, y2 + i - 1]) + Mathf.Abs (secondHeights [x, y2 + i] - secondHeights [x, y2 + i - 1]) / levelSmooth) * (range - i) / range + secondHeights [x, y2 + i] * i / range;
					}
				
				}
				terrainData.SetHeightsDelayLOD (terrainData.heightmapWidth - width, yBase, heights);
				terrain.terrainData = terrainData;

				secondData.SetHeightsDelayLOD (0, yBase, secondHeights);
				second.terrainData = secondData;

			} else {
				
				if (side == Side.Top) {
					
					float[,] heights = terrainData.GetHeights (xBase, terrainData.heightmapHeight - height, width, height);

					float[,] secondHeights = secondData.GetHeights (xBase, 0, width, range > 0 ? range : 1);


					checkLength = height;
					int y = 0;
					int x = height - 1;
					//Debug.Log (x + " " + height + " " + range + " " + heights.GetLength (0) + " " + heights.GetLength (1) + " " + secondHeights.GetLength (0) + " " + secondHeights.GetLength (1));
				

					int x2 = 0;
				
					for (y = 0; y < heights.GetLength (1); y++) {
						
						if (heights [x, y] == secondHeights [x2, y])
							continue;
					
						heights [x, y] = average (heights [x, y], secondHeights [x2, y]);
					
						if (smooth)
							heights [x, y] += Mathf.Abs (heights [x - 1, y] - secondHeights [x2 + 1, y]) / levelSmooth;
					
					
						secondHeights [x2, y] = heights [x, y];
					
						for (int i = 1; i < checkLength; i++) {
						
							heights [x - i, y] = (average (heights [x - i, y], heights [x - i + 1, y]) + Mathf.Abs (heights [x - i, y] - heights [x - i + 1, y]) / levelSmooth) * (checkLength - i) / checkLength + heights [x - i, y] * i / checkLength;

						}
						for (int i = 1; i < range; i++) {
							
							secondHeights [x2 + i, y] = (average (secondHeights [x2 + i, y], secondHeights [x2 + i - 1, y]) + Mathf.Abs (secondHeights [x2 + i, y] - secondHeights [x2 + i - 1, y]) / levelSmooth) * (range - i) / range + secondHeights [x2 + i, y] * i / range;

						}
					
					}


					terrainData.SetHeightsDelayLOD (xBase, terrainData.heightmapHeight - height, heights);
					terrain.terrainData = terrainData;

					secondData.SetHeightsDelayLOD (xBase, 0, secondHeights);
					second.terrainData = secondData;

				}
			}
		
		
	
		
			terrain.Flush ();
			second.Flush ();
		
		
		}

		/// <summary>
		/// Stitchs the terrains and repairs errors.
		/// </summary>
		/// <param name="terrain">Terrain.</param>
		/// <param name="second">Second.</param>
		/// <param name="side">Side.</param>
		/// <param name="xBase">X base.</param>
		/// <param name="yBase">Y base.</param>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public static void StitchTerrainsRepair (Terrain terrain, Terrain second, Side side, int xBase, int yBase, int width, int height)
		{
	
		
		
			TerrainData terrainData = terrain.terrainData;
			TerrainData secondData = second.terrainData;
		
		
		

		
		
		
			if (side == Side.Right) {

				float[,] heights = terrainData.GetHeights (terrainData.heightmapWidth - 1, yBase, 1, height);
				float[,] secondHeights = secondData.GetHeights (0, yBase, 1, height);

		
				int y = 0;
				int x = 0;
			
				int y2 = 0;
			
				for (x = 0; x < heights.GetLength (0); x++) {

					if (heights [x, y] == secondHeights [x, y2])
						continue;

					heights [x, y] = average (heights [x, y], secondHeights [x, y2]);

				
					secondHeights [x, y2] = heights [x, y];

				
				} 

				terrainData.SetHeightsDelayLOD (terrainData.heightmapWidth - 1, yBase, heights);
				terrain.terrainData = terrainData;

				secondData.SetHeightsDelayLOD (0, yBase, secondHeights);
				second.terrainData = secondData;
			} else {
				
				if (side == Side.Top) {
					//Debug.Log ("top");
					float[,] heights = terrainData.GetHeights (xBase, terrainData.heightmapHeight - 1, width, 1);

					float[,] secondHeights = secondData.GetHeights (xBase, 0, width, 1);

					int y = 0;
					int x = 0;
				
					int x2 = 0;
				
					for (y = 0; y < heights.GetLength (1); y++) {
						
						if (heights [x, y] == secondHeights [x2, y])
							continue;
					
						heights [x, y] = average (heights [x, y], secondHeights [x2, y]);
				
					
						secondHeights [x2, y] = heights [x, y];

					}

					terrainData.SetHeightsDelayLOD (xBase, terrainData.heightmapHeight - 1, heights);
					terrain.terrainData = terrainData;

					secondData.SetHeightsDelayLOD (xBase, 0, secondHeights);
					second.terrainData = secondData;

				}

			
			}
		
		
		
		
			terrain.Flush ();
			second.Flush ();
		
		
		}

		/// <summary>
		/// Stitchs the terrains corners.
		/// </summary>
		/// <param name="terrain11">Terrain11.</param>
		/// <param name="terrain21">Terrain21.</param>
		/// <param name="terrain12">Terrain12.</param>
		/// <param name="terrain22">Terrain22.</param>
		public static void StitchTerrainsRepairCorner (Terrain terrain11, Terrain terrain21, Terrain terrain12, Terrain terrain22)
		{
			int size = 0;

			if (terrain11 != null)
				size = terrain11.terrainData.heightmapHeight - 1;
			else if (terrain21 != null)
				size = terrain21.terrainData.heightmapHeight - 1;
			else if (terrain12 != null)
				size = terrain12.terrainData.heightmapHeight - 1;
			else if (terrain22 != null)
				size = terrain22.terrainData.heightmapHeight - 1;
			

			int size0 = 0;
			List<float> heights = new List<float> ();
		
			if (terrain11 != null)
				heights.Add (terrain11.terrainData.GetHeights (size, size0, 1, 1) [0, 0]);
			if (terrain21 != null)
				heights.Add (terrain21.terrainData.GetHeights (size0, size0, 1, 1) [0, 0]);
			if (terrain12 != null)
				heights.Add (terrain12.terrainData.GetHeights (size, size, 1, 1) [0, 0]);
			if (terrain22 != null)
				heights.Add (terrain22.terrainData.GetHeights (size0, size, 1, 1) [0, 0]);
		
		
			float[,] height = new float[1, 1];
			height [0, 0] = heights.Max ();

			if (terrain11 != null)
				terrain11.terrainData.SetHeightsDelayLOD (size, size0, height);
			if (terrain21 != null)
				terrain21.terrainData.SetHeightsDelayLOD (size0, size0, height);
			if (terrain12 != null)
				terrain12.terrainData.SetHeightsDelayLOD (size, size, height);
			if (terrain22 != null)
				terrain22.terrainData.SetHeightsDelayLOD (size0, size, height);

			if (terrain11 != null)
				terrain11.Flush ();
			if (terrain12 != null)
				terrain12.Flush ();
			if (terrain21 != null)
				terrain21.Flush ();
			if (terrain22 != null)
				terrain22.Flush ();
		
		
		}
	}
}
