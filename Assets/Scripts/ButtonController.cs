using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonController : MonoBehaviour {

    // Canvas group references
    public CanvasGroup canvas, menuButtons, title;

    // Generator reference
    public Generator generator;

    // Start button function upon pressed
	public void StartPress()
    {
        generator.Generate();
    }

    // Exit button functio upon pressed
    public void ExitPress()
    {
        Application.Quit();
    }
}
