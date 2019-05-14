﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DEVN
{

/// <summary>
/// 
/// </summary>
public class CharacterManager : MonoBehaviour
{
	// singleton
	private static CharacterManager m_instance;

	// scene manager ref
	private SceneManager m_sceneManager;

	private CharacterTransformer m_characterTransformer;

    [Header("Character Prefab")]
	[SerializeField] private GameObject m_characterPrefab;
	private List<GameObject> m_characters;

    [Header("Canvas Panels")]
	[SerializeField] private GameObject m_backgroundPanel;
	[SerializeField] private GameObject m_foregroundPanel;

	#region getters

	public static CharacterManager GetInstance() { return m_instance; }
	public CharacterTransformer GetCharacterTransformer() { return m_characterTransformer; }
    public GameObject GetBackgroundPanel() { return m_backgroundPanel; }

	#endregion

	/// <summary>
	/// 
	/// </summary>
	void Awake()
	{
		m_instance = this; // initialise singleton

		// cache scene manager reference
		m_sceneManager = GetComponent<SceneManager>();
		Debug.Assert(m_sceneManager != null, "DEVN: SceneManager cache unsuccessful!");

		// create character transformer for scaling/translation
		m_characterTransformer = new CharacterTransformer();

		m_characters = new List<GameObject>();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="character"></param>
	/// <param name="sprite"></param>
	/// <param name="xPosition"></param>
	/// <param name="fadeInTime"></param>
	/// <param name="waitForFinish"></param>
	/// <param name="isInvert"></param>
	public void EnterCharacter(Character character, Sprite sprite, float xPosition,
		float fadeInTime = 0.5f, bool waitForFinish = false, bool isInvert = false)
	{
		// check to see if the character already exists in the scene
		if (TryGetCharacter(character) != null)
		{
			Debug.LogWarning("DEVN: Do not attempt to enter two of the same characters.");
			m_sceneManager.NextNode();
			return;
		}

		// create new character and parent it to canvas
		GameObject characterObject = Instantiate(m_characterPrefab);
		characterObject.transform.SetParent(m_backgroundPanel.transform, false);

		// invert if necessary
		if (isInvert)
			m_characterTransformer.SetCharacterScale(characterObject, new Vector2(1, -1));

		// set sprite
		SetSprite(characterObject, sprite); 

		// set position
		m_characterTransformer.SetCharacterPosition(characterObject, new Vector2(xPosition, 0)); 

		// set character info
		CharacterInfo characterInfo = characterObject.GetComponent<CharacterInfo>();
		characterInfo.SetCharacter(character);

		m_characters.Add(characterObject);

		// perform fade-in
		StartCoroutine(FadeIn(characterObject, fadeInTime));
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="character"></param>
	/// <param name="sprite"></param>
	/// <param name="fadeOutTime"></param>
	/// <param name="waitForFinish"></param>
	public void ExitCharacter(Character character, Sprite sprite, float fadeOutTime = 0.5f, 
		bool waitForFinish = false)
	{
		GameObject characterObject = TryGetCharacter(character);

		if (characterObject != null)
		{
			SetSprite(characterObject, sprite); // set sprite

			// perform fade-out and removal
			StartCoroutine(FadeOut(characterObject, fadeOutTime));
			return; // exit succeeded
		}

		Debug.LogWarning("DEVN: Do not attempt to exit a character that is not in the scene.");
		m_sceneManager.NextNode();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="character"></param>
	/// <returns></returns>
	public GameObject TryGetCharacter(Character character)
	{
		// iterate over all characters in scene
		for (int i = 0; i < m_characters.Count; i++)
		{
			GameObject existingCharacter = m_characters[i];
			CharacterInfo characterInfo = existingCharacter.GetComponent<CharacterInfo>();

			if (characterInfo.GetCharacter() == character)
				return existingCharacter; // character found
		}

		return null; // character not found
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="character"></param>
	/// <param name="sprite"></param>
	public void SetSprite(GameObject character, Sprite sprite)
	{
		Image characterImage = character.GetComponent<Image>();
		characterImage.sprite = sprite;
		characterImage.preserveAspect = true;
	}
		
	/// <summary>
	/// 
	/// </summary>
	/// <param name="speakingCharacter"></param>
	public void HighlightSpeakingCharacter(Character speakingCharacter)
	{
		for (int i = 0; i < m_characters.Count; i++)
		{
			GameObject character = m_characters[i];
			character.transform.SetParent(m_backgroundPanel.transform);
			character.GetComponent<Image>().color = new Color(0.75f, 0.75f, 0.75f);
		}

		GameObject characterToHighlight = TryGetCharacter(speakingCharacter);
		characterToHighlight.transform.SetParent(m_foregroundPanel.transform);
		characterToHighlight.GetComponent<Image>().color = new Color(1, 1, 1);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="character"></param>
	/// <param name="fadeInTime"></param>
	/// <param name="waitForFinish"></param>
	/// <returns></returns>
	private IEnumerator FadeIn(GameObject character, float fadeInTime = 0.5f, bool waitForFinish = false)
	{
		// if not "wait for finish", jump straight to next node
		if (!waitForFinish)
			m_sceneManager.NextNode();

		float elapsedTime = 0.0f;

		while (elapsedTime < fadeInTime)
		{
			// adjust character transparency
			float percentage = elapsedTime / fadeInTime;
			character.GetComponent<Image>().color = new Color(1, 1, 1, percentage);
				
			elapsedTime += Time.deltaTime; // increment time
			yield return null;
		}

		// if "wait for finish", go to next node after fade
		if (waitForFinish)
			m_sceneManager.NextNode(); 
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="character"></param>
	/// <param name="fadeOutTime"></param>
	/// <param name="waitForFinish"></param>
	/// <returns></returns>
	private IEnumerator FadeOut(GameObject character, float fadeOutTime = 0.5f, bool waitForFinish = false)
	{
		// if not "wait for finish", jump straight to next node
		if (!waitForFinish)
			m_sceneManager.NextNode();

		float elapsedTime = 0.0f;

		while (elapsedTime < fadeOutTime)
		{
			// adjust character transparency
			float percentage = 1 - (elapsedTime / fadeOutTime);
			character.GetComponent<Image>().color = new Color(1, 1, 1, percentage);
				
			elapsedTime += Time.deltaTime; // increment time
			yield return null;
		}

		// destroy the character
		m_characters.Remove(character);
		Destroy(character);

		// if "wait for finish", go to next node after fade
		if (waitForFinish)
			m_sceneManager.NextNode();
	}
}

}
