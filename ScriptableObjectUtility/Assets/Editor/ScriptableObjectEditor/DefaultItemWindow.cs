using UnityEngine;
using UnityEditor;
using Voodoo.Utilities;

public class DefaultItemWindow
{
	#region Declarations

	ScriptableObject _data;
	ScriptableEditorWindow _ownerWindow;
	Editor _editor;

	#endregion

	#region Constructors

	public DefaultItemWindow (ScriptableEditorWindow ownerWindow, ScriptableObject data)
	{
		this._data = data;
		this._ownerWindow = ownerWindow;
	}

	#endregion

	#region Properties

	public Rect WindowRect { 
		get; 
		set; 
	}

	public int Id { 
		get; 
		set;
	}

	public ScriptableObject Data {
		get { return _data; }
	}

	#endregion

	#region Methods

	public void DrawWindow (int id)
	{
		if (_editor == null)
			_editor = Editor.CreateEditor (_data);

		_editor.DrawDefaultInspector ();

		// We can also display the path if necessary, commented this out as if the path is large it makes the window width excessive
		//string path = AssetDatabase.GetAssetPath (_data);
		//GUILayout.Label(path);
		
		GUILayout.BeginHorizontal ();
		if (GUILayout.Button ("Remove")) {
			_ownerWindow.ViewableObjectWindows.RemoveAt (id);
		}
		
		if (GUILayout.Button ("Delete")) {
			
		}

		GUILayout.EndHorizontal ();
	}

	#endregion
}
