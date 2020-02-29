using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GoogleSheet : MonoBehaviour
{

    public GameObject Name;
    public GameObject Email;
    public GameObject Phone;

    private string NameStr;
    private string EmailStr;
    private string PhoneStr;

    public string BASE_URL = "https://docs.google.com/forms/u/0/d/e/1FAIpQLSfVD98HYPSFqwRD7B9UIfIgAGruzN1vFsIcYLZnufPEYXCIAA/formResponse";
    //表單網頁按右鍵 檢視網頁原始碼 搜尋"form action"

    IEnumerator Post(string name, string email, string phone)
    {
        WWWForm form = new WWWForm();
        form.AddField("entry.2031331896", name);  //去表單輸入為按右鍵 檢查 Name的區域
        form.AddField("entry.1207079978", email);
        form.AddField("entry.1536568566", phone);
        byte[] rawData = form.data;
        //WWW www = new WWW(BASE_URL, rawData);  
        //yield return www;
        UnityWebRequest www = UnityWebRequest.Post(BASE_URL, form); //www過時了 所以改用UnityWebRequest
        yield return www.SendWebRequest();  //return 這邊記得要寫對


    }



    public void Send()
    {
        NameStr = Name.GetComponent<InputField>().text;
        EmailStr = Email.GetComponent<InputField>().text;
        PhoneStr = Phone.GetComponent<InputField>().text;

        StartCoroutine(Post(NameStr,EmailStr,PhoneStr));
    }



}
