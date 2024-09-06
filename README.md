## Peticiones HTTP GET, POST y PATCH🖥️
<p align="center">
  <img style="width: 700px; height: auto;" src="https://github.com/TatoDesignn/Repositorios-Imagenes/blob/main/Http/act3.png">
</p> 

Consultas web por medio de unity, utilizando el sistema `UnityWebRequest`, sid-restapi para el gestionamiento de registro y entrada de nuevos perfiles, almecenamiento de datos en su objeto *Data*. 

## Procedimiento 🃏
- Importamos la libreria de Unity Networking:
```C#
    using UnityEngine.Networking;
```
- Definimos la direccion de la api y 3 variables:
```C#
    private string url = "https://sid-restapi.onrender.com";
    private string token;
    private string username;
    private int score;
```
- Definimos una clase con la cual convertimos los datos almacenados para luego ser enviados a la *API* por el metodo `POST`, esta debe de tener los mismo nombres que en la *API*. 
```C#
    public class JsonData
    {
        public string username;
        public string password;
    }
```
- Definimos dos metedos para el registro, el primero se encarga de obtener los datos del input, para asi enviarlos al nuevo objeto de la clase, seguido son convertidos en un tipo json (*string*). El segundo metodo se encarga de hacer el registro por el metodo `POST`, para esto utilizamos `UnityWebRequest.Post` el cual recibe tres parametros `url`, `data`, `"application/json"`.
```C#
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
```
```C#
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
```
- Para el **Login** funciona casi igual que el registro, nuestro primer metodo se encarga de obtener los datos del input, el segundo metodo cambiamos la url por `/api/auth/login`, de la misma manera que el anterior utilizamos el metodo `POST` y en caso de ser exitoso el acceso guardamos el *token*, *username* y *score* en los `PlayerPrefs` para luego con esto acceder automaticamente.
```C#
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
```

## Mas Información 💾
<ul>
  <li><a href="https://tatodesignn.github.io/HTTPS-Trabajo3-POST/">Link de GitHub pages</a></li>
</ul>
