using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AudioController : MonoBehaviour
{
    [SerializeField] private AudioSource bg_adudio;
    [SerializeField] private AudioSource audioPlayer_wl;
    [SerializeField] private AudioSource audioPlayer_button;
    [SerializeField] private AudioSource audioPlayer_Spin;
    [SerializeField] private AudioSource audioPlayer_shoot_effect;
    [SerializeField] private AudioSource audioPlayer_blast_effect;
    [SerializeField] private AudioSource audioPlayer_pull_effect;

    [Header("clips")]
    [SerializeField] private AudioClip SpinButtonClip;
    [SerializeField] private AudioClip SpinClip;
    [SerializeField] private AudioClip Button;
    [SerializeField] private AudioClip Win_Audio;
    [SerializeField] private AudioClip NormalBg_Audio;
    [SerializeField] private AudioClip FreeSpinBg_Audio;
    [SerializeField] private AudioClip LaserShoot_Audio;
    [SerializeField] private AudioClip Blast_Audio;
    [SerializeField] private AudioClip pull_Audio;

    private void Awake()
    {
        playBgAudio();
        audioPlayer_blast_effect.clip=Blast_Audio;
        audioPlayer_pull_effect.clip=pull_Audio;
        audioPlayer_shoot_effect.clip=LaserShoot_Audio;
        //if (bg_adudio) bg_adudio.Play();
        //audioPlayer_button.clip = clips[clips.Length - 1];
    }

    internal void PlayWLAudio( )
    {
        StopWLAaudio();
        // audioPlayer_wl.loop=loop;

        audioPlayer_wl.clip = Win_Audio;

        audioPlayer_wl.Play();

    }

    internal void PlaySpinAudio()
    {
            audioPlayer_Spin.clip = SpinClip;

            audioPlayer_Spin.Play();

    }

    internal void StopSpinAudio()
    {

        if (audioPlayer_Spin) audioPlayer_Spin.Stop();

    }

    private void OnApplicationFocus(bool focus)
    {
        if (!focus)
        {

            bg_adudio.Pause();
            audioPlayer_wl.Pause();
            audioPlayer_button.Pause();
            audioPlayer_Spin.Pause();
            audioPlayer_shoot_effect.Pause();
            audioPlayer_pull_effect.Pause();
        }
        else
        {
            bg_adudio.UnPause();
            audioPlayer_wl.UnPause();
            audioPlayer_button.UnPause();
            audioPlayer_Spin.UnPause();
            audioPlayer_shoot_effect.UnPause();
            audioPlayer_pull_effect.UnPause();


        }
    }



    internal void playBgAudio(string type = "default")
    {


        //int randomIndex = UnityEngine.Random.Range(0, Bg_Audio.Length);
        StopBgAudio();
        if (bg_adudio)
        {
            if (type == "bonus")
                bg_adudio.clip = FreeSpinBg_Audio;
            else
                bg_adudio.clip = NormalBg_Audio;


            bg_adudio.Play();
        }

    }

    internal void PlayButtonAudio(string type = "default")
    {

        if (type == "spin")
            audioPlayer_button.clip = SpinButtonClip;
        else
            audioPlayer_button.clip = Button;

        //StopButtonAudio();
        audioPlayer_button.Play();
        // Invoke("StopButtonAudio", audioPlayer_button.clip.length);

    }

    internal void StopWLAaudio()
    {
        audioPlayer_wl.Stop();
        audioPlayer_wl.loop = false;
    }

    internal void StopButtonAudio()
    {

        audioPlayer_button.Stop();

    }


    internal void StopBgAudio()
    {
        bg_adudio.Stop();

    }

    internal void PlayShootAudio(){

        audioPlayer_shoot_effect.Play();
    }

    internal void PlayBlastAudio(){

        audioPlayer_blast_effect.Play();
    }
    internal void PlayPullAudio(){
    audioPlayer_pull_effect.Play();

    }

    internal void ToggleMute(float value, string type = "all")
    {

        switch (type)
        {
            case "bg":
                    bg_adudio.mute = value <0.1;
                    bg_adudio.volume = value;
                break;
            case "button":
                audioPlayer_button.mute = value<0.1;
                audioPlayer_Spin.mute=value<0.1;

                audioPlayer_button.volume = value;
                audioPlayer_Spin.volume = value;
                break;
            case "wl":
                audioPlayer_wl.mute = value<0.1;
                audioPlayer_wl.volume = value;
                break;
            case "all":
                audioPlayer_wl.mute = value<0.1;
                bg_adudio.mute = value<0.1;
                audioPlayer_button.mute = value<0.1;
                
                audioPlayer_wl.volume = value;
                bg_adudio.volume = value;
                audioPlayer_button.volume = value;
                break;
        }
    }

}
