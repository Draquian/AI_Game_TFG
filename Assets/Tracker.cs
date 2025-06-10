using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TextMeshProTracker : MonoBehaviour
{
	public static TextMeshProTracker Instance;

	// Public list you can inspect in the Inspector
	public List<TextMeshPro> trackedTMPs = new List<TextMeshPro>();

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
			StartCoroutine(ScanForNewTMPs());
		}
		else
		{
			Destroy(gameObject);
		}
	}

	private IEnumerator ScanForNewTMPs()
	{
		Debug.LogError("SCANING");
		while (true)
		{
			var allTMPs = Resources.FindObjectsOfTypeAll<TextMeshPro>();

			foreach (var tmp in allTMPs)
			{
				// Ensure it belongs to a valid scene (not prefab or hidden)
				if (tmp.gameObject.scene.IsValid() && !trackedTMPs.Contains(tmp))
				{
					trackedTMPs.Add(tmp);
					Debug.Log($"New TMP detected: {tmp.name}");
				}
			}

			yield return new WaitForSeconds(1f);
		}
	}
}
