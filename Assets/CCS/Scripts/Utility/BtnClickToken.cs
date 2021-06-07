using UnityEngine;

public class BtnClickToken{

	// Use this for initialization

	private static float lastTime=0;
	
	/// <summary>
	/// 返回false ,不可点击
	/// </summary>
	/// <param name="duration"></param>
	/// <returns></returns>
	public static bool  TakeToken(float duration=0.5f)
	{
		if (lastTime > Time.time)
		{
			return false;
		}
		else
		{
			lastTime = Time.time + (duration);
			return true;
		}
	}

	public static void LockToken(float duration=0.5f)
	{
		lastTime = Time.time + duration;
	}

	public static void ReleaseToken()
	{
		lastTime = 0;
	}
}
