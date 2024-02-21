using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Security.Cryptography;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class PlayerScript : MonoBehaviour // Bugs: No cae si es 3 hueco entre dos bloques (Problema de caer?) // Cambiar getComponentsInChildren por una funcion que lo utlice y elimine el padre
{

    public GameObject actualObject;
    private GameObject nextObject;
    private GameObject holdObject;
    public  GameObject[] listObject;
    public GameObject spawnPoint;
    public GameObject nextPoint;
    public float fallTime;
    public float timer;
    public int fallDistance;
    public int x_leftLimit = 2;
    public int x_rightLimit = 9;
    public int y_limit = 1;
    private Transform[,] bloques;
    public GameObject gameOverScreen;
    private int level = 1;
    // Start is called before the first frame update
    void Start()
    {
        bloques = new Transform[20,10];
        for(int i = 0; i < 20; i++)
        {
            for(int j = 0; j<10; j++)
            {
                bloques[i, j] = null;
            }
        }
        int random = UnityEngine.Random.Range(0, listObject.Length - 1);
        Vector3 spawnPosition = SpawnPosition(listObject[random].name+"(Clone)");
        actualObject = Instantiate(listObject[random], spawnPosition, spawnPoint.transform.rotation);
         random = UnityEngine.Random.Range(0, listObject.Length - 1);
        nextObject = Instantiate(listObject[random], nextPoint.transform.position, spawnPoint.transform.rotation);
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameOverScreen.activeSelf)
        {
            Timer();
            blockController();
        }
    }

    private float tiempoUltimoPaso = 0f;
    public float retardoEntrePasos = 0.5f;
    void blockController() // Falta: Cambiar los Can, tiene que comprobar cada objeto no solo el mas izq o mas der, si no da error Mejoras: Poder mantener la tecla pulsada pero que no se vaya demasiado
    {
        if (Input.GetKey(KeyCode.A) && CanMoveLeft() && Time.time - tiempoUltimoPaso > retardoEntrePasos)
        {
            actualObject.transform.position += Vector3.left;
            tiempoUltimoPaso = Time.time;
        }
        if (Input.GetKey(KeyCode.D) && CanMoveRight() && Time.time - tiempoUltimoPaso > retardoEntrePasos)
        {
            actualObject.transform.position += Vector3.right;
            tiempoUltimoPaso = Time.time;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            Caer();
        }
        if (Input.GetKeyDown(KeyCode.W)) // Añadir para comprobar si puede rotar antes
        {
            Quaternion actualRotation = actualObject.transform.rotation;
            bool correctRotation = true;
            do
            {
                correctRotation = true;
                actualObject.transform.Rotate(0, 0, 90);
                foreach (Transform t in actualObject.GetComponentsInChildren<Transform>())
                {
                    if (t.gameObject != actualObject)
                    {
                        if(t.position.x >= 0 && t.position.x <= 9 && t.position.y > 0)
                        {
                            if(bloques[(int)t.position.y, (int)t.position.x] != null){
                                correctRotation = false;
                            }
                        }
                    }
                }

            } while (actualRotation != actualObject.transform.rotation && !correctRotation);

            checkGrid();
        }
    }
    void Timer()
    {
        if (timer > fallTime)
        {
            Caer();
        }
        else
        {
            timer += Time.deltaTime;
        }
    }

    void Caer() 
    {
        if (CanMoveDown())
        {
            actualObject.transform.position += Vector3.down;
            timer = 0;
        }
        else
        {
            Set();
            checkLineas();
            Spawn();
        }
    }
    
    void Spawn()
    {
        int random = UnityEngine.Random.Range(0, listObject.Length - 1);
        Vector3 spawnPosition = SpawnPosition(nextObject.name);
        actualObject = nextObject;
        actualObject.transform.position = spawnPosition;
        nextObject = Instantiate(listObject[random], nextPoint.transform.position, spawnPoint.transform.rotation);
    }

    Vector3 SpawnPosition(String name)
    {
        if (name != "Cubo(Clone)" && name != "Linea(Clone)")
        {
            
            return spawnPoint.transform.position + (float) 0.5*(Vector3.left+Vector3.down);
        }
        else
        {
            return spawnPoint.transform.position;
        }
    }
    void Set()
    {
       
        foreach(Transform o in actualObject.transform.GetComponentsInChildren<Transform>())
        {
            if (o.position.y >= 20)
            {
                EndGame();
            }
            else
            {
                if (o != actualObject.transform)
                {
                    bloques[(int)Math.Floor(o.position.y), (int)Math.Floor(o.position.x)] = o;

                }
            }
        }
    }
    void EndGame()
    {
        gameOverScreen.SetActive(true);
        Time.timeScale = 0.0f;
    }

    public void restartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Time.timeScale = 1.0f;
    }
    void checkLineas() 
    {
        ArrayList finishedLines = new ArrayList();
        for(int i = 0; i < 20; i++)
        {
            Boolean r = true;
            int j = 0;
            while (j < 10 && r)
            {
                
                if (bloques[i,j] == null)
                {
                    r = false;
                }
                j++;
            }
            if (r)
            {
                finishedLines.Add(i);
                for (j = 0; j < 10; j++)
                {
                    Destroy(bloques[i, j].gameObject);
                }

            }
        }
        int nLines = 0;
        if(finishedLines.Count > 0)
        {
            for (int i = 0; i < 20; i++)
            {
                if (finishedLines.Contains(i))
                {
                    nLines++;
                }
                else
                {
                    for (int j = 0; j < 10; j++)
                    {

                        if (bloques[i, j] != null)
                        {
                            bloques[i, j].position += Vector3.down * nLines;
                            bloques[i - nLines, j] = bloques[i, j];
                            if (nLines > 0)
                            {
                                bloques[i, j] = null;
                            }

                        }
                    }
                }
            }
        }

    }
    
    void checkGrid()
    {
        float maxDifLeft = 10;
        float maxDifRight = -10;
        float maxDifDown = 10;
        float maxDifUp = -10;
        foreach (Transform t in actualObject.GetComponentsInChildren<Transform>())
        {
            if (t.position.x < 0)
            {
                if (t.position.x < maxDifLeft)
                {
                    maxDifLeft = t.position.x;
                }
            }
            if (t.position.x > 10)
            {

                if (t.position.x > maxDifRight)
                {
                    maxDifRight = t.position.x;

                }

            }
            if (t.position.y < 0)
            {
                if (t.position.y < maxDifDown)
                {
                    maxDifDown = t.position.y;

                }
            }
            if (t.position.y > 20)
            {
                if (t.position.y > maxDifUp)
                {
                    maxDifUp = t.position.y;

                }
            }
        }

        if (maxDifLeft != 10)
        {
            actualObject.transform.position += Vector3.right * ((float)Math.Round(-maxDifLeft, MidpointRounding.AwayFromZero));
        }
        if (maxDifRight != -10)
        {
            actualObject.transform.position += Vector3.left * ((float)Math.Round(maxDifRight - 10, MidpointRounding.AwayFromZero));
        }
        if (maxDifDown != 10)
        {
            actualObject.transform.position += Vector3.up * ((float)Math.Round(-maxDifDown, MidpointRounding.AwayFromZero));
        }
        if (maxDifUp != -10)
        {
            actualObject.transform.position += Vector3.down * ((float)Math.Round(maxDifUp - 20, MidpointRounding.AwayFromZero));
        }
    }


    bool CanMoveLeft()
    {
        bool res = true;

        foreach(Transform t in actualObject.GetComponentsInChildren<Transform>()) 
        {
            
            if(t.position.x < x_leftLimit) // Test Limite de Grid
            {
                res = false;
            }
            
        }

        if (!res)
        {
            return res;
        }
        else
        {
            foreach (Transform t in actualObject.GetComponentsInChildren<Transform>())
            {

                if(t.gameObject != actualObject)
                {
                    Transform obj = bloques[(int) Math.Floor(t.position.y), (int)Math.Floor(t.position.x - 1)];
                    if (obj != null && !obj.IsChildOf(actualObject.transform))
                    {
                        res = false;
                    }
                }

            }
        }
        return res;
        

        
    }

    bool CanMoveRight()
    {
        bool res = true;
        foreach (Transform t in actualObject.GetComponentsInChildren<Transform>())
        {
            if (t.position.x > x_rightLimit)
            {
                res = false;
            }
        }
        if (!res)
        {
            return res;
        }
        else
        {
            foreach (Transform t in actualObject.GetComponentsInChildren<Transform>())
            {

                if (t.gameObject != actualObject)
                {
                    if (bloques[(int)t.position.y, (int)t.position.x + 1] != null && !bloques[(int)t.position.y, (int)t.position.x + 1].IsChildOf(actualObject.transform))
                    {
                        res = false;
                    }
                }

            }
        }
        return res;
    }

    bool CanMoveDown()
    {
        bool res = true;
        foreach (Transform t in actualObject.GetComponentsInChildren<Transform>())
        {
            
            if (t.position.y < y_limit)
            {
                
                res = false;
            }
        }

        if (!res)
        {
            return res;
        }
        else
        {
            foreach (Transform t in actualObject.GetComponentsInChildren<Transform>())
            {

                if (t.gameObject != actualObject)
                {
                    if (bloques[(int)t.position.y - 1, (int)t.position.x] != null && !bloques[(int)t.position.y - 1, (int)t.position.x].IsChildOf(actualObject.transform))
                    {
                        res = false;
                    }
                }

            }
        }
        return res;
    }
}
