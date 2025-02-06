using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TJ.Scripts
{
    public class Vehicle : MonoBehaviour
    {
        public ColorEnum vehicleColor;
        public List<Transform> seats;
        [SerializeField] private List<MeshRenderer> vehMesh;
        public Tween movingZdir;
        private float distance = 30f;
        private bool isCollided = false;
        public bool isFull = false;
        public Vector3 originalPosition;
        public List<GameObject> removableParts;
        public Vector3 ogScale;
        public Vector3 newScale;
        public int playersInSeat = 0;
        private ParkingSlots slot;
        private bool canCollideWitOtherVehicle = true;
        public static bool isMovingStraight = false;
        public static ColorEnum LastTouchedCarcolor;
        public bool isMovingForward = false;
        public Garage garage;

        public int SeatCount => seats.Count;


        private void Awake()
        {
            //UpdatePlayerSeat = SeatCount;
        }
        //public int UpdateSeatCountNo()
        //{
        //    for (int i = 0; i < seats.Count; i++)
        //    {
        //        if (seats[i].childCount == 0)
        //        {
        //            updatePlayerSeat++;
        //        }
        //    }
        //    return updatePlayerSeat;

        //}

        void Start()
        {
            SetInitialPosition();
            ogScale = transform.localScale;
            isMovingStraight = false;
            Vector3 currentRotation = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(0, currentRotation.y, 0);
            //UpdateSeatCountNo();
            //UpdatePlayerSeat = SeatCount;
        }

        private void OnValidate()
        {
            Vector3 currentRotation = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(0, currentRotation.y, 0);
        }

        public void SetInitialPosition()
        {
            originalPosition = transform.position;
        }

        private void OnMouseDown()
        {
            Debug.Log("Mouse" + "moving:" + isMovingStraight + "");
            if (isMovingStraight || GameManager.instance.gameOver || EventSystem.current.IsPointerOverGameObject())
                return;
            if (garage != null && !garage.canMoveNext)
                return;

            if (Helper.instance)
                Helper.instance.MoveHand();
            LastTouchedCarcolor = vehicleColor;
            if (CheckForVehicleInFront(out RaycastHit hitInfo))
            {
                isMovingStraight = true;
                isMovingForward = true;
                Vibration.Vibrate(40);
                // Move the car straight forward towards the detected vehicle
                Vector3 targetPosition =
                    transform.position +
                    transform.forward * (hitInfo.distance + 1); // Slightly before the collision point
                movingZdir = transform.DOMove(targetPosition, 0.2f).SetEase(Ease.InQuad);

                return;
            }

            slot = ParkingManager.instance.CheckForFreeSlot();
            if (slot == null)
            {
                Debug.Log("All Slots are Full");
                return;
            }

            if (garage)
                garage.RemoveObstacle(this);
            MoveCarStraight();
            //transform.GetChild(0).DOShakeScale(0.3f, new Vector3(0, 0, 0.2f), 1, 1);
            //Debug.Log("UpdateSeatCount : " + UpdateSeatCountNo());
        }

        //public int GetSeatcount()
        //{
        //    seatCount = seats.Count;
        //    return seatCount;
        //}
        public void ChangeScale(bool shift)
        {
            newScale = Vector3.one;
            if (shift)
            {
                transform.localScale = newScale;
            }
            else
            {
                transform.localScale = ogScale;
            }
        }

        public Transform GetFreeSeat()
        {
            for (int i = 0; i < seats.Count; i++)
            {
                if (seats[i].childCount == 0)
                {
                    playersInSeat++;
                    IsVehicleFull();
                    return seats[i];
                }
            }

            return null;
        }

        public void IsVehicleFull()
        {
            if (playersInSeat == seats.Count)
            {
                isFull = true;
                DOVirtual.DelayedCall(1f, () =>
                {
                    VehicleGoing();
                    GameManager.instance.CheckGameWin();
                });
            }
        }

        public void VehicleGoing()
        {
            // ParkingSlots slot = this.transform.parent.GetComponent<ParkingSlots>();
            VehicleController.instance.vehicles = VehicleController.instance.vehicles
                .Where(v => v != this.transform)
                .ToArray();
            transform.DORotateQuaternion(ParkingManager.instance.exitPoint.rotation, 0.2f);
            transform.DOMove(
                new Vector3(slot.enterPoint.transform.position.x, transform.position.y,
                    slot.enterPoint.transform.position.z), 30f).SetSpeedBased().OnComplete(() =>
            {
                slot.isOccupied = false;
                canCollideWitOtherVehicle = false;
                ParkingManager.instance.parkedVehicles.Remove(this);
                transform.parent = null;
                transform.DOMove(ParkingManager.instance.exitPoint.transform.position, 35f).SetSpeedBased().SetEase(Ease.InBack)
                    .OnComplete(() => { transform.gameObject.SetActive(false); });
            });
            SoundController.Instance.PlayFullSound();
            SoundController.Instance.PlayOneShot(SoundController.Instance.moving);
        }

        public void ChangeColor(ColorEnum colorEnum)
        {
            this.vehicleColor = colorEnum;
            Material mats = VehicleController.instance.VehiclesMaterialHolder.FindMaterialByName(colorEnum);
            if (mats != null)
            {
                for (int i = 0; i < vehMesh.Count; i++)
                {
                    vehMesh[i].material = mats;
                }
            }
        }


        public void MoveCarStraight()
        {
            Vibration.Vibrate(20);
            SoundController.Instance.PlayOneShot(SoundController.Instance.tapSound, 0.5f);
            slot.isOccupied = true;
            isMovingStraight = true;
            isMovingForward = true;
            Vector3 localPosition = transform.localPosition;
            Vector3 localForwardDirection = transform.localRotation * Vector3.forward;

            Vector3 pointAtDistance = localPosition + localForwardDirection * distance;

            Vector3 worldPoint = transform.parent.TransformPoint(pointAtDistance);

            Debug.DrawLine(transform.position, worldPoint, Color.green);
            movingZdir = transform.DOMove(worldPoint, 12f).SetSpeedBased();
            GetComponent<AudioSource>().enabled = true;
        }
        public bool CheckForObstacles()
        {
            // Define the offsets for the left and right raycasts
            float offset = 1.0f; // Adjust this value based on your needs
            float rayDistance = Mathf.Infinity;

            // Define the forward direction
            Vector3 forward = transform.TransformDirection(Vector3.forward);

            // Calculate the left and right ray directions
            Vector3 leftRayDirection = transform.TransformDirection(Vector3.forward + Vector3.left * offset);
            Vector3 rightRayDirection = transform.TransformDirection(Vector3.forward + Vector3.right * offset);

            // Check for obstacles in the forward direction and slightly to the left and right
            if (Physics.Raycast(transform.position, leftRayDirection, out RaycastHit hitInfoLeft, rayDistance) &&
                Physics.Raycast(transform.position, rightRayDirection, out RaycastHit hitInfoRight, rayDistance))
            {
                if (hitInfoLeft.collider != null && hitInfoLeft.collider.TryGetComponent(out Vehicle vehicleLeft) &&
                    vehicleLeft.canCollideWitOtherVehicle && !vehicleLeft.isMovingForward)
                {
                    Debug.Log("Vehicle detected on the left!");
                    return true;
                }

                if (hitInfoRight.collider != null && hitInfoRight.collider.TryGetComponent(out Vehicle vehicleRight) &&
                    vehicleRight.canCollideWitOtherVehicle && !vehicleRight.isMovingForward)
                {
                    Debug.Log("Vehicle detected on the right!");
                    return true;
                }
            }

            return false;
        }


        private bool CheckForVehicleInFront(out RaycastHit hitInfo)
        {
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            float rayDistance = Mathf.Infinity;

            if (Physics.Raycast(transform.position, forward, out hitInfo, rayDistance))
            {
                if (hitInfo.collider.TryGetComponent(out Vehicle vehicle) && vehicle.canCollideWitOtherVehicle &&
                    !vehicle.isMovingForward)
                {
                    Debug.Log("Vehicle detected in front!");
                    return true;
                }
            }

            return false;

            /*hitInfo = new RaycastHit();
            // Get the vehicle's BoxCollider
            BoxCollider boxCollider = GetComponent<BoxCollider>();
            // Calculate the CheckForObstacles parameters
            Vector3 boxCenter = transform.position + transform.TransformDirection(boxCollider.center);
            Vector3 boxHalfExtents = boxCollider.size / 2;
            Vector3 direction = transform.TransformDirection(Vector3.forward);
            float rayDistance = distance;
            // Perform the CheckForObstacles
            if (Physics.CheckForObstacles(boxCenter, boxHalfExtents, direction, out hitInfo, transform.rotation, rayDistance))
            {
                if (hitInfo.collider.TryGetComponent(out Vehicle vehicle) && vehicle.canCollideWitOtherVehicle)
                {
                    Debug.Log("Vehicle detected in front!");
                    return true;
                }
            }
            return false;*/
        }

        private void StrikeAndMoveBack(Vehicle targetVehicle)
        {
            Vector3 targetPosition = targetVehicle.transform.position;

            transform.DOMove(targetPosition, 0.5f).OnComplete(() =>
            {
                targetVehicle.ShakeVehicle();

                transform.DOMove(originalPosition, 0.5f);
            });
        }

        public void ShakeVehicle()
        {
            transform.DOShakeRotation(0.2f, transform.forward * 2, vibrato: 10, randomness: 90).SetEase(Ease.InBounce);
        }

        private static int counter = 0;
        private bool toggle;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Down"))
            {
                canCollideWitOtherVehicle = false;
                movingZdir.Pause();
                Debug.Log("HitDown");
                toggle = !toggle;
                /*if (toggle)
                {
                    MoveToSideBorder(VehicleController.instance.rightCollider, 20f);
                    return;
                }*/
                MoveToSideBorder(VehicleController.instance.leftCollider, -20f);

                if (!toggle)
                {
                }

                return;
            }

            if (canCollideWitOtherVehicle && other.TryGetComponent(out Vehicle vehicle) &&
                vehicle.canCollideWitOtherVehicle)
            {
                if (slot && canCollideWitOtherVehicle)
                    slot.isOccupied = false;
                movingZdir.Pause();

                if (!isCollided)
                {
                    if (isMovingStraight && counter == 0 && isMovingForward)
                    {
                        counter++;
                        Debug.Log("playing straight");
                        GetComponent<AudioSource>().enabled = false;
                        SoundController.Instance.PlayOneShot(SoundController.Instance.hitSound);
                        EffectsManager.instance.PlayEffect(EffectsManager.instance.hitEffect,
                            other.ClosestPoint(transform.position + new Vector3(0, 0.25f, 0)), Quaternion.identity);
                    }

                    vehicle.ShakeVehicle();
                    transform.DOMove(originalPosition, 0.3f).SetEase(Ease.OutBack)
                        .OnComplete(() =>
                        {
                            counter = 0;
                            isMovingStraight = false;
                            isMovingForward = false;
                        });
                }
            }

            if (other.gameObject.CompareTag("Border") && !isCollided)
            {
                isCollided = true;
                isMovingStraight = false;
                canCollideWitOtherVehicle = false;
                Debug.Log("COLLIDED");
                MoveToTargetFromBorder();
                VehicleController.instance.RemoveVehicle(this);
                movingZdir.Pause();
            }

            if (other.gameObject.CompareTag("Upborder") && !isCollided)
            {
                isCollided = true;
                isMovingStraight = false;
                canCollideWitOtherVehicle = false;
                MoveToTargetFromUpBorder();
                VehicleController.instance.RemoveVehicle(this);
                movingZdir.Pause();
            }
        }

        public void MoveToSideBorder(Transform collider, float distance)
        {
            isMovingStraight = false;
            canCollideWitOtherVehicle = false;

            Transform cube = collider.transform;
            Vector3 cubePos = cube.position;

            Vector3 directionToCube = new Vector3(cubePos.x - transform.position.x, 0, 0);

            Quaternion targetRotation = Quaternion.LookRotation(directionToCube, Vector3.up);

            transform.DORotateQuaternion(targetRotation, 0.1f);
            transform.DOLocalMoveX(distance, 0.8f);
            VehicleController.instance.RemoveVehicle(this);
        }

        public void MoveToTargetFromBorder()
        {
            Transform road = VehicleController.instance.Road;
            Vector3 roadPos = road.position;

            Vector3[] path = new Vector3[]
            {
                transform.position,
                new Vector3(transform.position.x, transform.position.y, road.position.z)
            };

            transform.DORotate(Vector3.zero, 0.1f);
            transform.DOPath(path, 0.3f, PathType.Linear).SetEase(Ease.Linear).OnComplete(() =>
            {
                transform.DOLookAt(roadPos, 0.1f);
                //ParkingManager.instance.GetSlot(this);
                MoveToSlot();
                foreach (var parts in removableParts)
                {
                    parts.SetActive(false);
                }
            });
        }

        public void MoveToTargetFromUpBorder()
        {
            transform.DOLookAt(slot.transform.position, 0.1f);
            MoveToSlot();
            foreach (var parts in removableParts)
            {
                parts.SetActive(false);
            }
        }

        public void MoveToSlot()
        {
            // slot.isOccupied = true;
            Vector3[] waypoints = new Vector3[]
            {
                new(slot.enterPoint.position.x, transform.position.y, slot.enterPoint.position.z),
                new(slot.stopPoint.position.x, transform.position.y + 0.5f, slot.stopPoint.position.z),
            };
            ChangeScale(true);
            transform.DOPath(waypoints, .5f, PathType.CatmullRom).OnWaypointChange(waypointindex =>
                {
                    if (waypointindex == 1)
                    {
                        transform.DORotateQuaternion(slot.stopPoint.rotation, 0.2f);
                    }
                })
                .OnComplete(() =>
                {
                    isMovingStraight = false;
                    ParkingManager.instance.parkedVehicles.Add(this);
                    //slot.isOccupied = true;
                    transform.parent = slot.transform;
                    GetComponent<BoxCollider>().enabled = false;
                    //PlayerManager.instance.CheckPlayersOfsameColor(veh);
                    // PlayerManager.instance.CheckColor(0);
                    Debug.Log("Moved to slot");
                    if (!PlayerManager.instance.isColormatched)
                        EventManager.OnNewVehArrived?.Invoke();
                    GetComponent<AudioSource>().enabled = false;
                });
        }
    }
}
/*using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace TJ.Scripts
{
    public class Vehicle : MonoBehaviour
    {
        public ColorEnum vehicleColor;
        public List<Transform> seats;
        [SerializeField] private List<MeshRenderer> vehMesh;
        public Tween movingZdir;
        private float distance = 20f;
        private bool isCollided = false;
        public bool isFull = false;
        public Vector3 originalPosition;
        public List<GameObject> removableParts;
        public Vector3 ogScale;
        public Vector3 newScale;
        public int playersInSeat = 0;
        private ParkingSlots slot;
        private bool canCollideWitOtherVehicle = true;
        public static bool isMovingStraight = false;
        public static ColorEnum LastTouchedCarcolor;
        public bool isMovingForward = false;
        public Garage garage;

        public int SeatCount => seats.Count;


        private void Awake()
        {
            //UpdatePlayerSeat = SeatCount;
        }
        //public int UpdateSeatCountNo()
        //{
        //    for (int i = 0; i < seats.Count; i++)
        //    {
        //        if (seats[i].childCount == 0)
        //        {
        //            updatePlayerSeat++;
        //        }
        //    }
        //    return updatePlayerSeat;

        //}

        void Start()
        {
            SetInitialPosition();
            ogScale = transform.localScale;
            isMovingStraight = false;
            Vector3 currentRotation = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(0, currentRotation.y, 0);
            //UpdateSeatCountNo();
            //UpdatePlayerSeat = SeatCount;
        }

        private void OnValidate()
        {
            Vector3 currentRotation = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(0, currentRotation.y, 0);
        }

        public void SetInitialPosition()
        {
            originalPosition = transform.position;
        }

        private void OnMouseDown()
        {
            Debug.Log("Mouse" + "moving:" + isMovingStraight + "");
            if (isMovingStraight || GameManager.instance.gameOver)
                return;
            if (garage != null && !garage.canMoveNext)
                return;

            if (Helper.instance)
                Helper.instance.MoveHand();
            LastTouchedCarcolor = vehicleColor;
            if (CheckForVehicleInFront(out RaycastHit hitInfo))
            {
                isMovingStraight = true;
                isMovingForward = true;
                Vibration.Vibrate(40);
                // Move the car straight forward towards the detected vehicle
                Vector3 targetPosition =
                    transform.position +
                    transform.forward * (hitInfo.distance + 1); // Slightly before the collision point
                movingZdir = transform.DOMove(targetPosition, 0.2f).SetEase(Ease.InQuad);

                return;
            }

            slot = ParkingManager.instance.CheckForFreeSlot();
            if (slot == null)
            {
                Debug.Log("All Slots are Full");
                return;
            }

            if (garage)
                garage.RemoveObstacle(this);
            MoveCarStraight();
            //transform.GetChild(0).DOShakeScale(0.3f, new Vector3(0, 0, 0.2f), 1, 1);
            //Debug.Log("UpdateSeatCount : " + UpdateSeatCountNo());
        }

        //public int GetSeatcount()
        //{
        //    seatCount = seats.Count;
        //    return seatCount;
        //}
        public void ChangeScale(bool shift)
        {
            newScale = Vector3.one;
            if (shift)
            {
                transform.localScale = newScale;
            }
            else
            {
                transform.localScale = ogScale;
            }
        }

        public Transform GetFreeSeat()
        {
            for (int i = 0; i < seats.Count; i++)
            {
                if (seats[i].childCount == 0)
                {
                    playersInSeat++;
                    IsVehicleFull();
                    return seats[i];
                }
            }

            return null;
        }

        public void IsVehicleFull()
        {
            if (playersInSeat == seats.Count)
            {
                isFull = true;
                DOVirtual.DelayedCall(1f, () =>
                {
                    VehicleGoing();
                    GameManager.instance.CheckGameWin();
                });
            }
        }

        public void VehicleGoing()
        {
            // ParkingSlots slot = this.transform.parent.GetComponent<ParkingSlots>();
            VehicleController.instance.vehicles = VehicleController.instance.vehicles
                .Where(v => v != this.transform)
                .ToArray();
            transform.DORotateQuaternion(ParkingManager.instance.exitPoint.rotation, 0.35f);
            transform.DOMove(
                new Vector3(slot.enterPoint.transform.position.x, transform.position.y,
                    slot.enterPoint.transform.position.z), 0.3f).OnComplete(() =>
            {
                slot.isOccupied = false;
                canCollideWitOtherVehicle = false;
                ParkingManager.instance.parkedVehicles.Remove(this);
                transform.parent = null;
                transform.DOMove(ParkingManager.instance.exitPoint.transform.position, 0.3f).SetEase(Ease.InBack)
                    .OnComplete(() => { transform.gameObject.SetActive(false); });
            });
        }

        public void ChangeColor(ColorEnum colorEnum)
        {
            this.vehicleColor = colorEnum;
            Material mats = VehicleController.instance.VehiclesMaterialHolder.FindMaterialByName(colorEnum);
            if (mats != null)
            {
                for (int i = 0; i < vehMesh.Count; i++)
                {
                    vehMesh[i].material = mats;
                }
            }
        }


        public void MoveCarStraight()
        {
            Vibration.Vibrate(20);
            SoundController.Instance.PlayOneShot(SoundController.Instance.tapSound, 0.5f);
            slot.isOccupied = true;
            isMovingStraight = true;
            isMovingForward = true;
            Vector3 localPosition = transform.localPosition;
            Vector3 localForwardDirection = transform.localRotation * Vector3.forward;

            Vector3 pointAtDistance = localPosition + localForwardDirection * distance;

            Vector3 worldPoint = transform.parent.TransformPoint(pointAtDistance);

            Debug.DrawLine(transform.position, worldPoint, Color.green);
            movingZdir = transform.DOMove(worldPoint, 1f);
        }

        private bool CheckForVehicleInFront(out RaycastHit hitInfo)
        {
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            float rayDistance = distance;

            if (Physics.Raycast(transform.position, forward, out hitInfo, rayDistance))
            {
                if (hitInfo.collider.TryGetComponent(out Vehicle vehicle) && vehicle.canCollideWitOtherVehicle && !vehicle.isMovingForward)
                {
                    Debug.Log("Vehicle detected in front!");
                    return true;
                }
            }

            return false;

            /*hitInfo = new RaycastHit();
            // Get the vehicle's BoxCollider
            BoxCollider boxCollider = GetComponent<BoxCollider>();
            // Calculate the CheckForObstacles parameters
            Vector3 boxCenter = transform.position + transform.TransformDirection(boxCollider.center);
            Vector3 boxHalfExtents = boxCollider.size / 2;
            Vector3 direction = transform.TransformDirection(Vector3.forward);
            float rayDistance = distance;
            // Perform the CheckForObstacles
            if (Physics.CheckForObstacles(boxCenter, boxHalfExtents, direction, out hitInfo, transform.rotation, rayDistance))
            {
                if (hitInfo.collider.TryGetComponent(out Vehicle vehicle) && vehicle.canCollideWitOtherVehicle)
                {
                    Debug.Log("Vehicle detected in front!");
                    return true;
                }
            }
            return false;#1#
        }

        private void StrikeAndMoveBack(Vehicle targetVehicle)
        {
            Vector3 targetPosition = targetVehicle.transform.position;

            transform.DOMove(targetPosition, 0.5f).OnComplete(() =>
            {
                targetVehicle.ShakeVehicle();

                transform.DOMove(originalPosition, 0.5f);
            });
        }

        public void ShakeVehicle()
        {
            transform.DOShakeRotation(0.2f, transform.forward * 2, vibrato: 10, randomness: 90).SetEase(Ease.InBounce);
        }

        private static int counter = 0;

        private void OnTriggerEnter(Collider other)
        {
            if (canCollideWitOtherVehicle && other.TryGetComponent(out Vehicle vehicle) &&
                vehicle.canCollideWitOtherVehicle)
            {
                if (slot && canCollideWitOtherVehicle)
                    slot.isOccupied = false;
                movingZdir.Pause();

                if (!isCollided)
                {
                    if (isMovingStraight && counter==0 && isMovingForward)
                    {
                        counter++;
                        Debug.Log("playing straight");
                        SoundController.Instance.PlayOneShot(SoundController.Instance.hitSound);
                        EffectsManager.instance.PlayEffect(EffectsManager.instance.hitEffect,
                            other.ClosestPoint(transform.position + new Vector3(0, 0.25f, 0)), Quaternion.identity);
                    }

                    vehicle.ShakeVehicle();
                    transform.DOMove(originalPosition, 0.3f).SetEase(Ease.OutBack)
                        .OnComplete(() =>
                        {
                            counter = 0;
                            isMovingStraight = false;
                            isMovingForward = false;
                        });
                }
            }

            if (other.gameObject.CompareTag("Border") && !isCollided)
            {
                isCollided = true;
                isMovingStraight = false;
                canCollideWitOtherVehicle = false;
                Debug.Log("COLLIDED");
                MoveToTargetFromBorder();
                VehicleController.instance.RemoveVehicle(this);
                movingZdir.Pause();
            }

            if (other.gameObject.CompareTag("Upborder") && !isCollided)
            {
                isCollided = true;
                isMovingStraight = false;
                canCollideWitOtherVehicle = false;
                MoveToTargetFromUpBorder();
                VehicleController.instance.RemoveVehicle(this);
                movingZdir.Pause();
            }

            if (other.gameObject.CompareTag("Down"))
            {
                canCollideWitOtherVehicle = false;
                Debug.Log("HitDown");
                int rand = Random.Range(0, 2);
                if (rand == 0)
                {
                    MoveToSideBorder(VehicleController.instance.rightCollider, 20f);
                    return;
                }

                if (rand == 1)
                {
                    MoveToSideBorder(VehicleController.instance.leftCollider, -20f);
                }
                movingZdir.Pause();
            }
        }

        public void MoveToSideBorder(Transform collider, float distance)
        {
            isMovingStraight = false;
            canCollideWitOtherVehicle = false;

            Transform cube = collider.transform;
            Vector3 cubePos = cube.position;

            Vector3 directionToCube = new Vector3(cubePos.x - transform.position.x, 0, 0);

            Quaternion targetRotation = Quaternion.LookRotation(directionToCube, Vector3.up);

            transform.DORotateQuaternion(targetRotation, 0.1f);
            transform.DOLocalMoveX(distance, 0.8f);
            VehicleController.instance.RemoveVehicle(this);
        }

        public void MoveToTargetFromBorder()
        {
            Transform road = VehicleController.instance.Road;
            Vector3 roadPos = road.position;

            Vector3[] path = new Vector3[]
            {
                transform.position,
                new Vector3(transform.position.x, transform.position.y, road.position.z)
            };

            transform.DORotate(Vector3.zero, 0.1f);
            transform.DOPath(path, 0.3f, PathType.Linear).SetEase(Ease.Linear).OnComplete(() =>
            {
                transform.DOLookAt(roadPos, 0.1f);
                //ParkingManager.instance.GetSlot(this);
                MoveToSlot();
                foreach (var parts in removableParts)
                {
                    parts.SetActive(false);
                }
            });
        }

        public void MoveToTargetFromUpBorder()
        {
            transform.DOLookAt(slot.transform.position, 0.1f);
            MoveToSlot();
            foreach (var parts in removableParts)
            {
                parts.SetActive(false);
            }
        }

        public void MoveToSlot()
        {
            // slot.isOccupied = true;
            Vector3[] waypoints = new Vector3[]
            {
                new(slot.enterPoint.position.x, transform.position.y, slot.enterPoint.position.z),
                new(slot.stopPoint.position.x, transform.position.y + 0.5f, slot.stopPoint.position.z),
            };
            ChangeScale(true);
            transform.DOPath(waypoints, .5f, PathType.CatmullRom).OnWaypointChange(waypointindex =>
                {
                    if (waypointindex == 1)
                    {
                        transform.DORotateQuaternion(slot.stopPoint.rotation, 0.2f);
                    }
                })
                .OnComplete(() =>
                {
                    isMovingStraight = false;
                    ParkingManager.instance.parkedVehicles.Add(this);
                    //slot.isOccupied = true;
                    transform.parent = slot.transform;
                    GetComponent<BoxCollider>().enabled = false;
                    //PlayerManager.instance.CheckPlayersOfsameColor(veh);
                    // PlayerManager.instance.CheckColor(0);
                    Debug.Log("Moved to slot");
                    if (!PlayerManager.instance.isColormatched)
                        EventManager.OnNewVehArrived?.Invoke();
                });
        }
    }
}*/