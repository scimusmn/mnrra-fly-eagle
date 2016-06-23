using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

namespace MultiTerrain
{
	/// <summary>
	/// Multi terrain editor.
	/// </summary>
	public class MultiTerrainEditor : EditorWindow
	{


		#region variables editing terrain

		/// <summary>
		/// The first position for terrain tile management.
		/// </summary>
		public Vector2 firstPosition;

		/// <summary>
		/// The projector transform.
		/// </summary>
		Transform projectorTransform;
		/// <summary>
		/// The projector mouse.
		/// </summary>
		Projector projectorMouse;
		/// <summary>
		/// The projector material.
		/// </summary>
		Material projectorMaterial;
		/// <summary>
		/// The new terrain height.
		/// </summary>
		float newHeight = 5;

		/// <summary>
		/// The terrain max height.
		/// </summary>
		float maxHeight = 100;

		/// <summary>
		/// The drawing opacity.
		/// </summary>
		float opacity = 5;
		/// <summary>
		/// The range to be changed.
		/// </summary>
		int guiRange = 50;
		/// <summary>
		/// The draw range.
		/// </summary>
		int brushRange = 50;
		/// <summary>
		/// The current splat map.
		/// </summary>
		int currentSplatMap = 1;
		/// <summary>
		/// The size of the blur.
		/// </summary>
		float blurSize = 5;

		/// <summary>
		/// The brush texture.
		/// </summary>
		Texture2D brushTexture;
		/// <summary>
		/// The edit.
		/// </summary>
		bool edit = false;
		/// <summary>
		/// The selected method.
		/// </summary>
		int selectedMethod = 0;
		/// <summary>
		/// The _terrain dict.
		/// </summary>
		Dictionary<int[],Terrain> _terrainDict = null;
		/// <summary>
		/// The _terrains list.
		/// </summary>
		Terrain[] _terrains;
		/// <summary>
		/// The big picture to draw.
		/// </summary>
		Color[,] bigPicture;

		/// <summary>
		/// The terrain to height.
		/// </summary>
		float terrainToheight = 1;
		/// <summary>
		/// The terrain to width.
		/// </summary>
		float terrainTowidth = 1;

		//splatmaps
		/// <summary>
		/// The splats texture.
		/// </summary>
		List<Texture2D> splatsTexture = new List<Texture2D> ();
		/// <summary>
		/// The splat prototypes.
		/// </summary>
		List<SplatPrototype> splatPrototypes = new List<SplatPrototype> ();

		/// <summary>
		/// The label message important.
		/// </summary>
		string lbMessageImportant = "";
		/// <summary>
		/// The no terrain message.
		/// </summary>
		string noTerrainMessage = "No Terrain to edit in scene.";

		List<Terrain> terrainsChanged = new List<Terrain> ();


		#endregion

		#region variables gui

		/// <summary>
		/// The brush icons.
		/// </summary>
		GUIContent[] brushIcons;
		/// <summary>
		/// The selected brush icon.
		/// </summary>
		public int selbrushIcon = 0;
		/// <summary>
		/// The button style grid.
		/// </summary>
		GUIStyle buttonStyleGrid;
		/// <summary>
		/// The splat style grid.
		/// </summary>
		GUIStyle splatStyleGrid;
		/// <summary>
		/// The button editing style.
		/// </summary>
		GUIStyle buttonEditingStyle;

	
	
		#endregion

		/// <summary>
		/// Init this instance.
		/// </summary>
		[MenuItem ("Window/Multi Terrain Editor")]
		static void ShowWindow ()
		{

			EditorWindow.GetWindow (typeof(MultiTerrainEditor), false, "Multi Terrain");

		}

		/// <summary>
		/// Raises the focus event.
		/// </summary>
		void OnFocus ()
		{

			SceneView.onSceneGUIDelegate -= this.OnSceneGUI;

			SceneView.onSceneGUIDelegate += this.OnSceneGUI;
			List<GUIContent> listIcon = new List<GUIContent> ();
			for (int i = 1; i <= 20; i++) {

				listIcon.Add (EditorGUIUtility.IconContent ("builtin_brush_" + i));

			}
			brushIcons = listIcon.ToArray ();
			if (projectorTransform == null) {
				GameObject projectorGO = GameObject.Find ("ProjectorParent");
				if (projectorGO != null)
					projectorTransform = projectorGO.transform;
			}

		}

		/// <summary>
		/// Raises the lost focus event.
		/// </summary>
		void OnLostFocus ()
		{
		
			if (projectorTransform != null)
				DestroyProjector ();
		}

		/// <summary>
		/// Raises the destroy event.
		/// </summary>
		void OnDestroy ()
		{
			SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
			if (projectorTransform != null)
				DestroyProjector ();
		}

		/// <summary>
		/// Raises the GU event.
		/// </summary>
		void OnGUI ()
		{
			if (buttonStyleGrid == null) {
				buttonStyleGrid = new GUIStyle (GUI.skin.GetStyle ("GridList"));
			
			
				buttonStyleGrid.margin.right = 2;
				buttonStyleGrid.margin.left = 2;
				buttonStyleGrid.padding.right = 0;
				buttonStyleGrid.padding.left = 0;
				buttonStyleGrid.padding.top = 0;
				buttonStyleGrid.padding.bottom = 0;
				buttonStyleGrid.fixedWidth = 30;
				buttonStyleGrid.fixedHeight = 30;
				buttonStyleGrid.stretchWidth = false;
				buttonStyleGrid.stretchHeight = false;
			
			}
		
			if (splatStyleGrid == null) {
			
			
				splatStyleGrid = new GUIStyle (GUI.skin.GetStyle ("GridList"));
			
				splatStyleGrid.margin.right = 4;
				splatStyleGrid.margin.left = 4;
				splatStyleGrid.padding.right = 0;
				splatStyleGrid.padding.left = 0;
				splatStyleGrid.padding.top = 0;
				splatStyleGrid.padding.bottom = 4;
				splatStyleGrid.fixedWidth = 60;
				splatStyleGrid.fixedHeight = 60;
			
			}

		
			buttonEditingStyle = new GUIStyle (GUI.skin.GetStyle ("Button"));
			buttonEditingStyle.fontStyle = FontStyle.Bold;
			buttonEditingStyle.fontSize = 15;

			EditorGUILayout.Space ();

			if (edit) {
				if (_terrainDict == null || _terrains.Length < 1 || _terrains [0] == null) {
					GetTerrains ();
				}
			}



			if (!edit) {
				GUI.color = Color.green;


				if (GUILayout.Button ("Start editing", buttonEditingStyle, GUILayout.Height (30))) {
					edit = true;
					Repaint ();
					Selection.objects = new Object[0]{ };
					GetTerrains ();


				}
			} else if (edit) {

			
				Color baseCol = GUI.color;
				GUI.color = Color.red;

				if (GUILayout.Button ("Stop editing", buttonEditingStyle, GUILayout.Height (30))) {
					edit = false;
					if (projectorTransform != null)
						DestroyProjector ();
				}
				GUI.color = baseCol;
	
		


				EditorGUILayout.Space ();

				EditorGUILayout.BeginHorizontal ();


		
				GUILayout.FlexibleSpace ();

				GUIStyle buttonStyle;
				buttonStyle = new GUIStyle (UnityEditor.EditorStyles.miniButtonLeft);
				if (selectedMethod == 0)
					buttonStyle.normal.background = buttonStyle.onActive.background;

			
				if (GUILayout.Button (EditorGUIUtility.IconContent ("TerrainInspector.TerrainToolRaise"), buttonStyle)) {
				
					selectedMethod = 0;
				}

				buttonStyle = new GUIStyle (UnityEditor.EditorStyles.miniButtonMid);
				if (selectedMethod == 1)
					buttonStyle.normal.background = buttonStyle.onActive.background;
				if (GUILayout.Button (EditorGUIUtility.IconContent ("TerrainInspector.TerrainToolSetheight"), buttonStyle)) {
				
					selectedMethod = 1;
				}

				buttonStyle = new GUIStyle (UnityEditor.EditorStyles.miniButtonMid);
				if (selectedMethod == 2)
					buttonStyle.normal.background = buttonStyle.onActive.background;
				if (GUILayout.Button (EditorGUIUtility.IconContent ("TerrainInspector.TerrainToolSmoothheight"), buttonStyle)) {
				
					selectedMethod = 2;
				}

				buttonStyle = new GUIStyle (UnityEditor.EditorStyles.miniButtonRight);
				if (selectedMethod == 4)
					buttonStyle.normal.background = buttonStyle.onActive.background;
			
			
				if (GUILayout.Button (EditorGUIUtility.IconContent ("TerrainInspector.TerrainToolSplat"), buttonStyle)) {
				
					selectedMethod = 4;
				}
				GUILayout.FlexibleSpace ();
				EditorGUILayout.EndHorizontal ();

			
				GUILayout.Label ("Brushes", EditorStyles.boldLabel);

				EditorGUILayout.BeginHorizontal ("Box");
		


				selbrushIcon = GUILayout.SelectionGrid (selbrushIcon, brushIcons, Screen.width / 32, buttonStyleGrid, GUILayout.Width (Screen.width - 20));



			
				EditorGUILayout.EndHorizontal ();



				EditorGUILayout.Space ();


				if (selectedMethod == 4) {
				
				
				
				
					//////SplatMaps
				
					GUILayout.Label ("Textures", EditorStyles.boldLabel);
					EditorGUILayout.BeginHorizontal ("Box");
				
				
				 
					currentSplatMap = GUILayout.SelectionGrid (currentSplatMap, splatsTexture.ToArray (), Screen.width / 64, splatStyleGrid, GUILayout.Width (Screen.width - 20));



					EditorGUILayout.EndHorizontal ();

					EditorGUILayout.BeginHorizontal ();
					if (GUILayout.Button ("Add Texture")) {
				
						SplatTextureEditor splatTextureEditor = (SplatTextureEditor)EditorWindow.GetWindow (typeof(SplatTextureEditor), false, "Splat Texture Editor");
						splatTextureEditor.add = true;
						splatTextureEditor.multiTerrainEditor = this;

					}

					if (GUILayout.Button ("Edit Texture")) {
					
						SplatTextureEditor splatTextureEditor = (SplatTextureEditor)EditorWindow.GetWindow (typeof(SplatTextureEditor), false, "Splat Texture Editor");
						splatTextureEditor.add = false;
						splatTextureEditor.multiTerrainEditor = this;
						splatTextureEditor.splatTexture = splatsTexture [currentSplatMap];
						splatTextureEditor.normalTexture = splatPrototypes [currentSplatMap].normalMap;
						splatTextureEditor.tileSize = splatPrototypes [currentSplatMap].tileSize;
						splatTextureEditor.tileOffset = splatPrototypes [currentSplatMap].tileOffset;
						splatTextureEditor.metallic = splatPrototypes [currentSplatMap].metallic;
						splatTextureEditor.smoothness = splatPrototypes [currentSplatMap].smoothness;
					}
					if (GUILayout.Button ("Remove Texture")) {
						RemoveSplatPrototype ();
					}
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.Space ();
			


					if (splatsTexture == null)
						splatsTexture = new List<Texture2D> ();
				

			

					if (GUILayout.Button ("Get splat textures from selected terrains")) {
						GetSplatFromSelected ();
					}

					if (GUILayout.Button ("Set splat textures in selected terrains")) {
						SetSelectedTerrainSplatPrototypes ();
					}

					if (GUILayout.Button ("Set splat textures in all terrains")) {

						if (EditorUtility.DisplayDialog ("Confirmation", "Do You want to set  splat textures to all terrains?", "Yes", "No"))
							SetTerrainSplatPrototypes ();
					}
				
					EditorGUILayout.Space ();
								
//				if (GUILayout.Button ("TerrainDarw")) {
//									
//					DrawSteppnes ();
//				}
				
				}
			
				EditorGUILayout.Space ();
				GUILayout.Label ("Settings", EditorStyles.boldLabel);

				EditorGUILayout.BeginHorizontal ("Label");
				guiRange = EditorGUILayout.IntSlider ("Brush Size", (int)guiRange, 1, 200);
				brushRange = (guiRange % 2 == 1) ? guiRange + 1 : guiRange;
				EditorGUILayout.EndHorizontal ();

				if (selectedMethod != 0) {
					EditorGUILayout.BeginHorizontal ("Label");
					opacity = EditorGUILayout.Slider ("Opacity", opacity, 0.1f, 100);
					
					EditorGUILayout.EndHorizontal ();

				}

				if (selectedMethod == 0) {
					EditorGUILayout.BeginHorizontal ("Label");
					newHeight = EditorGUILayout.Slider ("Height", newHeight, 0.1f, 100);
				
					EditorGUILayout.EndHorizontal ();
				}
			
				if (selectedMethod == 1) {
					EditorGUILayout.BeginHorizontal ("Label");
					newHeight = EditorGUILayout.Slider ("Height", newHeight, 0, maxHeight);
					if (GUILayout.Button ("Flatten", GUILayout.Width (50))) {
						if (EditorUtility.DisplayDialog ("Confirmation", "Do You want to flatten all terrains, undo maybe impossible?", "Yes", "No"))
							Flatten ();
					}
				
					EditorGUILayout.EndHorizontal ();
				}

				int brushScaledRangeX = (int)(brushRange * terrainTowidth);
				int brushScaledRangeZ = (int)(brushRange * terrainToheight);
		

				ScaleTexture ((Texture2D)brushIcons [selbrushIcon].image, brushScaledRangeX, brushScaledRangeZ);


			}
			if (!string.IsNullOrEmpty (lbMessageImportant)) {
				EditorGUILayout.Space ();

				EditorGUILayout.BeginHorizontal ("Label");

				GUIStyle style = new GUIStyle (EditorStyles.boldLabel);
				style.alignment = TextAnchor.UpperCenter;
				style.normal.textColor = Color.red;
				style.fontSize = 14;
				style.fixedHeight = 50;

				EditorGUILayout.LabelField (lbMessageImportant, style);

				EditorGUILayout.EndHorizontal ();
			}

	

		}

		/// <summary>
		/// Destroies the projector.
		/// </summary>
		void DestroyProjector ()
		{

			DestroyImmediate (brushTexture);
			DestroyImmediate (projectorMaterial);

			DestroyImmediate (projectorTransform.gameObject);
			System.GC.Collect ();
			Resources.UnloadUnusedAssets ();
		
		}

		/// <summary>
		/// Sets the terrain splat prototypes.
		/// </summary>
		public void SetTerrainSplatPrototypes ()
		{

			if (_terrains.Length > 0) {
				foreach (var t in _terrains) {
					TerrainData data = t.terrainData;

					float[,,] newMap = ClearAlphaMaps (t);

					data.splatPrototypes = splatPrototypes.ToArray ();
					data.RefreshPrototypes ();

					if (newMap.GetLength (2) == t.terrainData.alphamapLayers)
						t.terrainData.SetAlphamaps (0, 0, newMap);

					t.Flush ();
				}
			}
		}

		/// <summary>
		/// Sets the selected terrains splat prototypes.
		/// </summary>
		public void SetSelectedTerrainSplatPrototypes ()
		{
			List<Terrain> terrainsSelected = new List<Terrain> ();


			foreach (var item in Selection.gameObjects) {
				Terrain terrain = item.GetComponent<Terrain> ();
				if (terrain != null)
					terrainsSelected.Add (terrain);
			}

		
			if (terrainsSelected.Count > 0) {
				foreach (var t in terrainsSelected) {
					TerrainData data = t.terrainData;
					float[,,] newMap = ClearAlphaMaps (t);
					data.splatPrototypes = splatPrototypes.ToArray ();

					data.RefreshPrototypes ();

					if (newMap.GetLength (2) == t.terrainData.alphamapLayers)
						t.terrainData.SetAlphamaps (0, 0, newMap);

					t.Flush ();
				}
			}
		}

		public float[,,] ClearAlphaMaps (Terrain t)
		{
			float[,,] map = t.terrainData.GetAlphamaps (0, 0, t.terrainData.alphamapWidth, t.terrainData.alphamapHeight);
	
			if (splatPrototypes.Count == 0 || splatPrototypes.Count >= t.terrainData.alphamapLayers) {
				return map;
			}

			float[,,] newMap = new float[t.terrainData.alphamapWidth, t.terrainData.alphamapHeight, splatPrototypes.Count];

			for (int i = 0; i < splatPrototypes.Count; i++) {
				for (int y = 0; y < t.terrainData.alphamapHeight; y++) {
					for (int x = 0; x < t.terrainData.alphamapWidth; x++) {
						newMap [x, y, i] = map [x, y, i];
					}
				}
			}

			for (int y = 0; y < t.terrainData.alphamapHeight; y++) {
				for (int x = 0; x < t.terrainData.alphamapWidth; x++) {
					for (int i = splatPrototypes.Count; i < t.terrainData.alphamapLayers; i++) {

						newMap [x, y, 0] += map [x, y, i];
						if (newMap [x, y, splatPrototypes.Count - 1] > 1)
							newMap [x, y, splatPrototypes.Count - 1] = 1;
					}
				}
			}

			return newMap;

		}

		/// <summary>
		/// Adds the splat prototype.
		/// </summary>
		/// <param name="prototype">Prototype.</param>
		public void AddSplatPrototype (SplatPrototype prototype)
		{
			splatPrototypes.Add (prototype);
			splatsTexture.Clear ();
			foreach (var item in splatPrototypes) {
				splatsTexture.Add (item.texture);
			}
		}

		/// <summary>
		/// Edits the splat prototype.
		/// </summary>
		/// <param name="prototype">Prototype.</param>
		public void EditSplatPrototype (SplatPrototype prototype)
		{
			splatPrototypes [currentSplatMap] = prototype;
			splatsTexture.Clear ();
			foreach (var item in splatPrototypes) {

				splatsTexture.Add (item.texture);
			}
		}

		/// <summary>
		/// Remove the splat prototype.
		/// </summary>
		/// <param name="prototype">Prototype.</param>
		public void RemoveSplatPrototype ()
		{
			if (splatPrototypes.Count < currentSplatMap)
				return;
			splatPrototypes.Remove (splatPrototypes [currentSplatMap]);

			splatsTexture.Clear ();
			foreach (var item in splatPrototypes) {
				splatsTexture.Add (item.texture);
			}
		}

		/// <summary>
		/// Gets the splat from selected.
		/// </summary>
		void GetSplatFromSelected ()
		{
			if (Selection.activeGameObject == null)
				return;
			Terrain terrain = Selection.activeGameObject.GetComponent<Terrain> ();
			if (terrain != null) {
				splatPrototypes.Clear ();
				splatPrototypes.AddRange (terrain.terrainData.splatPrototypes);
				splatsTexture.Clear ();
				foreach (var item in splatPrototypes) {
					splatsTexture.Add (item.texture);
				}
			}
		}

		/// <summary>
		/// Draws the steppnes.
		/// </summary>
		void DrawSteppnes ()
		{
			foreach (var t in _terrains) {
				float[,,] map = new float[t.terrainData.alphamapWidth, t.terrainData.alphamapHeight, t.terrainData.alphamapLayers];
			
				// For each point on the alphamap...
				for (var y = 0; y < t.terrainData.alphamapHeight; y++) {
					for (var x = 0; x < t.terrainData.alphamapWidth; x++) {
						// Get the normalized terrain coordinate that
						// corresponds to the the point.
						float normX = x * 1.0f / (t.terrainData.alphamapWidth - 1);
						float normY = y * 1.0f / (t.terrainData.alphamapHeight - 1);
					
						// Get the steepness value at the normalized coordinate.
						float angle = t.terrainData.GetSteepness (normY, normX);
					
						// Steepness is given as an angle, 0..90 degrees. Divide
						// by 90 to get an alpha blending value in the range 0..1.
						float frac = angle / 90.0f;
						map [x, y, 1] = frac;
						map [x, y, 0] = 1 - frac;
					}
				}
			
				t.terrainData.SetAlphamaps (0, 0, map);
			}
		}

		/// <summary>
		/// Draws the brush.
		/// </summary>
		/// <param name="pos">Position.</param>
		/// <param name="radius">Radius.</param>
		/// <param name="terrain">Terrain.</param>
		/// <param name="color">Color.</param>
		public void DrawBrush (Vector3 pos, float radius, Terrain terrain, Color color)
		{
			//incline is the height delta in one unit distance
			Handles.color = color;
		
			int numCorners = 32;
			Vector3[] corners = new Vector3[numCorners + 1];
			float step = 360f / numCorners;
			for (int i = 0; i <= corners.Length - 1; i++) {
				corners [i] = new Vector3 (Mathf.Sin (step * i * Mathf.Deg2Rad), 0, Mathf.Cos (step * i * Mathf.Deg2Rad)) * radius + pos;
				corners [i].y = terrain.SampleHeight (corners [i]);
			}
			Handles.DrawAAPolyLine (4, corners);
		}



		/// <summary>
		/// Raises the scene GU event.
		/// </summary>
		/// <param name="sceneView">Scene view.</param>
		public void OnSceneGUI (SceneView sceneView)
		{


			if (edit) {

				if (_terrainDict == null || _terrains.Length < 1 || _terrains [0] == null) {
					GetTerrains ();
					return;
				}

				HandleUtility.AddDefaultControl (GUIUtility.GetControlID (FocusType.Passive));
				Camera sceneCamera = SceneView.lastActiveSceneView.camera;
				Vector2 mousePos = Event.current.mousePosition;
				mousePos.y = Screen.height - mousePos.y - 40;
				Ray ray = sceneCamera.ScreenPointToRay (mousePos);

				RaycastHit[] hits = Physics.RaycastAll (ray, Mathf.Infinity);




				if (hits.Length > 0) {
					Terrain terrain = null;
					Vector3 brushPos = Vector3.zero;
					foreach (var hit in hits) {
						if (hit.collider is TerrainCollider) {
							brushPos = hit.point;
							terrain = hit.collider.GetComponent<Terrain> ();
							break;
						}
					}


		

					if (!terrain)
						return;

					if (projectorTransform == null) {
						GameObject projGO = GameObject.Find ("ProjectorParent");

						if (projGO != null)
							projectorTransform = projGO.transform;
						else
							projectorTransform = new GameObject ("ProjectorParent").transform;	
						GameObject child = new GameObject ("Projector");
						child.transform.parent = projectorTransform;
						child.transform.localPosition = new Vector3 (0, 20000, 0);
						child.transform.eulerAngles = new Vector3 (90, 180, 0);
						projectorMouse = child.AddComponent<Projector> ();
						projectorMouse.farClipPlane = 30000;
						projectorMouse.nearClipPlane = 1000;
						projectorMouse.orthographicSize = 100;
						projectorMouse.orthographic = true;
						if (projectorMaterial == null) {
							projectorMaterial = new Material (Shader.Find ("Projector/Projector MultiplyMine"));

						}
						projectorMaterial.color = new Color32 (0, 86, 255, 255);
						projectorMouse.material = projectorMaterial;
				

					} else {

						if (projectorMouse && projectorMaterial != null) {
							projectorMaterial.SetTexture ("_ShadowTex", brushTexture);
							projectorMouse.orthographicSize = brushRange * 0.5f;

					
						}
					
					}

					projectorTransform.position = brushPos;
					//projectorTransform.hideFlags = HideFlags.HideAndDontSave;

					if (Event.current.alt)
						return;

					if (Event.current.type == EventType.MouseUp && Event.current.button == 0) {

						foreach (var item in _terrains) {
							item.ApplyDelayedHeightmapModification ();
							
						}
						terrainsChanged.Clear ();
						return;

					}

				
					if (!(Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag) || Event.current.button != 0)
						return;

					if (Event.current.type == EventType.MouseDown && Event.current.button == 0) {
//						System.GC.Collect ();
//						Resources.UnloadUnusedAssets ();

				

					}
				
				
					int brushScaledRangeX = (int)(brushRange * terrainTowidth);
					int brushScaledRangeZ = (int)(brushRange * terrainToheight);
				

					if (brushTexture == null || brushTexture.width == 0)
						ScaleTexture ((Texture2D)brushIcons [selbrushIcon].image, brushScaledRangeX, brushScaledRangeZ);
				
					if (brushTexture.width != 0) {

					

						Color[] colors = brushTexture.GetPixels ();

						if (colors.Length != brushScaledRangeX * brushScaledRangeZ) {
							ScaleTexture ((Texture2D)brushIcons [selbrushIcon].image, brushScaledRangeX, brushScaledRangeZ);
							colors = brushTexture.GetPixels ();
						}

						bigPicture = new Color[brushScaledRangeX, brushScaledRangeZ];

						for (int i = 0; i < brushScaledRangeX; i++) {
							for (int h = 0; h < brushScaledRangeZ; h++) {
							
								bigPicture [i, h] = colors [i + h * brushScaledRangeX];
							
							}
						}

						DrawOnTerrain (brushPos, terrain);

					}
				}

			}
		}

		/// <summary>
		/// Scales the texture.
		/// </summary>
		/// <returns>The texture.</returns>
		/// <param name="source">Source.</param>
		/// <param name="targetWidth">Target width.</param>
		/// <param name="targetHeight">Target height.</param>
		private Texture2D ScaleTexture (Texture2D source, int targetWidth, int targetHeight)
		{


			if (brushTexture == null)
				brushTexture = new Texture2D (targetWidth, targetHeight, source.format, false);
			else
				brushTexture.Resize (targetWidth, targetHeight, source.format, false);
		
	
			for (int i = 0; i < brushTexture.height; ++i) {
				for (int j = 0; j < brushTexture.width; ++j) {
					Color newColor = source.GetPixelBilinear ((float)j / (float)brushTexture.width, (float)i / (float)brushTexture.height);
					brushTexture.SetPixel (j, i, newColor);
				}
			}
		
			brushTexture.Apply ();
			return brushTexture;
		}


		#region terrain editing



		/// <summary>
		/// Gets the terrains.
		/// </summary>
		public void GetTerrains ()
		{
			_terrains = Terrain.activeTerrains;

			if (_terrains.Length > 0) {

				float yHeight = _terrains [0].transform.position.y;

				float width = _terrains [0].terrainData.size.x;
				float height = _terrains [0].terrainData.size.y;
				float length = _terrains [0].terrainData.size.z;

				float hWidth = _terrains [0].terrainData.heightmapWidth;
				float hHeight = _terrains [0].terrainData.heightmapHeight;

				bool heightGood = true;
				bool size = true;
				bool resolution = true;

				foreach (var item in _terrains) {
					if (yHeight != item.transform.position.y) {

						heightGood = false;
					}
					if (width != item.terrainData.size.x || height != item.terrainData.size.y || length != item.terrainData.size.z) {

						size = false;
					}
					if (hWidth != item.terrainData.heightmapWidth || hHeight != item.terrainData.heightmapHeight) {
						resolution = false;
					}
				}

				if (!heightGood)
					EditorUtility.DisplayDialog ("Terrain errors", "Not all terrains are at the same height, this could effect holes on the borders.", "OK");
				if (!size)
					EditorUtility.DisplayDialog ("Terrain errors", "Not all terrains are the same size, this could effect in errors. Turn off or fix mismatched terrians.", "OK");
				if (!resolution)
					EditorUtility.DisplayDialog ("Terrain errors", "Not all terrains are the same resolution, this could effect in errors. Turn off or fix mismatched terrians", "OK");

			}

		
			if (_terrainDict == null)
				_terrainDict = new Dictionary<int[], Terrain> (new IntArrayComparer ());
			else {
				_terrainDict.Clear ();
			}
		
			if (_terrains.Length > 0) {
				int sizeX = (int)_terrains [0].terrainData.size.x;
				int sizeZ = (int)_terrains [0].terrainData.size.z;

				float maxHeightTemp = float.MinValue;

				if (_terrains.Length > 0)
					firstPosition = new Vector2 (_terrains [0].transform.position.x, _terrains [0].transform.position.z);

				foreach (var terrain in _terrains) {
				
					int[] posTer = new int[] {
						(int)(Mathf.RoundToInt ((terrain.transform.position.x - firstPosition.x) / sizeX)),
						(int)(Mathf.RoundToInt ((terrain.transform.position.z - firstPosition.y) / sizeZ))
					};
					//Debug.Log (terrain.transform.position.ToString () + " " + posTer [0] + " " + posTer [1]);
					if (!_terrainDict.ContainsKey (posTer))
						_terrainDict.Add (posTer, terrain);
					else {
						EditorUtility.DisplayDialog ("Terrain errors", terrain.name + " has unexpected position.", "OK");
					}
				
					if (terrain.terrainData.size.y > maxHeightTemp)
						maxHeightTemp = terrain.terrainData.size.y;
				
				}
				maxHeight = maxHeightTemp;
			
				foreach (var item in _terrainDict) {
					int[] posTer = item.Key;
					Terrain top = null;
					Terrain left = null;
					Terrain right = null;
					Terrain bottom = null;
					_terrainDict.TryGetValue (new int[] {
						posTer [0],
						posTer [1] + 1
					}, out top);
					_terrainDict.TryGetValue (new int[] {
						posTer [0] - 1,
						posTer [1]
					}, out left);
					_terrainDict.TryGetValue (new int[] {
						posTer [0] + 1,
						posTer [1]
					}, out right);
					_terrainDict.TryGetValue (new int[] {
						posTer [0],
						posTer [1] - 1
					}, out bottom);
				
					item.Value.SetNeighbors (left, top, right, bottom);
				
					item.Value.Flush ();
				
				
				}

				lbMessageImportant = "";
			} else {
				edit = false;
				lbMessageImportant = noTerrainMessage;
			}
		}

		/// <summary>
		/// Flatten terrains.
		/// </summary>
		public void Flatten ()
		{
			try {
				foreach (var t in _terrains) {
					Undo.RegisterUndo (t.terrainData, "Terrain draw end");

					//if (selectedMethod == 4)
					//	Undo.RegisterUndo (t.terrainData.alphamapTextures, "alpha");
				}
			} catch (System.Exception ex) {
				Debug.Log ("Too many terrains to undo flatten");
			}
			

			foreach (var item in Terrain.activeTerrains) {
				FlattenHeights (item);
			}
		}

		/// <summary>
		/// Flattens the heights.
		/// </summary>
		/// <param name="terrain">Terrain.</param>
		public void FlattenHeights (Terrain terrain)
		{
		
			float[,] heights = new float[terrain.terrainData.heightmapWidth, terrain.terrainData.heightmapHeight];
		
			for (int i = 0; i < terrain.terrainData.heightmapWidth; i++) {
				for (int k = 0; k < terrain.terrainData.heightmapHeight; k++) {
					heights [i, k] = newHeight / terrain.terrainData.size.y;
				}
			}
		
			terrain.terrainData.SetHeightsDelayLOD (0, 0, heights);
		}

		/// <summary>
		/// Distances the point to rectangle.
		/// </summary>
		/// <returns>The point to rectangle.</returns>
		/// <param name="point">Point.</param>
		/// <param name="rect">Rect.</param>
		public static float DistancePointToRectangle (Vector2 point, Rect rect)
		{

		
			if (point.x < rect.xMin) {
				if (point.y < rect.yMin) {
					Vector2 diff = point - new Vector2 (rect.xMin, rect.yMin);
					return diff.magnitude;
				} else if (point.y > rect.yMax) { 
					Vector2 diff = point - new Vector2 (rect.xMin, rect.yMax);
					return diff.magnitude;
				} else { 
					return rect.xMin - point.x;
				}
			} else if (point.x > rect.xMax) {
				if (point.y < rect.yMin) {
					Vector2 diff = point - new Vector2 (rect.xMax, rect.yMin);
					return diff.magnitude;
				} else if (point.y > rect.yMax) {
					Vector2 diff = point - new Vector2 (rect.xMax, rect.yMax);
					return diff.magnitude;
				} else {
					return point.x - rect.xMax;
				}
			} else { 
				if (point.y < rect.yMin) { 
					return rect.yMin - point.y;
				} else if (point.y > rect.yMax) {
					return point.y - rect.yMax;
				} else {
					return 0f;
				}
			}
		}

		/// <summary>
		/// Draws the on terrain.
		/// </summary>
		/// <param name="brushPos">Brush position.</param>
		/// <param name="terrain">Terrain.</param>
		/// <param name="checkTerrainSides">If set to <c>true</c> check terrain sides.</param>
		void DrawOnTerrain (Vector3 brushPos, Terrain terrain, bool checkTerrainSides = true)
		{
			

			if (terrain == null)
				return;

			if (!terrainsChanged.Contains (terrain)) {
				
				terrainsChanged.Add (terrain);

				Undo.RegisterUndo (terrain.terrainData, "Terrain draw end");

				if (selectedMethod == 4) {
					Undo.RegisterUndo (terrain, "Terrain draw texture");
					Undo.RegisterUndo (terrain.terrainData.alphamapTextures, "alpha");
				}


			}


			//Undo.RegisterUndo (terrain.terrainData, "Terrain draw end");
			//if (selectedMethod == 4)
			//	Undo.RegisterUndo (terrain.terrainData.alphamapTextures, "alpha");
		
		
			TerrainData data = terrain.terrainData;
			Rect terrainRect = new Rect (terrain.transform.position.x, terrain.transform.position.z, data.size.x, data.size.z);

	
			if (DistancePointToRectangle (new Vector2 (brushPos.x, brushPos.z), terrainRect) > brushRange * 0.5f)
				return;

			int sizeToCheckX = (selectedMethod == 4 ? data.alphamapHeight : data.heightmapHeight);
			int sizeToCheckZ = (selectedMethod == 4 ? data.alphamapWidth : data.heightmapWidth);

			float heightToPaint = data.alphamapHeight / (float)data.heightmapHeight;

			terrainToheight = (1 / data.size.x * data.heightmapHeight) * (selectedMethod == 4 ? heightToPaint : 1);
			terrainTowidth = (1 / data.size.z * data.heightmapWidth) * (selectedMethod == 4 ? heightToPaint : 1);

			int rangeTerrainX = (int)(brushRange * terrainToheight);
			int rangeTerrainZ = (int)(brushRange * terrainTowidth);


			Vector3 terrainSpaceCoords = brushPos - terrain.transform.position;
		

			float posX = terrainSpaceCoords.x * terrainToheight;
			

			float posZ = terrainSpaceCoords.z * terrainTowidth;

			int posXFinal = (int)(posX - 0.5f * rangeTerrainX) + 1;
			int rangeX = rangeTerrainX;

			int posZFinal = (int)(posZ - 0.5f * rangeTerrainZ) + 1;
			int rangeZ = rangeTerrainZ;

			int rangeImageX = 0;
			int rangeImageZ = 0;



			Terrain left = null;
			Terrain bottomleft = null;
			Terrain topleft = null;
			Terrain right = null;
			Terrain bottomright = null;
			Terrain topright = null;
			Terrain bottom = null;
			Terrain top = null;

			if (posXFinal < 0) {
				rangeX = rangeX + posXFinal;
				posXFinal = 0;



				int[] posTer = new int[] {
					(int)(Mathf.RoundToInt ((terrain.transform.position.x - firstPosition.x) / data.size.x)),
					(int)(Mathf.RoundToInt ((terrain.transform.position.z - firstPosition.y) / data.size.z))
				};
				

				_terrainDict.TryGetValue (new int[] {
					posTer [0] - 1,
					posTer [1]
				}, out left);

				if (checkTerrainSides)
					DrawOnTerrain (brushPos, left, false);

			
				if (posZFinal < 0) {
					posTer = new int[] {
						(int)(Mathf.RoundToInt ((terrain.transform.position.x - firstPosition.x) / data.size.x)),
						(int)(Mathf.RoundToInt ((terrain.transform.position.z - firstPosition.y) / data.size.z))
					};

					_terrainDict.TryGetValue (new int[] {
						posTer [0] - 1,
						posTer [1] - 1
					}, out bottomleft);
					if (checkTerrainSides)
						DrawOnTerrain (brushPos, bottomleft, false);

				}

				if (posZFinal + rangeZ > sizeToCheckZ - 1) {

					posTer = new int[] {
						(int)(Mathf.RoundToInt ((terrain.transform.position.x - firstPosition.x) / data.size.x)),
						(int)(Mathf.RoundToInt ((terrain.transform.position.z - firstPosition.y) / data.size.z))
					};
					

					_terrainDict.TryGetValue (new int[] {
						posTer [0] - 1,
						posTer [1] + 1
					}, out topleft);
					if (checkTerrainSides)
						DrawOnTerrain (brushPos, topleft, false);


				}
				
			
			}

			if (posXFinal + rangeX > sizeToCheckX - 1) {
				rangeX = sizeToCheckX - posXFinal;
				rangeImageX = rangeTerrainX - rangeX;
			
		
				int[] posTer = new int[] {
					(int)(Mathf.RoundToInt ((terrain.transform.position.x - firstPosition.x) / data.size.x)),
					(int)(Mathf.RoundToInt ((terrain.transform.position.z - firstPosition.y) / data.size.z))
				};
				

				_terrainDict.TryGetValue (new int[] {
					posTer [0] + 1,
					posTer [1]
				}, out right);
				if (checkTerrainSides)
					DrawOnTerrain (brushPos, right, false);

				if (posZFinal < 0) {
					posTer = new int[] {
						(int)(Mathf.RoundToInt ((terrain.transform.position.x - firstPosition.x) / data.size.x)),
						(int)(Mathf.RoundToInt ((terrain.transform.position.z - firstPosition.y) / data.size.z))
					};
					
				
					_terrainDict.TryGetValue (new int[] {
						posTer [0] + 1,
						posTer [1] - 1
					}, out bottomright);
					if (checkTerrainSides)
						DrawOnTerrain (brushPos, bottomright, false);
				}
				
				if (posZFinal + rangeZ > sizeToCheckZ - 1) {
					
					posTer = new int[] {
						(int)(Mathf.RoundToInt ((terrain.transform.position.x - firstPosition.x) / data.size.x)),
						(int)(Mathf.RoundToInt ((terrain.transform.position.z - firstPosition.y) / data.size.z))
					};
					

					_terrainDict.TryGetValue (new int[] {
						posTer [0] + 1,
						posTer [1] + 1
					}, out topright);
					if (checkTerrainSides)
						DrawOnTerrain (brushPos, topright, false);
				}


			}

			if (posZFinal < 0) {


				rangeZ = rangeZ + posZFinal;
				posZFinal = 0;


				int[] posTer = new int[] {
					(int)(Mathf.RoundToInt ((terrain.transform.position.x - firstPosition.x) / data.size.x)),
					(int)(Mathf.RoundToInt ((terrain.transform.position.z - firstPosition.y) / data.size.z))
				};
				

				_terrainDict.TryGetValue (new int[] {
					posTer [0],
					posTer [1] - 1
				}, out bottom);


				if (checkTerrainSides)
					DrawOnTerrain (brushPos, bottom, false);


			}
			if (posZFinal + rangeZ > sizeToCheckZ - 1) {
				rangeZ = sizeToCheckZ - posZFinal;
				rangeImageZ = rangeTerrainZ - rangeZ;
			
		
				int[] posTer = new int[] {
					(int)(Mathf.RoundToInt ((terrain.transform.position.x - firstPosition.x) / data.size.x)),
					(int)(Mathf.RoundToInt ((terrain.transform.position.z - firstPosition.y) / data.size.z))
				};
				

				_terrainDict.TryGetValue (new int[] {
					posTer [0],
					posTer [1] + 1
				}, out top);
				if (checkTerrainSides)
					DrawOnTerrain (brushPos, top, false);

			}

	
	
			if (rangeX <= 0 || rangeZ <= 0)
				return;
	
			if (selectedMethod < 3) {

				if (data.heightmapHeight < posXFinal + rangeX || data.heightmapWidth < posZFinal + rangeZ)
					return;

				float[,] height;
				try {

					height = data.GetHeights (posXFinal, posZFinal, rangeX, rangeZ);
				} catch {
					return;
				}

				int shift = (Event.current.shift) ? -1 : 1;
				float scale = newHeight / data.size.y;



				if (selectedMethod == 1 && shift == -1 && checkTerrainSides) {
					newHeight = terrain.terrainData.GetHeight ((int)posX, (int)posZ);
					Repaint ();
				}

				for (int i = 0; i < height.GetLength (0); i++) {
					for (int h = 0; h < height.GetLength (1); h++) {
						float imagePixel;
						try {


							imagePixel = bigPicture [height.GetLength (0) - i - 1 + rangeImageZ, height.GetLength (1) - h - 1 + rangeImageX].a;
							//imagePixel = bigPicture [i, h].a;


						} catch {
							continue;
						}


						switch (selectedMethod) {
						case 0:
							height [i, h] = height [i, h] + shift * imagePixel * scale;
							break;
						case 1:
							if (shift == 1)
								height [i, h] = Mathf.Lerp (height [i, h], scale, imagePixel * opacity / 100.0f);

							break;
						case 2:

							float sum = 0;
							float count = 0;

							for (int x = (i - blurSize * 0.5f) > 0 ? (int)(i - blurSize * 0.5f) : i; (x < blurSize * 0.5f + i && x < height.GetLength (0)); x++) {
								for (int y = (h - blurSize * 0.5f) > 0 ? (int)(h - blurSize * 0.5f) : h; (y < blurSize * 0.5f + h && y < height.GetLength (1)); y++) {
									sum += height [x, y];
									count++;
								}
							}
					
							float average = sum / count;


							height [i, h] = Mathf.Lerp (height [i, h], average, imagePixel * opacity / 100.0f);

							break;
						default:
							break;
						}



					}
				}


				data.SetHeightsDelayLOD (posXFinal, posZFinal, height);

			} else if (selectedMethod == 4) {

				//Undo.RegisterUndo (terrain, "Terrain draw texture");
			

				//Undo.RegisterUndo (terrain.terrainData.alphamapTextures, "alpha");
			
			

				if (data.alphamapHeight < posXFinal + rangeX || data.alphamapHeight < posZFinal + rangeZ)
					return;


				float[,,] map;
				try {
				
					map = data.GetAlphamaps (posXFinal, posZFinal, rangeX, rangeZ);
				} catch {
					return;
				}


			

				for (int i = 0; i < map.GetLength (0); i++) {
					for (int h = 0; h < map.GetLength (1); h++) {

						if (currentSplatMap >= map.GetLength (2))
							continue;
						float imagePixel;
						try {
						
							imagePixel = (bigPicture [map.GetLength (0) - i - 1 + rangeImageZ, map.GetLength (1) - h - 1 + rangeImageX].a);

						
						} catch {
							continue;
						}
						float oldValue = map [i, h, currentSplatMap];

						map [i, h, currentSplatMap] = Mathf.Lerp (map [i, h, currentSplatMap], 1, imagePixel * opacity / 100.0f);

						for (int s = 0; s < data.alphamapLayers; s++) {
							if (s != currentSplatMap)
								map [i, h, s] = Mathf.Clamp01 (map [i, h, s] / (1 - oldValue) * (1 - map [i, h, currentSplatMap]));
						}

				

					}
				}
			
				data.SetAlphamaps (posXFinal, posZFinal, map);
			}
			terrain.Flush ();

			if (selectedMethod == 2) {
				if (checkTerrainSides) {
					if (right != null) {
						TerrainStitcherMini.StitchTerrains (terrain, right, TerrainStitcherMini.Side.Right, posXFinal, posZFinal, rangeX, rangeZ, rangeTerrainX - rangeX);
			
					}
					if (left != null) {
						TerrainStitcherMini.StitchTerrains (left, terrain, TerrainStitcherMini.Side.Right, posXFinal, posZFinal, rangeTerrainX - rangeX, rangeZ, rangeX);
					}
			
					if (top != null) {
						TerrainStitcherMini.StitchTerrains (terrain, top, TerrainStitcherMini.Side.Top, posXFinal, posZFinal, rangeX, rangeZ, rangeTerrainZ - rangeZ);			
					}	
			
					if (bottom != null) {
			
						TerrainStitcherMini.StitchTerrains (bottom, terrain, TerrainStitcherMini.Side.Top, posXFinal, posZFinal, rangeX, rangeTerrainZ - rangeZ, rangeZ);			
					}

					if (right != null && bottom != null && bottomright != null) {

				
						TerrainStitcherMini.StitchTerrainsRepair (terrain, right, TerrainStitcherMini.Side.Right, posXFinal, posZFinal, rangeX, rangeZ);
						TerrainStitcherMini.StitchTerrainsRepair (bottom, terrain, TerrainStitcherMini.Side.Top, posXFinal, posZFinal, rangeX, rangeZ);

						TerrainStitcherMini.StitchTerrainsRepair (bottom, bottomright, TerrainStitcherMini.Side.Right, 0, bottomright.terrainData.heightmapWidth - rangeZ, rangeX, rangeZ);

						TerrainStitcherMini.StitchTerrainsRepair (bottomright, right, TerrainStitcherMini.Side.Top, 0, 0, rangeX, rangeZ);
				

						//TerrainStitcherMini.StitchTerrainsRepairCorner (terrain, right, bottom, bottomright);
					}
				
					if (left != null && bottomleft != null && bottom != null) {

						TerrainStitcherMini.StitchTerrainsRepair (left, terrain, TerrainStitcherMini.Side.Right, posXFinal, posZFinal, rangeX, rangeZ);
						TerrainStitcherMini.StitchTerrainsRepair (bottomleft, left, TerrainStitcherMini.Side.Top, left.terrainData.heightmapWidth - rangeX, posZFinal, rangeX, rangeZ);

						TerrainStitcherMini.StitchTerrainsRepair (bottomleft, bottom, TerrainStitcherMini.Side.Right, 0, bottom.terrainData.heightmapWidth - rangeZ, rangeX, rangeZ);
						TerrainStitcherMini.StitchTerrainsRepair (bottom, terrain, TerrainStitcherMini.Side.Top, 0, 0, rangeX, rangeZ);

						//TerrainStitcherMini.StitchTerrainsRepairCorner (left, terrain, bottomleft, bottom);
					}
				
					if (topleft != null && top != null && left != null) {

				
						TerrainStitcherMini.StitchTerrainsRepair (topleft, top, TerrainStitcherMini.Side.Right, 0, 0, rangeX, rangeZ);
						TerrainStitcherMini.StitchTerrainsRepair (left, topleft, TerrainStitcherMini.Side.Top, topleft.terrainData.heightmapWidth - rangeX, 0, rangeX, rangeZ);

						TerrainStitcherMini.StitchTerrainsRepair (left, terrain, TerrainStitcherMini.Side.Right, 0, terrain.terrainData.heightmapWidth - rangeZ, rangeX, rangeZ);
						TerrainStitcherMini.StitchTerrainsRepair (terrain, top, TerrainStitcherMini.Side.Top, 0, 0, rangeX, rangeZ);

						//TerrainStitcherMini.StitchTerrainsRepairCorner (topleft, top, left, terrain);
					}
				
					if (top != null && topright != null && right != null) {

						TerrainStitcherMini.StitchTerrainsRepair (top, topright, TerrainStitcherMini.Side.Right, 0, 0, rangeX, rangeZ);
						TerrainStitcherMini.StitchTerrainsRepair (terrain, top, TerrainStitcherMini.Side.Top, posXFinal, posZFinal, rangeX, rangeZ);

						TerrainStitcherMini.StitchTerrainsRepair (terrain, right, TerrainStitcherMini.Side.Right, 0, right.terrainData.heightmapWidth - rangeZ, rangeX, rangeZ);
						TerrainStitcherMini.StitchTerrainsRepair (right, topright, TerrainStitcherMini.Side.Top, 0, 0, rangeX, rangeZ);



						//TerrainStitcherMini.StitchTerrainsRepairCorner (top, topright, terrain, right);
					}

					TerrainStitcherMini.StitchTerrainsRepairCorner (terrain, right, bottom, bottomright);
					TerrainStitcherMini.StitchTerrainsRepairCorner (left, terrain, bottomleft, bottom);
					TerrainStitcherMini.StitchTerrainsRepairCorner (topleft, top, left, terrain);
					TerrainStitcherMini.StitchTerrainsRepairCorner (top, topright, terrain, right);

				} else {
					if (right != null) {
						TerrainStitcherMini.StitchTerrainsRepair (terrain, right, TerrainStitcherMini.Side.Right, posXFinal, posZFinal, rangeX, rangeZ);
					
					}
					if (left != null) {
						TerrainStitcherMini.StitchTerrainsRepair (left, terrain, TerrainStitcherMini.Side.Right, posXFinal, posZFinal, rangeTerrainX - rangeX, rangeZ);
					}
				
					if (top != null) {
						TerrainStitcherMini.StitchTerrainsRepair (terrain, top, TerrainStitcherMini.Side.Top, posXFinal, posZFinal, rangeX, rangeZ);			
					}	
				
					if (bottom != null) {
					
						TerrainStitcherMini.StitchTerrainsRepair (bottom, terrain, TerrainStitcherMini.Side.Top, posXFinal, posZFinal, rangeX, rangeTerrainZ - rangeZ);			
					}
				}		
			}
		}

		#endregion
	}
}