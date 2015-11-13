using UnityEngine;
using System.Collections;

public class PlayerDefault : ScriptableObject {

	[SerializeField]
	string Name = "defaultname";

	[SerializeField]
	float RunSpeed = 2f;

	[SerializeField]
	float IdleSpeed = 0f;
}
