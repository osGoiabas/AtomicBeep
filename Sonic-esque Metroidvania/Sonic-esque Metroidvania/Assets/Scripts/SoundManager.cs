using System;
using UnityEngine;
using UnityEngine.Audio;


[System.Serializable]
public class Sound {
    public string name;
    public AudioClip clipe;
    public bool loop;

    [Range(0f, 1f)]
    public float volume;

    [HideInInspector]
    public AudioSource source;
}

public class SoundManager : MonoBehaviour
{
    public Sound[] sounds;
    public static SoundManager instance;

    void Awake() {

        if (instance == null)
            instance = this;
        else {
            Destroy(gameObject);
            return;
        }

        //DontDestroyOnLoad(gameObject);

        foreach (Sound s in sounds) {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clipe;
            s.source.loop = s.loop;
            s.source.volume = s.volume;
        }
    }

    public void Play (string name) {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null) {
            Debug.LogWarning("O som " + name + " não foi encontrado.");
            return;
        }
        s.source.Play();
    }
}
