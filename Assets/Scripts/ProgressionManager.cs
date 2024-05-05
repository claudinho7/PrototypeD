using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProgressionManager : MonoBehaviour
{
    public bool easyMeleeTownDone;
    public bool hardMeleeTownDone;
    public bool easyRangedTownDone;
    public bool hardRangedTownDone;

    public int sceneCounter;

    private readonly List<int> _remainingScenes = new();

    private void Start()
    {
        _remainingScenes.Add(1);
        _remainingScenes.Add(3);
        _remainingScenes.Add(5);
        _remainingScenes.Add(7);

        // Shuffle the list of scenes
        Shuffle(_remainingScenes);
        
        // Listen for scene changes
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void Shuffle(IList<int> list)
    {
        for (var i = 0; i < list.Count; i++)
        {
            var temp = list[i];
            var randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.buildIndex)
        {
            case 1:
                easyMeleeTownDone = true;
                break;
            case 3:
                hardMeleeTownDone = true;
                break;
            case 5:
                easyRangedTownDone = true;
                break;
            case 7:
                hardRangedTownDone = true;
                break;
        }
    }

    public void PickNextScene()
    {
        var nextSceneIndex = GetNextSceneIndex();
        if (nextSceneIndex != -1)
        {
            NextScene(nextSceneIndex);
            sceneCounter++;
        }
        else
        {
            // Handle the case where all scenes have been completed
            Debug.Log("All scenes done!");
            SceneManager.LoadScene(9);
        }
    }

    private int GetNextSceneIndex()
    {
        foreach (var sceneIndex in _remainingScenes.Where(CanLoadScene))
        {
            _remainingScenes.Remove(sceneIndex);
            return sceneIndex;
        }

        return -1; // Indicates that all scenes are done
    }

    private bool CanLoadScene(int sceneIndex)
    {
        return sceneIndex switch
        {
            1 => !easyMeleeTownDone,
            3 => !hardMeleeTownDone,
            5 => !easyRangedTownDone,
            7 => !hardRangedTownDone,
            _ => false
        };
    }

    private static void NextScene(int index)
    {
        SceneManager.LoadScene(index);
    }
}
