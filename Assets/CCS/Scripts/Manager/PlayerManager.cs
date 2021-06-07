using System;
using System.Collections;
using System.Collections.Generic;
using UMP;
using UnityEngine;

public enum FileType
{
	Video2D=1,
	Video3DLR,
	Video3DTB,
	Video1802D,
	Video1803DLR,
	Video1803DTB,
	Video3602D,
	Video3603DLR,
	Video3603DTB,
	Picture360=10,
	APP=20
}
public class PlayerManager : Manager
{
	private Transform playerRoot;
	private Transform playerCamera;
	private UniversalMediaPlayer universalMediaPlayer;
	private Transform playerGameObjectRoot;
	private Transform videoPlayerRoot;
	
	//
	private Renderer QuadScreen;
	private Renderer HemisphereScreen;
	private Renderer SphereScreen;
	public string currentPlayVideoCommond;
	public static bool isCanDragCamera;
	private void Awake()
	{
		playerRoot = PanManager.GlobalUIRoot.Find("3DVideoRoot");
		playerCamera  = playerRoot.Find("VideoCamera");
		videoPlayerRoot  = playerRoot.Find("360SphereVideo");
		universalMediaPlayer = videoPlayerRoot.Find("UniversalMediaPlayer").GetComponent<UniversalMediaPlayer>();
		playerGameObjectRoot  = playerRoot.Find("PlayerRoot");

		QuadScreen = playerGameObjectRoot.Find("BendQuadScreen").GetComponent<Renderer>();
		HemisphereScreen = playerGameObjectRoot.Find("HemisphereScreen").GetComponent<Renderer>();
		SphereScreen = playerGameObjectRoot.Find("OctahedronSphere").GetComponent<Renderer>();
		currentPlayVideoCommond = string.Empty;
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

	public void PlayImage(Texture tex)
	{
		SphereScreen.sharedMaterial.mainTexture = tex;
	}

	public void SetPlayerModel(FileType fileType)
	{
		switch (fileType)
		{
			case FileType.Picture360:
				SphereScreen.sharedMaterial.SetTextureScale("_MainTex",new Vector2(1,1));
				SphereScreen.gameObject.SetActive(true);
				QuadScreen.gameObject.SetActive(false);
				HemisphereScreen.gameObject.SetActive(false);
				isCanDragCamera = true;
				break;
			case FileType.Video1803DTB:
				HemisphereScreen.sharedMaterial.SetTextureScale("_MainTex",new Vector2(1,0.5f));
				isCanDragCamera = true;
				HemisphereScreen.gameObject.SetActive(true);
				SphereScreen.gameObject.SetActive(false);
				QuadScreen.gameObject.SetActive(false);
				universalMediaPlayer.RenderingObjects[0] = HemisphereScreen.gameObject;
				break;
			case FileType.Video1802D:
				HemisphereScreen.sharedMaterial.SetTextureScale("_MainTex",new Vector2(1,1));
				isCanDragCamera = true;
				HemisphereScreen.gameObject.SetActive(true);
				SphereScreen.gameObject.SetActive(false);
				QuadScreen.gameObject.SetActive(false);
				universalMediaPlayer.RenderingObjects[0] = HemisphereScreen.gameObject;
				break;
			case FileType.Video1803DLR:
				HemisphereScreen.sharedMaterial.SetTextureScale("_MainTex",new Vector2(0.5f,1));
				isCanDragCamera = true;
				HemisphereScreen.gameObject.SetActive(true);
				SphereScreen.gameObject.SetActive(false);
				QuadScreen.gameObject.SetActive(false);
				universalMediaPlayer.RenderingObjects[0] = HemisphereScreen.gameObject;
				break;
			case FileType.Video3602D:
				SphereScreen.sharedMaterial.SetTextureScale("_MainTex",new Vector2(1,1));
				isCanDragCamera = true;
				SphereScreen.gameObject.SetActive(true);
				QuadScreen.gameObject.SetActive(false);
				HemisphereScreen.gameObject.SetActive(false);
				universalMediaPlayer.RenderingObjects[0] = SphereScreen.gameObject;
				break;
			case FileType.Video3603DLR:
				SphereScreen.sharedMaterial.SetTextureScale("_MainTex",new Vector2(0.5f,1));
				isCanDragCamera = true;
				SphereScreen.gameObject.SetActive(true);
				QuadScreen.gameObject.SetActive(false);
				HemisphereScreen.gameObject.SetActive(false);
				universalMediaPlayer.RenderingObjects[0] = SphereScreen.gameObject;
				break;
			case FileType.Video3603DTB:
				SphereScreen.sharedMaterial.SetTextureScale("_MainTex",new Vector2(1,0.5f));
				isCanDragCamera = true;
				SphereScreen.gameObject.SetActive(true);
				QuadScreen.gameObject.SetActive(false);
				HemisphereScreen.gameObject.SetActive(false);
				universalMediaPlayer.RenderingObjects[0] = SphereScreen.gameObject;
				break;
			case FileType.Video2D:
				QuadScreen.sharedMaterial.SetTextureScale("_MainTex",new Vector2(1,1));
				isCanDragCamera = false;
				SphereScreen.gameObject.SetActive(false);
				QuadScreen.gameObject.SetActive(true);
				HemisphereScreen.gameObject.SetActive(false);
				universalMediaPlayer.RenderingObjects[0] = QuadScreen.gameObject;
				break;
			case FileType.Video3DLR:
				QuadScreen.sharedMaterial.SetTextureScale("_MainTex",new Vector2(0.5f,1));
				isCanDragCamera = false;
				SphereScreen.gameObject.SetActive(false);
				QuadScreen.gameObject.SetActive(true);
				HemisphereScreen.gameObject.SetActive(false);
				universalMediaPlayer.RenderingObjects[0] = QuadScreen.gameObject;
				break;
			case FileType.Video3DTB:
				QuadScreen.sharedMaterial.SetTextureScale("_MainTex",new Vector2(1,0.5f));
				isCanDragCamera = false;
				SphereScreen.gameObject.SetActive(false);
				QuadScreen.gameObject.SetActive(true);
				HemisphereScreen.gameObject.SetActive(false);
				universalMediaPlayer.RenderingObjects[0] = QuadScreen.gameObject;
				break;
			default:
				QuadScreen.sharedMaterial.SetTextureScale("_MainTex",new Vector2(1,1));
				isCanDragCamera = false;
				SphereScreen.gameObject.SetActive(false);
				QuadScreen.gameObject.SetActive(true);
				HemisphereScreen.gameObject.SetActive(false);
				universalMediaPlayer.RenderingObjects[0] = QuadScreen.gameObject;
				break;
		}
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
		SphereScreen.gameObject.SetActive(false);
		QuadScreen.gameObject.SetActive(false);
		HemisphereScreen.gameObject.SetActive(false);
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
