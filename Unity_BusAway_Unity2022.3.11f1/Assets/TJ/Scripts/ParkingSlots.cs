using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using TTSDK.UNBridgeLib.LitJson;
using TTSDK;
using StarkSDKSpace;
using System.Collections.Generic;

namespace TJ.Scripts
{
    public class ParkingSlots : MonoBehaviour
    {
        public Transform enterPoint;
        public Transform stopPoint;

        public bool isOccupied;
        [SerializeField] private GameObject normal;
        [SerializeField] private GameObject locked;
        public string clickid;
        private StarkAdManager starkAdManager;

        // Start is called before the first frame update
        void Start()
        {
            enterPoint = transform.GetChild(0).transform;
            stopPoint = transform.GetChild(1).transform;
        }

        /*private void OnTriggerStay(Collider other)
        {
            isOccupied = true;
        }*/
        private void OnMouseDown()
        {
            if (GameManager.instance.gameOver || EventSystem.current.IsPointerOverGameObject()) return;
            ShowVideoAd("192if3b93qo6991ed0",
            (bol) => {
                if (bol)
                {
                    
                    //if (locked.activeInHierarchy)
                    //{
                    //    CheckLockStatus();
                    //    return;
                    //}

                    UnlockSlot_Callback();


                    clickid = "";
                    getClickid();
                    apiSend("game_addiction", clickid);
                    apiSend("lt_roi", clickid);


                }
                else
                {
                    StarkSDKSpace.AndroidUIManager.ShowToast("观看完整视频才能获取奖励哦！");
                }
            },
            (it, str) => {
                Debug.LogError("Error->" + str);
                //AndroidUIManager.ShowToast("广告加载异常，请重新看广告！");
            });
            
        }

        private void CheckLockStatus()
        {
            ParkingManager.instance.parkingSlot_Rv = this;
            ISManager.instance.ShowRewardedVideo(AdState.ParkingSlot);
        }

        public void UnlockSlot_Callback()
        {
            var slots = ParkingManager.instance.slots;
            if (!slots.Contains(this))
            {
                slots.Add(this);
                locked.SetActive(false);
                normal.SetActive(true);
            }
            Debug.Log("Added Slot");
        }
        public void getClickid()
        {
            var launchOpt = StarkSDK.API.GetLaunchOptionsSync();
            if (launchOpt.Query != null)
            {
                foreach (KeyValuePair<string, string> kv in launchOpt.Query)
                    if (kv.Value != null)
                    {
                        Debug.Log(kv.Key + "<-参数-> " + kv.Value);
                        if (kv.Key.ToString() == "clickid")
                        {
                            clickid = kv.Value.ToString();
                        }
                    }
                    else
                    {
                        Debug.Log(kv.Key + "<-参数-> " + "null ");
                    }
            }
        }

        public void apiSend(string eventname, string clickid)
        {
            TTRequest.InnerOptions options = new TTRequest.InnerOptions();
            options.Header["content-type"] = "application/json";
            options.Method = "POST";

            JsonData data1 = new JsonData();

            data1["event_type"] = eventname;
            data1["context"] = new JsonData();
            data1["context"]["ad"] = new JsonData();
            data1["context"]["ad"]["callback"] = clickid;

            Debug.Log("<-data1-> " + data1.ToJson());

            options.Data = data1.ToJson();

            TT.Request("https://analytics.oceanengine.com/api/v2/conversion", options,
               response => { Debug.Log(response); },
               response => { Debug.Log(response); });
        }


        /// <summary>
        /// </summary>
        /// <param name="adId"></param>
        /// <param name="closeCallBack"></param>
        /// <param name="errorCallBack"></param>
        public void ShowVideoAd(string adId, System.Action<bool> closeCallBack, System.Action<int, string> errorCallBack)
        {
            starkAdManager = StarkSDK.API.GetStarkAdManager();
            if (starkAdManager != null)
            {
                starkAdManager.ShowVideoAdWithId(adId, closeCallBack, errorCallBack);
            }
        }
    }
}