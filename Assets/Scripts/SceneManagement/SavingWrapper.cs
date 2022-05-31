using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Saving;

namespace RPG.SceneManagement
{
    public class SavingWrapper : MonoBehaviour
    {
        [SerializeField] float fadeInTime = 0.2f;

        SavingSystem savingSystem;
        const string defaultSaveFile = "save";

        void Awake() {
            savingSystem = GetComponent<SavingSystem>();
            StartCoroutine(LoadLastScene());
        }

        public void Load()
        {
            //call to saving system load
            savingSystem.Load(defaultSaveFile);
        }

        public void Save(){
            savingSystem.Save(defaultSaveFile);
        }

        public void Delete(){
            savingSystem.Delete(defaultSaveFile);
        }

        private IEnumerator LoadLastScene(){
            yield return savingSystem.LoadLastScene(defaultSaveFile);
            Fader fader = FindObjectOfType<Fader>();
            fader.FadeOutImmediate();
            yield return fader.FadeIn(fadeInTime);
        }

        private void Update() {
            if(Input.GetKeyDown(KeyCode.L)){
                Load();
            }

            if(Input.GetKeyDown(KeyCode.S)){
                Save();
            }

            if(Input.GetKeyDown(KeyCode.Delete)){
                Delete();
            }
        }
    }
}
