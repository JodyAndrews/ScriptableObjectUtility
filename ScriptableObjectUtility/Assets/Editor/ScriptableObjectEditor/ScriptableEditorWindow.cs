using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System;

namespace Voodoo.Utilities
{
	public class ScriptableEditorWindow : EditorWindow
	{
		#region Constants

		const float STARTY = 20f;
		const float STARTX = 20f;
		const float XPADDING = 10f;
		const float YPADDING = 10f;
		const float GRIDSIZE = 15f;
		const float DEFAULTWIDTH = 900f;
		const float DEFAULTSPLITX = 150f;

		#endregion

		#region Declarations

		float _splitViewX;
		ScriptableObject[] _scriptableObjects;
		List<DefaultItemWindow> _scriptableObjectWindows = new List<DefaultItemWindow> ();
		GUILayoutOption[] _options = {
			GUILayout.ExpandHeight (true),
			GUILayout.ExpandWidth (true),
			GUILayout.MinHeight (50f),
			GUILayout.MinWidth (256)
		};
		List<DefaultItemWindow> _viewableObjectWindows = new List<DefaultItemWindow> ();
		List<Type> _scriptableTypes = new List<Type> ();

		#endregion

		#region Properties

		public List<DefaultItemWindow> ScriptableObjectWindows {
			get { return _scriptableObjectWindows; }
			set { _scriptableObjectWindows = value; }
		}

		public List<DefaultItemWindow> ViewableObjectWindows {
			get { return _viewableObjectWindows; }
			set { _viewableObjectWindows = value; }
		}

		#endregion

		#region Methods

		[MenuItem ("Voodoo/ScriptableObject Viewer %s")]
		static void OpenInventory ()
		{
			
			ScriptableEditorWindow window = (ScriptableEditorWindow)EditorWindow.GetWindow<ScriptableEditorWindow> ("ScriptableObject Viewer", typeof(SceneView));
			window.Show (true);
		}

		/// <summary>
		/// Raises the enable event.
		/// </summary>
		void OnEnable ()
		{
			// Setup default window size
			Rect thisRect = this.position;
			thisRect.width = DEFAULTWIDTH;
			this.position = thisRect;
			_splitViewX = DEFAULTSPLITX;

			// Load all the current inventory items
			List<ScriptableObject> sObjs = new List<ScriptableObject> ();

			var pathGuids = AssetDatabase.FindAssets ("t:ScriptableObject");
			foreach (string pathGuid in pathGuids) {
				string assetpath = AssetDatabase.GUIDToAssetPath (pathGuid);
				ScriptableObject so = AssetDatabase.LoadAssetAtPath<ScriptableObject> (assetpath);
				sObjs.Add (so);
			}
			_scriptableObjects = sObjs.ToArray ();
		}

		/// <summary>
		/// Raises the GUI event.
		/// </summary>
		void OnGUI ()
		{
			GUILayout.BeginHorizontal ();

			// Draw the side panel
			DrawSidePanel ();

			DrawWindows ();
			
			GUILayout.EndHorizontal ();

		}

		void OnInspectorUpdate ()
		{
			Repaint (); // Causing(?) Exacerbating(?) a memory leak on OSX in 5.3+
		}

		/// <summary>
		/// Draws the windows.
		/// </summary>
		void DrawWindows ()
		{
			// Won't be doing anything if there's no windows
			if (_viewableObjectWindows.Count == 0)
				return;

			BeginWindows ();

			// Size of each window with padding is..
			float windowWidth = 100f;
			if (_viewableObjectWindows.Count > 0)
				windowWidth = _viewableObjectWindows [0].WindowRect.width + XPADDING;
			
			// Absolute number we can fit into this editor window width
			int remainder = 0;
			float windowsWidthCount = System.Math.DivRem ((int)(this.position.width - _splitViewX), (int)windowWidth, out remainder);
			
			int currentXCount = 0;
			float yPosition = STARTY;
			float maxPreviousRowHeight = 0;
			
			for (int i = 0; i < _viewableObjectWindows.Count (); i++) {
				_viewableObjectWindows [i].WindowRect = GUILayout.Window (i, _viewableObjectWindows [i].WindowRect, _viewableObjectWindows [i].DrawWindow, _viewableObjectWindows [i].Data.name, _options);
				
				// Current Window Rect
				Rect windowPositionRect = _viewableObjectWindows [i].WindowRect;
				windowPositionRect.x = _splitViewX + STARTX;
				windowPositionRect.x += (windowWidth * currentXCount);
				windowPositionRect.y = yPosition;
				_viewableObjectWindows [i].WindowRect = windowPositionRect;
				
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
		/// Draws the side panel.
		/// </summary>
		void DrawSidePanel ()
		{
			GUILayout.BeginVertical ();

			// If we haven't yet initialized..
			if (_scriptableObjectWindows.Count == 0)
				foreach (ScriptableObject sObj in _scriptableObjects) {
					Type t = sObj.GetType ();

					if (!_scriptableTypes.Contains (t)) {
						_scriptableTypes.Add (t);
					}
					
					_scriptableObjectWindows.Add (new DefaultItemWindow (this, sObj));
				}

			foreach (Type t in _scriptableTypes) {
				if (GUILayout.Button (t.Name, GUILayout.Width (_splitViewX))) {
					_viewableObjectWindows = _scriptableObjectWindows.Where (a => a.Data.GetType ().Name.ToLower () == t.Name.ToLower ()).ToList ();
				}
			}


			GUILayout.EndVertical ();
		}

		#endregion
	}
}