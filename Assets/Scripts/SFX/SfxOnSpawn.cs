using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SfxOnSpawn : MonoBehaviour
{
    [SerializeField] private AudioClip[] clips;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.9f, 1.1f);

    private void Start()
    {
        if (clips == null || clips.Length == 0) return;

        AudioSource source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.clip = clips[Random.Range(0, clips.Length)];
        source.volume = volume;
        source.pitch = Random.Range(pitchRange.x, pitchRange.y);
        source.Play();
    }
}