using System.Collections;
using DG.Tweening;
using UnityEngine;
using StarkSDKSpace;
using TTSDK.UNBridgeLib.LitJson;
using TTSDK;

namespace TJ.Scripts
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;

        public MaterialHolder vehMaterialHolder;
        public MaterialHolder stickmanMaterialHolder;
        public int winCount = 0;

        public bool gameOver = false;
        private StarkAdManager starkAdManager;

        public string clickid;
        // Start is called before the first frame update
        private void Awake()
        {
            instance = this;
            Vibration.Init();
            //MaterialHolder.InitializeMaterialDictionary();
        }

        private void Start()
        {
            Application.targetFrameRate = 120;
        }

        private bool IfSameColorVehicleParked()
        {
            var vehicles = ParkingManager.instance.parkedVehicles;
            if (vehicles.Count > 0 && PlayerManager.instance.activePlayerList.Count > 0)
            {
                foreach (var VARIABLE in vehicles)
                {
                    if (VARIABLE.vehicleColor == PlayerManager.instance.activePlayerList[0].color)
                    {
                        return true;
                    }
                }
            }
            else if (vehicles.Count <= 0)
            {
                return true;
            }
            else if (PlayerManager.instance.activePlayerList.Count <= 0)
            {
                return true;
            }

            return false;
        }

        public bool ChekIfSlotFull()
        {
            var vehicles = ParkingManager.instance.parkedVehicles;
            if (vehicles.Count == ParkingManager.instance.slots.Count - 1)
            {
                Debug.Log("<color=yellow>Warning: Only One Slot Left</color>");
            }

            if (vehicles.Count == ParkingManager.instance.slots.Count)
                return true;
            return false;
        }

        public IEnumerator CheckIfGameOver()
        {
            yield return new WaitForSeconds(3f);
            if (ChekIfSlotFull() && IfSameColorVehicleParked() == false)
            {
                gameOver = true;
                SoundController.Instance.PlayOneShot(SoundController.Instance.fail);
                UIManager.instance.TogglePanel(UIManager.instance.gameOverPanle, true);
                //Debug.Log("<color=red>Warning: Game Over</color>");
            }
        }

        private bool alreaduCalled;

        public void CheckGameWin()
        {
            if (alreaduCalled)
                return;

            winCount++;
            if (winCount == VehicleController.instance.totalVehicles)
            {
                ShowInterstitialAd("1lcaf5895d5l1293dc",
            () => {
                Debug.LogError("--插屏广告完成--");

            },
            (it, str) => {
                Debug.LogError("Error->" + str);
            });
                Debug.Log("Activating win panel");
                alreaduCalled = true;
                
                DOVirtual.DelayedCall(1.5f, () => SoundController.Instance.PlayOneShot(SoundController.Instance.win));
                DOVirtual.DelayedCall(2f, () => UIManager.instance.TogglePanel(UIManager.instance.winPanel, true));
                LevelManager.LevelProgressed();
                //Debug.Log("<color=Green>Success: Game Win</color>");
            }
        }
        /// <summary>
        /// 播放插屏广告
        /// </summary>
        /// <param name="adId"></param>
        /// <param name="errorCallBack"></param>
        /// <param name="closeCallBack"></param>
        public void ShowInterstitialAd(string adId, System.Action closeCallBack, System.Action<int, string> errorCallBack)
        {
            starkAdManager = StarkSDK.API.GetStarkAdManager();
            if (starkAdManager != null)
            {
                var mInterstitialAd = starkAdManager.CreateInterstitialAd(adId, errorCallBack, closeCallBack);
                mInterstitialAd.Load();
                mInterstitialAd.Show();
            }
        }
    }
}