using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.IO;
using System.Reflection;
using Photon.Pun;


namespace GamblingMachine;
public class GamblingMachineScript : MonoBehaviour
{
    public GameObject[] reels;

    public float spinTimePerReel = 1.5f;
    public float rotationSpeed = 720f;

    private bool isSpinning = false;

    // gameplay
    private int prixMachine = GamblingMachine.bet.Value;
    private float winrate = GamblingMachine.winrate.Value;

    private float winMulti = GamblingMachine.winMultiplicator.Value;
    private bool debug = GamblingMachine.debug.Value;

    private Light lightComponent;
    
    private AudioClip spinClip;
    private AudioClip jackpotClip;
    private AudioClip looseClip;
    private AudioClip cancelClip;

    private string modPath;
    private AudioSource audioSource;
    private int playerId;
    public GameObject machine;

    void Start()
    {
        modPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f; 
        audioSource.volume = 0.5f; 

        StartCoroutine(LoadAudioClips());

        GameObject lightObj = GameObject.Find("Light");
        if (lightObj != null)
        {
            lightComponent = lightObj.GetComponent<Light>();
        }
        else
        {
            GamblingMachine.Logger.LogWarning("SlotMachineLight not found!"); 
        }

    }

    private IEnumerator LoadAudioClips()
    {
        yield return StartCoroutine(LoadClip("spin.mp3", clip => spinClip = clip));
        yield return StartCoroutine(LoadClip("win.mp3", clip => jackpotClip = clip));
        yield return StartCoroutine(LoadClip("loose.mp3", clip => looseClip = clip));
        yield return StartCoroutine(LoadClip("cancel.mp3", clip => cancelClip = clip));
    }

    private IEnumerator LoadClip(string relativePath, System.Action<AudioClip> onLoaded)
    {
        string path = "file://" + Path.Combine(modPath, relativePath);
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error loading audio: " + www.error);
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                onLoaded?.Invoke(clip);
                if (debug)
                    Debug.Log("Loaded clip from: " + path);
            }
        }
    }

    public void Spin()
    {
        if (SemiFunc.StatGetRunCurrency() >= prixMachine && !isSpinning)
        {
            PlaySound(spinClip);
            SemiFunc.StatSetRunCurrency(SemiFunc.StatGetRunCurrency() - prixMachine);
            StartCoroutine(SpinRoutine());
        }
        else
        {
            PlaySound(cancelClip);
            StartCoroutine(NotEnoughMoney());
            GamblingMachine.Logger.LogWarning("Not enough money!");
            return;
        }
    }

    private IEnumerator NotEnoughMoney()
    {
        if (lightComponent != null)
        {
            lightComponent.color = Color.red;
            yield return new WaitForSeconds(0.5f);
            lightComponent.color = Color.white; 
        } 
        else 
        {
            GamblingMachine.Logger.LogWarning("lightComponent not found!"); 
        }
    }

    public void SetPlayerId(int id)
    {
        playerId = id;
    }

    public int GetPlayerId()
    {
        return playerId;
    }

    private IEnumerator SpinRoutine()
    {
        isSpinning = true;

        bool isJackpot = Random.value < winrate; 

        float finalXRotation = isJackpot ? Random.Range(0f, 360f) : 0f;

        for (int i = 0; i < reels.Length; i++)
        {
            float targetRotation = isJackpot
                ? finalXRotation
                : Random.Range(0f, 360f); 

            StartCoroutine(RotateReel(reels[i], spinTimePerReel, targetRotation));
            yield return new WaitForSeconds(0.5f);
        }

        yield return new WaitForSeconds(spinTimePerReel + 0.5f);
        isSpinning = false;

        if (isJackpot)
        {
            PlaySound(jackpotClip);
            SemiFunc.StatSetRunCurrency(SemiFunc.StatGetRunCurrency() + Mathf.RoundToInt(prixMachine * winMulti));
        }
        else
        {
            PlaySound(looseClip);
            GamblingMachine.Logger.LogWarning("BIG L");
        }
    }
    private IEnumerator RotateReel(GameObject reel, float duration, float endRotation)
    {
        reel.transform.localEulerAngles = new Vector3(0f, 0f, 0f);

        float timer = 0f;

        while (timer < duration)
        {
            reel.transform.Rotate(Vector3.right * rotationSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        Vector3 currentRotation = reel.transform.localEulerAngles;
        reel.transform.localEulerAngles = new Vector3(endRotation, currentRotation.y, currentRotation.z);
    }
    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop(); 
            }

            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}
