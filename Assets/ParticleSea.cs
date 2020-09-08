using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class ParticleSea : MonoBehaviour
{
    public ParticleSystem particleSystem;
    private ParticleSystem.Particle[] particles;
    public int seaResolution = 25;
    public float spacing = 0.25f;
    public float noiseScale = 0.2f; 
    private float heightScale = 3f;
    private float perlinNoiseAnimX = 0.01f; 
    private float perlinNoiseAnimY = 0.01f;
    public float angularFactor = 0.01f;
    public int AQI = 1;
    public Gradient colorGradient;
    public Text cityName;
    private string[] cities = {"Sheffield","London","Gaya","Tokyo","Madrid","Beijing","Delhi","Lisbon"};
    public int cityIndex = 0;
    private bool canChange = false;  
    private bool firstTime = true;
    public int multiplier = 5;
    // Start is called before the first frame update
    void Start()
    {
        // A correct website page.
        StartCoroutine(fetchAQI());
        particles = new ParticleSystem.Particle[seaResolution * seaResolution];
        particleSystem.maxParticles = seaResolution * seaResolution;
        particleSystem.Emit(seaResolution * seaResolution);
        particleSystem.GetParticles(particles);
       // InvokeRepeating("changeCity", 5.0f, 5f);

    }
    void changeCity () {
       // if(canChange){
        if(cityIndex == cities.Length -1){
            cityIndex = 0;
            
        } else{
            ++cityIndex;
        }
        //}
    }
    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < seaResolution; i++) { 
            for(int j = 0; j < seaResolution; j++) { 
                float zPos = Mathf.PerlinNoise(i * noiseScale + perlinNoiseAnimX, j * noiseScale + perlinNoiseAnimY) * heightScale;    
                Color color;
                if(AQI <= 50){
                    color = new Color(0,153/255.0f,102/255.0f,1);
                } else if( AQI <= 100){
                    color = new Color(255.0f/255.0f,222/255.0f,51/255.0f,1);
                } else if(AQI <= 150){
                    color = new Color(255.0f/255.0f,153/255.0f,51/255.0f,1);
                } else if(AQI <= 200){
                    color = new Color(204/255.0f,0/255.0f,51/255.0f,1);
                } else if(AQI <= 300){
                    color = new Color(102/255.0f,0/255.0f,153/255.0f,1);
                } else{
                    color = new Color(126/255.0f,0/255.0f,35/255.0f,1);
                }
                heightScale = AQI / multiplier;
                particles[i * seaResolution + j].color = color;    
                particles[i * seaResolution + j].position = new Vector3(i * spacing, zPos, j * spacing); 
            } 
        } 
        perlinNoiseAnimX += 0.01f; perlinNoiseAnimY += 0.01f;     
        particleSystem.SetParticles(particles, particles.Length); 
    }

    IEnumerator fetchAQI(){
        float lastTime = Time.time;
        while(true){
            if(Time.time - lastTime >= 5 || firstTime){
                firstTime = false;
                //canChange = false;
                lastTime = Time.time;
                Debug.Log("Fetching AQI for: " + cities[cityIndex]);
                UnityWebRequest www = UnityWebRequest.Get("http://api.waqi.info/feed/"+cities[cityIndex]+"/?token=3d23ae70180b539813cf9dbc45744037d482065d");
                yield return www.SendWebRequest();
        
                if(www.isNetworkError || www.isHttpError) {
                    Debug.Log(www.error);
                }
                else {
                    // Show results as text
                string jsonString =  www.downloadHandler.text;
                    var rx = new System.Text.RegularExpressions.Regex("\"aqi\":");
                    AQI = int.Parse(rx.Split(jsonString)[1].Split(',')[0]);
                    Debug.Log(AQI);
                    cityName.text = cities[cityIndex];
                    changeCity();
                    // Debug.Log(jsonString.Split(new[] { "\"aqi\":" }));
                
                  //  canChange= true;
                }
            }
            else{
                yield return new WaitForSeconds(5f);
            }
        }
    }
}

