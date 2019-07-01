using UnityEngine;
using System.Collections;

public class NetMessageConst
{
    /// <summary>
    /// WebSocket消息通知
    /// </summary>
    public const string AdminEvent = "1";                       
    public const string AdminResourceManageEvent = "2";    
    public const string StatusEvent = "3";
    public const string UserResourceManageEvent = "4";
    public const string DeviceInfo = "5";
    public const string SameCamera = "6";

    /// <summary>
    /// Http消息通知
    /// </summary>
    public const string PAGE_TOGGLELIST = "Page_ToggleList";           //获取toggle列表
    public const string GetToggleInfoList = "ccweb/api/categories/list";    //获取菜单列表
    public const string GetTagsInfoList = "ccweb/api/tags/list";    //获取标签列表
    public const string GetTagsListByToggle = "ccweb/api/tags/list?catid=";//按菜单获取标签
    public const string GetDevicesInfoList = "ccweb/api/devices/list";    //获取已注册设备：
   
    public const string GetVideoListByMenu = "ccweb/api/resources/list?page=1&numsperpage=50&catid=";    //按菜单获取视频
    public const string GetVideoListCountByMenuTag = "ccweb/api/resources/count?catid={0}&tagid={1}";    //按菜单和标签获取视频总数
    public const string GetVideoListByMenuTag = "ccweb/api/resources/list?page=1&numsperpage=50&catid={0}&tagid={1}";  //按页、菜单和标签获取视频资源
    public const string GetSameScreenMsg = "ccweb/api/setquaternionsyncdevice";    //获取同屏设备 Param: sn=”deviceserialnumber”

    /// <summary>
    ///内部消息
    /// </summary>
    public const string UpdateAllDeviceInfo = "1000";
    public const string UpdateOnlineDeviceInfo = "1001";

}
