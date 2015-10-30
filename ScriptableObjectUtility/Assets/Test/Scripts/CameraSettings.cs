using UnityEngine;
using System.Collections;

public class CameraSettings : ScriptableObject {

	[SerializeField]
	[Range(0.95f, 32f)]
	float Aperture = 2.8f;

	[SerializeField]
	[Range(12f, 500f)]
	float FocalLengthMM = 50;
}
