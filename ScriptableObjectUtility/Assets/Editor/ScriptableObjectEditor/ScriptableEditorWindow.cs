using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;

namespace Voodoo.Utilities
{
	public class ScriptableEditorWindow : EditorWindow
	{
		#region Constants

		const float STARTY = 20f;
		const float STARTX = 35f;
		const float XPADDING = 10f;
		const float YPADDING = 10f;
		const float DEFAULTWIDTH = 900f;
		const float SIDEWINDOWWIDTH = 150f;

		#endregion

		#region Fields

		/// <summary>
		/// Cache any scriptable object types found in the main assembly
		/// </summary>
		List<Type> _scriptableTypes = new List<Type>();

		/// <summary>
		/// All of the drawable windows
		/// </summary>
		List<DefaultItemWindow> _scriptableObjectWindows = new List<DefaultItemWindow> ();

		/// <summary>
		/// Some basic options
		/// </summary>
		GUILayoutOption[] _options = {
			GUILayout.ExpandHeight (true),
			GUILayout.ExpandWidth (true),
			GUILayout.MinHeight (50f),
			GUILayout.MinWidth (256)
		};

		/// <summary>
		/// Side menu scroll view position
		/// </summary>
		Vector2 menuScrollPosition = Vector2.zero;

		/// <summary>
		/// Main scroll view position
		/// </summary>
		Vector2 mainScrollPosition = Vector2.zero;

		/// <summary>
		/// The current selected type if any
		/// </summary>
		Type _selectedType = null;

		#endregion

		#region Properties

		/// <summary>
		/// Accessor for individual windows
		/// </summary>
		/// <value>The scriptable object windows.</value>
		public List<DefaultItemWindow> ScriptableObjectWindows 
		{
			get { return _scriptableObjectWindows; }
			set { _scriptableObjectWindows = value; }
		}

		#endregion

		#region Methods

		/// <summary>
		/// Opens the inventory.
		/// </summary>
		[MenuItem ("Voodoo/Scriptable Objects %s")]
		static void OpenWindow ()
		{
			ScriptableEditorWindow window = (ScriptableEditorWindow)EditorWindow.GetWindow<ScriptableEditorWindow> ("Scriptable Objects", typeof(SceneView));
			window.Show (true);
		}

		/// <summary>
		/// Called on enable
		/// </summary>
		void OnEnable ()
		{
			// Setup default window size
			Rect thisRect = this.position;
			thisRect.width = DEFAULTWIDTH;
			this.position = thisRect;

			_scriptableTypes = GetScriptableObjectTypes ();
		}

		/// <summary>
		/// Draw our UI
		/// </summary>
		void OnGUI ()
		{
			GUILayout.BeginHorizontal ();

			DrawSidePanel ();

			DrawMainWindow ();

			HandleMousePointerClick ();
			
			GUILayout.EndHorizontal ();

			Repaint ();
		}

		/// <summary>
		/// Handles all mouse pointer clicking
		/// </summary>
		void HandleMousePointerClick()
		{
			var e = Event.current;
			
			HandleMousePointerRightClick(e);
		}

		/// <summary>
		/// Handle the mouse pointer right click.
		/// </summary>
		/// <param name="e">E.</param>
		void HandleMousePointerRightClick(Event e)
		{
			if (e.button == 1 && e.type == EventType.MouseUp && _selectedType != null)
			{
				var menu = new GenericMenu();
				menu.AddItem (new GUIContent("Add New " + _selectedType.Name), false, delegate { 

					string folderPath = "";
					// Get the path of an existing scriptable object for this type, if applicable
					// Find a window of the same type
					DefaultItemWindow window = _scriptableObjectWindows.Where(a => a.Data.GetType() == _selectedType).FirstOrDefault();

					if (window != null)
					{
						string fullPath = AssetDatabase.GetAssetPath (window.Data);
						folderPath = System.IO.Path.GetDirectoryName( fullPath );
					}

					if (folderPath == string.Empty)
						folderPath = "Assets/";

					string path = EditorUtility.SaveFilePanel ("Create Scriptable Object", folderPath, String.Format("{0}.asset", _selectedType.Name), "asset");
					
					if (path == "")
						return;
					
					path = FileUtil.GetProjectRelativePath (path);
					
					var asset = CreateInstance (_selectedType.ToString());
					AssetDatabase.CreateAsset (asset, path);
					AssetDatabase.SaveAssets ();
					ReloadWindows();
				});
				
				menu.ShowAsContext();
				e.Use();
			}
		}

		/// <summary>
		/// Draws the main window with (if selected) the editor for each viewable scriptable object
		/// </summary>
		void DrawMainWindow() 
		{
			// Begin drawing
			BeginWindows ();

			// Size of each window with padding is..
			float windowWidth = 100f;

			// No need to do anything here if there's nothing to draw, this must be checked within BeginWindows
			// and not before
			if (_scriptableObjectWindows.Count () == 0)
				return;

			windowWidth = _scriptableObjectWindows [0].WindowRect.width + XPADDING;

			// Absolute number we can fit into this editor window width
			int remainder = 0;
			float windowsWidthCount = System.Math.DivRem ((int)(this.position.width - SIDEWINDOWWIDTH - STARTX), (int)windowWidth, out remainder);
			
			int currentXCount = 0;
			float yPosition = STARTY;
			float maxPreviousRowHeight = 0;
			
			for (int i = 0; i < _scriptableObjectWindows.Count (); i++) {

				DefaultItemWindow scriptableObjectWindow = _scriptableObjectWindows[i];

				if (scriptableObjectWindow == null || scriptableObjectWindow.Data == null)
					continue;
				
				scriptableObjectWindow.WindowRect = GUILayout.Window (i, scriptableObjectWindow.WindowRect, scriptableObjectWindow.DrawWindow, scriptableObjectWindow.Data.name, _options);
				
				// Current Window Rect
				Rect windowPositionRect = scriptableObjectWindow.WindowRect;
				windowPositionRect.x = SIDEWINDOWWIDTH + STARTX;
				windowPositionRect.x += (windowWidth * currentXCount);
				windowPositionRect.y = yPosition;
				scriptableObjectWindow.WindowRect = windowPositionRect;
				
				if (windowPositionRect.height > maxPreviousRowHeight)
					maxPreviousRowHeight = windowPositionRect.height;
				
				// If we've exceeded the max column count then increment the row count and reset the column's
				currentXCount++;
				if (currentXCount >= (int)windowsWidthCount) {
					currentXCount = 0;
					yPosition += maxPreviousRowHeight + YPADDING;
					maxPreviousRowHeight = 0f;
				}
			}

			EndWindows ();
		}
	
		/// <summary>
		/// Gets the windows for scriptable objects.
		/// </summary>
		/// <returns>The windows for objects.</returns>
		/// <param name="scriptableObjects">Scriptable objects.</param>
		List<DefaultItemWindow> GetWindowsForObjects(List<ScriptableObject> scriptableObjects)
		{
			List<DefaultItemWindow> windows = new List<DefaultItemWindow> ();

			foreach (ScriptableObject sObj in scriptableObjects) {
				windows.Add (new DefaultItemWindow (this, sObj));
			}

			return windows;
		}

		/// <summary>
		/// Finds and returns all scriptableobjects for a given type
		/// </summary>
		/// <returns>The scriptable objects.</returns>
		/// <param name="type">Type.</param>
		List<ScriptableObject> GetScriptableObjects(Type type) 
		{
			List<ScriptableObject> sObjs = new List<ScriptableObject> ();
			string[] pathGuids = AssetDatabase.FindAssets ("t:ScriptableObject");
			foreach (string pathGuid in pathGuids) {

				// Get the path
				string assetpath = AssetDatabase.GUIDToAssetPath (pathGuid);

				// Load the asset at that path and store
				ScriptableObject so = AssetDatabase.LoadAssetAtPath<ScriptableObject> (assetpath);
				sObjs.Add (so);
			}

			List<ScriptableObject> returnObjects = sObjs.Where (a => a.GetType ().Name == type.Name).ToList ();

			// Return only those that match this type
			return returnObjects;
		}

		/// <summary>
		/// Reloads (caches) the windows for each scriptableobject
		/// </summary>
		void ReloadWindows() 
		{
			_scriptableObjectWindows = GetWindowsForObjects(GetScriptableObjects(_selectedType));
		}

		/// <summary>
		/// Draws the GUI side panel with the list of scriptable types
		/// </summary>
		void DrawSidePanel() 
		{
			menuScrollPosition = EditorGUILayout.BeginScrollView(menuScrollPosition, GUILayout.Width (SIDEWINDOWWIDTH + 25f), GUILayout.Height(position.height));

			foreach (Type type in _scriptableTypes) {
				if (GUILayout.Button (type.Name, GUILayout.Width (SIDEWINDOWWIDTH))) {
					_selectedType = type;
					ReloadWindows();
				}
			}
			EditorGUILayout.EndScrollView ();
		}

		/// <summary>
		/// Gets the scriptableobject types in this project. This should be polled for or triggered when focus is changed
		/// and cached for the majority of the time
		/// </summary>
		/// <returns>The node window types.</returns>
		List<Type> GetScriptableObjectTypes()
		{
			List<ScriptableObject> menuItems = new List<ScriptableObject>();

			// TODO : Just load the assembly instead??
			Assembly mainAssembly = AppDomain.CurrentDomain.GetAssemblies ().Where (a => a.FullName == "Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null").FirstOrDefault ();

			List<Type> ts = mainAssembly.GetTypes ().Where (a => typeof(ScriptableObject).IsAssignableFrom (a)).ToList ();
			return ts;
		}
	
		#endregion
	}
}