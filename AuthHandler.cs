using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

public class AuthHandler : MonoBehaviour
{
    [SerializeField] private TMP_InputField[] datos;
    [SerializeField] private GameObject emergente;
    [SerializeField] private GameObject emergente2;
    [SerializeField] private GameObject ventana1;
    [SerializeField] private GameObject ventana2;
    [SerializeField] private GameObject recordVentana;
    [SerializeField] private GameObject alerta;
    [SerializeField] private TextMeshProUGUI nombre;
    [SerializeField] private TextMeshProUGUI record;
    [SerializeField] private TextMeshProUGUI nombreLista;

    private string url = "https://sid-restapi.onrender.com";
    private string token;
    private string username;
    private int score;


    void Start()
    {
        token = PlayerPrefs.GetString("Token");
        username = PlayerPrefs.GetString("Username");
        score = PlayerPrefs.GetInt("Score");

        if(token != null) 
        {
            StartCoroutine(Autenticacion());
        } 

    }

    private string ObtenerDatos()
    {
        JsonData data = new JsonData();

        data.username = datos[0].text;
        data.password = datos[1].text;

        string postData = JsonUtility.ToJson(data);

        return postData;
    }

    public void BotonLogin()
    {
        string postData = ObtenerDatos();
        StartCoroutine(LoginPost(postData));
    }

    public void BotonRegistro()
    {
        string postData = ObtenerDatos();
        StartCoroutine(RegistroPost(postData));
    }

    public void BotonScore(int funcion)
    {
        if (funcion == 1)
        {
            if (int.TryParse(datos[2].text, out score))
            {
                string jsonString = $"{{\"username\":\"{username}\",\"data\":{{\"score\":{score}}}}}";
                StartCoroutine(ActualizarRecord(jsonString));
            }
            else
            {
                alerta.SetActive(true);
            }
        }
        else
        {
            ventana2.SetActive(false);
            recordVentana.SetActive(true);
        }

    }

    public void BotonLista()
    {
        StartCoroutine(ListaUsuarios());
    }

    public void BotonContinuar()
    {
        ventana1.SetActive(true);
        emergente.SetActive(false);
        emergente2.SetActive(false);
    }

    public void BotonCerrarSesion()
    {
        ventana2.SetActive(false);
        ventana1.SetActive(true);
    }

    IEnumerator RegistroPost(string data)
    {
        UnityWebRequest request = UnityWebRequest.Post(url + "/api/usuarios", data, "application/json");
        yield return request.SendWebRequest();

        if(request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            if(request.responseCode == 200)
            {
                ventana1.SetActive(false);
                emergente.SetActive(true);
            }
            else
            {
                Debug.Log($"Status: {request.responseCode} \n Error: {request.error}");
            }
        }
    }

    IEnumerator LoginPost(string data)
    {
        UnityWebRequest request = UnityWebRequest.Post(url + "/api/auth/login", data, "application/json");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            if (request.responseCode == 200)
            {
                AuthData authData = JsonUtility.FromJson<AuthData>(request.downloadHandler.text);

                token = authData.token;
                username = authData.usuario.username;

                PlayerPrefs.SetString("Token", token);
                PlayerPrefs.SetString("Username", username);

                if(authData.usuario.data.score != 0)
                {
                    PlayerPrefs.SetInt("Score", authData.usuario.data.score);
                    score = authData.usuario.data.score;
                }
                else
                {
                    PlayerPrefs.SetInt("Score", 0);
                    score = 0;
                }

                CargarElementos();
            }
            else
            {
                ventana1.SetActive(false);
                emergente2.SetActive(true);
            }
        }
    }

    IEnumerator Autenticacion()
    {

        UnityWebRequest request = UnityWebRequest.Get(url + "/api/usuarios/" + username);
        request.SetRequestHeader("x-token", token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            if (request.responseCode == 200)
            {
                CargarElementos();
            }
            else
            {
                ventana1.SetActive(true);
            }
        }
    }

    private void CargarElementos()
    {
        ventana1.SetActive(false);
        ventana2.SetActive(true);

        nombre.text = username;
        record.text = score.ToString(); 

        StartCoroutine(ListaUsuarios());
    }

    IEnumerator ActualizarRecord(string scoreData)
    {
        UnityWebRequest request = UnityWebRequest.Put(url + "/api/usuarios", scoreData);
        request.method = "PATCH";
        request.SetRequestHeader("x-token", token);
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            if (request.responseCode == 200)
            {
                alerta.SetActive(false);
                recordVentana.SetActive(false);
                ventana2.SetActive(true);
                record.text = score.ToString();
                StartCoroutine(ListaUsuarios());
            }
            else
            {
                ventana1.SetActive(true);
            }
        }
    }

    public IEnumerator ListaUsuarios()
    {
        UnityWebRequest request = UnityWebRequest.Get(url + "/api/usuarios");
        request.SetRequestHeader("x-token", token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            if (request.responseCode == 200)
            {
                ListUser users = JsonUtility.FromJson<ListUser>(request.downloadHandler.text);

                List<User> listaOrdenada = users.usuarios.OrderByDescending(u => u.data.score).ToList();

                string usuariosTexto = "Lista de Usuarios:\n";
                foreach (var user in listaOrdenada)
                {
                    if (user.data != null)
                    {
                        usuariosTexto += $"Nombre: {user.username}, Score: {user.data.score}\n";
                    }
                    else
                    {
                        usuariosTexto += $"Nombre: {user.username}, Score: No disponible\n";
                    }
                }

                nombreLista.text = usuariosTexto;
            }
        }
    }
}

public class JsonData
{
    public string username;
    public string password;
}

[System.Serializable]
public class AuthData
{
    public User usuario;
    public string token;
}

[System.Serializable]
public class ListUser
{
    public User[] usuarios;
}

[System.Serializable]
public class User
{
    public string _id;
    public string username;
    public bool estado;
    public UserData data;
}

[System.Serializable]
public class UserData
{
    public int score;
}