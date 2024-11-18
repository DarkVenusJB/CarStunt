using System;
using System.Collections.Generic;
using Database;
using UnityEngine;

namespace Components
{
    public class PlayerSkinController : MonoBehaviour
    {
        public List<CharacterInfo> Characters = new();

        public int CurrentCharacterIndex = 0;

        private void OnValidate()
        {
            CurrentCharacterIndex = Mathf.Clamp(CurrentCharacterIndex, 0, Characters.Count);
            ValidateInEditor();
        }

        public void ValidateCharacter(bool isPhysics)
        {
            CurrentCharacterIndex = Characters.FindIndex(e => e.CharacterId == GameDataController.SelectedCharacter);
            
            for (int i = 0; i < Characters.Count; i++)
            {
                CharacterInfo character = Characters[i];
                character.Parent.SetActive(i == CurrentCharacterIndex);
                if (i == CurrentCharacterIndex)
                {
                    character.Animator.enabled = !isPhysics;
                    foreach (Rigidbody rigidbody in character.Rigidbodies) rigidbody.isKinematic = !isPhysics;
                    foreach (Collider collider in character.Colliders) collider.enabled = isPhysics;
                }
            }
        }

        private void ValidateInEditor()
        {
            for (int i = 0; i < Characters.Count; i++)
            {
                CharacterInfo character = Characters[i];
                character.Parent.SetActive(i == CurrentCharacterIndex);
                if (i == CurrentCharacterIndex)
                {
                    character.Animator.enabled = false;
                    foreach (Rigidbody rigidbody in character.Rigidbodies) rigidbody.isKinematic = false;
                    foreach (Collider collider in character.Colliders) collider.enabled = true;
                }
            }   
        }
    }

    [Serializable]
    public class CharacterInfo
    {
        public string CharacterId;
        public GameObject Parent;
        public Animator Animator;
        public Transform CameraTarget;
        public List<Collider> Colliders;
        public List<Rigidbody> Rigidbodies;
    }
}