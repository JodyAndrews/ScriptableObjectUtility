using UnityEngine;
using System.Collections;
using UnityEditor;

public class PokeableObject : Editor
{
		
	[MenuItem ("Voodoo/Poke Scriptable")]
	static void Poke ()
	{
		object obj = Selection.activeObject;
		MonoScript script = (MonoScript)obj;
	
		System.Type monoClass = script.GetClass ();
	
		if (script != null) {
			if (typeof(ScriptableObject).IsAssignableFrom (monoClass)) {
				Debug.Log ("Serializing Object");
				string path = EditorUtility.SaveFilePanel ("Create Scriptable Object", "Assets/", "default.asset", "asset");
			
				if (path == "")
					return;
			
				path = FileUtil.GetProjectRelativePath (path);
			
				var asset = CreateInstance (monoClass.ToString ());
				AssetDatabase.CreateAsset (asset, path);
				AssetDatabase.SaveAssets ();
			
			} else {
				Debug.Log ("Is not a scriptable object");
			}
		} else {
			Debug.Log ("Ensure you are selecting a scriptable object");
		}
	}
}