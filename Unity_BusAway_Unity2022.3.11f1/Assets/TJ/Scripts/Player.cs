using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace TJ.Scripts
{
    public class Player : MonoBehaviour
    {
        private static readonly int Sit = Animator.StringToHash("Sit");
        public static readonly int Walk = Animator.StringToHash("Walk");
        public ColorEnum color;
        public Renderer meshRenderer;
        public Animator anim;
        public GameObject animGo;

        private void Awake()
        {
            anim = animGo.GetComponent<Animator>();
        }

        public void ChangeColor(ColorEnum colorEnum)
        {
            Material mats = VehicleController.instance.stickmanMaterialHolder.FindMaterialByName(colorEnum);
            meshRenderer.material = mats;
            //gameObject.name = gameObject.name.Replace("blue", "");
            gameObject.name += colorEnum.ToString();
            color = colorEnum;
           
            //if (mats != null)
            //{

            //    //for (int i = 0; i < vehMesh.Count; i++)
            //    //{
            //    //    vehMesh[i].material = mats;
            //    //}
            //}
        }

        public IEnumerator MoveToSlot1(Vector3 mid, Transform pickpoint, Vector3 point, float delay)
        {
            yield return new WaitForSeconds(delay);
            transform.DOMove(mid, 0.3f).OnComplete(() =>
            {
                transform.rotation = pickpoint.rotation;
                transform.DOMove(point, 0.3f).OnComplete(() =>
                {
                    anim.SetBool(Walk, false);
                });
            });
        }
        public IEnumerator MoveToSlot2(Vector3 point, float delay)
        {
            yield return new WaitForSeconds(delay);
            //DOVirtual.DelayedCall(0.2f, () =>
            //{          
            transform.DOMove(point, 0.3f).OnComplete(() =>
            {
                anim.SetBool(Walk, false);
            });
            //});
        }

        public IEnumerator MoveToTruck(Vehicle vehicle)
        {
            PlayerManager.instance.playersInScene.Remove(this);
            var seat = vehicle.GetFreeSeat();
            transform.parent = seat.transform;
            anim.SetBool(Walk, true);
            Vector3[] path = new Vector3[]
            {
                transform.position,
                transform.position - new Vector3(0,0,1),
                vehicle.transform.position,
                seat.transform.position
            };
            transform.DOPath(path, 0.8f, PathType.CatmullRom).OnComplete(() =>
            {
                anim.SetBool(Walk, true);
                anim.SetBool(Sit, true);
                transform.localRotation = Quaternion.identity;
                transform.localPosition += new Vector3(0, -0.34f, 0.2f);
                transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            });
            yield return new WaitForSeconds(0.1f);
            VehicleController.instance.UpdatePlayerCount();
            PlayerManager.instance.RepositionPlayers();
            SoundController.Instance.PlayOneShot(SoundController.Instance.sort);
            
        }

        /*public IEnumerator MoveToTruck(Player player1, Vehicle veh, Transform walkPoint, float delay,int totalCount,List<Player> players)
        {
            yield return new WaitForSeconds(delay);
            player1.transform.rotation = PlayerManager.instance.spawnPoint.rotation;
            player1.anim.SetBool(Walk, true);
            var seat = veh.GetFreeSeat();
            player1.transform.parent = seat;
            Vector3[] path = new Vector3[]
            {
                walkPoint.transform.position,
                veh.transform.position + new Vector3(0,0,3),
            };
            player1.transform.DOPath(path, 0.2f).OnComplete(() =>
            {
                PlayerManager.instance.UpdatePlayerPos(VehicleController.instance.UpdatePosCount);          
                VehicleController.instance.UpdatePosCount++;
                player1.transform.DOJump(seat.position, 1f, 1, 0.1f).OnComplete(() =>
                {
                    player1.transform.rotation = seat.rotation;
                    player1.anim.SetBool(Sit, true);
                    VehicleController.instance.plyrCount++;             
                    veh.playersInSeat++;
                    VehicleController.instance.playerCount--;
                    veh.IsVehicleFull();

                    if (VehicleController.instance.plyrCount == totalCount)
                    {
                        veh.UpdatePlayerSeat -= veh.playersInSeat;
                        //Debug.Log("updateSeatCount :" + veh.UpdateSeatCountNo());
                        Debug.Log("players are full");
                        PlayerManager.instance.StartCoroutine(PlayerManager.instance.RemovePlayerAndCheck(players));
                        VehicleController.instance.plyrCount = 0;
                        VehicleController.instance.UpdatePosCount = 1;
                        /*DOVirtual.DelayedCall(0.5f, () =>
                        {
                            if (ParkingManager.instance.CheckIfAllOccupied())
                            {
                                Debug.Log("You Lose");
                            }

                        });   #1#               
                    }
                });
            });
        
            Debug.Log("plyrCount : " + VehicleController.instance.plyrCount);
        }*/
    }
}
