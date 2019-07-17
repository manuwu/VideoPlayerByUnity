using CCS;
using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UMP;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class VideoPlayPanel : PanelBase
{
    //Trans
    private Button backBtn;
    private Button playBtn;
    private Button pauseBtn;
    private Toggle restartBtn;
    private Slider videoSeekSlider;
    private Text currentTimeTxt;
    private Text totalTimeTxt;
    private UniversalMediaPlayer mediaPlayer;
    private Transform videoCameraTran;
    private Transform videoPLayerRoot;
    private bool IsVoluntary;
    private bool isLoop;
    //Data
    private Quaternion cameraRow;
    public override void Init(params object[] args)
    {
        base.Init(args);
        videoPLayerRoot =PanManager.GlobalUIRoot.Find("3DVideoRoot").transform;
        videoPLayerRoot.gameObject.SetActive(true);
        mediaPlayer = videoPLayerRoot.Find("360SphereVideo/UniversalMediaPlayer").GetComponent<UniversalMediaPlayer>();
        videoCameraTran= videoPLayerRoot.Find("VideoCamera");
        mediaPlayer.Path = args[0].ToString();
        mediaPlayer.Play();
    }

    public override void OnShowing()
    {
        base.OnShowing();
        if (isInit)
            return;
        isInit = true;
        base.OnShowing();
        backBtn = skin.transform.Find("BtnRoot/backBtn").GetComponent<Button>();
        playBtn = skin.transform.Find("BtnRoot/PlayButton").GetComponent<Button>();
        pauseBtn = skin.transform.Find("BtnRoot/PauseButton").GetComponent<Button>();
        restartBtn = skin.transform.Find("BtnRoot/resartBtn").GetComponent<Toggle>();
        currentTimeTxt = skin.transform.Find("BtnRoot/currentTime").GetComponent<Text>();
        totalTimeTxt = skin.transform.Find("BtnRoot/totalTime").GetComponent<Text>();
        videoSeekSlider = skin.transform.Find("BtnRoot/VideoSeekSlider").GetComponent<Slider>();
        IsVoluntary = false;
        isLoop = false;
        AddUIEvent();
        InvokeRepeating("UpdateProcess",1f,1f);
    }

    void UpdateProcess()
    {
        IsVoluntary = true;
        totalTimeTxt.text = Util.MillisecondToData(mediaPlayer.Length);
        currentTimeTxt.text = Util.MillisecondToData(mediaPlayer.Time);
        videoSeekSlider.value = mediaPlayer.Position;
    }

    void AddUIEvent()
    {
        backBtn.onClick.AddListener(OnClickBack);
        playBtn.onClick.AddListener(OnPlayButton);
        pauseBtn.onClick.AddListener(OnPauseButton);
        restartBtn.onValueChanged.AddListener(OnRestartButton);
        videoSeekSlider.onValueChanged.AddListener(OnVideoSeekSliderOnVideoSeekSlider);
        mediaPlayer.AddEndReachedEvent(OnPLlayerEnd);
    }

    public override void AddEvent()
    {
        base.AddEvent();
        NetMsgHandler.AddListener(NetMessageConst.SameCamera, UpdateCameraRotation);
    }

    public override void RemoveEvent()
    {
        base.RemoveEvent();
        NetMsgHandler.RemoveListener(NetMessageConst.SameCamera, UpdateCameraRotation);
    }

    void OnClickBack()
    {
        AdminMessage msg = new AdminMessage();
        msg.Type = DataType.AdminEvent;
        msg.Data.Control = ControlState.Stop;
        NetManager.SendMessage(Util.ObjectToJson(msg));
        mediaPlayer.Stop();
        videoPLayerRoot.gameObject.SetActive(false);
        PanManager.AllOpenWithout(PanelName.VideoPlayPanel);
        PanManager.ClosePanel(PanelName.VideoPlayPanel);
    }

    // 播放
    private void OnPlayButton()
    {
        if (mediaPlayer)
        {
            mediaPlayer.Play();
        }
        pauseBtn.gameObject.SetActive(true);
        playBtn.gameObject.SetActive(false);
        AdminMessage msg = new AdminMessage();
        msg.Type = DataType.AdminEvent;
        msg.Data.Progress = mediaPlayer.Time;
        msg.Data.Control = ControlState.Play;
        NetManager.SendMessage(Util.ObjectToJson(msg));
    }

    // 暂停
    private void OnPauseButton()
    {
        if (mediaPlayer)
        {
            mediaPlayer.Pause();
        }
        playBtn.gameObject.SetActive(true);
        pauseBtn.gameObject.SetActive(false);
        AdminMessage msg = new AdminMessage();
        msg.Type = DataType.AdminEvent;
        msg.Data.Control = ControlState.Pause;
        NetManager.SendMessage(Util.ObjectToJson(msg));
    }

    void OnRestartButton(bool isOn)
    {
        if (isOn)
        {
            isLoop = true;
            mediaPlayer.Loop = true;
            AdminMessage msg = new AdminMessage();
            msg.Type = DataType.AdminEvent;
            msg.Data.Control = ControlState.Loop;
            NetManager.SendMessage(Util.ObjectToJson(msg));
        }
        else
        {
            isLoop = false;
            mediaPlayer.Loop = false;
            AdminMessage msg = new AdminMessage();
            msg.Type = DataType.AdminEvent;
            msg.Data.Control = ControlState.NoLoop;
            NetManager.SendMessage(Util.ObjectToJson(msg));
        }
    }

    // 调节音量
    private void OnAudioVolumeSlider()
    {
        //if (mediaPlayer && _audioVolumeSlider && _audioVolumeSlider.value != _setAudioVolumeSliderValue)
        //{
        //    mediaPlayer.Control.SetVolume(_audioVolumeSlider.value);
        //}
    }

    // 静音
    private void OnMuteChange()
    {
        //if (mediaPlayer)
        //{
        //    mediaPlayer.Control.MuteAudio(_MuteToggle.isOn);
        //}
    }

    // 拖动进度
    private void OnVideoSeekSliderOnVideoSeekSlider(float point)
    {
        if (IsVoluntary)
        {
            IsVoluntary = false;
            return;
        }

        mediaPlayer.Position = videoSeekSlider.value;
        AdminMessage msg = new AdminMessage();
        msg.Type = DataType.AdminEvent;
        msg.Data.Control = ControlState.Play;
        msg.Data.Progress = mediaPlayer.Time;
        NetManager.SendMessage(Util.ObjectToJson(msg));
    }

    void UpdateCameraRotation(string msg)
    {
        JSONNode json = JSON.Parse(msg);
        cameraRow.x =float.Parse( json["X"]);
        cameraRow.y = float.Parse(json["Y"]);
        cameraRow.z = float.Parse(json["Z"]);
        cameraRow.w = float.Parse(json["W"]);
        videoCameraTran.rotation = Quaternion.Lerp(videoCameraTran.rotation,cameraRow, 10f * Time.deltaTime);
    }

    void  OnPLlayerEnd()
    {
        if (!isLoop)
            OnClickBack();
    }
}
