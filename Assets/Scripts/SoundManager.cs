using System;
using UnityEngine;
using UnityEngine.Audio;


[System.Serializable]
public class Sound {
    public string name;
    public AudioClip clipe;
    public bool loop;

    [Range(0f, 1f)]
    public float volume = 0.1f;

    public AudioSource ostSource;
    public AudioSource sfxSource;
}

public class SoundManager : MonoBehaviour
{
    public Sound[] SFX, OST;
    public static SoundManager instance;


    void Awake() {

        if (instance == null)
            instance = this;
        else {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        foreach (Sound s in SFX) {
            s.sfxSource = gameObject.AddComponent<AudioSource>();
            s.sfxSource.clip = s.clipe;
            s.sfxSource.loop = s.loop;
            s.sfxSource.volume = s.volume;
        }
    }

    public void PlaySFX(string name) {
        Sound s = Array.Find(SFX, sound => sound.name == name);
        if (s == null) {
            Debug.LogWarning("O som " + name + " não foi encontrado.");
            return;
        }
        s.sfxSource.PlayOneShot(s.sfxSource.clip);
    }

    /*
    public void PlayOST(string name) {
        Sound s = Array.Find(OST, sound => sound.name == name);
        if (s == null) {
            Debug.LogWarning("A música " + name + " não foi encontrado.");
            return;
        }
        s.ostSource.Play();
    }
    */

}
