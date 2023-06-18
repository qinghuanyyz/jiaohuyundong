using Baidu.Aip.BodyAnalysis;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System.Collections;
using static UnityEngine.GraphicsBuffer;
using System.IO;
using UnityEngine.SceneManagement;
using TMPro;

public class posecamera : MonoBehaviour
{
    public TextMeshProUGUI text;

    private WebCamTexture webcamTexture;
    public RawImage show;
    public RawImage show_sample;
    private Texture2D texture2D;
    private Texture2D sample;

    const string APP_ID = "33985236";
    const string API_KEY = "jmqIVfs829AmACb2y0tWYx3L";
    const string SECRET_KEY = "ywapkHm6i4GsjNLbZyG8ZeFG7bzIrj9C";

    private Body client;

    private Dictionary<string, targetInfo.BodyPart> targetDict;
    private Dictionary<string, sampleInfo.BodyPart> sampleDict;

    private targetInfo target_points = new targetInfo();
    private sampleInfo sample_points = new sampleInfo();

    private Texture2D[] images;
    private int images_count = 0;
    private int images_count_max;

    public class targetInfo
    {
        public class BodyPart
        {
            public float x = 0;
            public float y = 0;
            public bool isactive = false;
        }
        public class BodyParts
        {
            //public BodyPart nose = new BodyPart();
            //public BodyPart left_eye = new BodyPart();
            //public BodyPart right_eye = new BodyPart();
            public BodyPart left_ear = new BodyPart();
            public BodyPart right_ear = new BodyPart();
            public BodyPart left_shoulder = new BodyPart();
            public BodyPart right_shoulder = new BodyPart();
            public BodyPart left_elbow = new BodyPart();
            public BodyPart right_elbow = new BodyPart();
            public BodyPart left_wrist = new BodyPart();
            public BodyPart right_wrist = new BodyPart();
            public BodyPart left_hip = new BodyPart();   
            public BodyPart right_hip = new BodyPart();
            public BodyPart left_knee = new BodyPart();
            public BodyPart right_knee = new BodyPart();
            public BodyPart left_ankle = new BodyPart();
            public BodyPart right_ankle = new BodyPart();
            public BodyPart left_mouth_corner = new BodyPart();
            public BodyPart right_mouth_corner = new BodyPart();
            public BodyPart neck = new BodyPart();
            public BodyPart top_head = new BodyPart();
        }
        public BodyParts body_parts = new BodyParts();
    }
    public class sampleInfo
    {
        public class BodyPart
        {
            public float x = 0;
            public float y = 0;
            public bool isactive = false;
            public bool ismatched = false;
        }
        public class BodyParts
        {
           // public BodyPart nose;
           // public BodyPart left_eye;
           // public BodyPart right_eye;
            public BodyPart left_ear = new BodyPart();
            public BodyPart right_ear = new BodyPart();
            public BodyPart left_shoulder = new BodyPart();
            public BodyPart right_shoulder = new BodyPart();
            public BodyPart left_elbow = new BodyPart();
            public BodyPart right_elbow = new BodyPart();
            public BodyPart left_wrist = new BodyPart();
            public BodyPart right_wrist = new BodyPart();
            public BodyPart left_hip = new BodyPart();
            public BodyPart right_hip = new BodyPart();
            public BodyPart left_knee = new BodyPart();
            public BodyPart right_knee = new BodyPart();
            public BodyPart left_ankle = new BodyPart();
            public BodyPart right_ankle = new BodyPart();
            public BodyPart left_mouth_corner = new BodyPart();
            public BodyPart right_mouth_corner = new BodyPart();
            public BodyPart neck = new BodyPart();
            public BodyPart top_head = new BodyPart();
        }
        public BodyParts body_parts = new BodyParts();
    }
    public class PersonInfo
    {
        public class BodyPart
        {
            public float x;
            public float y;
            public float score;
        }

        public class Location
        {
            public float height;
            public float width;
            public float top;
            public float left;
            public float score;
        }
        public class BodyParts
        {
            public BodyPart nose = new BodyPart();
            public BodyPart left_eye = new BodyPart();
            public BodyPart right_eye = new BodyPart();
            public BodyPart left_ear = new BodyPart();
            public BodyPart right_ear = new BodyPart();
            public BodyPart left_shoulder = new BodyPart();
            public BodyPart right_shoulder = new BodyPart();
            public BodyPart left_elbow = new BodyPart();
            public BodyPart right_elbow = new BodyPart();
            public BodyPart left_wrist = new BodyPart();
            public BodyPart right_wrist = new BodyPart();
            public BodyPart left_hip = new BodyPart();
            public BodyPart right_hip = new BodyPart();
            public BodyPart left_knee = new BodyPart();
            public BodyPart right_knee = new BodyPart();
            public BodyPart left_ankle = new BodyPart();
            public BodyPart right_ankle = new BodyPart();
            public BodyPart left_mouth_corner = new BodyPart();
            public BodyPart right_mouth_corner = new BodyPart();
            public BodyPart neck = new BodyPart();
            public BodyPart top_head = new BodyPart();
        }

        public BodyParts body_parts = new BodyParts();
        public Location location = new Location();
    }
    public class ApiResponse
    {
        public int person_num;
        public PersonInfo[] person_info;
        public string log_id;

        public ApiResponse()
        {
            person_info = new PersonInfo[0];
        }
    }

    void Start()
    {
        webcamTexture = new WebCamTexture();
        webcamTexture.Play();

        images_count_max = setimges()-1;

        texture2D = new Texture2D(webcamTexture.width, webcamTexture.height, TextureFormat.RGB24, false);
        show.texture = texture2D;

        client = new Body(API_KEY, SECRET_KEY);
        client.Timeout = 60000;

        SettargeDict();
        Setsample();
       
        StartCoroutine(ExampleCoroutine());
    }
    void Update()
    {
        texture2D.SetPixels(webcamTexture.GetPixels());

        sample_target_match();

        foreach (var Dict in targetDict)
        {
            DrawCircle_target(new Vector2(Dict.Value.x, Dict.Value.y), Dict.Value.isactive, 10f, Color.blue);
        }
        foreach (var Dict in sampleDict)
        {
            DrawCircle_sample(new Vector2(Dict.Value.x, Dict.Value.y), Dict.Value.isactive, Dict.Value.ismatched, 15f, Dict.Value.ismatched ? Color.green : Color.red);
        }
        Drawtargetlines();
        Drawsamplelines();
        texture2D.Apply();

        text.text = images_count.ToString() + "/" + (images_count_max+1).ToString();
        wincheck();

       
    }
    IEnumerator ExampleCoroutine()
    {
        while (true)
        {
            if (texture2D != null)
                BodyAnalysis(texture2D.EncodeToJPG());
            yield return new WaitForSeconds(1f / 30f);
        }
    }
    public void BodyAnalysis(byte[] target)
    {
        JObject result = client.BodyAnalysis(target);
        
        string jsonText = result.ToString();

        if (string.IsNullOrEmpty(jsonText))
        {
            return;
        }

        ApiResponse response = JsonConvert.DeserializeObject<ApiResponse>(jsonText);

        foreach (PersonInfo person in response.person_info)
        {
            foreach (FieldInfo part in person.body_parts.GetType().GetFields())
            {
                PersonInfo.BodyPart bodyPart = (PersonInfo.BodyPart)part.GetValue(person.body_parts);
                string partName = part.Name;

                if (targetDict.ContainsKey(partName) && bodyPart.score >= 0.6)
                {
                    targetDict[partName].isactive = true;
                    targetDict[partName].x = bodyPart.x;
                    targetDict[partName].y = -bodyPart.y;
                }
                else if(targetDict.ContainsKey(partName) && bodyPart.score < 0.6)
                    targetDict[partName].isactive = false;
            }
            break;
        }


    }
    private void DrawCircle_target(Vector2 center,bool isactive, float radius, Color color)
    {
        if (!isactive)
            return;

        int x, y, px, nx, py, ny, d;

        for (x = 0; x <= radius; x++)
        {
            d = (int)Mathf.Round(Mathf.Sqrt(radius * radius - x * x));

            for (y = 0; y <= d; y++)
            {
                px = (int)center.x + x;
                nx = (int)center.x - x;
                py = (int)center.y + y;
                ny = (int)center.y - y;

                texture2D.SetPixel(px, py, color);
                texture2D.SetPixel(nx, py, color);
                texture2D.SetPixel(px, ny, color);
                texture2D.SetPixel(nx, ny, color);
            }
        }
    }
    private void DrawCircle_sample(Vector2 center, bool isactive, bool ismatch, float radius, Color color)
    {
        if (!isactive)
            return;

        int x, y, px, nx, py, ny, d;

        for (x = 0; x <= radius; x++)
        {
            d = (int)Mathf.Round(Mathf.Sqrt(radius * radius - x * x));

            for (y = 0; y <= d; y++)
            {
                px = (int)center.x + x;
                nx = (int)center.x - x;
                py = (int)center.y + y;
                ny = (int)center.y - y;

                texture2D.SetPixel(px, py, color);
                texture2D.SetPixel(nx, py, color);
                texture2D.SetPixel(px, ny, color);
                texture2D.SetPixel(nx, ny, color);
            }
        }
    }
    private void Drawtargetlines()
    {
        DrawLine(targetDict["top_head"].isactive, targetDict["left_ear"].isactive, new Vector2(targetDict["top_head"].x, -targetDict["top_head"].y), new Vector2(targetDict["left_ear"].x, -targetDict["left_ear"].y), Color.blue);
        DrawLine(targetDict["top_head"].isactive, targetDict["right_ear"].isactive, new Vector2(targetDict["top_head"].x, -targetDict["top_head"].y), new Vector2(targetDict["right_ear"].x, -targetDict["right_ear"].y), Color.blue);
        DrawLine(targetDict["left_ear"].isactive, targetDict["neck"].isactive, new Vector2(targetDict["left_ear"].x, -targetDict["left_ear"].y), new Vector2(targetDict["neck"].x, -targetDict["neck"].y), Color.blue);
        DrawLine(targetDict["right_ear"].isactive, targetDict["neck"].isactive, new Vector2(targetDict["right_ear"].x, -targetDict["right_ear"].y), new Vector2(targetDict["neck"].x, -targetDict["neck"].y), Color.blue);
        DrawLine(targetDict["neck"].isactive, targetDict["left_shoulder"].isactive, new Vector2(targetDict["neck"].x, -targetDict["neck"].y), new Vector2(targetDict["left_shoulder"].x, -targetDict["left_shoulder"].y), Color.blue);
        DrawLine(targetDict["neck"].isactive, targetDict["right_shoulder"].isactive, new Vector2(targetDict["neck"].x, -targetDict["neck"].y), new Vector2(targetDict["right_shoulder"].x, -targetDict["right_shoulder"].y), Color.blue);
        DrawLine(targetDict["left_shoulder"].isactive, targetDict["left_elbow"].isactive, new Vector2(targetDict["left_shoulder"].x, -targetDict["left_shoulder"].y), new Vector2(targetDict["left_elbow"].x, -targetDict["left_elbow"].y), Color.blue);
        DrawLine(targetDict["right_shoulder"].isactive, targetDict["right_elbow"].isactive, new Vector2(targetDict["right_shoulder"].x, -targetDict["right_shoulder"].y), new Vector2(targetDict["right_elbow"].x, -targetDict["right_elbow"].y), Color.blue);
        DrawLine(targetDict["left_elbow"].isactive, targetDict["left_wrist"].isactive, new Vector2(targetDict["left_elbow"].x, -targetDict["left_elbow"].y), new Vector2(targetDict["left_wrist"].x, -targetDict["left_wrist"].y), Color.blue);
        DrawLine(targetDict["right_elbow"].isactive, targetDict["right_wrist"].isactive, new Vector2(targetDict["right_elbow"].x, -targetDict["right_elbow"].y), new Vector2(targetDict["right_wrist"].x, -targetDict["right_wrist"].y), Color.blue);
        DrawLine(targetDict["left_shoulder"].isactive, targetDict["left_hip"].isactive, new Vector2(targetDict["left_shoulder"].x, -targetDict["left_shoulder"].y), new Vector2(targetDict["left_hip"].x, -targetDict["left_hip"].y), Color.blue);
        DrawLine(targetDict["right_shoulder"].isactive, targetDict["right_hip"].isactive, new Vector2(targetDict["right_shoulder"].x, -targetDict["right_shoulder"].y), new Vector2(targetDict["right_hip"].x, -targetDict["right_hip"].y), Color.blue);
        DrawLine(targetDict["left_hip"].isactive, targetDict["left_knee"].isactive, new Vector2(targetDict["left_hip"].x, -targetDict["left_hip"].y), new Vector2(targetDict["left_knee"].x, -targetDict["left_knee"].y), Color.blue);
        DrawLine(targetDict["right_hip"].isactive, targetDict["right_knee"].isactive, new Vector2(targetDict["right_hip"].x, -targetDict["right_hip"].y), new Vector2(targetDict["right_knee"].x, -targetDict["right_knee"].y), Color.blue);
        DrawLine(targetDict["left_knee"].isactive, targetDict["left_ankle"].isactive, new Vector2(targetDict["left_knee"].x, -targetDict["left_knee"].y), new Vector2(targetDict["left_ankle"].x, -targetDict["left_ankle"].y), Color.blue);
        DrawLine(targetDict["right_knee"].isactive, targetDict["right_ankle"].isactive, new Vector2(targetDict["right_knee"].x, -targetDict["right_knee"].y), new Vector2(targetDict["right_ankle"].x, -targetDict["right_ankle"].y), Color.blue);
        DrawLine(targetDict["left_shoulder"].isactive, targetDict["right_shoulder"].isactive, new Vector2(targetDict["left_shoulder"].x, -targetDict["left_shoulder"].y), new Vector2(targetDict["right_shoulder"].x, -targetDict["right_shoulder"].y), Color.blue);
        DrawLine(targetDict["left_hip"].isactive, targetDict["right_hip"].isactive, new Vector2(targetDict["left_hip"].x, -targetDict["left_hip"].y), new Vector2(targetDict["right_hip"].x, -targetDict["right_hip"].y), Color.blue);
    }
    private void Drawsamplelines()
    {
        DrawLine(sampleDict["top_head"].isactive, sampleDict["left_ear"].isactive, new Vector2(sampleDict["top_head"].x, -sampleDict["top_head"].y), new Vector2(sampleDict["left_ear"].x, -sampleDict["left_ear"].y), sampleDict["top_head"].ismatched ? Color.green : Color.red);
        DrawLine(sampleDict["top_head"].isactive, sampleDict["right_ear"].isactive, new Vector2(sampleDict["top_head"].x, -sampleDict["top_head"].y), new Vector2(sampleDict["right_ear"].x, -sampleDict["right_ear"].y), sampleDict["top_head"].ismatched ? Color.green : Color.red);
        DrawLine(sampleDict["left_ear"].isactive, sampleDict["neck"].isactive, new Vector2(sampleDict["left_ear"].x, -sampleDict["left_ear"].y), new Vector2(sampleDict["neck"].x, -sampleDict["neck"].y), sampleDict["left_ear"].ismatched ? Color.green : Color.red);
        DrawLine(sampleDict["right_ear"].isactive, sampleDict["neck"].isactive, new Vector2(sampleDict["right_ear"].x, -sampleDict["right_ear"].y), new Vector2(sampleDict["neck"].x, -sampleDict["neck"].y), sampleDict["right_ear"].ismatched ? Color.green : Color.red);
        DrawLine(sampleDict["neck"].isactive, sampleDict["left_shoulder"].isactive, new Vector2(sampleDict["neck"].x, -sampleDict["neck"].y), new Vector2(sampleDict["left_shoulder"].x, -sampleDict["left_shoulder"].y), sampleDict["neck"].ismatched ? Color.green : Color.red);
        DrawLine(sampleDict["neck"].isactive, sampleDict["right_shoulder"].isactive, new Vector2(sampleDict["neck"].x, -sampleDict["neck"].y), new Vector2(sampleDict["right_shoulder"].x, -sampleDict["right_shoulder"].y), sampleDict["neck"].ismatched ? Color.green : Color.red);
        DrawLine(sampleDict["left_shoulder"].isactive, sampleDict["left_elbow"].isactive, new Vector2(sampleDict["left_shoulder"].x, -sampleDict["left_shoulder"].y), new Vector2(sampleDict["left_elbow"].x, -sampleDict["left_elbow"].y), sampleDict["left_shoulder"].ismatched ? Color.green : Color.red);
        DrawLine(sampleDict["right_shoulder"].isactive, sampleDict["right_elbow"].isactive, new Vector2(sampleDict["right_shoulder"].x, -sampleDict["right_shoulder"].y), new Vector2(sampleDict["right_elbow"].x, -sampleDict["right_elbow"].y), sampleDict["right_shoulder"].ismatched ? Color.green : Color.red);
        DrawLine(sampleDict["left_elbow"].isactive, sampleDict["left_wrist"].isactive, new Vector2(sampleDict["left_elbow"].x, -sampleDict["left_elbow"].y), new Vector2(sampleDict["left_wrist"].x, -sampleDict["left_wrist"].y), sampleDict["left_elbow"].ismatched ? Color.green : Color.red);
        DrawLine(sampleDict["right_elbow"].isactive, sampleDict["right_wrist"].isactive, new Vector2(sampleDict["right_elbow"].x, -sampleDict["right_elbow"].y), new Vector2(sampleDict["right_wrist"].x, -sampleDict["right_wrist"].y), sampleDict["right_elbow"].ismatched ? Color.green : Color.red);
        DrawLine(sampleDict["left_shoulder"].isactive, sampleDict["left_hip"].isactive, new Vector2(sampleDict["left_shoulder"].x, -sampleDict["left_shoulder"].y), new Vector2(sampleDict["left_hip"].x, -sampleDict["left_hip"].y), sampleDict["left_shoulder"].ismatched ? Color.green : Color.red);
        DrawLine(sampleDict["right_shoulder"].isactive, sampleDict["right_hip"].isactive, new Vector2(sampleDict["right_shoulder"].x, -sampleDict["right_shoulder"].y), new Vector2(sampleDict["right_hip"].x, -sampleDict["right_hip"].y), sampleDict["right_shoulder"].ismatched ? Color.green : Color.red);
        DrawLine(sampleDict["left_hip"].isactive, sampleDict["left_knee"].isactive, new Vector2(sampleDict["left_hip"].x, -sampleDict["left_hip"].y), new Vector2(sampleDict["left_knee"].x, -sampleDict["left_knee"].y), sampleDict["left_hip"].ismatched ? Color.green : Color.red);
        DrawLine(sampleDict["right_hip"].isactive, sampleDict["right_knee"].isactive, new Vector2(sampleDict["right_hip"].x, -sampleDict["right_hip"].y), new Vector2(sampleDict["right_knee"].x, -sampleDict["right_knee"].y), sampleDict["right_hip"].ismatched ? Color.green : Color.red);
        DrawLine(sampleDict["left_knee"].isactive, sampleDict["left_ankle"].isactive, new Vector2(sampleDict["left_knee"].x, -sampleDict["left_knee"].y), new Vector2(sampleDict["left_ankle"].x, -sampleDict["left_ankle"].y), sampleDict["left_knee"].ismatched ? Color.green : Color.red);
        DrawLine(sampleDict["right_knee"].isactive, sampleDict["right_ankle"].isactive, new Vector2(sampleDict["right_knee"].x, -sampleDict["right_knee"].y), new Vector2(sampleDict["right_ankle"].x, -sampleDict["right_ankle"].y), sampleDict["right_knee"].ismatched ? Color.green : Color.red);
        DrawLine(sampleDict["left_shoulder"].isactive, sampleDict["right_shoulder"].isactive, new Vector2(sampleDict["left_shoulder"].x, -sampleDict["left_shoulder"].y), new Vector2(sampleDict["right_shoulder"].x, -sampleDict["right_shoulder"].y), sampleDict["left_shoulder"].ismatched || sampleDict["right_shoulder"].ismatched ? Color.green : Color.red);
        DrawLine(sampleDict["left_hip"].isactive, sampleDict["right_hip"].isactive, new Vector2(sampleDict["left_hip"].x, -sampleDict["left_hip"].y), new Vector2(sampleDict["right_hip"].x, -sampleDict["right_hip"].y), sampleDict["left_hip"].ismatched || sampleDict["right_hip"].ismatched ? Color.green : Color.red);
    }
    private void DrawLine(bool start_isactive, bool end_isactive, Vector2 start, Vector2 end, Color color)
    {
        if (!start_isactive || !end_isactive) return;

        int width = webcamTexture.width;
        int height = webcamTexture.height;

        float dx = end.x - start.x;
        float dy = end.y - start.y;
        float slope = dy / dx;
        int thickness;

        if(color == Color.red || color == Color.green)
            thickness = 3;
        else
            thickness = 10;

        if (Mathf.Abs(dx) > Mathf.Abs(dy)) // 横向线条
        {
            if (start.x > end.x)
            {
                Vector2 temp = start;
                start = end;
                end = temp;
            }

            for (int x = (int)start.x; x <= (int)end.x; x++)
            {
                for (int i = -(thickness / 2); i <= thickness / 2; i++)
                {
                    float y = start.y + slope * (x - start.x) + i;
                    texture2D.SetPixel(x, height - (int)y - 1, color);
                }
            }
        }
        else // 竖向线条
        {
            if (start.y > end.y)
            {
                Vector2 temp = start;
                start = end;
                end = temp;
            }

            for (int y = (int)start.y; y <= (int)end.y; y++)
            {
                for (int i = -(thickness / 2); i <= thickness / 2; i++)
                {
                    float x = start.x + (y - start.y) / slope + i;
                    texture2D.SetPixel((int)x, height - y - 1, color);
                }
            }
        }
    }
    private void SettargeDict()
    {
        targetDict = new Dictionary<string, targetInfo.BodyPart>()
        {
            //{ "nose", target_points.body_parts.nose },
            //{ "left_eye", target_points.body_parts.left_eye },
            //{ "right_eye", target_points.body_parts.right_eye },
            { "left_ear", target_points.body_parts.left_ear },
            { "right_ear", target_points.body_parts.right_ear },
            { "left_shoulder", target_points.body_parts.left_shoulder },
            { "right_shoulder", target_points.body_parts.right_shoulder },
            { "left_elbow", target_points.body_parts.left_elbow },
            { "right_elbow", target_points.body_parts.right_elbow },
            { "left_wrist", target_points.body_parts.left_wrist },
            { "right_wrist", target_points.body_parts.right_wrist },
            { "left_hip", target_points.body_parts.left_hip },
            { "right_hip", target_points.body_parts.right_hip },
            { "left_knee", target_points.body_parts.left_knee },
            { "right_knee", target_points.body_parts.right_knee },
            { "left_ankle", target_points.body_parts.left_ankle },
            { "right_ankle", target_points.body_parts.right_ankle },
            { "neck", target_points.body_parts.neck },
            { "top_head", target_points.body_parts.top_head }
        };
    }
    private void SetsampleDict()
    {
        sampleDict = new Dictionary<string, sampleInfo.BodyPart>()
        {
            //{ "nose", sample_points.body_parts.nose },
            //{ "left_eye", sample_points.body_parts.left_eye },
            //{ "right_eye", sample_points.body_parts.right_eye },
            { "left_ear", sample_points.body_parts.left_ear },
            { "right_ear", sample_points.body_parts.right_ear },
            { "left_shoulder", sample_points.body_parts.left_shoulder },
            { "right_shoulder", sample_points.body_parts.right_shoulder },
            { "left_elbow", sample_points.body_parts.left_elbow },
            { "right_elbow", sample_points.body_parts.right_elbow },
            { "left_wrist", sample_points.body_parts.left_wrist },
            { "right_wrist", sample_points.body_parts.right_wrist },
            { "left_hip", sample_points.body_parts.left_hip },
            { "right_hip", sample_points.body_parts.right_hip },
            { "left_knee", sample_points.body_parts.left_knee },
            { "right_knee", sample_points.body_parts.right_knee },
            { "left_ankle", sample_points.body_parts.left_ankle },
            { "right_ankle", sample_points.body_parts.right_ankle },
            { "neck", sample_points.body_parts.neck },
            { "top_head", sample_points.body_parts.top_head }
        };
    }
    private void Setsample()
    {
        SetsampleDict();

        if (images_count > images_count_max)
        {
            jumptomain();
            return;
        }
        foreach (var Dict in sampleDict)
        {
            Dict.Value.isactive = false;
        }

        sample = images[images_count];
        show_sample.texture = sample;

        float zhuanghuanbilv_x = (float)sample.width / ((float)webcamTexture.width);
        float zhuanghuanbilv_y = (float)sample.height / ((float)webcamTexture.height);
       

        JObject result = client.BodyAnalysis(sample.EncodeToJPG());

        string jsonText = result.ToString();

        if (string.IsNullOrEmpty(jsonText))
        {
            return;
        }

        ApiResponse response = JsonConvert.DeserializeObject<ApiResponse>(jsonText);

        foreach (PersonInfo person in response.person_info)
        {
            foreach (FieldInfo part in person.body_parts.GetType().GetFields())
            {
                PersonInfo.BodyPart bodyPart = (PersonInfo.BodyPart)part.GetValue(person.body_parts);
                string partName = part.Name;

                if (sampleDict.ContainsKey(partName) && bodyPart.score >= 0.6)
                {
                    sampleDict[partName].isactive = true;
                    sampleDict[partName].x = bodyPart.x/zhuanghuanbilv_x;
                    sampleDict[partName].y = -bodyPart.y/zhuanghuanbilv_y;
                }
            }
            break;
        }
       
    }
    private void sample_target_match()
    {
        foreach (var Dict in sampleDict)
        {
            if(targetDict.ContainsKey(Dict.Key))
            {
                if (targetDict[Dict.Key].x >= Dict.Value.x-30 && targetDict[Dict.Key].x <= Dict.Value.x + 30 && targetDict[Dict.Key].y >= Dict.Value.y - 30 && targetDict[Dict.Key].y <= Dict.Value.y + 30)
                    Dict.Value.ismatched = true;
                else 
                    Dict.Value.ismatched = false;
            }
            else
            {
                Dict.Value.ismatched = false;
            }
        }
    }
    private int setimges()
    {
        string folderPath = Application.dataPath + "/photo";
        string[] filePaths = System.IO.Directory.GetFiles(folderPath, "*.jpg");
        images = new Texture2D[folderPath.Length];

        for(int i=0;i<filePaths.Length;i++)
        {
            byte[] imageData = System.IO.File.ReadAllBytes(filePaths[i]);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageData);

            images[i] = texture;
        }
        return filePaths.Length;
    }
    private void wincheck()
    {
        foreach (var Dict in sampleDict) 
        { 
            if(Dict.Value.isactive && !Dict.Value.ismatched)
                return;
        }
        images_count++;
        foreach(var Dict in sampleDict)
        {
            Dict.Value.isactive = false;
        }
        Setsample();
    }
    private void jumptomain()
    {
        close_cam();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    public void go_mainmenu()
    {
        close_cam();
        SceneManager.LoadScene(1);
    }
    private void close_cam()
    {
        webcamTexture.Stop();
    }

    public void kaifaceshi()
    {
        images_count++;
        Setsample();
    }
}


