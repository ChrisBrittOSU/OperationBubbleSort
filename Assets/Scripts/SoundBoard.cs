using UnityEngine;
using System.Collections;

public class SoundBoard : MonoBehaviour {
    public AudioSource board;
    public AudioClip bounce, pop, button, victory, walk;
	
    public void playBounce()
    {
        board.clip = bounce;
        board.Play();
    }

    public void playPop()
    {
        board.clip = pop;
        board.Play();
    }

    public void playButton()
    {
        board.clip = button;
        board.Play();
    }
    
    public void playVictory()
    {
        board.clip = victory;
        board.Play();
    }

    public void playWalk()
    {
        board.clip = walk;
        board.Play();
    }
}
