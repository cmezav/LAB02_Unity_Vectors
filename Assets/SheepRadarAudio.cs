using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SheepRadarAudio : MonoBehaviour
{
    [Header("Sonidos de ovejas")]
    public AudioClip[] sheepSounds;

    [Range(0f, 1f)]
    public float volume = 1.0f;

    private AudioSource audioSource;

    private FlockManager flockManager;
    private Move[] sheep;

    private bool[] wasInsideRadar;

    private int nextSoundIndex = 0;

    private Queue<AudioClip> soundQueue =
        new Queue<AudioClip>();

    private FlockManager.HerdMode lastMode;

    void Start()
    {
        audioSource =
            GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;

        flockManager =
            FindFirstObjectByType<FlockManager>();

        sheep =
            FindObjectsByType<Move>(
                FindObjectsSortMode.None
            );

        Array.Sort(
            sheep,
            (a, b) => string.Compare(
                a.name,
                b.name,
                StringComparison.Ordinal
            )
        );

        wasInsideRadar =
            new bool[sheep.Length];

        if (flockManager != null)
        {
            lastMode =
                flockManager.currentMode;

            SyncRadarState();
        }
    }

    void Update()
    {
        if (flockManager == null)
            return;

        if (
            flockManager.currentMode !=
            lastMode
        )
        {
            lastMode =
                flockManager.currentMode;

            SyncRadarState();

            if (
                lastMode ==
                FlockManager.HerdMode.Wandering
            )
            {
                soundQueue.Clear();
                audioSource.Stop();
            }
        }

        if (
            flockManager.currentMode ==
            FlockManager.HerdMode.Seeking
        )
        {
            DetectRadarEntries();
        }

        PlayQueuedSound();
    }

    void DetectRadarEntries()
    {
        for (
            int i = 0;
            i < sheep.Length;
            i++
        )
        {
            if (sheep[i] == null)
                continue;

            bool isInside =
                flockManager
                    .IsInsideAttractionRadius(
                        sheep[i].transform.position
                    );

            if (
                isInside &&
                !wasInsideRadar[i]
            )
            {
                QueueNextSheepSound();
            }

            wasInsideRadar[i] =
                isInside;
        }
    }

    void QueueNextSheepSound()
    {
        if (
            sheepSounds == null ||
            sheepSounds.Length == 0
        )
        {
            return;
        }

        AudioClip clip =
            sheepSounds[nextSoundIndex];

        nextSoundIndex++;

        if (
            nextSoundIndex >=
            sheepSounds.Length
        )
        {
            nextSoundIndex = 0;
        }

        if (clip != null)
        {
            soundQueue.Enqueue(clip);
        }
    }

    void PlayQueuedSound()
    {
        if (audioSource.isPlaying)
            return;

        if (soundQueue.Count == 0)
            return;

        AudioClip clip =
            soundQueue.Dequeue();

        audioSource.clip = clip;
        audioSource.volume = volume;

        audioSource.Play();
    }

    void SyncRadarState()
    {
        if (
            wasInsideRadar == null ||
            flockManager == null
        )
        {
            return;
        }

        for (
            int i = 0;
            i < sheep.Length;
            i++
        )
        {
            if (sheep[i] == null)
                continue;

            wasInsideRadar[i] =
                flockManager
                    .IsInsideAttractionRadius(
                        sheep[i].transform.position
                    );
        }
    }
}