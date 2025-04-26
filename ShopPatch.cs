using HarmonyLib;
using UnityEngine;
using Photon.Pun;
using Object = UnityEngine.Object;

namespace GamblingMachine
{
    [HarmonyPatch(typeof(ShopManager), "ShopInitialize")]
    public  class ShopPatch
    {
        public int playerId;
        private static bool debug = GamblingMachine.debug.Value;
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
                //machine.transform.localPosition = machine.transform.localPosition + new Vector3(-0.3f,0,0);
                machine.tag = "Phys Grab Object";
                machine.layer = LayerMask.NameToLayer("PhysGrabObject");

                // Collider 
                GameObject colliderHolder = GameObject.CreatePrimitive(PrimitiveType.Cube);
                colliderHolder.name = "ColliderHolder";
                colliderHolder.tag = "Grab Area";
                colliderHolder.layer = LayerMask.NameToLayer("Ignore Raycast");
                colliderHolder.transform.SetParent(machine.transform);
                colliderHolder.transform.localPosition = new Vector3(-0.00598148257f, 0.086336486f, 0.00997269899f);
                colliderHolder.transform.localScale = new Vector3(0.0783522129f, 0.172635332f, 0.101678953f);

                BoxCollider boxCollider = colliderHolder.GetComponent<BoxCollider>();
                boxCollider.center = Vector3.zero;
                boxCollider.size = Vector3.one;
                boxCollider.isTrigger = true;

                ApplyTransparentMaterial(colliderHolder);

                // Lights
                GameObject light = new GameObject("Light");
                light.transform.SetParent(machine.transform);
                light.transform.localPosition = new Vector3(0, 0.0738f, 0.0687f);
                Light lightComponent = light.AddComponent<Light>();
                lightComponent.color = Color.white;
                lightComponent.intensity = 1.5f;

                // Scripts
                GamblingMachineScript slotScript = machine.GetComponent<GamblingMachineScript>() ?? machine.AddComponent<GamblingMachineScript>();
                StaticGrabObject staticGrabObject = machine.GetComponent<StaticGrabObject>() ?? machine.AddComponent<StaticGrabObject>();
                staticGrabObject.colliderTransform = colliderHolder.transform;

                PhysGrabObjectGrabArea grabArea = machine.GetComponent<PhysGrabObjectGrabArea>() ?? machine.AddComponent<PhysGrabObjectGrabArea>();
                PhysGrabObjectGrabArea.GrabArea newGrabArea = new PhysGrabObjectGrabArea.GrabArea();
                newGrabArea.grabAreaTransform = colliderHolder.transform;

                
                if (staticGrabObject.playerGrabbing.Count > 0)
                {

                    int playerId = staticGrabObject.playerGrabbing[0].photonView.OwnerActorNr;

                    slotScript.SetPlayerId(playerId);
                }
                else
                {
                    if (debug)
                        GamblingMachine.Logger.LogWarning("No players are grabbing the object.");
                }

                if (slotScript != null)
                {
                    GameObject[] reels = new GameObject[4];
                    reels[0] = machine.transform.FindDeepChild("reel 1")?.gameObject;
                    reels[1] = machine.transform.FindDeepChild("reel 2")?.gameObject;
                    reels[2] = machine.transform.FindDeepChild("reel 3")?.gameObject;
                    reels[3] = machine.transform.FindDeepChild("reel 4")?.gameObject;
                    slotScript.reels = reels;
                }
                else
                {
                    GamblingMachine.Logger.LogWarning("GamblingMachineScript script not found !");
                }

                newGrabArea.grabAreaEventOnStart.AddListener(slotScript.Spin);
                grabArea.grabAreas.Add(newGrabArea);

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

        static void ApplyTransparentMaterial(GameObject obj)
        {
            Material transparentMat = new Material(Shader.Find("Standard"));
            transparentMat.color = new Color(0f, 0f, 0f, 0f);
            transparentMat.SetFloat("_Mode", 3);
            transparentMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            transparentMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            transparentMat.SetInt("_ZWrite", 0);
            transparentMat.DisableKeyword("_ALPHATEST_ON");
            transparentMat.EnableKeyword("_ALPHABLEND_ON");
            transparentMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            transparentMat.renderQueue = 3000;

            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
                renderer.material = transparentMat;
        }
    }
}
