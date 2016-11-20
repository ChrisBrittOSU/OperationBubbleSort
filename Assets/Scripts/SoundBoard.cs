using UnityEngine;
using System.Collections;

public class SoundBoard : MonoBehaviour {
    public AudioSource bounce, pop, button, victory, walk;
	
    public void playBounce()
    {
        bounce.Play();
    }

    public void playPop()
    {
        pop.Play();
    }

    public void playButton()
    {
        button.Play();
    }
    
    public void playVictory()
    {
        victory.Play();
    }

    public void playWalk()
    {
        walk.Play();
    }
}
