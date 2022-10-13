using StixGames.GrassShader;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using Valve.VR;
using MEC;

public class MainMenuScript : MonoBehaviour
{
    [Space(10)]
    [Header("Menu must be tagged 'MainMenu'")]

    // Editor Fields
    [SerializeField, Tooltip("Hand used to aim and shoot")]
    private GameObject aimHand;
    [SerializeField]
    private GameObject sunLightObject;
    [SerializeField]
    private GameObject sunSetObject;
    [SerializeField]
    private GameObject playerPP;
    [SerializeField]
    private List<GameObject> grassObjects;
    [SerializeField, Tooltip("Starts from first, swaps to second on click.")]
    private List<PostProcessProfile> PostProcessProfiles;
    [SerializeField]
    private float grassTransition;    
    [SerializeField]
    private float sunSetTransition;
    [SerializeField, Tooltip("Determines how fast does the sunset move.")]
    private float sunSetMoveSpeed;
    [SerializeField, Tooltip("For ALL else because these values needed to finish transitioning at the same time.")]
    private float lerpSpeedForAllElse;
    [SerializeField, Tooltip("SkyboxBlended here.")]
    private Material skyBox;
    [SerializeField]
    private Material hellGrass;
    [SerializeField, Tooltip("We need this to smooth transition to hellgrass by changing color of this grass.")]
    private Material grass;
    [SerializeField]
    private Color smoothTransitionColor;


    // Non-editor fields
    private Component playerPostProcess;
    private string buttonClicked;
    private bool lerpUp = false;
    private bool canMoveSunset = false;
    private bool setElse;
    private Color basicGrassColor;
    


    private void Awake()
    {
        playerPostProcess = playerPP.GetComponent<PostProcessVolume>();

        // Setting values back to default <<<
        skyBox.SetFloat("_SkyBlend", 0f);
        skyBox.SetFloat("_Exposure", 0.5f);                
        basicGrassColor = ColorConverter.HexToColor("4EC04D");
        grass.SetColor("_Color00", basicGrassColor);
        setElse = false;
    }

    private void Update()
    {
        RaycastHit hit;

        // For debugging
        Debug.DrawRay(aimHand.transform.position, aimHand.transform.forward * 1000f, Color.magenta);

        // Raycast select for menu - buttons
        if (Physics.Raycast(aimHand.transform.position, aimHand.transform.forward, out hit))
        {
            if (hit.collider.transform.tag == "MenuButton")
            {
                var btn = hit.collider.transform.GetComponent<Button>();
                btn.Select();
                if (SteamVR_Actions.default_GrabPinch.GetStateDown(SteamVR_Input_Sources.RightHand))
                {
                    buttonClick(btn);
                }

            }
        }

        // When we click Start
        if (buttonClicked == "Start")
        {

            // SunSet. Below after 
            if(canMoveSunset)
            {                
                sunSetObject.transform.position += new Vector3(0,0, sunSetMoveSpeed);
                // Smooth transition to hell grass
                grass.SetColor("_Color00", Color.Lerp(grass.GetColor("_Color00"), smoothTransitionColor, grassTransition));
            }

            var ppv = playerPP.GetComponent<PostProcessVolume>();           

            if (!canMoveSunset)
            {
                                
                StartCoroutine(lerpAll());                                                
            }
            
            if(setElse)
            {
                ppv.profile = PostProcessProfiles[1];
                int i = 0;
                foreach(var grassChunk in grassObjects)
                {
                    grassObjects[i].GetComponent<GrassRenderer>().Material = hellGrass;
                    i++;
                }
                
            }

        }


    }

    // call the buttons assigned OnClick - method.
    private void buttonClick(Button btn)
    {
        Debug.Log(btn.transform.name);
        btn.onClick.Invoke();
    }


    // OnClick methods defined below
    public void Button0()
    {
        Debug.Log("Clicked the Start-button!" + buttonClicked);
        startClick();
    }

    public void Button1()
    {
        Debug.Log("Clicked the New Game - button.");
    }

    public void Button2()
    {
        Debug.Log("Clicked dunno - button.");
    }

    private void startClick()
    {        
        buttonClicked = "Start";
        Timing.RunCoroutine(moveSunSet());
    }

    IEnumerator<float> moveSunSet()
    {
        canMoveSunset = true;
        yield return Timing.WaitForSeconds(sunSetTransition);
        canMoveSunset = false;
    }

    IEnumerator lerpAll()
    {       
        for(float t = 0; t <= lerpSpeedForAllElse; t += Time.deltaTime)
        {
            skyBox.SetFloat("_SkyBlend", Mathf.Lerp(skyBox.GetFloat("_SkyBlend"), 1f, t / (lerpSpeedForAllElse * 1000)));
            skyBox.SetFloat("_Exposure", Mathf.Lerp(skyBox.GetFloat("_Exposure"), 1.5f, t / (lerpSpeedForAllElse * 1000)));
            sunLightObject.GetComponent<Light>().intensity = Mathf.Lerp(sunLightObject.GetComponent<Light>().intensity, 1.5f, t / (lerpSpeedForAllElse*1000));
            yield return null;
        }

        setElse = true;
    }
}
