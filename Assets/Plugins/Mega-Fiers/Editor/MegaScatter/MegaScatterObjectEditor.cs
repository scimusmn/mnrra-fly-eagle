
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MegaScatterObject))]
public class MegaScatterObjectEditor : Editor
{
	private     MegaScatterObject	src;
	private     MegaUndo			undoManager;

	[MenuItem("GameObject/Create Other/MegaScatter/Scatter Object")]
	static void CreateScatteObject()
	{
		Vector3 pos = Vector3.zero;

		if ( UnityEditor.SceneView.lastActiveSceneView != null )
			pos = UnityEditor.SceneView.lastActiveSceneView.pivot;

		GameObject go = new GameObject("Scatter Object");

		go.AddComponent<MegaScatterObject>();
		go.transform.position = pos;
		Selection.activeObject = go;
	}

	private void OnEnable()
	{
		src = target as MegaScatterObject;

		undoManager = new MegaUndo(src, "Mega Scatter Object Param");
	}

	public override void OnInspectorGUI()
	{
		undoManager.CheckUndo();

#if !UNITY_5
		EditorGUIUtility.LookLikeControls();
#endif

		DisplayGUI();

		if ( GUI.changed )
			EditorUtility.SetDirty(target);

		undoManager.CheckDirty();
	}

	public void DisplayGUI()
	{
		MegaScatterObject mod = (MegaScatterObject)target;

		EditorGUILayout.LabelField("Objs: " + mod.objcount + " scattered: " + mod.scattercount);

		MegaShape shape = (MegaShape)EditorGUILayout.ObjectField("Shape", mod.shape, typeof(MegaShape), true);
		if ( shape != mod.shape )
		{
			mod.SetShape(shape);
		}
		mod.seed = EditorGUILayout.IntField("Seed", mod.seed);
		mod.countmode = (MegaScatterMode)EditorGUILayout.EnumPopup("Count Mode", mod.countmode);
		mod.Density = EditorGUILayout.FloatField("Density", mod.Density);
		mod.forcecount = EditorGUILayout.IntField("Count", mod.forcecount);
		mod.globalScale = EditorGUILayout.Vector3Field("Global Scale", mod.globalScale);
		mod.fillHoles = EditorGUILayout.Toggle("Fill Holes", mod.fillHoles);
		mod.StartCurve = EditorGUILayout.IntSlider("Start Curve", mod.StartCurve, 0, mod.NumCurves());
		mod.EndCurve = EditorGUILayout.IntSlider("End Curve", mod.EndCurve, 0, mod.NumCurves());

		mod.queryObject = (MegaScatterQuery)EditorGUILayout.ObjectField("Query Object", mod.queryObject, typeof(MegaScatterQuery), true);
		mod.dostaticbatching = EditorGUILayout.Toggle("Static Batching", mod.dostaticbatching);

		mod.raycast = EditorGUILayout.BeginToggleGroup("Raycast", mod.raycast);
		mod.NeedsGround = EditorGUILayout.Toggle("Needs Ground", mod.NeedsGround);
		mod.collisionOffset = EditorGUILayout.FloatField("Collision Offset", mod.collisionOffset);
		mod.showignoreobjs = EditorGUILayout.Foldout(mod.showignoreobjs, "Show Ignore Objects");

		if ( mod.showignoreobjs )
		{
			if ( GUILayout.Button("Add Ignore Obj") )
				mod.ignoreobjs.Add(new MegaScatterCollisionObj());

			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("Ignore Objects");
			for ( int i = 0; i < mod.ignoreobjs.Count; i++ )
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label("" + i + ":", GUILayout.MaxWidth(20));
				GUILayout.Label("On", GUILayout.MaxWidth(20));
				mod.ignoreobjs[i].active = EditorGUILayout.Toggle("", mod.ignoreobjs[i].active, GUILayout.MaxWidth(20));
				GUILayout.Label("Children", GUILayout.MaxWidth(52));
				mod.ignoreobjs[i].includechildren = EditorGUILayout.Toggle("", mod.ignoreobjs[i].includechildren, GUILayout.MaxWidth(20));
				mod.ignoreobjs[i].collider = (Collider)EditorGUILayout.ObjectField("", mod.ignoreobjs[i].collider, typeof(Collider), true, GUILayout.MaxWidth(180));

				//mod.ignoreobjs[i] = (Collider)EditorGUILayout.ObjectField("" + i + ":", mod.ignoreobjs[i], typeof(Collider), true);
				if ( GUILayout.Button("Delete", GUILayout.MaxWidth(50)) )
					mod.ignoreobjs.RemoveAt(i);

				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.EndVertical();
		}

		if ( mod.raycast )
		{
			if ( mod.surfaces.Count == 0 )
				EditorGUILayout.HelpBox("No Surface Objects Defined! No Objects will be scattered", MessageType.Info, true);
		}

		mod.showsurfaceobjs = EditorGUILayout.Foldout(mod.showsurfaceobjs, "Show Surface Objects");
		if ( mod.showsurfaceobjs )
		{
			if ( GUILayout.Button("Add Surface Obj") )
				mod.surfaces.Add(new MegaScatterCollisionObj());

			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("Surface Objects");
			for ( int i = 0; i < mod.surfaces.Count; i++ )
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label("" + i + ":");
				mod.surfaces[i].active = EditorGUILayout.Toggle("", mod.surfaces[i].active, GUILayout.MaxWidth(20));
				mod.surfaces[i].collider = (Collider)EditorGUILayout.ObjectField("", mod.surfaces[i].collider, typeof(Collider), true);

				mod.surfaces[i].prebuildenable = EditorGUILayout.Toggle("", mod.surfaces[i].prebuildenable, GUILayout.MaxWidth(20));
				mod.surfaces[i].postbuilddisable = EditorGUILayout.Toggle("", mod.surfaces[i].postbuilddisable, GUILayout.MaxWidth(20));

				//mod.ignoreobjs[i] = (Collider)EditorGUILayout.ObjectField("" + i + ":", mod.ignoreobjs[i], typeof(Collider), true);
				if ( GUILayout.Button("Delete", GUILayout.MaxWidth(50)) )
					mod.surfaces.RemoveAt(i);

				EditorGUILayout.EndHorizontal();
			}

			EditorGUILayout.EndVertical();
		}

		EditorGUILayout.EndToggleGroup();

		mod.hideObjects = EditorGUILayout.Toggle("Hide Objects", mod.hideObjects);
		mod.buildOnStart = EditorGUILayout.Toggle("Build on Start", mod.buildOnStart);
		mod.useCoRoutine = EditorGUILayout.Toggle("Scatter Over Frames", mod.useCoRoutine);
		mod.NumPerFrame = EditorGUILayout.IntField("Number Per Frame", mod.NumPerFrame);
		mod.displaygizmo = EditorGUILayout.Toggle("Display Gizmo", mod.displaygizmo);

		EditorGUILayout.BeginHorizontal();
		if ( GUILayout.Button("Hide/Show Objects") )
			mod.HideShow();

		if ( GUILayout.Button("Remove Objects") )
			mod.RemoveObjects();

		if ( GUILayout.Button("Add Mesh") )
		{
			MegaScatterLayer inf = new MegaScatterLayer();
			mod.layers.Add(inf);
			mod.currentEdit = mod.layers.Count - 1;
		}
		EditorGUILayout.EndHorizontal();

		mod.showadvanced = EditorGUILayout.Foldout(mod.showadvanced, "Advanced");
		if ( mod.showadvanced )
		{
			mod.FailCount = EditorGUILayout.IntField("Fail Count", mod.FailCount);
			mod.PosFailCount = EditorGUILayout.IntField("Pos Fail Count", mod.PosFailCount);
		}

		EditorGUILayout.BeginVertical("box");
		EditorGUILayout.LabelField("Layers");
		for ( int i = 0; i < mod.layers.Count; i++ )
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("" + i + " - " + mod.layers[i].LayerName);
			mod.layers[i].Enabled = EditorGUILayout.Toggle("", mod.layers[i].Enabled, GUILayout.MaxWidth(20));

			if ( GUILayout.Button("Edit", GUILayout.MaxWidth(50)) )
			{
				mod.currentEdit = i;
				EditorUtility.SetDirty(mod);
			}

			if ( GUILayout.Button("Delete", GUILayout.MaxWidth(50)) )
				mod.layers.RemoveAt(i);

			if ( GUILayout.Button("U", GUILayout.MaxWidth(20)) )
			{
				if ( i > 0 )
					MegaScatterMeshEditor.SwapInf(mod, i, i - 1);
			}

			if ( GUILayout.Button("D", GUILayout.MaxWidth(22)) )
			{
				if ( i < mod.layers.Count - 1 )
					MegaScatterMeshEditor.SwapInf(mod, i, i + 1);
			}

			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndVertical();

		mod.showusesplines = EditorGUILayout.Foldout(mod.showusesplines, "Show Splines");

		if ( mod.showusesplines )
		{
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("Spline Selection");
			for ( int i = 0; i < mod.usespline.Count; i++ )
			{
				mod.usespline[i] = EditorGUILayout.Toggle("Spline " + i, mod.usespline[i]);
			}
			EditorGUILayout.EndVertical();
		}

		GUI.backgroundColor = Color.green;
		if ( GUILayout.Button("Update", GUILayout.Height(30)) )
		{
			mod.update = true;
			EditorUtility.SetDirty(target);
		}
		GUI.backgroundColor = Color.white;

		if ( mod.currentEdit >= mod.layers.Count )
		{
			mod.currentEdit = mod.layers.Count - 1;
			EditorUtility.SetDirty(mod);
		}

		int ci = mod.currentEdit;

		if ( ci >= 0 )
		{
			MegaScatterLayer inf = mod.layers[ci];

			EditorGUILayout.BeginVertical("box");

			EditorGUILayout.BeginHorizontal();
			if ( GUILayout.Button("Copy", GUILayout.Width(50)) )
				MegaScatterLayer.copylayer = inf;

			if ( MegaScatterLayer.copylayer != null )
			{
				if ( GUILayout.Button("Paste from " + MegaScatterLayer.copylayer.LayerName) )
					inf.Copy(MegaScatterLayer.copylayer);
			}
			EditorGUILayout.EndHorizontal();

			inf.LayerName = EditorGUILayout.TextField("Name", inf.LayerName);
			inf.Enabled = EditorGUILayout.BeginToggleGroup("Enabled", inf.Enabled);
			inf.markstatic = EditorGUILayout.Toggle("Mark Static", inf.markstatic);
			inf.obj = (GameObject)EditorGUILayout.ObjectField("Object", inf.obj, typeof(GameObject), true);
			inf.perCurveCount = EditorGUILayout.Toggle("Counts Per Curve", inf.perCurveCount);
			inf.forcecount = EditorGUILayout.IntField("Force Count", inf.forcecount);
			inf.maxcount = EditorGUILayout.IntField("Max Count", inf.maxcount);
			inf.weight = EditorGUILayout.FloatField("Weight", inf.weight);
			inf.scale = EditorGUILayout.FloatField("Scale", inf.scale);
			inf.offsetLow = EditorGUILayout.Vector3Field("Offset Low", inf.offsetLow);
			inf.offsetHigh = EditorGUILayout.Vector3Field("Offset High", inf.offsetHigh);
			inf.prerot = EditorGUILayout.Vector3Field("Pre Rot", inf.prerot);
			inf.rotLow = EditorGUILayout.Vector3Field("Rot Low", inf.rotLow);
			inf.rotHigh = EditorGUILayout.Vector3Field("Rot High", inf.rotHigh);
			inf.uniformScaling = EditorGUILayout.Toggle("Uniform Scaling", inf.uniformScaling);

			if ( inf.uniformScaling )
			{
				inf.uniscalemode = (MegaScatterScaleMode)EditorGUILayout.EnumPopup("Mode", inf.uniscalemode);
				inf.uniscaleLow = EditorGUILayout.FloatField("Scale Low", inf.uniscaleLow);
				inf.uniscaleHigh = EditorGUILayout.FloatField("Scale High", inf.uniscaleHigh);
			}
			else
			{
				inf.scaleLow = EditorGUILayout.Vector3Field("Scale Low", inf.scaleLow);
				inf.scaleHigh = EditorGUILayout.Vector3Field("Scale High", inf.scaleHigh);
			}

			inf.snap = EditorGUILayout.Vector3Field("Snap", inf.snap);
			inf.snapRot = EditorGUILayout.Vector3Field("Snap Rot", inf.snapRot);
			inf.distCrv = EditorGUILayout.CurveField("Dist Curve", inf.distCrv);
			inf.seed = EditorGUILayout.IntField("Seed", inf.seed);
			inf.noOverlap = EditorGUILayout.Toggle("No Overlap", inf.noOverlap);
			inf.clearOverlap = EditorGUILayout.Toggle("Clear Overlap", inf.clearOverlap);
			inf.radius = EditorGUILayout.FloatField("Radius", inf.radius);
			inf.colradiusadj = EditorGUILayout.FloatField("Col Radius Adj", inf.colradiusadj);
			inf.raycount = EditorGUILayout.IntSlider("Ray Count", inf.raycount, 1, 8);
			inf.align = EditorGUILayout.Slider("Align", inf.align, 0.0f, 1.0f);
			inf.minslope = EditorGUILayout.Slider("Min Slope", inf.minslope, 0.0f, 90.0f);
			inf.maxslope = EditorGUILayout.Slider("Max Slope", inf.maxslope, 0.0f, 90.0f);
			inf.collisionOffset = EditorGUILayout.FloatField("Collision Offset", inf.collisionOffset);

			inf.useheight = EditorGUILayout.BeginToggleGroup("Use Height Limits", inf.useheight);
			inf.minheight = EditorGUILayout.FloatField("Min Height", inf.minheight);
			inf.maxheight = EditorGUILayout.FloatField("Max Height", inf.maxheight);
			EditorGUILayout.EndToggleGroup();

			inf.showcolvari = EditorGUILayout.Foldout(inf.showcolvari, "Color Variations");

			if ( inf.showcolvari )
			{
				EditorGUILayout.BeginVertical("box");
				if ( GUILayout.Button("Add Color") )
				{
					inf.colvariations.Add(Color.white);
				}

				for ( int cv = 0; cv < inf.colvariations.Count; cv++ )
				{
					EditorGUILayout.BeginHorizontal();
					inf.colvariations[cv] = EditorGUILayout.ColorField("" + cv + ":", inf.colvariations[cv]);

					if ( GUILayout.Button("Delete", GUILayout.MaxWidth(50)) )
						inf.colvariations.RemoveAt(cv);

					EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.EndVertical();
			}

			inf.disableCollider = EditorGUILayout.Toggle("Disable Collider", inf.disableCollider);

			EditorGUILayout.EndToggleGroup();

			if ( GUILayout.Button("Delete") )
			{
				mod.layers.RemoveAt(ci);
				mod.currentEdit = 0;
				EditorUtility.SetDirty(mod);
			}
			EditorGUILayout.EndVertical();

			GUI.backgroundColor = Color.green;
			if ( GUILayout.Button("Update", GUILayout.Height(30)) )
			{
				mod.update = true;
				EditorUtility.SetDirty(target);
			}
			GUI.backgroundColor = Color.white;
		}
	}

	void OnSceneGUI()
	{
		MegaScatterObject mod = (MegaScatterObject)target;
		
		if ( mod.shape )
			MegaScatterMeshEditor.DisplayGizmo(mod, mod.shape.transform.localToWorldMatrix);

	}
}
