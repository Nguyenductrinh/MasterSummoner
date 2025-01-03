using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDetails : MonoBehaviour
{
    [SerializeField] List<SceneDetails> connectedScenes;
    [SerializeField] AudioClip sceneMusic;

    List<SavableEntity> savableEntities;
    public bool IsLoaded { get; private set; }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            Debug.Log($" Entered {gameObject.name} ");

            LoadScene();
            GameController.i.SetCurrentScene(this);

            // Audio
            if(sceneMusic != null)
            {
                AudioManager.i.PlayMusic(sceneMusic, fade: true);
            }
                

            // Load all scene
            foreach (var scene in connectedScenes)
            {
                scene.LoadScene();
            }

            // Unload the scene
            var prevScene = GameController.i.PrevScene;
            if(GameController.i.PrevScene != null)
            {
                var previoslyLoadScenes = GameController.i.PrevScene.connectedScenes;
                foreach (var scene in connectedScenes)
                {
                    if (!connectedScenes.Contains(scene) && scene != this)
                    {
                        scene.UnLoadScene();
                    }
                }

                if (!connectedScenes.Contains(prevScene))
                {
                    prevScene.UnLoadScene();
                }
                
            }
        }
    }

    public void LoadScene()
    {
        if (!IsLoaded)
        {
            var operation =  SceneManager.LoadSceneAsync(gameObject.name, LoadSceneMode.Additive);
            IsLoaded = true;

            operation.completed += (AsyncOperation op) =>
            {
                var savableEntities = GetSavableEntitiesInScene();
                SavingSystem.i.RestoreEntityStates(savableEntities);
            };
        }
    }

    public void UnLoadScene()
    {
        if (IsLoaded)
        {
            SavingSystem.i.CaptureEntityStates(savableEntities);

            SceneManager.UnloadSceneAsync(gameObject.name);
            IsLoaded = false;
        }
    }

    List<SavableEntity> GetSavableEntitiesInScene()
    {
        var currScene = SceneManager.GetSceneByName(gameObject.name);
        var savableEntities = FindObjectsOfType<SavableEntity>().Where(x => x.gameObject.scene == currScene).ToList();

        return savableEntities;
    }

    public AudioClip SceneMusic => sceneMusic;
}
