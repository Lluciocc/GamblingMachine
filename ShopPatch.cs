using HarmonyLib;
using UnityEngine;
using Photon.Pun;
using Object = UnityEngine.Object;
using TMPro;
using UnityEngine.UI;

namespace GamblingMachine
{
    [HarmonyPatch(typeof(ShopManager), "ShopInitialize")]
    public  class ShopPatch
    {
        public int playerId;
        private static bool debug = GamblingMachine.debug.Value;
        private static int bet = GamblingMachine.bet.Value;
        private static float multi = GamblingMachine.winMultiplicator.Value;
        static void Postfix(ShopManager __instance)
        {
            if (GamblingMachine.SlotMachinePrefab == null)
            {
                if (debug)
                    GamblingMachine.Logger.LogWarning("SlotMachinePrefab is null !");
                return;
            }

            GameObject target = GameObject.Find("Shop Magazine Holder");
            if (target == null)
            {
                if (debug)
                    GamblingMachine.Logger.LogWarning("Obj named 'Shop Magazine Holder' not found !");
                return;
            }

            Transform parent = target.transform.parent;
            Vector3 pos = target.transform.position;
            Quaternion rot = target.transform.rotation * Quaternion.Euler(0, 180f, 0);

            GameObject machine = null;

            if (!SemiFunc.IsMultiplayer() || SemiFunc.IsMasterClient())
            {
                if (SemiFunc.IsMultiplayer())
                {
                    machine = PhotonNetwork.Instantiate(GamblingMachine.SlotMachinePrefab.name, pos, rot);
                    SetParent(parent, machine);
                    machine.transform.localScale = Vector3.one * 16f; 
                }
                else
                {
                    machine = Object.Instantiate(GamblingMachine.SlotMachinePrefab, pos, rot, parent);
                }
                machine.tag = "Phys Grab Object";
                machine.layer = LayerMask.NameToLayer("PhysGrabObject");

                var colliderHolderTransform = machine.transform.FindDeepChild("ColliderHolder");
                if (colliderHolderTransform == null)
                {
                    GamblingMachine.Logger.LogWarning("ColliderHolder not found !");
                    return;
                }
                GameObject colliderHolder = colliderHolderTransform.gameObject;

                BoxCollider boxCollider = colliderHolder.GetComponent<BoxCollider>();

                // Scripts
                GamblingMachineScript slotScript = machine.GetComponent<GamblingMachineScript>() ?? machine.AddComponent<GamblingMachineScript>();
                StaticGrabObject staticGrabObject = machine.GetComponent<StaticGrabObject>() ?? machine.AddComponent<StaticGrabObject>();
                staticGrabObject.colliderTransform = colliderHolder.transform;

                PhysGrabObjectGrabArea grabArea = machine.GetComponent<PhysGrabObjectGrabArea>() ?? machine.AddComponent<PhysGrabObjectGrabArea>();
                PhysGrabObjectGrabArea.GrabArea newGrabArea = new PhysGrabObjectGrabArea.GrabArea();
                newGrabArea.grabAreaTransform = colliderHolder.transform;

                
                if (slotScript != null)
                {
                    GameObject[] reels = new GameObject[4];
                    reels[0] = machine.transform.FindDeepChild("reel 1").gameObject;
                    reels[1] = machine.transform.FindDeepChild("reel 2").gameObject;
                    reels[2] = machine.transform.FindDeepChild("reel 3").gameObject;
                    reels[3] = machine.transform.FindDeepChild("reel 4").gameObject;
                    slotScript.reels = reels;

                    newGrabArea.grabAreaEventOnStart.AddListener(slotScript.Spin);
                    grabArea.grabAreas.Add(newGrabArea);
                }
                else
                {
                    GamblingMachine.Logger.LogWarning("GamblingMachineScript script not found !");
                }

                GameObject BetAmountText = machine.transform.FindDeepChild("TMPTextBet").gameObject;
                GameObject BetMultiplicatorText = machine.transform.FindDeepChild("TMPTextMulti").gameObject;

                TextMeshPro tmp1 = BetAmountText.GetComponent<TextMeshPro>();
                TextMeshPro tmp2 = BetMultiplicatorText.GetComponent<TextMeshPro>();

                if (tmp1 != null) tmp1.text = $"Bet amount: {bet}k";
                if (tmp2 != null) tmp2.text = $"Bet multiplicator: x{multi:0.##}";

                Object.Destroy(target);
                GamblingMachine.Logger.LogInfo("GamblingMachine placed in shop !");
            }
            else
            {
                target.SetActive(false);
                if (debug)
                    GamblingMachine.Logger.LogInfo("Client non-host : objet désactivé.");
            }
        }

        static void SetParent(Transform parent, GameObject go)
        {
            go.transform.SetParent(parent);
            go.transform.localScale = Vector3.one;
        }
    }
}
