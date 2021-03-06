﻿using UnityEngine;
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
    public const string GetIconImageMsg = "/ccweb/api/userprofile/list";   //获取Icon
    public const string PAGE_TOGGLELIST = "Page_ToggleList";           //获取toggle列表
    public const string GetToggleInfoList = "/ccweb/api/categories/list";    //获取菜单列表
    public const string GetTagsInfoList = "/ccweb/api/tags/list";    //获取标签列表
    public const string GetTagsListByToggle = "/ccweb/api/tags/list?catid=";//按菜单获取标签
    public const string GetDevicesInfoList = "/ccweb/api/devices/list";    //获取已注册设备：
   
    public const string GetVideoListByMenu = "/ccweb/api/resources/list?page=1&numsperpage=50&catid=";    //按菜单获取视频
    public const string GetVideoListCountByMenuTag = "/ccweb/api/resources/count?catid={0}&tagid={1}";    //按菜单和标签获取视频总数
    public const string GetVideoListByMenuTag = "/ccweb/api/resources/list?page=1&numsperpage=50&catid={0}&tagid={1}";  //按页、菜单和标签获取视频资源
    public const string SetSameScreenMsg = "/ccweb/api/setquaternionsyncdevice";    //设置同屏设备 Param: sn=”deviceserialnumber”
    public const string GetSameScreenMsg = "/ccweb/api/getadminevent?devicesn={0}";    //获取当前设备播放信息
    public const string GetQuatSyncEvent = "/ccweb/api/getquatsyncevent?devicesn={0}";    //获取当前设备同屏信息
    public const string CancelSameScreenMsg = "/ccweb/api/cancelquaternionsync";    //取消同屏
    public const string GetSameScreenDeviceMsg = "/ccweb/api/getquaternionsyncdevice";    //获取当前同屏设备
    

    /// <summary>
    ///内部消息
    /// </summary>
    public const string UpdateAllDeviceInfo = "1000";
    public const string UpdateOnlineDeviceInfo = "1001";
    public const string SocketServerClosed = "1002";
}
