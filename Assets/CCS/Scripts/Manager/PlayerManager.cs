using System;
using System.Collections;
using System.Collections.Generic;
using UMP;
using UnityEngine;

public class PlayerManager : Manager
{
	private Transform playerRoot;
	private Transform playerCamera;
	private UniversalMediaPlayer universalMediaPlayer;
	private Transform imagePlayerRoot;
	private Transform videoPlayerRoot;
	
	//
	private Renderer QuadScreen;
	private Renderer HemisphereScreen;
	private Renderer SphereScreen;
	
	private void Awake()
	{
		playerRoot = PanManager.GlobalUIRoot.Find("3DVideoRoot");
		playerCamera  = playerRoot.Find("VideoCamera");
		videoPlayerRoot  = playerRoot.Find("360SphereVideo");
		universalMediaPlayer = videoPlayerRoot.Find("UniversalMediaPlayer").GetComponent<UniversalMediaPlayer>();
		imagePlayerRoot  = playerRoot.Find("imagePlayerRoot");

		QuadScreen = imagePlayerRoot.Find("BendQuadScreen").GetComponent<Renderer>();
		HemisphereScreen = imagePlayerRoot.Find("HemisphereScreen").GetComponent<Renderer>();
		SphereScreen = imagePlayerRoot.Find("OctahedronSphere").GetComponent<Renderer>();
	}

	#region VideoPlayer

	public void ShowVideoPlayerRoot()
	{
		videoPlayerRoot.gameObject.SetActive(true);
	}
	
	public void HideideoPlayerRoot()
	{
		videoPlayerRoot.gameObject.SetActive(false);
	}
	#endregion
	public UniversalMediaPlayer GetVideoPlayer()
	{
		return universalMediaPlayer;
	}
	
	public Transform GetPlayerCamera()
	{
		return playerCamera;
	}

	#region ImagePlayer
	public void ShowImagePlayerRoot()
	{
		imagePlayerRoot.gameObject.SetActive(true);
	}
	
	public void HideImagePlayerRoot()
	{
		imagePlayerRoot.gameObject.SetActive(false);
	}

	public void PlayImage(Texture tex)
	{
		QuadScreen.sharedMaterial.mainTexture = tex;
	}

	public void ResetImagePlayer()
	{
		if(QuadScreen.sharedMaterial.mainTexture !=null)
			DestroyImmediate(QuadScreen.sharedMaterial.mainTexture);
		if(HemisphereScreen.sharedMaterial.mainTexture !=null)
			DestroyImmediate(QuadScreen.sharedMaterial.mainTexture );
		if(SphereScreen.sharedMaterial.mainTexture !=null)
			DestroyImmediate(QuadScreen.sharedMaterial.mainTexture );
		
		QuadScreen.sharedMaterial.mainTexture = null;
		HemisphereScreen.sharedMaterial.mainTexture = null;
		SphereScreen.sharedMaterial.mainTexture = null;
	}
	
	
	#endregion
	
	public void ShowPlayerRoot()
	{
		playerRoot.gameObject.SetActive(true);
	}
	
	public void HidePlayerRoot()
	{
		playerRoot.gameObject.SetActive(false);
	}
}
