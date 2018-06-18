using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Services.SpeechToText.v1;
using IBM.Watson.DeveloperCloud.Utilities;
using IBM.Watson.DeveloperCloud.DataTypes;
using System.Text;
using UnityEngine.UI;

public class TextDisplay : MonoBehaviour {

    [SerializeField]
    private Text display;

    private string displayString 
    {
        get { return display.text; }
        set { display.text = value; }
    }

    public void SpeechRecognized (SpeechRecognitionResult result) 
    {
        if (result.final && result.alternatives.Length > 0)
        {
            Log.Debug("TextDisplay.SpeechRecognized()", result.alternatives[0].transcript);
            displayString += " " + result.alternatives[0].transcript;
        }
    }

}
