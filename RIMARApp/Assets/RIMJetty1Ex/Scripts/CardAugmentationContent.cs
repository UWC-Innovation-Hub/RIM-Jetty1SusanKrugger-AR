using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardAugmentationContent : MonoBehaviour
{
    [Header("Content Types")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private GameObject floatingCard;
    [SerializeField] private ParticleSystem magicEffect;
    [SerializeField] private TextMeshProUGUI infoText;
    
    [Header("Card Information")]
    [SerializeField] private string[] cardFacts = {
        "Standard deck has 52 cards",
        "4 suits: Hearts, Diamonds, Clubs, Spades",
        "Each suit has 13 cards",
        "Face cards: Jack, Queen, King",
        "Jokers are often included as extras"
    };
    
    [Header("Animation")]
    [SerializeField] private float floatHeight = 0.1f;
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private bool animateFloatingCard = true;
    
    private int currentFactIndex = 0;
    private Vector3 initialCardPosition;
    
    void Start()
    {
        if (floatingCard != null)
        {
            initialCardPosition = floatingCard.transform.localPosition;
        }
        
        // Show initial card fact
        ShowRandomFact();
        
        // Start magic effect
        if (magicEffect != null)
        {
            magicEffect.Play();
        }
        
        // Animate floating card
        if (animateFloatingCard && floatingCard != null)
        {
            StartCoroutine(AnimateFloatingCard());
        }
    }
    
    public void ShowRandomFact()
    {
        if (infoText != null && cardFacts.Length > 0)
        {
            currentFactIndex = Random.Range(0, cardFacts.Length);
            infoText.text = cardFacts[currentFactIndex];
        }
    }
    
    public void NextFact()
    {
        if (cardFacts.Length > 0)
        {
            currentFactIndex = (currentFactIndex + 1) % cardFacts.Length;
            if (infoText != null)
            {
                infoText.text = cardFacts[currentFactIndex];
            }
        }
    }
    
    private System.Collections.IEnumerator AnimateFloatingCard()
    {
        while (floatingCard != null && floatingCard.activeInHierarchy)
        {
            float newY = initialCardPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            floatingCard.transform.localPosition = new Vector3(
                initialCardPosition.x, 
                newY, 
                initialCardPosition.z
            );
            
            // Gentle rotation
            floatingCard.transform.Rotate(0, 30f * Time.deltaTime, 0);
            
            yield return null;
        }
    }
    
    public void ToggleInfoPanel()
    {
        if (infoPanel != null)
        {
            infoPanel.SetActive(!infoPanel.activeSelf);
        }
    }
    
    public void PlayMagicEffect()
    {
        if (magicEffect != null)
        {
            magicEffect.Stop();
            magicEffect.Play();
        }
    }
}