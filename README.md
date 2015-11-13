# ScriptableObjectUtility
Basic ScriptableObject utility UI for Unity3d

Allows viewing and editing of all scriptableobjects in a window rather than relying on the project pane

Note : Currently WIP. This will be a passively updated repository that I've used for my own projects, with plenty of missing features I haven't got around to adding yet.

Usage :

1. Clone and open as a project to see how it works with the test scriptable objects, or just copy the Editor folder to own project.

2. Open the editor window via Voodoo -> Scriptable Objects.

The left pane contains all found scriptable object types

After selecting a type, the main window shows all scriptable objects found

You can edit, remove from view, or delete ( permanently ) from there.

Right click the main window to create a new serialized object

NOTE: Due to the way Unity manages scriptable objects, if you rename the underlying scriptableobject class Unity will link any objects to it somehow. This utility does not handle this yet.
