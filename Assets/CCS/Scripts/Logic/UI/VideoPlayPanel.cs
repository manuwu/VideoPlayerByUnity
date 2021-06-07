using CCS;
using SimpleJSON;
using UMP;
using UnityEngine;
using UnityEngine.UI;

public class VideoPlayPanel : PanelBase
{
    //Trans
    private Button backBtn;
    private Button playBtn;
    private Button pauseBtn;
    private Button deviceBtn;
    private Toggle restartBtn;
    private Slider videoSeekSlider;
    private Text currentTimeTxt;
    private Text totalTimeTxt;
    private UniversalMediaPlayer mediaPlayer;
    private Transform videoCameraTran;
    private GameObject videoControlRoot;
    private bool IsVoluntary;
    private bool isLoop;

    //Data
    private Quaternion cameraRow;
    private Vector3 lastMousePosition;
    private float   mouseTime;
    
    public override void Init(params object[] args)
    {
        base.Init(args);
        mediaPlayer = PlayerManager.GetVideoPlayer();
        videoCameraTran  = PlayerManager.GetPlayerCamera();
        videoCameraTran.localPosition=Vector3.zero;
        videoCameraTran.localRotation=Quaternion.Euler(0,0,0);
        PlayerManager.ShowVideoPlayerRoot();
        PlayerManager.ShowPlayerRoot();
        
        mediaPlayer.Path = args[0].ToString();
        mediaPlayer.Play();
        if (args[1] != null)
        {
            mediaPlayer.Time = (long)args[1];
        }
    }

    public override void OnShowing()
    {
        base.OnShowing();
        if (isInit)
        {
            return;
        }
        isInit = true;
        base.OnShowing();

        backBtn         = skin.transform.Find("BtnRoot/LeftCorner/Corner/backBtn").GetComponent<Button>();
        deviceBtn         = skin.transform.Find("BtnRoot/LeftCorner/deviceBtn").GetComponent<Button>();
        playBtn         = skin.transform.Find("BtnRoot/PlayButton").GetComponent<Button>();
        pauseBtn        = skin.transform.Find("BtnRoot/PauseButton").GetComponent<Button>();
        videoControlRoot=skin.transform.Find("BtnRoot/LeftCorner").gameObject;
        restartBtn      = skin.transform.Find("BtnRoot/LeftCorner/Corner/resartBtn").GetComponent<Toggle>();
        restartBtn.gameObject.SetActive(false);
        currentTimeTxt  = skin.transform.Find("BtnRoot/currentTime").GetComponent<Text>();
        totalTimeTxt    = skin.transform.Find("BtnRoot/totalTime").GetComponent<Text>();
        videoSeekSlider = skin.transform.Find("BtnRoot/VideoSeekSlider").GetComponent<Slider>();
        IsVoluntary     = false;
        isLoop          = false;
        AddUIEvent();
        InvokeRepeating("UpdateProcess",1f,1f);
    }

    private void UpdateProcess()
    {
        IsVoluntary = true;
        totalTimeTxt.text     = Util.MillisecondToData(mediaPlayer.Length);
        currentTimeTxt.text   = Util.MillisecondToData(mediaPlayer.Time);
        videoSeekSlider.value = mediaPlayer.Position;

        SetCursorVisiable();
    }

    void SetCursorVisiable()
    {
        if (Input.mousePosition != lastMousePosition)
        {
            lastMousePosition = Input.mousePosition;

            videoControlRoot.SetActive(true);
            Cursor.visible = true;
            mouseTime = 0;
        }
        else
        {
            mouseTime += 1;

            if (mouseTime >= 5)
            {
                Cursor.visible = false;
                videoControlRoot.SetActive(false);
            }
        } 
    }

    private void AddUIEvent()
    {
        backBtn.onClick.AddListener(OnClickBack);
        deviceBtn.onClick.AddListener(OnClickDeviceBtn);
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

    void OnClickDeviceBtn()
    {
        PanManager.OpenPanel<ChooseSameSceneDevicePanel>(PanelName.ChooseSameSceneDevicePanel);
    }
    
    private void OnClickBack()
    {
//        AdminMessage msg = new AdminMessage();
//        msg.Type         = DataType.AdminEvent;
//        msg.Data.Control = ControlState.Stop;
//        NetManager.SendMessage(Util.ObjectToJson(msg));
        
        mediaPlayer.Stop();
        PlayerManager.HidePlayerRoot();
        PlayerManager.HideideoPlayerRoot();
        PanManager.ClosePanel(PanelName.ChooseSameSceneDevicePanel);
        PanManager.AllOpenWithout(PanelName.VideoPlayPanel);
        PanManager.ClosePanel(PanelName.VideoPlayPanel);
        PlayerManager.ResetImagePlayer();
        Cursor.visible = true;
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
        msg.Data.Control  = ControlState.Play;

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

    private void OnRestartButton(bool isOn)
    {
        if (isOn)
        {
            isLoop               = true;
            mediaPlayer.Loop     = true;

            AdminMessage msg     = new AdminMessage();
            msg.Type             = DataType.AdminEvent;
            msg.Data.Control     = ControlState.Loop;
            NetManager.SendMessage(Util.ObjectToJson(msg));
        }
        else
        {
            isLoop               = false;
            mediaPlayer.Loop     = false;
            AdminMessage msg     = new AdminMessage();
            msg.Type             = DataType.AdminEvent;
            msg.Data.Control     = ControlState.NoLoop;
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
        AdminMessage msg  = new AdminMessage();
        msg.Type          = DataType.AdminEvent;
        msg.Data.Control  = ControlState.Play;
        msg.Data.Progress = mediaPlayer.Time;

        NetManager.SendMessage(Util.ObjectToJson(msg));
    }

    private void UpdateCameraRotation(string msg)
    {
//        Debug.Log("manu UpdateCameraRotation msg" + msg);
        if (PlayerManager.isCanDragCamera)
        {
            JSONNode json = JSON.Parse(msg);
            cameraRow.x = json["x"].AsFloat;
            cameraRow.y = json["y"].AsFloat;
            cameraRow.z = json["z"].AsFloat;
            cameraRow.w = json["w"].AsFloat;
            videoCameraTran.rotation = Quaternion.Lerp(videoCameraTran.rotation, cameraRow, 10f * Time.deltaTime);
        }
    }

    private void OnPLlayerEnd()
    {
        if (!isLoop)
        {
            OnClickBack();
        }
        else
        {
            NetManager.SendMessage(PlayerManager.currentPlayVideoCommond);
        }
    }
}
