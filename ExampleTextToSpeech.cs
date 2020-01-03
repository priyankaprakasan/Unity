using System.Collections;
using UnityEngine;
using IBM.Watson.TextToSpeech.V1;

using IBM.Cloud.SDK;
using IBM.Cloud.SDK.Utilities;
using IBM.Cloud.SDK.Authentication;

using IBM.Watson.Assistant.V2;
using IBM.Watson.Assistant.V2.Model;

using IBM.Cloud.SDK.DataTypes;
using System.Collections.Generic;
using System.Text;
using IBM.Cloud.SDK.Connection;
using IBM.Watson.TextToSpeech.V1.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine.Networking;
using IBM.Cloud.SDK.Authentication.Iam;

public class ExampleTextToSpeech : MonoBehaviour
{

    #region PLEASE SET THESE VARIABLES IN THE INSPECTOR
       [Header("Text to Speech")]
       [SerializeField]
       [Tooltip("The service URL (optional). This defaults to \"https://stream.watsonplatform.net/text-to-speech/api\"")]
       private string TextToSpeechURL;
       [Header("IAM Authentication")]
       [Tooltip("The IAM apikey.")]
       [SerializeField]
      private string _iamApikey;
    [Tooltip("The IAM url used to authenticate the apikey (optional). This defaults to \"https://iam.bluemix.net/identity/token\".")]
       [SerializeField]
       private string TextToSpeechIamUrl;
       #endregion
  
   
   

    private TextToSpeechService textToSpeech;


    void Start()
    {
        LogSystem.InstallDefaultReactors();

        Runnable.Run(CreateService());
    }
    private IEnumerator CreateService()
    {
        if (string.IsNullOrEmpty(_iamApikey))
        {
            throw new IBMException("Plesae provide IAM ApiKey for the service.");
        }

        IamAuthenticator authenticator = new IamAuthenticator(apikey: _iamApikey);

        //  Wait for tokendata
        while (!authenticator.CanAuthenticate())
            yield return null;

        textToSpeech = new TextToSpeechService(authenticator);
        textToSpeech.SetServiceUrl(TextToSpeechURL);
       // textToSpeech.StreamMultipart = true;

      //  Active = true;
        Runnable.Run(CallTextToSpeech("Select Mangoe!"));
    }
    public IEnumerator CallTextToSpeech(string outputText)
    {
        byte[] synthesizeResponse = null;
        AudioClip clip = null;
        textToSpeech.Synthesize(
            callback: (DetailedResponse<byte[]> response, IBMError error) =>
            {
                synthesizeResponse = response.Result;
                clip = WaveFile.ParseWAV("myClip", synthesizeResponse);
                PlayClip(clip);

            },
            text: outputText,
            voice: "en-US_AllisonVoice",
            accept: "audio/wav"
        );

        while (synthesizeResponse == null)
            yield return null;

        yield return new WaitForSeconds(clip.length);
    }

    private void PlayClip(AudioClip clip)
    {
        if (Application.isPlaying && clip != null)
        {
            GameObject audioObject = new GameObject("AudioObject");
            AudioSource source = audioObject.AddComponent<AudioSource>();
            source.spatialBlend = 0.0f;
            source.loop = false;
            source.clip = clip;
            source.Play();

            GameObject.Destroy(audioObject, clip.length);
        }
    }

}